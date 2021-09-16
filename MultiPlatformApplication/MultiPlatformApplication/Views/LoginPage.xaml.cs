using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Rainbow;
using Rainbow.Events;
using Rainbow.Model;

using MultiPlatformApplication.Controls;
using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.ViewModels;

namespace MultiPlatformApplication.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LoginPage : CtrlContentPage
    {
        private LoginViewModel vm;

        public LoginPage ()
		{
			InitializeComponent ();
            
            vm = new LoginViewModel();
            BindingContext = vm;

            EntryLogin.Focused += EntryLogin_Focused;
        }

        private void EntryLogin_Focused(object sender, FocusEventArgs e)
        {
            if (e.IsFocused)
                vm.LoginFieldFocused();
        }

        protected override void OnAppearing()
        {
            vm.Initialize();
            base.OnAppearing();
        }

    }
}