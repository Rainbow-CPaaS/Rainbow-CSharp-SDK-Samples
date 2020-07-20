using Rainbow;

using InstantMessaging.Helpers;
using InstantMessaging.Model;
using NLog;

namespace InstantMessaging.ViewModel
{
    class FavoritesViewModel
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(FavoritesViewModel));

        public RangeObservableCollection<FavoriteViewModel> FavoritesList { get; set; } // Need to be public - Used as Binding from XAML

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
