﻿using BrilliantSee.Models;
using BrilliantSee.Services;
using BrilliantSee.Views;

namespace BrilliantSee
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();

            //注册页面路由
            Routing.RegisterRoute("SettingPage", typeof(SettingPage));
            Routing.RegisterRoute("SearchPage", typeof(SearchPage));
            Routing.RegisterRoute("DetailPage", typeof(DetailPage));
            Routing.RegisterRoute("BrowsePage", typeof(BrowsePage));
            Routing.RegisterRoute("AIPage", typeof(AIPage));
            Routing.RegisterRoute("VideoPage", typeof(VideoPage));
            Routing.RegisterRoute("NovelPage", typeof(NovelPage));
        }
    }
}