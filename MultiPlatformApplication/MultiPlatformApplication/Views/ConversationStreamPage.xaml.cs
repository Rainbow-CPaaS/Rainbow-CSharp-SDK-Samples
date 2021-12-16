using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Rainbow;

using MultiPlatformApplication.Controls;
using MultiPlatformApplication.Models;
using MultiPlatformApplication.ViewModels;

using Microsoft.Extensions.Logging;


namespace MultiPlatformApplication.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConversationStreamPage : CtrlContentPage
    {
        private static readonly ILogger log = Rainbow.LogFactory.CreateLogger<ConversationStreamPage>();

        private ConversationStreamViewModel vm2;

        private String conversationId;

        public ConversationStreamPage(String conversationId)
        {
            InitializeComponent();

            DisplayDisconnection = true;

            this.conversationId = conversationId;

            MessagesStream.BindingContext = conversationId;

            vm2 = new ConversationStreamViewModel();

            BindingContext = vm2;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            vm2.Initialize(conversationId);
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();
            if (Parent == null)
                DisposeBindingContext();
        }

        protected void DisposeBindingContext()
        {
            if (BindingContext is IDisposable disposableBindingContext)
            {
                disposableBindingContext.Dispose();
            }

            if (vm2 != null)
            {
                vm2.Uninitialize();
                vm2 = null;
            }

            BindingContext = null;

        }

        ~ConversationStreamPage()
        {
            DisposeBindingContext();
        }


    }
}