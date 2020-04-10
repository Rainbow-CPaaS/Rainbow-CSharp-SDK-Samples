using System;
using System.Collections.Generic;
using System.Text;

using Rainbow;

using InstantMessaging.Helpers;
using InstantMessaging.ViewModel;

using log4net;

namespace InstantMessaging.Model
{
    public class FavoritesModel : ObservableObject
    {
        private static readonly ILog log = LogConfigurator.GetLogger(typeof(LoginModel));
        App currentApplication = (App)System.Windows.Application.Current;

        public SortableObservableCollection<FavoriteViewModel> FavoritesList { get; set; } // Need to be public - Used as Binding from XAML

        public FavoritesModel()
        {
            // Set capacity to 5 (max numbers of element displayed)
            FavoritesList = new SortableObservableCollection<FavoriteViewModel>(new FavoriteViewModel.FavoriteViewModelComparer());

            if (currentApplication.USE_DUMMY_DATA)
                LoadFakeConversations();
        }

        private void LoadFakeConversations()
        {
            //List<FavoriteViewModel> favoritesList = new List<FavoriteViewModel>();

            FavoriteViewModel favorite = null;

            // SET NEW FAKE Conversation Light
            favorite = new FavoriteViewModel();
            favorite.Id = "convID1";
            favorite.Name = "User Name1";
            favorite.PresenceSource = "presence_online.png";
            favorite.NbMsgUnread = favorite.Position = 1;
            favorite.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            FavoritesList.Add(favorite);

            // SET NEW FAKE Conversation Light
            favorite = new FavoriteViewModel();
            favorite.Id = "convID2";
            favorite.Name = "User Name2 avec tres tres tres tres long";
            favorite.PresenceSource = "presence_busy.png";
            favorite.NbMsgUnread = favorite.Position = 2;
            favorite.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            FavoritesList.Add(favorite);

            // SET NEW FAKE Conversation Light
            favorite = new FavoriteViewModel();
            favorite.Id = "convID3";
            favorite.Name = "Group Name3";
            favorite.PresenceSource = "";
            favorite.NbMsgUnread = favorite.Position = 3;
            favorite.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            FavoritesList.Add(favorite);

            // SET NEW FAKE Conversation Light
            favorite = new FavoriteViewModel();
            favorite.Id = "convID4";
            favorite.Name = "Group Name4 tres tres tres long";
            favorite.PresenceSource = "";
            favorite.NbMsgUnread = favorite.Position = 6;
            favorite.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            FavoritesList.Add(favorite);

            // SET NEW FAKE Conversation Light
            favorite = new FavoriteViewModel();
            favorite.Id = "convID5";
            favorite.Name = "Group Name6";
            favorite.PresenceSource = "";
            favorite.NbMsgUnread = favorite.Position = 5;
            favorite.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            FavoritesList.Add(favorite);

            // SET NEW FAKE Conversation Light
            favorite = new FavoriteViewModel();
            favorite.Id = "convID6";
            favorite.Name = "Group Name6";
            favorite.PresenceSource = "";
            favorite.NbMsgUnread = favorite.Position = 4;
            favorite.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            FavoritesList.Add(favorite);

            // SET NEW FAKE Conversation Light
            favorite = new FavoriteViewModel();
            favorite.Id = "convID7";
            favorite.Name = "Group Name7 tres tres tres long";
            favorite.PresenceSource = "";
            favorite.NbMsgUnread = favorite.Position = 0;
            favorite.AvatarImageSource = Helper.GetBitmapImageFromResource("avatar1.jpg");

            FavoritesList.Add(favorite);
        }
    }
}
