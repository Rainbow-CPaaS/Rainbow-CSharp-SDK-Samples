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
    public partial class MenuControl : ContentView
    {

#region OrientationProperty

        public static readonly BindableProperty OrientationProperty =
            BindableProperty.Create(nameof(Orientation),
            typeof(StackOrientation),
            typeof(MenuControl),
            defaultValue: StackOrientation.Horizontal,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: OrientationPropertyChanged);

        private static void OrientationPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (MenuControl)bindable;
            SetMenuOrientation(control);
        }

        public StackOrientation Orientation
        {
            get
            {
                var obj = base.GetValue(OrientationProperty);
                if (obj != null)
                    return (StackOrientation)obj;
                return StackOrientation.Vertical;
            }
            set
            {
                base.SetValue(OrientationProperty, value);
            }
        }
#endregion OrientationProperty

        static private void SetMenuOrientation(MenuControl control)
        {
            control.StackLayout.Orientation = control.Orientation;
            if (control.Orientation == StackOrientation.Horizontal)
            {
                control.GridColumnDefinitions.Width = GridLength.Star;
                control.GridRowDefinitions.Height = GridLength.Auto;
            }
            else
            {
                control.GridColumnDefinitions.Width = GridLength.Auto;
                control.GridRowDefinitions.Height = GridLength.Star;
            }
        }

        public MenuControl()
        {
            InitializeComponent();

        }
    }
}