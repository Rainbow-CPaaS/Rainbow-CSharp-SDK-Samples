using MultiPlatformApplication.Effects;
using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConversationsPanel : ContentView
    {
        private ConversationsViewModel vm;

        public ConversationsPanel()
        {
            InitializeComponent();

            // We prevent MouseOver effect on ListView. "SelecionMode" must also be set to "None"
            //ConversationsListView.On<Windows>().SetSelectionMode(Xamarin.Forms.PlatformConfiguration.WindowsSpecific.ListViewSelectionMode.Inaccessible);

            // Create / Init ViewModel
            vm = new ConversationsViewModel();

            ConversationsListView.BindingContext = vm;
        }

        public void Initialize()
        {
            vm.Initialize();
        }

    }
}