using PCL.CS.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using XeF4Core.WPF;

namespace PCL.CS.Controls
{
    public class MyIconButton:Control
    {
        public Geometry Logo
        {
            get => (Geometry)GetValue(LogoProperty);
            set => SetValue(LogoProperty, value);
        }
        public static readonly DependencyProperty LogoProperty = DependencyProperty.Register("Logo", typeof(Geometry), typeof(MyIconButton));

        public double LogoScale
        {
            get => (double)GetValue(LogoScaleProperty);
            set => SetValue(LogoScaleProperty, value);
        }
        public static readonly DependencyProperty LogoScaleProperty = DependencyProperty.Register("LogoScale", typeof(double), typeof(MyIconButton));



        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Scale.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register(nameof(Scale), typeof(double), typeof(MyIconButton), new PropertyMetadata(1.0));




        public Color ForeColorNormal
        {
            get { return (Color)GetValue(ForeColorNormalProperty); }
            set { SetValue(ForeColorNormalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ForeColorNormal.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForeColorNormalProperty =
            DependencyProperty.Register(nameof(ForeColorNormal), typeof(Color), typeof(MyIconButton));


        public Color ForeColorHighlight
        {
            get { return (Color)GetValue(ForeColorHighlightProperty); }
            set { SetValue(ForeColorHighlightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ForeColorHighlight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForeColorHighlightProperty =
            DependencyProperty.Register(nameof(ForeColorHighlight), typeof(Color), typeof(MyIconButton));



        public Color ForeColor
        {
            get { return (Color)GetValue(ForeColorProperty); }
            set { SetValue(ForeColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ForeColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForeColorProperty =
            DependencyProperty.Register(nameof(ForeColor), typeof(Color), typeof(MyIconButton));




        public enum ColorState
        {
            Black,
            Color,
            Red,
            Custom
        }


        public ColorState ColorType
        {
            get { return (ColorState)GetValue(ColorTypeProperty); }
            set { SetValue(ColorTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColorType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorTypeProperty =
            DependencyProperty.Register(nameof(ColorType), typeof(ColorState), typeof(MyIconButton));



        private void ColorUpdate()
        {
            if (!this.IsEnabled)
            {
                this.OpacTo(0, 300);
                UnEnableTo(1, 300);
                return;
            }
            double Opac;
            double Time;
            if (this.IsMouseOver)
            {
                Opac = 1;
                Time = 120;
            }
            else
            {
                Opac = 0;
                Time = 150;
            }
            OpacTo(Opac, Time);
            UnEnableTo(0, 300);
        }
        private void OpacTo(double Opac,double Time)
        {
            new DoubleAnimation(MainMixer, ColorMixer.ColorARatioProperty, MainMixer.ColorARatio, Opac, Time).StartAnimation();
        }
        private void UnEnableTo(double val, double Time) =>
            new DoubleAnimation(FinalMixer, ColorMixer.ColorARatioProperty, FinalMixer.ColorARatio, val, Time).StartAnimation();



        private readonly ColorMixer MainMixer;
        private readonly ColorMixer FinalMixer;
        public MyIconButton()
        {
            LogoScale = 1;
            MainMixer = new ColorMixer();
            FinalMixer = new ColorMixer();
            OnInit();
            this.IsEnabledChanged += (s, e) =>
            {
                ScaleTo(1.0, 200, new AniEaseOutFluent(2));
                ColorUpdate();
            };
        }
        private void OnInit()
        {
            MainMixer.SetBinding(ColorMixer.ColorBProperty, new Binding(nameof(ForeColorNormal)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            MainMixer.SetBinding(ColorMixer.ColorAProperty, new Binding(nameof(ForeColorHighlight)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            FinalMixer.SetBinding(ColorMixer.ColorBProperty, new Binding(nameof(ColorMixer.ColorResult)) { Source = MainMixer, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            FinalMixer.SetResourceReference(ColorMixer.ColorAProperty, "ColorObjectGray3");
            this.SetBinding(ForeColorProperty, new Binding(nameof(ColorMixer.ColorResult)) { Source = FinalMixer, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
        }
        private bool IsMouseDown = false;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var PathLogo = Template.FindName("PART_Logo", this) as Path;
            var TransGroup = new TransformGroup();
            var LogoScale = new ScaleTransform();
            var LogoScaleBinding = new Binding(nameof(this.LogoScale)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay };
            LogoScale.SetBinding(ScaleTransform.ScaleXProperty, LogoScaleBinding);
            LogoScale.SetBinding(ScaleTransform.ScaleYProperty, LogoScaleBinding);
            TransGroup.Children.Add(LogoScale);
            var AnimScale = new ScaleTransform();
            var AnimScaleBinding = new Binding(nameof(Scale)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay };
            AnimScale.SetBinding(ScaleTransform.ScaleXProperty, AnimScaleBinding);
            AnimScale.SetBinding(ScaleTransform.ScaleYProperty, AnimScaleBinding);
            TransGroup.Children.Add(AnimScale);
            PathLogo.RenderTransform = TransGroup;
            var Fill = new SolidColorBrush();
            Fill.SetBinding(SolidColorBrush.ColorProperty, new Binding(nameof(ColorMixer.ColorResult)) { Source = FinalMixer, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            PathLogo.Fill = Fill;
        }


        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            ColorUpdate();
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            ColorUpdate();
            IsMouseDown = false;
            ScaleTo(1.0, 200, new AniEaseOutFluent(2));
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (!this.IsEnabled) return;
            IsMouseDown = true;
            ScaleTo(0.9, 200, new AniEaseOutFluent(5));
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (!this.IsEnabled) return;
            if (!IsMouseDown) return;
            IsMouseDown = false;
            RaiseEvent(new RoutedEventArgs(ClickEvent));
            ScaleTo(1.0, 200, new AniEaseOutFluent(2));
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

        public static readonly RoutedEvent ClickEvent = Button.ClickEvent;
        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }
    }
}
