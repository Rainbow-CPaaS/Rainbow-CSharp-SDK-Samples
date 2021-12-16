using Rainbow;

using InstantMessaging.Helpers;
using InstantMessaging.Model;
using Microsoft.Extensions.Logging;

namespace InstantMessaging.ViewModel
{
    class FavoritesViewModel
    {
        private static readonly ILogger log = Rainbow.LogFactory.CreateLogger<FavoritesViewModel>();

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
