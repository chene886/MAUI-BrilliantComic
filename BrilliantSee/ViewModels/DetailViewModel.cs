﻿using BrilliantSee.Models.Items;
using BrilliantSee.Models.Objs;
using BrilliantSee.Models.Enums;
using BrilliantSee.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BrilliantSee.ViewModels
{
    public partial class DetailViewModel : ObservableObject, IQueryAttributable
    {
        public readonly DBService _db;
        private readonly MessageService _ms;

        //private readonly AIService _ai;

        /// <summary>
        /// 当前漫画
        /// </summary>
        [ObservableProperty]
        private Obj? _obj;

        [ObservableProperty]
        public string _videoUrl = string.Empty;

        /// <summary>
        /// 收藏图标
        /// </summary>
        [ObservableProperty]
        private ImageSource? _favoriteImage;

        /// <summary>
        /// 排序图标
        /// </summary>
        [ObservableProperty]
        private ImageSource? _orderImage = ImageSource.FromFile("reverse.png");

        /// <summary>
        /// 是否允许反转章节列表
        /// </summary>
        [ObservableProperty]
        private bool _isReverseListEnabled;

        /// <summary>
        /// 是否正在获取结果
        /// </summary>
        [ObservableProperty]
        private bool _isGettingResult;

        [ObservableProperty]
        public IEnumerable<Item> _itemsOnDisPlay = new List<Item>();

        public DetailViewModel(DBService db, MessageService ms)
        {
            _db = db;
            _ms = ms;
            //_ai = MauiProgram.servicesProvider!.GetRequiredService<AIService>();
            //if (_ai.hasModel)
            //{
            //    _ai.RemovePlugins();
            //    _ai.ImportPlugins(new Services.Plugins.DetailPlugins(_db));
            //}
        }

        /// <summary>
        /// 设置当前漫画，加载更多漫画数据，储存漫画到历史记录
        /// </summary>
        /// <param name="query">储存导航传递数据的字典</param>
        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (Obj is not null)
            {
                return;
            }
            Obj = query["Obj"] as Obj;

            IsGettingResult = true;
            IsReverseListEnabled = false;
            var isExist = await _db.IsComicExistAsync(Obj!, DBObjCategory.Favorite);
            if (isExist)
            {
                FavoriteImage = ImageSource.FromFile("is_favorite.png");
                Obj!.Category = DBObjCategory.Favorite;
            }
            else FavoriteImage = ImageSource.FromFile("not_favorite.png");
            var isSuccess = true;
            if (!Obj!.Items.Any())
            {
                isSuccess = await Obj!.GetHtmlAsync();
                if (isSuccess)
                {
                    Obj!.LoadMoreData();
                    OnPropertyChanged(nameof(Obj));
                    await Task.Run(() => Obj!.LoadItemsAsync());
                    if (isExist && Obj!.IsUpdate)
                    {
                        Obj!.IsUpdate = false;
                        _ = _db.UpdateComicAsync(Obj!);
                    }
                }
                else _ms.WriteMessage("好像出了点小问题，用浏览器打开试试吧");
            }
            ItemsOnDisPlay = new ObservableCollection<Item>(Obj!.Items.Where(c => c.Route == "线路一"));
            IsReverseListEnabled = true;
            IsGettingResult = false;
            if (isSuccess) _ = AddHistoryAsync();
        }

        /// <summary>
        /// 储存漫画到历史记录
        /// </summary>
        private async Task AddHistoryAsync()
        {
            await _db.SaveObjAsync(Obj!, DBObjCategory.History);
        }

        /// <summary>
        /// 切换收藏状态并储存漫画或删除漫画
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        private async Task SwitchIsFavorite()
        {
            if (Obj is null)
            {
                return;
            }
            if (Obj.Category == DBObjCategory.Favorite)
            {
                FavoriteImage = ImageSource.FromFile("not_favorite.png");
                await _db.DeleteObjAsync(Obj, Obj.Category);
                Obj.Category = DBObjCategory.History;
            }
            else
            {
                FavoriteImage = ImageSource.FromFile("is_favorite.png");
                Obj.Category = DBObjCategory.Favorite;
                await _db.SaveObjAsync(Obj, Obj.Category);
            }
            OnPropertyChanged(nameof(Obj));
        }

        /// <summary>
        /// 跳转到浏览器浏览漫画
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        private async Task JumpToBrowserAsync()
        {
            await Launcher.OpenAsync(new Uri(Obj!.Url));
        }

        /// <summary>
        /// 章节列表倒序
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        private async Task ReverseListAsync()
        {
            IsGettingResult = true;
            IsReverseListEnabled = false;
            Obj!.IsReverseList = !Obj.IsReverseList;
            if (!Obj!.IsReverseList)
            {
                OrderImage = ImageSource.FromFile("positive.png");
            }
            else
            {
                OrderImage = ImageSource.FromFile("reverse.png");
            }
            await Task.Run(() =>
            {
                Obj.Items = Obj.Items.Reverse();
                if (ItemsOnDisPlay.Any())
                {
                    ItemsOnDisPlay = ItemsOnDisPlay.Reverse();
                }
            });
            IsReverseListEnabled = true;
            IsGettingResult = false;
        }

        /// <summary>
        /// 跳转到章节浏览页
        /// </summary>
        /// <param name="chapter">导航传递章节</param>
        /// <returns></returns>
        [RelayCommand]
        private async Task OpenChapterAsync(Item chapter)
        {
            if (chapter.Name != "暂无章节")
            {
                var page = chapter.Obj.SourceCategory == SourceCategory.Comic ? "BrowsePage" : "NovelPage";
                await Shell.Current.GoToAsync(page, new Dictionary<string, object> { { "Chapter", chapter } });
            }
            else
            {
                _ms.WriteMessage("章节无法显示");
            }
        }

        [RelayCommand]
        private async Task SetVideoAsync(Item video)
        {
            _ = video.Obj.ChangeLastReadedItemIndex(video.Index, _db);
            if (video.VideoUrl != string.Empty)
            {
                VideoUrl = video.VideoUrl;
                return;
            }
            try
            {
                await video.GetResourcesAsync();
            }
            catch (Exception e)
            {
                if (e.Message == "请求失败") _ms.WriteMessage(e.Message);
                else _ms.WriteMessage("好像出了点小问题，用浏览器打开试试吧");
            }
            VideoUrl = video.VideoUrl;
        }

        /// <summary>
        /// 跳转到最后浏览章节
        /// </summary>
        [RelayCommand]
        private async Task OpenHistoryAsync()
        {
            if (Obj!.LastReadedItemIndex != -1 && Obj.Items.Any())
            {
                var historyItem = Obj.Items.ToList().Find(c => c.Index == Obj.LastReadedItemIndex);
                if (Obj.SourceCategory == SourceCategory.Video) await SetVideoAsync(historyItem!);
                else await OpenChapterAsync(historyItem!);
            }
            else
            {
                _ms.WriteMessage("暂无章节浏览记录");
            }
        }

        public void SetItemsOnDisplay(string route)
        {
            //IsGettingResult = true;
            ItemsOnDisPlay = Obj!.Items.Where(c => c.Route == route);
            //IsGettingResult = false;
        }
    }
}