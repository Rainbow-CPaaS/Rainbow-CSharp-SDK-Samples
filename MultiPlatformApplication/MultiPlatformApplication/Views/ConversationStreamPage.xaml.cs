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

using NLog;


namespace MultiPlatformApplication.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConversationStreamPage : CtrlContentPage
    {
        private static readonly Logger log = LogConfigurator.GetLogger(typeof(ConversationStreamPage));

        private ConversationStreamViewModel vm2;

        private String conversationId;

        public ConversationStreamPage(String conversationId)
        {
            InitializeComponent();

            this.conversationId = conversationId;

            MessagesStream.BindingContext = conversationId;

            vm2 = new ConversationStreamViewModel();

            BindingContext = vm2;

            //BtnIMSend.Clicked += BtnIMSend_Clicked;
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



#region EVENTS FIRED ON THIS CONTENT PAGE

        private void EntryIm_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue != e.OldTextValue)
            {
                if (String.IsNullOrEmpty(e.NewTextValue))
                    vm2.SetIsTyping(false);
                else
                    vm2.SetIsTyping(e.NewTextValue.Length > 0);
            }
        }

        private void BtnIMSend_Clicked(object sender, EventArgs e)
        {
            //if (String.IsNullOrEmpty(EntryIM.Text))
            //    return;

            //vm2.SendMessage(EntryIM.Text);
            //EntryIM.Text = "";
        }

#endregion EVENTS FIRED ON THIS CONTENT PAGE



    }
}