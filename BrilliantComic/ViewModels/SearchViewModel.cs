﻿using BrilliantComic.Models;
using BrilliantComic.Models.Chapters;
using BrilliantComic.Models.Comics;
using BrilliantComic.Models.Sources;
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
    public partial class SearchViewModel : ObservableObject
    {
        private readonly SourceService _sourceService;

        public readonly DBService _db;

        private readonly AIService _ai;

        /// <summary>
        /// 储存搜索漫画的结果集合
        /// </summary>
        public ObservableCollection<Comic> Comics { get; set; } = new();

        /// <summary>
        /// 是否正在获取结果
        /// </summary>
        [ObservableProperty]
        private bool _isGettingResult;

        [ObservableProperty]
        private bool _isSourceListVisible = false;

        [ObservableProperty]
        private List<ISource> _sources = new();

        private List<SettingItem> SettingItems { get; set; } = new();

        public SearchViewModel(SourceService sourceService, DBService db)
        {
            _sourceService = sourceService;
            _db = db;
            _ai = MauiProgram.servicesProvider!.GetRequiredService<AIService>();
            Sources = _sourceService.GetSources();
            _ = InitSettingsAsync();
            if (_ai.hasModel)
            {
                _ai.RemovePlugins();
                _ai.ImportPlugins(new Services.Plugins.SearchPlugins(_db, _sourceService));
            }
        }

        public async Task InitSettingsAsync()
        {
            SettingItems = await _db.GetSettingItemsAsync("Source");
            foreach (var source in Sources)
            {
                source.IsSelected = SettingItems.First(s => s.Name == source.Name).Value == "IsSelected";
            }
        }

        /// <summary>
        /// 搜索漫画
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [RelayCommand]
        private async Task SearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                _ = Toast.Make("请输入正确的关键词").Show();
                return;
            }
            var hasSourceSelected = Sources.Where(s => s.IsSelected == true).Count() > 0;
            if (hasSourceSelected)
            {
                IsGettingResult = true;
                IsSourceListVisible = false;
                Comics.Clear();
                await _sourceService.SearchAsync(keyword, Comics, "Default");
                if (Comics.Count == 0) { _ = Toast.Make("搜索结果为空，换一个图源试试吧").Show(); }
                IsGettingResult = false;
            }
            else
            {
                _ = Toast.Make("请选择至少一个图源").Show();
                return;
            }
        }

        /// <summary>
        /// 打开漫画详情页并传递漫画对象
        /// </summary>
        /// <param name="comic">指定打开的漫画</param>
        /// <returns></returns>
        [RelayCommand]
        private async Task OpenComicAsync(Comic comic)
        {
            IsSourceListVisible = false;
            comic.Chapters = new List<Chapter>();
            await Shell.Current.GoToAsync("DetailPage", new Dictionary<string, object> { { "Comic", comic } });
        }

        [RelayCommand]
        private async Task ChangeIsSelectedAsync(ISource source)
        {
            source.IsSelected = !source.IsSelected;
            var item = SettingItems.First(s => s.Name == source.Name);
            item!.Value = source.IsSelected ? "IsSelected" : "NotSelected";
            await _db.UpdateSettingItemAsync(item);
        }

        [RelayCommand]
        private void ChangeSourceListVisible()
        {
            IsSourceListVisible = !IsSourceListVisible;
        }
    }
}