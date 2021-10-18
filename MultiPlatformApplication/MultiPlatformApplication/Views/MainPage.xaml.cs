using MultiPlatformApplication.Controls;
using MultiPlatformApplication.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : CtrlContentPage
    {
        private MainPageViewModel vm;

        public MainPage()
        {
            InitializeComponent();

            DisplayDisconnection = true;

            // Create / Init ViewModel
            vm = new MainPageViewModel(this);

            BindingContext = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            vm.Initialize();
        }


        protected override void OnDisappearing()
        {
            base.OnAppearing();
        }
    }
}