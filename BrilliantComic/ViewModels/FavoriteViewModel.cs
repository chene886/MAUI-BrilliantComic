﻿using BrilliantComic.Models;
using BrilliantComic.Models.Comics;
using BrilliantComic.Models.Enums;
using BrilliantComic.Services;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrilliantComic.ViewModels
{
    public partial class FavoriteViewModel : ObservableObject
    {
        public readonly DBService _db;
        public readonly AIService _ai;

        /// <summary>
        /// 是否正在获取结果
        /// </summary>
        [ObservableProperty]
        private bool _isGettingResult;

        private bool IsRefresh { get; set; } = false;

        public List<SettingItem> modelConfigs { get; set; } = new List<SettingItem>();

        /// <summary>
        /// 储存收藏漫画集合
        /// </summary>
        public ObservableCollection<Comic> Comics { get; set; } = new();

        /// <summary>
        /// 加载收藏漫画
        /// </summary>
        /// <returns></returns>
        public async Task OnLoadFavoriteComicAsync()
        {
            Comics.Clear();
            IsGettingResult = true;
            var comics = await _db.GetComicsAsync(DBComicCategory.Favorite);
            comics.Reverse();
            foreach (var item in comics)
            {
                Comics.Add(item);
            }
            if (!IsRefresh)
            {
                IsGettingResult = false;
            }
        }

        public FavoriteViewModel(DBService db)
        {
            _db = db;
            _ai = MauiProgram.servicesProvider!.GetRequiredService<AIService>();
            _ = InitKernelAsync();
        }

        private async Task InitKernelAsync()
        {
            modelConfigs = await _db.GetSettingItemsAsync("AIModel");
            var modelId = modelConfigs.Where(s => s.Name == "ModelId").First().Value;
            var apiKey = modelConfigs.Where(s => s.Name == "ApiKey").First().Value;
            var apiUrl = modelConfigs.Where(s => s.Name == "ApiUrl").First().Value;
            if (modelId != "" && apiKey != "" && apiUrl != "")
            {
                _ai.InitKernel(modelId, apiKey, apiUrl);
                _ai.hasModel = true;
            }
        }

        /// <summary>
        /// 导航到漫画详情页并传递漫画对象
        /// </summary>
        /// <param name="comic">指定打开的漫画</param>
        /// <returns></returns>
        [RelayCommand]
        private async Task OpenComicAsync(Comic comic)
        {
            await Shell.Current.GoToAsync("DetailPage", new Dictionary<string, object> { { "Comic", comic } });
        }

        [RelayCommand]
        private async Task CancelFavoriteAsync(Comic comic)
        {
            await _db.DeleteComicAsync(comic, comic.Category);
            Comics.Remove(comic);
        }

        /// <summary>
        /// 检查收藏漫画是否有更新
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        private async Task CheckForUpdatedAsync() =>
            await Task.Run(async () =>
                {
                    var hasComicUpdate = false;
                    var message = "暂无收藏漫画";
                    _ = MainThread.InvokeOnMainThreadAsync(() => { IsGettingResult = true; IsRefresh = true; });
                    var comics = await _db.GetComicsAsync(DBComicCategory.Favorite);
                    if (comics.Count() != 0)
                    {
                        foreach (var item in comics)
                        {
                            if (!item.IsUpdate)
                            {
                                var isSuccess = await item.GetHtmlAsync();
                                if (isSuccess)
                                {
                                    var lastestChapterName = item.GetLastestChapterName();
                                    if (lastestChapterName is not null && lastestChapterName != item.LastestChapterName)
                                    {
                                        item.IsUpdate = true;
                                        hasComicUpdate = true;
                                        await _db.SaveComicAsync(item, DBComicCategory.Favorite);
                                        _ = MainThread.InvokeOnMainThreadAsync(() => _ = OnLoadFavoriteComicAsync());
                                    }
                                }
                                else
                                {
                                    var message1 = $"{item.Name}检查更新失败,不如点开看看吧";
                                    _ = MainThread.InvokeOnMainThreadAsync(() =>
                                    {
                                        _ = Toast.Make(message1).Show();
                                    });
                                    continue;
                                }
                            }
                        }
                        if (!hasComicUpdate && message == "暂无收藏漫画") message = "暂无漫画更新";
                        else if (hasComicUpdate && message == "暂无收藏漫画") message = "检查更新完成";
                    }
                    _ = MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        IsRefresh = false;
                        IsGettingResult = false;
                        _ = Toast.Make(message).Show();
                    });
                });

        [RelayCommand]
        private async Task JumpToAIPage()
        {
            await Shell.Current.GoToAsync("AIPage");
        }
    }
}