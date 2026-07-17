using PCL.CS.Modules;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PCL.CS.Controls;

namespace PCL.CS.Pages
{
    public class MyLaunchButton : ContentControl, IMyButton
    {
        public double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }
        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register("Scale", typeof(double), typeof(MyLaunchButton),
            new PropertyMetadata(1.0, (d, e) => (d as MyLaunchButton).SetScale((double)e.NewValue)));
        private void SetScale(double NewValue)
        {
            this.ScaleTransform.ScaleX = NewValue;
            this.ScaleTransform.ScaleY = NewValue;
        }
        public ScaleTransform ScaleTransform
        {
            get => (ScaleTransform)GetValue(ScaleTransformProperty);
            set => SetValue(ScaleTransformProperty, value);
        }
        public static readonly DependencyProperty ScaleTransformProperty = DependencyProperty.Register("ScaleTransform", typeof(ScaleTransform), typeof(MyLaunchButton));
        

        private double ColorAnimLenth = 200;
        private static readonly DependencyProperty BackColor = DependencyProperty.Register("BackColor", typeof(Color), typeof(MyLaunchButton),
            new PropertyMetadata((d, e) => (d as MyLaunchButton).BackColorChange((Color)e.NewValue)));

        private AnimationGroup BackColorAnim;
        private void BackColorChange(Color NewValue)
        {
            if (this.Background is null) return;
            Animation.Stop(BackColorAnim);
            BackColorAnim = new AnimationGroup();
            BackColorAnim.TotalTime = TimeSpan.FromMilliseconds(ColorAnimLenth);
            BackColorAnim.Add(new ColorAnimation(this.Background, SolidColorBrush.ColorProperty, (this.Background as SolidColorBrush).Color, NewValue, ColorAnimLenth, 0));
            Animation.Start(BackColorAnim);
        }
        private static readonly DependencyProperty ForeColor = DependencyProperty.Register("ForeColor", typeof(Color), typeof(MyLaunchButton),
            new PropertyMetadata((d, e) => (d as MyLaunchButton).ForeColorChange((Color)e.NewValue)));
        private AnimationGroup ForeColorAnim;
        private void ForeColorChange(Color NewValue)
        {
            if (this.Foreground is null) return;
            Animation.Stop(ForeColorAnim);
            ForeColorAnim = new AnimationGroup();
            ForeColorAnim.TotalTime = TimeSpan.FromMilliseconds(ColorAnimLenth);
            ForeColorAnim.Add(new ColorAnimation(this.Foreground, SolidColorBrush.ColorProperty, (this.Foreground as SolidColorBrush).Color, NewValue, ColorAnimLenth, 0));
            Animation.Start(ForeColorAnim);
        }
        public enum ColorState
        {
            Normal,
            HighLight,
            Red
        }
        public ColorState ColorType
        {
            get;
            set;
        } = ColorState.HighLight;
        private string GetColor(bool IsBorder, bool IsHighlight)
        {
            string BorderColor;
            string BackColor;
            switch (ColorType)
            {
                case ColorState.Normal:
                    BorderColor = IsHighlight ? "ColorObject3" : "ColorObject1";
                    BackColor = IsHighlight ? "ColorObject7" : "ColorObjectWhite";
                    break;
                case ColorState.HighLight:
                    BorderColor = IsHighlight ? "ColorObject3" : "ColorObject2";
                    BackColor = IsHighlight ? "ColorObject7" : "ColorObjectWhite";
                    break;
                case ColorState.Red:
                    BorderColor = IsHighlight ? "ColorObjectRedLight" : "ColorObjectRedDark";
                    BackColor = IsHighlight ? "ColorObjectRedBack" : "ColorObjectWhite";
                    break;
                default:
                    BorderColor = "";
                    BackColor = "";
                    break;
            }
            return IsBorder ? BorderColor : BackColor;
        }

        private bool IsMouseDown = false;
        public MyLaunchButton()
        {
            this.ScaleTransform = new ScaleTransform();
            this.Loaded += (s, e) =>
            {
                this.Foreground = new SolidColorBrush((Color)App.Current.Resources[GetColor(true, false)]);
                this.Background = new SolidColorBrush((Color)App.Current.Resources[GetColor(false, false)]) { Opacity = 0.7 };
                this.SetResourceReference(ForeColor, GetColor(true, false));
                this.SetResourceReference(BackColor, GetColor(false, false));
            };
            this.MouseEnter += (s, e) => ColorUpdate();
            this.MouseLeave += (s, e) =>
            {
                ColorUpdate();
                if (!this.IsLoaded) return;
                ScaleTo(1.0, 300, new AniEaseOutFluent(2));
                IsMouseDown = false;
            };
            this.MouseLeftButtonDown += (s, e) =>
            {
                IsMouseDown = true;
                if (!this.IsLoaded) return;
                ScaleTo(0.955, 80, new AniEaseOutFluent(4));
            };
            this.MouseLeftButtonUp += (s, e) =>
            {
                if (!this.IsLoaded) return;
                if (IsMouseDown)
                {
                    IsMouseDown = false;
                    RaiseEvent(new RoutedEventArgs(ClickEvent));
                    ScaleTo(1.0, 300, new AniEaseOutFluent(2));
                }
            };
            this.IsEnabledChanged += (s, e) =>
            {
                if (!(bool)e.NewValue)
                {
                    ScaleTo(1.0, 300, new AniEaseOutFluent(2));
                    IsMouseDown = false;
                }
                ColorUpdate();
            };

        }

        private void ColorUpdate()
        {
            if (!this.IsLoaded) return;
            string BorderColor;
            string BackColor;
            if (!this.IsEnabled)
            {
                BorderColor = "ColorObjectGray4";
                BackColor = "ColorObjectWhite";
                this.ColorAnimLenth = 200;
            }
            else if (this.IsMouseOver)
            {
                BorderColor = GetColor(true, true);
                BackColor = GetColor(false, true);
                this.ColorAnimLenth = 100;
            }
            else
            {
                BorderColor = GetColor(true, false);
                BackColor = GetColor(false, false);
                this.ColorAnimLenth = 300;
            }
            if (!this.IsVisible)
                this.ColorAnimLenth = 0;
            this.SetResourceReference(ForeColor, BorderColor);
            this.SetResourceReference(MyLaunchButton.BackColor, BackColor);
            this.ColorAnimLenth = 200;
        }

        private AnimationGroup ScaleAnim;
        private void ScaleTo(double scale, double Time, AniEase Ease)
        {
            Animation.Stop(ScaleAnim);
            if (!this.IsVisible)
            {
                this.Scale = 1;
                return;
            }
            ScaleAnim = new AnimationGroup();
            ScaleAnim.TotalTime = TimeSpan.FromMilliseconds(Time);
            ScaleAnim.Add(new DoubleAnimation(this, ScaleProperty, this.Scale, scale, Time, 0, Ease));
            Animation.Start(ScaleAnim);
        }

        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent(
                "Click",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(MyLaunchButton));
        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }
    }
}
