﻿using BrilliantSee.Models.Objs;
using System.Text.RegularExpressions;

namespace BrilliantSee.Models.Items.Chapters
{
    public class GodaEnChapter : Item
    {
        public GodaEnChapter(string name, string url, int index, bool isSpecial) : base(name, url, index, isSpecial)
        {
        }

        /// <summary>
        /// 获取章节图片
        /// </summary>
        /// <returns>章节图片枚举器</returns>
        /// <exception cref="Exception"></exception>
        public override async Task GetResourcesAsync()
        {
            try
            {
                var html = await Obj.Source.GetHtmlAsync(Url);
                if (html == string.Empty)
                    throw new Exception("请求失败");
                var match = Regex.Matches(html, "<noscript>[\\s\\S]*?src=\"(.*?)\"[\\s\\S]*?</noscript>");
                foreach (Match item in match)
                {
                    PicUrls.Add(item.Groups[1].Value);
                }
                if (PicUrls.Count == 1) PicUrls.Add(PicUrls[0]);
                PageCount = PicUrls.Count;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}