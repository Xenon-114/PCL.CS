using PCL.CS.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PCL.CS.Controls
{
    public class MyIconButton:Control,IMyButton
    {
        public double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }
        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register("Scale", typeof(double), typeof(MyIconButton),
            new PropertyMetadata((d, e) => (d as MyIconButton).LogoScaleRefresh()));

        public ScaleTransform ScaleTransform
        {
            get => (ScaleTransform)GetValue(ScaleTransformProperty);
            set => SetValue(ScaleTransformProperty, value);
        }
        public static readonly DependencyProperty ScaleTransformProperty = DependencyProperty.Register("ScaleTransform", typeof(ScaleTransform), typeof(MyIconButton));

        public Geometry Logo
        {
            get => (Geometry)GetValue(LogoProperty);
            set => SetValue(LogoProperty, value);
        }
        public static readonly DependencyProperty LogoProperty=DependencyProperty.Register("Logo",typeof(Geometry), typeof(MyIconButton));

        public double LogoScale
        {
            get => (double)GetValue(LogoScaleProperty);
            set => SetValue(LogoScaleProperty, value);
        }
        public static readonly DependencyProperty LogoScaleProperty = DependencyProperty.Register("LogoScale", typeof(double), typeof(MyIconButton)
            , new PropertyMetadata((d, e) => (d as MyIconButton).LogoScaleRefresh()));

        private void LogoScaleRefresh()
        {
            ScaleTransform.ScaleX = LogoScale * Scale;
            ScaleTransform.ScaleY = LogoScale * Scale;
        }

        public Color ForeColor
        {
            get => (Color)GetValue(ForeColorProperty);
            set => SetValue(ForeColorProperty, value);
        }
        public static readonly DependencyProperty ForeColorProperty = DependencyProperty.Register("ForeColor", typeof(Color), typeof(MyIconButton));



        public double ForeAnimValue
        {
            get { return (double)GetValue(ForeAnimValueProperty); }
            set { SetValue(ForeAnimValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ForeAnimValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForeAnimValueProperty =
            DependencyProperty.Register(nameof(ForeAnimValue), typeof(double), typeof(MyIconButton), new PropertyMetadata(0.6));



        public enum ColorState
        {
            Black,
            Color,
            Red,
            Custom
        }
        public ColorState ColorType
        {
            get => _ColorType;
            set
            {
                _ColorType = value;
                ColorUpdate();
            }
        }
        private ColorState _ColorType = ColorState.Color;

        private void ColorUpdate()
        {
            if (!this.IsEnabled)
            {
                this.SetResourceReference(ColorNormalProperty, "ColorObjectGray3");
                this.OpacTo(0.6, 300);
                return;
            }
            switch (ColorType)
            {
                case ColorState.Black:
                    this.SetResourceReference(ColorNormalProperty, "ColorObject1");
                    break;
                case ColorState.Color:
                    this.SetResourceReference(ColorNormalProperty, "ColorObject3");
                    break;
                case ColorState.Red:
                    this.SetResourceReference(ColorNormalProperty, "ColorObjectRedLight");
                    break;
                default:
                    this.SetBinding(ColorNormalProperty, new Binding("ForeColor") { Source = this });
                    break;
            }
            double Opac;
            double Time;
            if (this.IsMouseOver)
            {
                Opac = 0.8;
                Time = 120;
            }
            else
            {
                Opac = 0.6;
                Time = 150;
            }
            OpacTo(Opac, Time);
        }

        public Color ColorNormal
        {
            get { return (Color)GetValue(ColorNormalProperty); }
            set { SetValue(ColorNormalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColorNormal.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorNormalProperty =
            DependencyProperty.Register(nameof(ColorNormal), typeof(Color), typeof(MyIconButton));



        private ColorMixer MainMixer;
        public MyIconButton()
        {
            ScaleTransform = new ScaleTransform();
            Scale = 1;
            LogoScale = 1;

            ColorUpdate();
            this.Foreground = new SolidColorBrush();
            MainMixer = new ColorMixer();
            MainMixer.SetBinding(ColorMixer.ColorAProperty, new Binding("ColorNormal") { Source = this });
            MainMixer.ColorB = Colors.White;
            MainMixer.SetBinding(ColorMixer.ColorARatioProperty, new Binding("ForeAnimValue") { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            BindingOperations.SetBinding(this.Foreground, SolidColorBrush.ColorProperty, new Binding("ColorResult") { Source = MainMixer });

            this.MouseEnter += (s, e) => ColorUpdate();
            this.MouseLeave += (s, e) =>
            {
                ColorUpdate();
                IsMouseDown = false;
                ScaleTo(1.0, 250, new AniEaseOutFluent(2));
            };
            this.MouseLeftButtonDown += (s, e) =>
            {
                if (!this.IsEnabled) return;
                IsMouseDown = true;
                ScaleTo(0.9, 200, new AniEaseOutFluent(5));
            };
            this.MouseLeftButtonUp += (s, e) =>
            {
                if (!this.IsEnabled) return;
                if (!IsMouseDown) return;
                IsMouseDown = false;
                RaiseEvent(new RoutedEventArgs(ClickEvent));
                ScaleTo(1.0, 200, new AniEaseOutFluent(2));
            };

            this.IsEnabledChanged += (s, e) => ColorUpdate();
        }

        private bool IsMouseDown = false;
        

        private void OpacTo(double TargetOpac,double Time)
        {
            Animation.Start(new DoubleAnimation(this, ForeAnimValueProperty, this.ForeAnimValue, TargetOpac, Time, 0));
        }

        private Animation ScaleAnim;
        private void ScaleTo(double scale, double Time, AniEase Ease)
        {
            Animation.Stop(ScaleAnim);
            if (!this.IsVisible)
            {
                this.Scale = 1;
                return;
            }
            ScaleAnim = new DoubleAnimation(this, ScaleProperty, this.Scale, scale, Time, 0, Ease);
            Animation.Start(ScaleAnim);
        }

        public static readonly RoutedEvent ClickEvent = MyButton.ClickEvent;
        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }
    }
}
