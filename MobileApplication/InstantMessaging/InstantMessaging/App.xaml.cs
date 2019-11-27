using System;
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

        //internal readonly string APP_ID = "6f8c5910725b11e9b55c81be00bebc2c";
        //internal readonly string APP_SECRET_KEY = "qSmr71s7idLiKRmhXGNNIOpPJynliqrS2sHKy3Wzk6ytauRSP13qebuJQmRHGwTN";
        //internal readonly string HOST_NAME = "openrainbow.net";

        //internal readonly string LOGIN_USER1 = "irlesuser1@sophia.com";
        //internal readonly string PASSWORD_USER1 = "Rainbow1!";
        //internal readonly string JID_NODE_USER1 = "7ebaaacfa0634f46a903bcdd83ae793b";   // ???
        //internal readonly string ID_USER1 = "59e9ad871b4c2046d17027ce";                 // ???


        //internal readonly string APP_ID = "6f8c5910725b11e9b55c81be00bebc2c";
        //internal readonly string APP_SECRET_KEY = "qSmr71s7idLiKRmhXGNNIOpPJynliqrS2sHKy3Wzk6ytauRSP13qebuJQmRHGwTN";
        //internal readonly string HOST_NAME = "openrainbow.net";

        //internal readonly string LOGIN_USER1 = "aliciawebrtc@jmr.test.openrainbow.net";
        //internal readonly string PASSWORD_USER1 = "1!Poulette";
        //internal readonly string JID_NODE_USER1 = "7ebaaacfa0634f46a903bcdd83ae793b";   // ???
        //internal readonly string ID_USER1 = "59e9ad871b4c2046d17027ce";                 // ???

        ///////////////////////////////////////////////
        /////
        //// COM ENVIRONMENT - START
        //// 
        internal readonly string APP_ID = "7d20cfd0fedd11e8906d03093170a237";
        internal readonly string APP_SECRET_KEY = "Ql8rPqTAeddxerX0mpNnnulZx2bxtCB29SFimb8NfjxzczQqFMIoEMjMQu8m2IPJ";
        internal readonly string HOST_NAME = "openrainbow.com";

        // Login, Password JId Node to connect on server
        internal readonly string LOGIN_USER1 = "christophe.irles@al-enterprise.com";
        internal readonly string PASSWORD_USER1 = "Back9fun!!!!";
        internal readonly string JID_NODE_USER1 = "bf036de2873749038589897a63eb4c42";
        internal readonly string ID_USER1 = "570e12832d768e9b52a8b7bf";
        ////
        /// </summary>
        //// COM ENVIRONMENT - END
        ////
        /////////////////////////////////////////////


        // Define all Rainbow objects we use
        internal Rainbow.Application RbApplication = null;
        internal Rainbow.Bubbles RbBubbles = null;
        internal Rainbow.Contacts RbContacts = null;
        internal Rainbow.Conversations RbConversations = null;
        internal Rainbow.InstantMessaging RbInstantMessaging = null;
        
        //internal Rainbow.Favorites RbFavorites = null;
        //internal Rainbow.FileStorage RbFileStorage = null;

        // Define Pages used in the Xamarin Application
        internal ConversationsPage ConversationsPage = null; // Page used to display the list of Conversations
        internal Dictionary<String,  ConversationStreamPage> ConversationStreamPageList = null; // Pages used to display the conversation stream - we will store 5 of them - use PeerID as Key

        public App()
        {
            InitializeComponent();

            RbApplication = new Rainbow.Application();
            RbApplication.SetTimeout(10000);

            RbApplication.SetApplicationInfo(APP_ID, APP_SECRET_KEY);
            RbApplication.SetHostInfo(HOST_NAME);

            RbBubbles = RbApplication.GetBubbles();
            RbContacts = RbApplication.GetContacts();
            RbConversations = RbApplication.GetConversations();
            RbInstantMessaging = RbApplication.GetInstantMessaging();

            //RbFavorites = RbApplication.GetFavorites();
            //RbFileStorage = RbApplication.GetFileStorage();

            InitAvatarPool();

            MainPage = new NavigationPage(new LoginPage());
            //MainPage = new NavigationPage(new ConversationStreamPage("0"));

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
