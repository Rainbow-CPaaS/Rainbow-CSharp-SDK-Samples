﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Rainbow;
using Rainbow.Events;
using Rainbow.Model;

using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.ViewModels;

namespace MultiPlatformApplication
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LoginPage : ContentPage
    {
        private LoginViewModel vm;

        public LoginPage ()
		{
			InitializeComponent ();
            
            vm = new LoginViewModel();
            BindingContext = vm;
        }

        protected override void OnAppearing()
        {
            vm.Initialize();
            base.OnAppearing();
        }

    }
}