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
    /// Interaction logic for RotateVectorIcon.xaml
    /// </summary>
    public partial class RotateVectorIcon : UserControl
    {
        #region define Kind property (registration + update) - use also a Dictionary
        private static Dictionary<RotateVectorIconKind, string> _dataIndex = VectorIconDataFactory.RotateData;

        public RotateVectorIconKind Kind
        {
            get => (RotateVectorIconKind)GetValue(KindProperty);
            set => SetValue(KindProperty, value);
        }

        public static readonly DependencyProperty KindProperty =
            DependencyProperty.Register(nameof(Kind), typeof(RotateVectorIconKind), typeof(RotateVectorIcon),
                new PropertyMetadata(RotateVectorIconKind.Spinner1, KindPropertyChanged));

        private static void KindPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RotateVectorIcon)d).UpdateData();
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
            DependencyProperty.Register("Color", typeof(Brush), typeof(RotateVectorIcon),
                new PropertyMetadata(Brushes.Black, new PropertyChangedCallback(ColorPropertyChanged)));

        private static void ColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = d as RotateVectorIcon;
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

        public RotateVectorIcon()
        {
            InitializeComponent();
        }

    }
}
