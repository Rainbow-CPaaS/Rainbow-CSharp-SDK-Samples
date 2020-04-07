using Rainbow;

using InstantMessaging.Helpers;
using InstantMessaging.Model;
using log4net;

namespace InstantMessaging.ViewModel
{
    class FavoritesViewModel
    {
        private static readonly ILog log = LogConfigurator.GetLogger(typeof(FavoritesViewModel));

        public ObservableRangeCollection<FavoriteViewModel> FavoritesList { get; set; } // Need to be public - Used as Binding from XAML

        FavoritesModel model;

        /// <summary>
        /// Constructor
        /// </summary>
        public FavoritesViewModel()
        {
            model = new FavoritesModel();
            FavoritesList = model.FavoritesList;
        }
    }
}
