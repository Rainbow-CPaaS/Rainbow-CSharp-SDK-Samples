﻿using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CustomButton : Frame
    {

#region ImageSourceProperty

        public static readonly BindableProperty ImageSourceProperty =
            BindableProperty.Create(nameof(ImageSource),
            typeof(ImageSource),
            typeof(CustomButton),
            defaultValue: null,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: ImageSourceChanged);

        private static void ImageSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            SetImageDisplay((CustomButton)bindable);
        }

        public ImageSource ImageSource
        {
            get
            {
                var obj = base.GetValue(ImageSourceProperty);
                if (obj is ImageSource)
                    return (ImageSource)obj;
                return null;
            }
            set
            {
                base.SetValue(ImageSourceProperty, value);
            }
        }
#endregion ImageSourceProperty


#region ImageSizeProperty

        public static readonly BindableProperty ImageSizeProperty =
            BindableProperty.Create(nameof(ImageSize),
            typeof(double),
            typeof(CustomButton),
            defaultValue: (double)0,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: ImageSizeChanged);

        private static void ImageSizeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (CustomButton)bindable;
            if (newValue is double)
            {
                control.Image.WidthRequest = (double)newValue;
                control.Image.HeightRequest = (double)newValue;
            }
        }

        public double ImageSize
        {
            get
            {
                var obj = base.GetValue(ImageSizeProperty);
                if (obj is double)
                    return (double)obj;
                return 0;
            }
            set
            {
                base.SetValue(ImageSizeProperty, value);
            }
        }
#endregion ImageSizeProperty


#region ImageSourceOnSelectedProperty

        public static readonly BindableProperty ImageSourceOnSelectedProperty =
            BindableProperty.Create(nameof(ImageSourceOnSelected),
            typeof(ImageSource),
            typeof(CustomButton),
            defaultValue: null,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: ImageSourceOnSelectedChanged);

        private static void ImageSourceOnSelectedChanged(BindableObject bindable, object oldValue, object newValue)
        {
            SetImageDisplay((CustomButton)bindable);
        }

        public ImageSource ImageSourceOnSelected
        {
            get
            {
                var obj = base.GetValue(ImageSourceOnSelectedProperty);
                if (obj is ImageSource)
                    return (ImageSource)obj;
                return null;
            }
            set
            {
                base.SetValue(ImageSourceOnSelectedProperty, value);
            }
        }
#endregion ImageSourceOnSelectedProperty


#region IsSelectedProperty

        public static readonly BindableProperty IsSelectedProperty =
            BindableProperty.Create(nameof(IsSelected),
            typeof(Boolean),
            typeof(CustomButton),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: IsSelectedPropertyChanged);

        private static void IsSelectedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            SetImageDisplay((CustomButton)bindable);
        }

        public Boolean IsSelected
        {
            get
            {
                var obj = base.GetValue(IsSelectedProperty);
                if(obj != null)
                    return (Boolean)obj;
                return false;
            }
            set
            {
                base.SetValue(IsSelectedProperty, value);
            }
        }
#endregion IsSelectedProperty


#region TextProperty

        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text),
            typeof(String),
            typeof(CustomButton),
            defaultValue: "",
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: TextPropertyChanged);

        private static void TextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (CustomButton)bindable;
            if (newValue is String)
            {
                String txt = newValue as String;
                control.Label.Text = txt;
                control.Label.IsVisible = !String.IsNullOrEmpty(txt);
            }
            else
                control.Label.IsVisible = false;
        }

        public String Text
        {
            get
            {
                var obj = base.GetValue(TextProperty);
                if (obj is String)
                    return obj as String;
                return "";
            }
            set
            {
                base.SetValue(TextProperty, value);
            }
        }
#endregion TextProperty


#region TextColorProperty

        public static readonly BindableProperty TextColorProperty =
            BindableProperty.Create(nameof(TextColor),
            typeof(Color),
            typeof(CustomButton),
            defaultValue: Color.Black,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: TextColorPropertyChanged);

        private static void TextColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            SetTextColorDisplay((CustomButton)bindable);
        }

        public Color TextColor
        {
            get
            {
                var obj = base.GetValue(TextColorProperty);
                if(obj != null)
                    return (Color)obj;
                return Color.Black;
            }
            set
            {
                base.SetValue(TextColorProperty, value);
            }
        }
#endregion TextColorProperty


#region FontSizeProperty

        public static readonly BindableProperty FontSizeProperty =
            BindableProperty.Create(nameof(FontSize),
            typeof(Double),
            typeof(CustomButton),
            defaultValue: (double)0,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: FontSizePropertyChanged);

        private static void FontSizePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (CustomButton)bindable;
            if (newValue is double)
                control.Label.FontSize = (double)newValue;
        }

        public double FontSize
        {
            get
            {
                var obj = base.GetValue(FontSizeProperty);
                if(obj != null)
                    return (double)obj;
                return (double)0;
            }
            set
            {
                base.SetValue(FontSizeProperty, value);
            }
        }
#endregion FontSizeProperty


#region FontAttributesProperty

        public static readonly BindableProperty FontAttributesProperty =
            BindableProperty.Create(nameof(FontAttributes),
            typeof(FontAttributes),
            typeof(CustomButton),
            defaultValue: FontAttributes.None,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: FontAttributesPropertyChanged);

        private static void FontAttributesPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (CustomButton)bindable;
            if (newValue is FontAttributes)
                control.Label.FontAttributes = (FontAttributes)newValue;
        }

        public FontAttributes FontAttributes
        {
            get
            {
                var obj = base.GetValue(FontAttributesProperty);
                if(obj != null)
                    return (FontAttributes)obj;
                return FontAttributes.None;
            }
            set
            {
                base.SetValue(FontAttributesProperty, value);
            }
        }
#endregion FontAttributesProperty


#region TextColorOnSelectedProperty

        public static readonly BindableProperty TextColorOnSelectedProperty =
            BindableProperty.Create(nameof(TextColorOnSelected),
            typeof(Color),
            typeof(CustomButton),
            defaultValue: Color.Black,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: TextColorOnSelectedPropertyChanged);

        private static void TextColorOnSelectedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            SetTextColorDisplay((CustomButton)bindable);
        }

        public Color TextColorOnSelected
        {
            get
            {
                var obj = base.GetValue(TextColorOnSelectedProperty);
                if(obj != null)
                    return (Color)obj;
                return Color.Black;
            }
            set
            {
                base.SetValue(TextColorOnSelectedProperty, value);
            }
        }
#endregion TextColorOnSelectedProperty


#region OrientationProperty

        public static readonly BindableProperty OrientationProperty =
            BindableProperty.Create(nameof(Orientation),
            typeof(StackOrientation),
            typeof(CustomButton),
            defaultValue: StackOrientation.Vertical,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: OrientationPropertyChanged);

        private static void OrientationPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (CustomButton)bindable;
            if (newValue is StackOrientation)
                control.StackLayout.Orientation = (StackOrientation)newValue;
        }

        public StackOrientation Orientation
        {
            get
            {
                var obj = base.GetValue(OrientationProperty);
                if(obj != null)
                    return (StackOrientation)obj;
                return StackOrientation.Vertical;
            }
            set
            {
                base.SetValue(OrientationProperty, value);
            }
        }
#endregion OrientationProperty


#region TextMarginProperty

        public static readonly BindableProperty TextMarginProperty =
            BindableProperty.Create(nameof(TextMargin),
            typeof(Thickness),
            typeof(CustomButton),
            defaultValue: new Thickness((double)0),
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: TextMarginPropertyChanged);

        private static void TextMarginPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (CustomButton)bindable;
            if (newValue is Thickness)
                control.Label.Margin = (Thickness)newValue;
        }

        public Thickness TextMargin
        {
            get
            {
                var obj = base.GetValue(TextMarginProperty);
                if(obj != null)
                    return (Thickness)obj;
                return new Thickness((double)0);
            }
            set
            {
                base.SetValue(TextMarginProperty, value);
            }
        }

#endregion TextMarginProperty


#region BackgroundColorOnMouseOverProperty

        public static readonly BindableProperty BackgroundColorOnMouseOverProperty =
            BindableProperty.Create(nameof(BackgroundColorOnMouseOver),
            typeof(Color),
            typeof(CustomButton),
            defaultValue: Color.Transparent,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: BackgroundColorOnMouseOverPropertyChanged);

        private static void BackgroundColorOnMouseOverPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            // Nothing to do here
        }

        public Color BackgroundColorOnMouseOver
        {
            get
            {
                var obj = base.GetValue(BackgroundColorOnMouseOverProperty);
                if(obj != null)
                    return (Color)obj;
                return Color.Transparent;
            }
            set
            {
                base.SetValue(BackgroundColorOnMouseOverProperty, value);
            }
        }
#endregion BackgroundColorOnMouseOverProperty


#region CommandProperty

        TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();

        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(CustomButton), null);

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

#endregion CommandProperty


#region CommandParameterProperty

        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create(nameof(CommandParameter),
            typeof(object),
            typeof(CustomButton),
            defaultValue: null,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: CommandParameterPropertyChanged);

        private static void CommandParameterPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (CustomButton)bindable;
            if (newValue is object)
                control.tapGestureRecognizer.CommandParameter = newValue;
        }

        public object CommandParameter
        {
            get
            {
                var obj = base.GetValue(CommandParameterProperty);
                if(obj != null)
                    return obj;
                return null;
            }
            set
            {
                base.SetValue(CommandParameterProperty, value);
            }
        }

#endregion CommandParameterProperty


        // Define commands used for  Mouse Over / Mouse Out purpose
        public MouseOverAndOutModel mouseCommands { get; set; }

        // Need to store original background color
        private Color originalBackgroundColor;
        private Boolean isMouseOver = false;

        private static void SetImageDisplay(CustomButton control)
        {
            if ( control.IsSelected && (control.ImageSourceOnSelected != null) )
            {
                control.Image.IsVisible = true;
                control.Image.Source = control.ImageSourceOnSelected;
                return;
            }

            if (control.ImageSource != null)
            {
                control.Image.IsVisible = true;
                control.Image.Source = control.ImageSource;
            }
            else
                control.Image.IsVisible = false;
        }

        private static void SetTextColorDisplay(CustomButton control)
        {
            if (control.IsSelected && (control.TextColorOnSelected != null))
            {
                control.Label.TextColor= control.TextColorOnSelected;
                return;
            }

            control.Label.TextColor = control.TextColor;
        }

        private void SetBackGroundColor()
        {
            if(IsEnabled)
            {
                if (isMouseOver)
                {
                    // Store original Background Color
                    originalBackgroundColor = Color.FromHex(BackgroundColor.ToHex());

                    // Set new color
                    BackgroundColor = BackgroundColorOnMouseOver;
                }
                else
                {
                    // Restore original Background Color
                    BackgroundColor = Color.FromHex(originalBackgroundColor.ToHex());
                }
            }
            else
                BackgroundColor = BackgroundColorOnMouseOver;
        }

        private void MouseOverCommand(object obj)
        {
            isMouseOver = true;
            SetBackGroundColor();
        }

        private void MouseOutCommand(object obj)
        {
            isMouseOver = false;
            SetBackGroundColor();
        }

        public CustomButton()
        {
            InitializeComponent();

            this.PropertyChanged += CustomButton_PropertyChanged;

            mouseCommands = new MouseOverAndOutModel();
            mouseCommands.MouseOverCommand = new RelayCommand<object>(new Action<object>(MouseOverCommand));
            mouseCommands.MouseOutCommand = new RelayCommand<object>(new Action<object>(MouseOutCommand));
            ContentView.BindingContext = mouseCommands;

            tapGestureRecognizer.Tapped += (s, e) => {
                if (Command != null && Command.CanExecute(CommandParameter))
                {
                    Command.Execute(CommandParameter);
                }
            };

            // Seems not necessary to specify Tap Gesture Recognizer on all components - only on ContentView seems enough
            //Label.GestureRecognizers.Add(tapGestureRecognizer);
            //Label.GestureRecognizers.Add(tapGestureRecognizer);
            //StackLayout.GestureRecognizers.Add(tapGestureRecognizer);
            ContentView.GestureRecognizers.Add(tapGestureRecognizer);
            
        }

        private void CustomButton_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsEnabled")
            {
                SetBackGroundColor();
            }
        }

#region PRIVATE METHODS
#endregion PRIVATE METHODS

    }
}