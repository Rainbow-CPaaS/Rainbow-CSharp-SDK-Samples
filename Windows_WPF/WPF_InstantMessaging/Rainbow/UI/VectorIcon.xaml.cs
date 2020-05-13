using InstantMessaging.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Rainbow.UI
{
    /// <summary>
    /// Interaction logic for Icon.xaml
    /// </summary>
    public partial class VectorIcon : UserControl
    {
        #region define Kind property (registration + update) - use also a Dictionary
        private static Dictionary<VectorIconKind, string> _dataIndex = VectorIconDataFactory.Data;

        public VectorIconKind Kind
        {
            get => (VectorIconKind)GetValue(KindProperty);
            set => SetValue(KindProperty, value);
        }

        public static readonly DependencyProperty KindProperty = 
            DependencyProperty.Register(nameof(Kind), typeof(VectorIconKind), typeof(VectorIcon), 
                new PropertyMetadata(VectorIconKind.Abc, KindPropertyChanged));

        private static void KindPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VectorIcon)d).UpdateData();
        }

        public void UpdateData()
        {
            string data = null;
            _dataIndex?.TryGetValue(Kind, out data);
            Path.Data = System.Windows.Media.Geometry.Parse(data); ;
        }

        #endregion define Kind property (registration + update) - use also a Dictionary

        #region define Color property (registration + update)
        public Brush Color
        {
            get { return (Brush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Brush), typeof(VectorIcon), 
                new PropertyMetadata(Brushes.Black, new PropertyChangedCallback(ColorPropertyChanged)));

        private static void ColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = d as VectorIcon;
            if (instance != null)
            {
                instance.Path.Fill = instance.Color;
            }
        }
        #endregion define COLOR property (registration + update)

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateData();
        }

        public VectorIcon()
        {
            InitializeComponent();
        }
    }
}
