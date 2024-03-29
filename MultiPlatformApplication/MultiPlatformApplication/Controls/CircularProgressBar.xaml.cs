﻿using MultiPlatformApplication.Helpers;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.Controls
{
    // Bsed on: https://github.com/VictorKochetkov/Xamarin.Forms.CircularProgressBar

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CircularProgressBar : ContentView
    {
        public static readonly BindableProperty ProgressProperty = BindableProperty.Create(
            nameof(Progress),
            typeof(double),
            typeof(CircularProgressBar),
            0d,
            propertyChanged: OnProgressPropertyChanged);

        public static readonly BindableProperty ColorProperty = BindableProperty.Create(
            nameof(Color),
            typeof(Color),
            typeof(CircularProgressBar),
            Color.Accent,
            propertyChanged: OnColorPropertyChanged);

        public static readonly BindableProperty StrokeProperty = BindableProperty.Create(
            nameof(Stroke),
            typeof(double),
            typeof(CircularProgressBar),
            4d,
            propertyChanged: OnStrokePropertyChanged);

        public static readonly BindableProperty SpinProperty = BindableProperty.Create(
            nameof(Spin),
            typeof(bool),
            typeof(CircularProgressBar),
            false,
            propertyChanged: OnAnimatedPropertyChanged);

        public static readonly BindableProperty EasingProperty = BindableProperty.Create(
            nameof(Easing),
            typeof(bool),
            typeof(CircularProgressBar),
            false,
            propertyChanged: OnEasingPropertyChanged);

        public static readonly BindableProperty FilledProperty = BindableProperty.Create(
            nameof(Filled),
            typeof(bool),
            typeof(CircularProgressBar),
            false,
            propertyChanged: OnFilledPropertyChanged);

        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set
            {
                this.currentProgress = (float)this.value;
                SetValue(ProgressProperty, Math.Max(0, Math.Min(100, value)));
            }
        }

        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public double Stroke
        {
            get => (double)GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }

        public bool Spin
        {
            get => (bool)GetValue(SpinProperty);
            set => SetValue(SpinProperty, value);
        }

        public bool Easing
        {
            get => (bool)GetValue(EasingProperty);
            set => SetValue(EasingProperty, value);
        }

        public bool Filled
        {
            get => (bool)GetValue(FilledProperty);
            set => SetValue(FilledProperty, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bindable"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        private static void OnProgressPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = (CircularProgressBar)bindable;
            view._easing = 0;
            view.canvas.InvalidateSurface();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bindable"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        private static void OnColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = (CircularProgressBar)bindable;
            view.UpdatePaint();
            view.canvas.InvalidateSurface();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bindable"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        private static void OnStrokePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = (CircularProgressBar)bindable;
            view.UpdatePaint();
            view.canvas.InvalidateSurface();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bindable"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        private static void OnAnimatedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = (CircularProgressBar)bindable;
            view.canvas.InvalidateSurface();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bindable"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        private static void OnEasingPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = (CircularProgressBar)bindable;
            view.canvas.InvalidateSurface();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bindable"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        private static void OnFilledPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = (CircularProgressBar)bindable;
            view.canvas.InvalidateSurface();
        }

        public CircularProgressBar()
        {
            UpdatePaint();
            InitializeComponent();
            canvas.PaintSurface += Canvas_PaintSurface;
        }

        protected override void InvalidateLayout()
        {
            base.InvalidateLayout();
            canvas.InvalidateSurface();
        }

        private void UpdatePaint()
        {
            paint = new SKPaint()
            {
                Style = Filled ? SKPaintStyle.Fill : SKPaintStyle.Stroke,
                Color = Color.ToSKColor(),
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round,
                StrokeWidth = (float)(Helper.GetDensity() * Stroke),
                IsAntialias = true,
            };

            paintBackground = paint.Clone();
            paintBackground.StrokeWidth *= 0.9f;
            paintBackground.Color = new SKColor(paintBackground.Color.Red, paintBackground.Color.Green, paintBackground.Color.Blue, (byte)50);
        }

        private SKRect rect;
        private SKPaint paint;
        private SKPaint paintBackground;
        private float _easing = 0;
        private float _rotate = 0;
        const int padding = 2;
        private Stopwatch time = new Stopwatch();
        private TimeSpan drawInterval = TimeSpan.FromMilliseconds(30);
        private double currentProgress;
        private double value;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            time.Stop();

            float a = (float)Math.Max(0.1, Math.Min(10, time.ElapsedMilliseconds / drawInterval.TotalMilliseconds));

            time.Reset();
            time.Start();

            if (Easing || Spin)
            {
                _easing += 0.05f * a;
                _easing = Math.Min(1, _easing);
            }
            else
            {
                _easing = 1;
            }

            if (Spin)
            {
                _rotate += 8f * a;

                if (_rotate > 360)
                    _rotate = _rotate - 360;
            }
            else
            {
                _rotate = 0;
            }


            if (Progress != this.value || (Spin && Progress > 0 && Progress < 100) || (Easing && _easing > 0 && _easing < 1))
            {
                Device.StartTimer(drawInterval, () =>
                {
                    this.canvas.InvalidateSurface();
                    return false;
                });
            }

            var canvas = e.Surface.Canvas;
            canvas.Clear();

            float strokeWidth = 0;
            if(!Filled)
                strokeWidth = Math.Max(paint.StrokeWidth, paintBackground.StrokeWidth);

            rect.Size = new SKSize(e.Info.Width - strokeWidth - padding, e.Info.Height - strokeWidth - padding);
            rect.Location = new SKPoint(strokeWidth / 2, strokeWidth / 2);

            double delta = (this.Progress - currentProgress) * (float)Xamarin.Forms.Easing.CubicInOut.Ease(_easing);
            this.value = currentProgress + delta;

            double _angle = value / 100d * 360d;

            double _startAngle = _rotate + 270;
            double _sweepAngle = _angle;

            SKPath path = new SKPath();
            if (Filled)
            {
                path.MoveTo((rect.Width + strokeWidth) / 2, (rect.Height + strokeWidth) / 2);
                path.LineTo((rect.Width + strokeWidth) / 2, 0);
                path.AddArc(rect, (float)_startAngle, (float)_sweepAngle);
                path.LineTo((rect.Width + strokeWidth) / 2, (rect.Height + strokeWidth) / 2);
            } 
            else
            {
                path.AddArc(rect, (float)_startAngle, (float)_sweepAngle);
            }
            

            SKPath pathBackground = new SKPath();
            pathBackground.AddArc(rect, 0, 360);

            canvas.DrawPath(path, paint);
            canvas.DrawPath(pathBackground, paintBackground);
        }
    }
}