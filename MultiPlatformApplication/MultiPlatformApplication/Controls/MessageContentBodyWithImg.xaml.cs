using MultiPlatformApplication.Helpers;
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
    public partial class MessageContentBodyWithImg : ContentView
    {
        private static int MAX_WIDTH = 300;
        private static int MAX_HEIGHT = 300;

        private Boolean isPlaying;
        private MessageElementModel message = null;

        public Boolean IsPlaying
        {
            get
            {
                return isPlaying;
            }

            set
            {
                isPlaying = value;
                OnPropertyChanged(nameof(IsPlaying));
            }
        }
        public ICommand AnimationCommand { get; set; }

        public MessageContentBodyWithImg()
        {
            InitializeComponent();

            IsPlaying = true;
            AnimationCommand = new RelayCommand<object>(new Action<object>(StartStopAnimationCommand));

            this.BindingContextChanged += MessageContentBodyWithImg_BindingContextChanged;

            Image.BindingContext = this;
            CustomButton.BindingContext = this;

            Image.SizeChanged += Image_SizeChanged;
            Image.PropertyChanged += Image_PropertyChanged;
        }

        private void StartStopAnimationCommand(object obj)
        {
            IsPlaying = !IsPlaying;
        }

        private void Image_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsLoading")
            {
                if (!Image.IsLoading) // It means that the GIF has been downloaded
                    IsPlaying = false;
            }
        }

        private void Image_SizeChanged(object sender, EventArgs e)
        {
            if( (Image.Width > MAX_WIDTH) || (Image.Height > MAX_HEIGHT))
            {
                if (Image.Width > Image.Height)
                    Image.WidthRequest = MAX_WIDTH;
                else
                    Image.HeightRequest = MAX_HEIGHT;
            }
        }

        private void MessageContentBodyWithImg_BindingContextChanged(object sender, EventArgs e)
        {
            if ((BindingContext != null) && (message == null))
            {
                message = (MessageElementModel)BindingContext;
                if (message != null)
                {
                    // Manage Image / Uri
                    String body = message.Content?.Body;

                    if (String.IsNullOrEmpty(body))
                        return;

                    String[] parts = body.Split(' ');
                    if (parts.Count() > 1)
                    {
                        String uri = parts[1];
                        
                        Image.Source = ImageSource.FromUri(new Uri(uri));
                    }
                }
            }
        }
    }
}