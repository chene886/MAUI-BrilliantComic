﻿using BrilliantComic.Models.Chapters;
using BrilliantComic.Models.Enums;
using BrilliantComic.Models.Sources;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrilliantComic.Models.Comics
{
    public abstract partial class Comic : ObservableObject
    {
        /// <summary>
        /// 储存数据库的主键
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// 封面链接
        /// </summary>
        public string Cover { get; set; } = string.Empty;

        /// <summary>
        /// 漫画名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 漫画作者
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// 漫画简介
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 漫画链接
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// 最后阅读章节索引
        /// </summary>
        public int LastReadedChapterIndex { get; set; } = -1;

        /// <summary>
        /// 漫画源
        /// </summary>
        public required ISource Source { get; set; }

        /// <summary>
        /// 漫画源名
        /// </summary>
        public string SourceName { get; set; } = string.Empty;

        /// <summary>
        /// 最新更新时间
        /// </summary>
        public string LastestUpdateTime { get; set; } = string.Empty;

        /// <summary>
        /// 漫画状态
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 漫画章节
        /// </summary>
        [ObservableProperty]
        public IEnumerable<Chapter> _chapters = new List<Chapter>();

        /// <summary>
        /// 漫画章节是否倒序
        /// </summary>
        public bool IsReverseList { get; set; } = false;

        /// <summary>
        /// 漫画分类
        /// </summary>
        public DBComicCategory Category { get; set; } = DBComicCategory.Default;

        /// <summary>
        /// 获取更多漫画数据
        /// </summary>
        /// <returns></returns>
        public abstract Task LoadMoreDataAsync();

        /// <summary>
        /// 加载章节消息
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public abstract Task LoadChaptersAsync(bool flag);

        /// <summary>
        /// 从当前章节获取上一章节或下一章节
        /// </summary>
        /// <param name="chapter">当前章节</param>
        /// <param name="flag">获取上一章节或下一章节的标志</param>
        /// <returns></returns>
        public abstract Chapter GetNearChapter(Chapter chapter, string flag);
    }
}