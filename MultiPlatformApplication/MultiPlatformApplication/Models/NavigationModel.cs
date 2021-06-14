using MultiPlatformApplication.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.Models
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class NavigationModel : ObservableObject
    {

        RelayCommand<object> m_backCommand;

        public RelayCommand<object> BackCommand
        {
            get { return m_backCommand; }
            set { SetProperty(ref m_backCommand, value); }
        }
    }
}
