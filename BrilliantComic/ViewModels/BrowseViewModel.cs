﻿using BrilliantComic.Models.Chapters;
using BrilliantComic.Models.Enums;
using BrilliantComic.Services;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BrilliantComic.ViewModels
{
    public partial class BrowseViewModel : ObservableObject, IQueryAttributable
    {
        private readonly AIService _ai;

        /// <summary>
        /// 当前章节
        /// </summary>
        [ObservableProperty]
        private Chapter? _chapter;

        /// <summary>
        /// 当前章节在已加载章节集合中的索引
        /// </summary>
        public int _currentChapterIndex = 0;

        /// <summary>
        /// 已加载章节集合
        /// </summary>
        [ObservableProperty]
        public List<Chapter> _loadedChapter = new List<Chapter>();

        /// <summary>
        /// 已加载章节图片集合
        /// </summary>
        public ObservableCollection<string> Images { get; set; } = new();

        /// <summary>
        /// 是否正在加载
        /// </summary>
        [ObservableProperty]
        public bool _isLoading = false;

        [ObservableProperty]
        public bool _isShowRefresh = false;

        [ObservableProperty]
        public bool _isShowButton = false;

        [ObservableProperty]
        public string _buttonContent = "点击加载下一话";

        /// <summary>
        /// 当前页码
        /// </summary>
        [ObservableProperty]
        public int _currentPageNum = 1;

        /// <summary>
        /// 定时器
        /// </summary>
        private readonly Timer _timer;

        /// <summary>
        /// 当前时间
        /// </summary>
        public string CurrentTime => DateTime.Now.ToString("HH:mm");

        private readonly DBService _db;

        public int CurrentChapterIndex
        {
            get => _currentChapterIndex;
            set
            {
                if (_currentChapterIndex != value)
                {
                    _currentChapterIndex = value;
                    OnPropertyChanged(nameof(CurrentChapterIndex));
                }
                Chapter!.IsSpecial = false;
                Chapter = LoadedChapter[value];
                Chapter.IsSpecial = true;
            }
        }

        public BrowseViewModel(DBService db)
        {
            _db = db;
            _ai = MauiProgram.servicesProvider!.GetRequiredService<AIService>();
            if (_ai.hasModel)
            {
                _ai.RemovePlugins();
                _ai.ImportPlugins(new Services.Plugins.BrowsePlugins(_db));
            }
            _timer = new Timer((o) => { OnPropertyChanged(nameof(CurrentTime)); }, null, (60 - DateTime.Now.Second) * 1000, 60000);
        }

        /// <summary>
        /// 获取通过导航传递的参数
        /// </summary>
        /// <param name="query">保存传递数据的字典</param>
        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            Chapter = (query["Chapter"] as Chapter)!;
            if (Chapter.Url == "")
            {
                return;
            }
            IsLoading = true;
            var LastIndex = Chapter.Comic.LastReadedChapterIndex;
            if (Chapter.Index != LastIndex)
            {
                if (LastIndex != -1)
                {
                    var position = LastIndex;
                    if (Chapter.Comic.IsReverseList) position = Chapter.Comic.Chapters.Count() - LastIndex - 1;
                    Chapter.Comic.Chapters.ToList()[position].IsSpecial = false;
                }
                Chapter.IsSpecial = true;
                _ = StoreLastReadedChapterIndex();
            }
            await LoadChapterPicAsync(Chapter, "Init");
            OnPropertyChanged(nameof(Chapter));
            IsLoading = false;
            IsShowButton = true;
            LoadedChapter.Add(Chapter);
        }

        /// <summary>
        /// 加载章节图片
        /// </summary>
        /// <param name="chapter">指定的章节</param>
        /// <param name="flag">加载模式</param>
        /// <returns></returns>
        private async Task LoadChapterPicAsync(Chapter chapter, string flag)
        {
            if (chapter.PicUrls.Count == 0)
            {
                try
                {
                    await chapter.GetPicEnumeratorAsync();
                }
                catch (Exception e)
                {
                    if (e.Message == "请求失败") _ = Toast.Make(e.Message).Show();
                    else _ = Toast.Make("好像出了点小问题，用浏览器打开试试吧").Show();
                    throw new Exception(e.Message);
                }
            }
            Images.Clear();
            foreach (var image in chapter.PicUrls)
            {
                Images.Add(image);
            }
            ButtonContent = chapter!.Index == chapter.Comic.ChapterCount - 1 ? "已是最新一话" : "点击加载下一话";
            //var tasks = new List<Task<ImageSource>>();
            //var sourceName = chapter.Comic.SourceName;
            //var results = Array.Empty<ImageSource>();
            //if (sourceName != "mangahasu")
            //{
            //    results = picEnumerator.Select(pic => ImageSource.FromUri(new Uri(pic))).ToArray();
            //}
            //else
            //{
            //foreach (var pic in picEnumerator)
            //{
            //    tasks.Add(Task.Run(async () =>
            //    {
            //        byte[] bytes = await Chapter!.Comic.Source.HttpClient.GetByteArrayAsync(new Uri(pic));
            //        return ImageSource.FromStream(() => new MemoryStream(bytes));
            //    }));
            //}
            //results = await Task.WhenAll(tasks);
            //}
        }

        /// <summary>
        /// 获取新章节并加载图片
        /// </summary>
        /// <param name="flag">指定上一话或下一话</param>
        /// <returns></returns>
        public async Task<bool> UpdateChapterAsync(string flag)
        {
            Chapter? newChapter;
            var hasNew = false;
            if (CurrentChapterIndex > 0 && flag == "Last")
            {
                newChapter = LoadedChapter[CurrentChapterIndex - 1];
                CurrentChapterIndex--;
            }
            else if (CurrentChapterIndex < LoadedChapter.Count - 1 && flag == "Next")
            {
                newChapter = LoadedChapter[CurrentChapterIndex + 1];
                CurrentChapterIndex++;
            }
            else
            {
                hasNew = true;
                newChapter = Chapter!.Comic.GetNearChapter(Chapter, flag);
                if (newChapter is null)
                {
                    return false;
                }
            }
            try
            {
                await LoadChapterPicAsync(newChapter, flag);
            }
            catch { }
            if (hasNew)
            {
                if (flag == "Next")
                {
                    LoadedChapter.Add(newChapter);
                    CurrentChapterIndex++;
                }
                else
                {
                    LoadedChapter.Insert(0, newChapter);
                    CurrentChapterIndex = 0;
                }
            }
            _ = StoreLastReadedChapterIndex();
            return true;
        }

        /// <summary>
        /// 同步收藏和历史的最后阅读章节索引
        /// </summary>
        /// <returns></returns>
        public async Task StoreLastReadedChapterIndex()
        {
            Chapter!.Comic.LastReadedChapterIndex = Chapter.Index;
            var category = Chapter.Comic.Category;
            Chapter.Comic.Category = DBComicCategory.History;
            await _db.UpdateComicAsync(Chapter.Comic);
            if (await _db.IsComicExistAsync(Chapter.Comic, DBComicCategory.Favorite))
            {
                Chapter.Comic.Category = DBComicCategory.Favorite;
                await _db.UpdateComicAsync(Chapter.Comic);
            }
            Chapter.Comic.Category = category;
        }

        [RelayCommand]
        public async Task LoadNearChapterAsync(string flag)
        {
            var result = false;
            var unSuccess = flag == "Next" ? "已是最新一话" : "已是第一话";
            IsLoading = true;
            _ = Toast.Make("正在加载...").Show();
            result = await UpdateChapterAsync(flag);
            if (result)
            {
                _ = Toast.Make("加载成功").Show();
            }
            else
            {
                _ = Toast.Make(unSuccess).Show();
            }
            IsLoading = false;
            IsShowRefresh = false;
        }
    }
}