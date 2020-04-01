using System;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using Rainbow;
using Rainbow.Model;

using InstantMessaging.Helpers;
using InstantMessaging.Model;

using log4net;

namespace InstantMessaging.ViewModel
{
    public class MainViewModel : ObservableObject
    {

        public MainViewModel()
        {
            // Init / Define properties not managed by the Model Object

            // Create Model object associated to the current ViewModel object
            //model = new LoginModel();

            // Define event(s) we want to manage from model
            //model.PropertyChanged += Model_PropertyChanged; // Use reflection to set ViewModel's properties from Model's properties

            // Init the Model - To set default values to ppropeties
            //model.Init();
        }
    }
}
