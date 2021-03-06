﻿using System;
using System.Collections.Generic;
using System.IO;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using InstantMessaging.Helpers;

using Rainbow.Helpers;


namespace InstantMessaging
{
    public partial class App : Application
    {
        internal readonly string APP_ID = "";
        internal readonly string APP_SECRET_KEY = "";
        internal readonly string HOST_NAME = "";

        // Define all Rainbow objects we use
        internal Rainbow.Application RbApplication = null;
        internal Rainbow.Bubbles RbBubbles = null;
        internal Rainbow.Contacts RbContacts = null;
        internal Rainbow.Conversations RbConversations = null;
        internal Rainbow.InstantMessaging RbInstantMessaging = null;
        internal Rainbow.FileStorage RbFileStorage = null;

        // Define Pages used in the Xamarin Application
        internal ConversationsPage ConversationsPage = null; // Page used to display the list of Conversations
        internal Dictionary<String,  ConversationStreamPage> ConversationStreamPageList = null; // Pages used to display the conversation stream - we will store 5 of them - use PeerID as Key

        // To store the current conversation followed
        internal String CurrentConversationId = null;

        public App()
        {
            InitializeComponent();

            string tempFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string appFolderPath = Path.Combine(tempFolder, "Rainbow.CSharp.SDK");

            RbApplication = new Rainbow.Application(appFolderPath);
            RbApplication.SetTimeout(10000);

            RbApplication.SetApplicationInfo(APP_ID, APP_SECRET_KEY);
            RbApplication.SetHostInfo(HOST_NAME);

            RbBubbles = RbApplication.GetBubbles();
            RbContacts = RbApplication.GetContacts();
            RbConversations = RbApplication.GetConversations();
            RbInstantMessaging = RbApplication.GetInstantMessaging();
            RbFileStorage = RbApplication.GetFileStorage();

            InitAvatarPool();
            InitFilePool();

            MainPage = new NavigationPage(new LoginPage());
        }

        private void InitAvatarPool()
        {
            // Get AvatarPool and initialize it
            string avatarsFolderPath = Path.Combine(Helper.GetTempFolder(), "Rainbow.CSharp.SDK", "Avatars");

            // FOR TEST PURPOSE ONLY
            //if (Directory.Exists(avatarsFolderPath))
            //    Directory.Delete(avatarsFolderPath, true);

            AvatarPool avatarPool = AvatarPool.Instance;
            avatarPool.SetAvatarSize(60);
            avatarPool.SetFont("Arial", 20);

            avatarPool.SetFolderPath(avatarsFolderPath);
            avatarPool.SetApplication(ref RbApplication);
        }

        private void InitFilePool()
        {
            // Get FilePool and initialize it
            string filePoolFolderPath = Path.Combine(Helper.GetTempFolder(), "Rainbow.CSharp.SDK", "FileStorage");

            FilePool filePool = FilePool.Instance;

            filePool.SetFolderPath(filePoolFolderPath);
            filePool.SetApplication(ref RbApplication);

            filePool.AllowAutomaticThumbnailDownload = true;
            filePool.AutomaticDownloadLimitSizeForImages = 250 * 1024; // 250 Ko
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }

}
