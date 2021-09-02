using MultiPlatformApplication.Effects;
using MultiPlatformApplication.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestPage3 : ContentPage
    {
        TapGestureRecognizer tapGestureRecognizer;

        int counter = 0;

        public TestPage3()
        {
            InitializeComponent();

            CustomButton1.Command = new RelayCommand<object>(new Action<object>(CustomButton1Command));
            CustomButton2.Command = new RelayCommand<object>(new Action<object>(CustomButton2Command));

            InformationButton1.Command = new RelayCommand<object>(new Action<object>(InformationButton1Command));
            InformationButton2.Command = new RelayCommand<object>(new Action<object>(InformationButton2Command));

            Logo.Command = new RelayCommand<object>(new Action<object>(TapGestureRecognizerCommand));

            //tapGestureRecognizer = new TapGestureRecognizer();
            //tapGestureRecognizer.Command = new RelayCommand<object>(new Action<object>(TapGestureRecognizerCommand));
            //Logo.GestureRecognizers.Add(tapGestureRecognizer);
        }

        private void TapGestureRecognizerCommand(object obj)
        {
            //Popup.Show("ContextMenu1", LayoutAlignment.Center, LayoutAlignment.Start, new Point(0,10));
        }

        private void CustomButton1Command(object obj)
        {
            //if (counter % 4 == 0)
            //    Popup.Show("ContextMenu1", "CustomButton1", LayoutAlignment.Start, false , LayoutAlignment.End, true, new Point(), -1);
            //else if (counter % 4 == 1)
            //    Popup.Show("ContextMenu1", "CustomButton1", LayoutAlignment.End, false, LayoutAlignment.End, true, new Point(), -1);
            //else if (counter % 4 == 2)
            //    Popup.Show("ContextMenu1", "CustomButton1", LayoutAlignment.End, false, LayoutAlignment.Start, true, new Point(), -1);
            //else 
            //    Popup.Show("ContextMenu1", "CustomButton1", LayoutAlignment.Start, false, LayoutAlignment.Start, true, new Point(), -1);

            //if (counter % 4 == 0)
            //    Popup.Show("ContextMenu1", "CustomButton1", LayoutAlignment.Start, true, LayoutAlignment.End, true, new Point(), -1);
            //else if (counter % 4 == 1)
            //    Popup.Show("ContextMenu1", "CustomButton1", LayoutAlignment.End, true, LayoutAlignment.End, true, new Point(), -1);
            //else if (counter % 4 == 2)
            //    Popup.Show("ContextMenu1", "CustomButton1", LayoutAlignment.End, true, LayoutAlignment.Start, true, new Point(), -1);
            //else
            //    Popup.Show("ContextMenu1", "CustomButton1", LayoutAlignment.Start, true, LayoutAlignment.Start, true, new Point(), -1);

            //if (counter % 4 == 0)
            //    Popup.Show("ContextMenu1", "CustomButton1", LayoutAlignment.Start, false, LayoutAlignment.End, false, new Point(), -1);
            //else if (counter % 4 == 1)
            //    Popup.Show("ContextMenu1", "CustomButton1", LayoutAlignment.End, false, LayoutAlignment.End, false, new Point(), -1);
            //else if (counter % 4 == 2)
            //    Popup.Show("ContextMenu1", "CustomButton1", LayoutAlignment.End, false, LayoutAlignment.Start, false, new Point(), -1);
            //else
            //    Popup.Show("ContextMenu1", "CustomButton1", LayoutAlignment.Start, false, LayoutAlignment.Start, false, new Point(), -1);

            Popup.Show("ContextMenu1", "CustomButton1");

            counter++;
        }

        private void CustomButton2Command(object obj)
        {
            //if (counter % 2 == 0)
            //    Popup.ShowContextMenu("ContextMenu2", "CustomButton2");
            //else
            //    Popup.ShowContextMenu("ContextMenu2", "CustomButton2");


            //if (counter % 4 == 0)
            //    Popup.Show("ContextMenu2", null, LayoutAlignment.Start, false, LayoutAlignment.End, false, new Point(), -1);
            //else if (counter % 4 == 1)
            //    Popup.Show("ContextMenu2", null, LayoutAlignment.End, false, LayoutAlignment.End, false, new Point(), -1);
            //else if (counter % 4 == 2)
            //    Popup.Show("ContextMenu2", null, LayoutAlignment.End, false, LayoutAlignment.Start, false, new Point(), -1);
            //else
            //    Popup.Show("ContextMenu2", null, LayoutAlignment.Start, false, LayoutAlignment.Start, false, new Point(), -1);


            //if (counter % 4 == 0)
            //    Popup.Show("ContextMenu2", "MainGridTestPage3", LayoutAlignment.Start, false, LayoutAlignment.End, false, new Point(), -1);
            //else if (counter % 4 == 1)
            //    Popup.Show("ContextMenu2", "MainGridTestPage3", LayoutAlignment.End, false, LayoutAlignment.End, false, new Point(), -1);
            //else if (counter % 4 == 2)
            //    Popup.Show("ContextMenu2", "MainGridTestPage3", LayoutAlignment.End, false, LayoutAlignment.Start, false, new Point(), -1);
            //else
            //    Popup.Show("ContextMenu2", "MainGridTestPage3", LayoutAlignment.Start, false, LayoutAlignment.Start, false, new Point(), -1);


            Popup.Show("ContextMenu2", new Rect(50, 50, 50, 50));

            counter++;
        }

        private void InformationButton1Command(object obj)
        {
            Popup.Show("Information1", "InformationButton1", 5000);
        }

        private void InformationButton2Command(object obj)
        {
            Popup.Show("Information2", "InformationButton2", 2000);
        }
    }
}