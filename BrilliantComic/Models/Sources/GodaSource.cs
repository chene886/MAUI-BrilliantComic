﻿using BrilliantComic.Models.Comics;
using BrilliantComic.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BrilliantComic.Models.Sources
{
    public partial class GodaSource : ObservableObject, ISource
    {
        public HttpClient HttpClient { get; set; } = new HttpClient();
        public string Name { get; set; } = "G站漫画";

        [ObservableProperty]
        public bool _isSelected = true;

        private readonly SourceService _sourceService;

        public GodaSource(SourceService sourceService)
        {
            _sourceService = sourceService;
        }

        /// <summary>
        /// 搜索匹配关键词的漫画
        /// </summary>
        /// <param name="keyword">搜索关键词</param>
        /// <returns></returns>
        public async Task<IEnumerable<Comic>> SearchAsync(string keyword)
        {
            if (!HttpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                HttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 16_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.6 Mobile/15E148 Safari/604.1 Edg/122.0.0.0");
            }
            HttpClient.DefaultRequestHeaders.Remove("Referer");
            HttpClient.DefaultRequestHeaders.Add("Referer", "https://godamanga.com/");

            var url = $"https://godamanga.com/s/{keyword}?pagw=1";
            try
            {
                var response = await HttpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return Array.Empty<Comic>();
                }
                var html = await response.Content.ReadAsStringAsync();
                string pattern = "pb-2\"[\\s\\S]*?href=\"(.*?)\"[\\s\\S]*?url=(.*?)&[\\s\\S]*?h3[\\s\\S]*?>(.*?)<";
                var matches = Regex.Matches(html, pattern);

                var comics = new List<Comic>();

                foreach (Match match in matches)
                {
                    var comic = new GodaComic("https://godamanga.com" + match.Groups[1].Value, match.Groups[3].Value, match.Groups[2].Value.Replace("%3A", ":").Replace("%2F", "/"), "暂无作者信息")
                    {
                        Source = this,
                        SourceName = "G站漫画",
                        LastestUpdateTime = "(暂无最后更新信息)"
                    };
                    comics.Add(comic);
                }

                return comics;
            }
            catch
            {
                return Array.Empty<Comic>();
            }
        }

        /// <summary>
        /// 从存储的漫画数据创建漫画实体
        /// </summary>
        /// <param name="dbComic"></param>
        /// <returns></returns>
        public Comic CreateComicFromDBComic(DBComic dbComic)
        {
            Comic comic = new GodaComic(dbComic.Url, dbComic.Name, dbComic.Cover, dbComic.Author)
            {
                Id = dbComic.Id,
                Category = dbComic.Category,
                LastReadedChapterIndex = dbComic.LastReadedChapterIndex,
                IsUpdate = dbComic.IsUpdate,
                LastestChapterName = dbComic.LastestChapterName,
                SourceName = dbComic.SourceName,
                Source = this
            };

            return comic;
        }

        /// <summary>
        /// 从漫画实体创建存储的漫画数据
        /// </summary>
        /// <param name="comic"></param>
        /// <returns></returns>
        public DBComic CreateDBComicFromComic(Comic comic)
        {
            return new DBComic
            {
                Id = comic.Id,
                Name = comic.Name,
                Author = comic.Author,
                Cover = comic.Cover,
                Source = _sourceService.GetSourceName(this)!,
                Url = comic.Url,
                Category = comic.Category,
                LastReadedChapterIndex = comic.LastReadedChapterIndex,
                IsUpdate = comic.IsUpdate,
                LastestChapterName = comic.LastestChapterName,
                SourceName = comic.SourceName
            };
        }
    }
}