using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.Assets
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ThemeLight : ResourceDictionary
    {
        public ThemeLight()
        {
            InitializeComponent();
        }
    }
}