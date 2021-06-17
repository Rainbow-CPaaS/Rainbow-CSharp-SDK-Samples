using MultiPlatformApplication.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace MultiPlatformApplication.Models
{
    public class MenuItemModel: ObservableObject
    {
        String id;
        String label;
        Boolean isSelected;
        Boolean isVisible;
        double heightRequest;
        double widthRequest;
        double imageSize;

        Color textColor;
        double fontSize;

        Color backgroundColor;
        Color backgroundColorOnSelected;
        Color backgroundColorOnMouseOver;

        String imageSourceId; // ID of the embedded resource or Id of the resouce in a merged dictionnary to use as Image

        ICommand command;
        object commandParameter;

        public string Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }

        public string Label
        {
            get { return label; }
            set { SetProperty(ref label, value); }
        }

        public Boolean IsSelected
        {
            get { return isSelected; }
            set { SetProperty(ref isSelected, value); }
        }

        public Boolean IsVisible
        {
            get { return isVisible; }
            set { SetProperty(ref isVisible, value); }
        }

        public double HeightRequest
        {
            get { return heightRequest; }
            set { SetProperty(ref heightRequest, value); }
        }

        public double WidthRequest
        {
            get { return widthRequest; }
            set { SetProperty(ref widthRequest, value); }
        }

        public double ImageSize
        {
            get { return imageSize; }
            set { SetProperty(ref imageSize, value); }
        }

        public Color TextColor
        {
            get { return textColor; }
            set { SetProperty(ref textColor, value); }
        }

        public double FontSize
        {
            get { return fontSize; }
            set { SetProperty(ref fontSize, value); }
        }

        public Color originalBackgroundColor;

        public Color BackgroundColor
        {
            get { return backgroundColor; }
            set { 
                SetProperty(ref backgroundColor, value);
                if (!IsSelected)
                    originalBackgroundColor = backgroundColor;
            }
        }

        public Color BackgroundColorOnSelected
        {
            get { return backgroundColorOnSelected; }
            set { SetProperty(ref backgroundColorOnSelected, value); }
        }

        public Color BackgroundColorOnMouseOver
        {
            get { return backgroundColorOnMouseOver; }
            set { SetProperty(ref backgroundColorOnMouseOver, value); }
        }

        // ID of the embedded resource or Id of the resouce in a merged dictionnary to use as Image
        public string ImageSourceId
        {
            get { return imageSourceId; }
            set { SetProperty(ref imageSourceId, value); }
        }

        public ICommand Command
        {
            get { return command; }
            set { SetProperty(ref command, value); }
        }

        public object CommandParameter
        {
            get { return commandParameter; }
            set { SetProperty(ref commandParameter, value); }
        }


        public void SetAsSelectedItem()
        {
            if (!IsSelected)
            {
                IsSelected = true;
                //BackgroundColor = BackgroundColorOnSelected;
            }
        }

        public void SetAsUnselectedItem()
        {
            if (IsSelected)
            {
                IsSelected = false;
                //BackgroundColor = originalBackgroundColor;
                
            }
        }

        public MenuItemModel()
        {
            IsSelected = false;
            IsVisible = true;

            ImageSize = WidthRequest = HeightRequest = 0;

            BackgroundColor = Color.FromHex("#2470E0");
            BackgroundColorOnSelected = Color.FromHex("#1A59B7");
            BackgroundColorOnMouseOver = Color.FromHex("#4783E5");

            TextColor = Color.FromHex("#FFFFFF");

            FontSize = 12;
        }

        public MenuItemModel(MenuItemModel original)
        {
            // Loop on each properties and set it's value using the original one
            foreach (PropertyInfo property in GetType().GetProperties())
            {
                /// Set to each property its name as value
                property.SetValue(this, property.GetValue(original));
            }
        }
    }
}
