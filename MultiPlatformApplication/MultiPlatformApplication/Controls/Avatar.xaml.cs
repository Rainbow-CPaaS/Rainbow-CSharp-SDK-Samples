using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Avatar : Frame
    {

#region AvatarFilePathProperty

        public static readonly BindableProperty AvatarFilePathProperty =
            BindableProperty.Create(nameof(AvatarFilePath),
            typeof(String),
            typeof(Avatar),
            defaultValue: null,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: AvatarFilePathChanged);

        private static void AvatarFilePathChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (Avatar)bindable;
            if(newValue == null)
            {
                control.Image.Source = null;
            }
            else if (newValue is String)
            {
                String filePath = (String)newValue;
                if(filePath != null)
                    control.Image.Source = ImageSource.FromFile(filePath);
            }
        }

        public String AvatarFilePath
        {
            get
            {
                var obj = base.GetValue(AvatarFilePathProperty);
                if (obj is String)
                    return (String)obj;
                return null;
            }
            set
            {
                base.SetValue(AvatarFilePathProperty, value);
            }
        }

#endregion AvatarFilePathProperty

        public Avatar()
        {
            InitializeComponent();
        }
    }
}