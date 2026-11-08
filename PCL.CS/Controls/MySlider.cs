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
    public class MySlider : Control
    {
        public Color TrackColor
        {
            get { return (Color)GetValue(TrackColorProperty); }
            set { SetValue(TrackColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TrackColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TrackColorProperty =
            DependencyProperty.Register(nameof(TrackColor), typeof(Color), typeof(MySlider));



        public Color ForeColorNormal
        {
            get { return (Color)GetValue(ForeColorNormalProperty); }
            set { SetValue(ForeColorNormalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ForeColorNormal.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForeColorNormalProperty =
            DependencyProperty.Register(nameof(ForeColorNormal), typeof(Color), typeof(MySlider));



        public Color ForeColorHover
        {
            get { return (Color)GetValue(ForeColorHoverProperty); }
            set { SetValue(ForeColorHoverProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ForeColorHover.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForeColorHoverProperty =
            DependencyProperty.Register(nameof(ForeColorHover), typeof(Color), typeof(MySlider));


        public Color ForeColor
        {
            get { return (Color)GetValue(ForeColorProperty); }
            set { SetValue(ForeColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ForeColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForeColorProperty =
            DependencyProperty.Register(nameof(ForeColor), typeof(Color), typeof(MySlider));


        public double DotScale
        {
            get { return (double)GetValue(DotScaleProperty); }
            set { SetValue(DotScaleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DotScale.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DotScaleProperty =
            DependencyProperty.Register(nameof(DotScale), typeof(double), typeof(MySlider), new PropertyMetadata(1.0));



        public object ValueToolTip
        {
            get { return (object)GetValue(ValueToolTipProperty); }
            set { SetValue(ValueToolTipProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValueToolTip.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueToolTipProperty =
            DependencyProperty.Register(nameof(ValueToolTip), typeof(object), typeof(MySlider));


        private readonly ColorMixer TrackFinalMixer;
        private readonly ColorMixer ForeColorMixer;
        private readonly ColorMixer ForeFinalMixer;

        public MySlider()
        {
            TrackFinalMixer = new ColorMixer();
            ForeColorMixer = new ColorMixer();
            ForeFinalMixer = new ColorMixer();
            Init();
        }

        private void Init()
        {
            TrackFinalMixer.SetBinding(ColorMixer.ColorBProperty, new Binding(nameof(TrackColor)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            TrackFinalMixer.SetResourceReference(ColorMixer.ColorAProperty, "ColorObjectGray4");
            TrackFinalMixer.SetBinding(ColorMixer.ColorARatioProperty, new Binding(nameof(ColorMixer.ColorARatio)) { Source = ForeFinalMixer, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            ForeColorMixer.SetBinding(ColorMixer.ColorBProperty, new Binding(nameof(ForeColorNormal)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            ForeColorMixer.SetBinding(ColorMixer.ColorAProperty, new Binding(nameof(ForeColorHover)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            ForeColorMixer.ColorARatio = 0;
            ForeFinalMixer.SetBinding(ColorMixer.ColorBProperty, new Binding(nameof(ColorMixer.ColorResult)) { Source = ForeColorMixer, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            ForeFinalMixer.SetResourceReference(ColorMixer.ColorAProperty, "ColorObjectGray4");
            ForeFinalMixer.ColorARatio = 0;
            this.SetBinding(ForeColorProperty, new Binding(nameof(ColorMixer.ColorResult)) { Source = ForeFinalMixer, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
        }
        private Ellipse PartDot;
        private RatioControl PartRatio;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var PartBack = Template.FindName("PART_LineBack", this) as Border;
            var PartFore = Template.FindName("PART_LineFore", this) as Border;
            PartDot = Template.FindName("PART_ShapeDot", this) as Ellipse;
            PartRatio = Template.FindName("PART_Ratio", this) as RatioControl;

            var ForeBrush = new SolidColorBrush();
            ForeBrush.SetBinding(SolidColorBrush.ColorProperty, new Binding(nameof(ForeColor)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            
            PartFore.Background = ForeBrush;
            PartDot.Fill = ForeBrush;

            var TrackBrush = new SolidColorBrush();
            TrackBrush.SetBinding(SolidColorBrush.ColorProperty, new Binding(nameof(ColorMixer.ColorResult)) { Source = TrackFinalMixer, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            PartBack.Background = TrackBrush;

            var DotScaleT = new ScaleTransform();
            DotScaleT.SetBinding(ScaleTransform.ScaleXProperty, new Binding(nameof(DotScale)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            DotScaleT.SetBinding(ScaleTransform.ScaleYProperty, new Binding(nameof(DotScale)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            PartDot.RenderTransform = DotScaleT;

            PartRatio.SetBinding(RatioControl.WidthRatioProperty, new Binding(nameof(Process)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            AnimRefresh();
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            IsMouseDown = false;
            AnimRefresh();
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            IsMouseDown = true;
            AnimRefresh();
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            IsMouseDown = false;
            AnimRefresh();
        }

        private bool IsMouseDown
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                if (value)
                    PartDot?.CaptureMouse();
                else
                    PartDot?.ReleaseMouseCapture();
            }
        }



        public int MaxValue
        {
            get { return (int)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register(nameof(MaxValue), typeof(int), typeof(MySlider), new PropertyMetadata(99, (s, e) => (s as MySlider)._ValueChanged(-1, (s as MySlider).Value)));

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(int), typeof(MySlider), new FrameworkPropertyMetadata(0,
                (s, e) => (s as MySlider)._ValueChanged((int)e.OldValue,(int)e.NewValue),
                ValueCoerce));

        private static object ValueCoerce(DependencyObject d, object baseValue)
        {
            if (d is MySlider slider)
            {
                if (baseValue is not int value) return baseValue;
                return (int)MathHelper.Clamp(value, 0, slider.MaxValue);
            }
            else return baseValue;
        }



        public double Process
        {
            get { return (double)GetValue(ProcessProperty); }
            set { SetValue(ProcessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Process.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProcessProperty =
            DependencyProperty.Register(nameof(Process), typeof(double), typeof(MySlider), new PropertyMetadata(0.0));

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(ValueChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<int>),
            typeof(MySlider));

        public event RoutedPropertyChangedEventHandler<double> ValueChanged
        {
            add => AddHandler(ValueChangedEvent, value);
            remove => RemoveHandler(ValueChangedEvent, value);
        }
        private Animation ValueChangedAnimation;
        private void _ValueChanged(int OldValue,int Value)
        {
            if (OldValue == Value) return;
            if (Value > MaxValue)
            {
                this.Value = MaxValue;
                return;
            }
            if (OldValue >= 0)
                RaiseEvent(new RoutedPropertyChangedEventArgs<double>(
                    OldValue,
                    Value,
                    ValueChangedEvent));

            ValueChangedAnimation?.StopAnimation();
            double UseAnim = IsVisible ? 1 : 0;

            double Process = Value * 1.0 / MaxValue;
            double Last = this.Process;
            double Delta = Math.Abs(Process - Last);
            double Time = (1 - Math.Pow(1 - Delta, 3)) * 300 * UseAnim;
            ValueChangedAnimation = new DoubleAnimation(this, ProcessProperty, this.Process, Process, Time, 0, Time > 50 ? new AniEaseOutFluent(3) : AniEaseLinear.Shared);
            ValueChangedAnimation.StartAnimation();
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!IsMouseDown) return;
            double X = e.GetPosition(PartRatio).X;
            double Width = PartRatio.ActualWidth;
            double Process = MathHelper.Clamp(X / Width, 0, 1);
            int value = (int)Math.Round(Process * MaxValue);
            Value = value;
        }
        private bool _mouseOver = false;
        private bool _mouseDown = false;
        private bool _enabled = true;
        private Animation MouseOverAnimation;
        private Animation MouseDownAnimation;
        private void AnimRefresh()
        {
            double UseAnim = this.IsVisible ? 1 : 0;
            bool IsMouseOver = this.IsMouseOver;
            bool IsMouseDown = this.IsMouseDown;
            if (!IsEnabled) IsMouseDown = false;
            if (IsMouseOver != _mouseOver)
            {
                _mouseOver = IsMouseOver;
                MouseOverAnimation?.StopAnimation();
                if (IsMouseOver)
                    MouseOverAnimation = new DoubleAnimation(ForeColorMixer, ColorMixer.ColorARatioProperty, ForeColorMixer.ColorARatio, 1, 40 * UseAnim, 0);
                else
                    MouseOverAnimation = new DoubleAnimation(ForeColorMixer, ColorMixer.ColorARatioProperty, ForeColorMixer.ColorARatio, 0, 100 * UseAnim, 0);
                MouseOverAnimation.StartAnimation();
            }
            if (IsMouseDown != _mouseDown)
            {
                _mouseDown = IsMouseDown;
                MouseDownAnimation?.StopAnimation();
                if (IsMouseDown)
                    MouseDownAnimation = new DoubleAnimation(this, DotScaleProperty, DotScale, 1.3, 40, 0, new AniEaseOutFluent(3));
                else
                    MouseDownAnimation = new DoubleAnimation(this, DotScaleProperty, DotScale, 1, 200, 0, new AniEaseOutFluent(3));
                MouseDownAnimation.StartAnimation();
            }

            if (IsEnabled != _enabled)
            {
                _enabled = IsEnabled;
                if (IsEnabled)
                    new DoubleAnimation(ForeFinalMixer, ColorMixer.ColorARatioProperty, ForeFinalMixer.ColorARatio, 0, 300 * UseAnim, 0).StartAnimation();
                else
                    new DoubleAnimation(ForeFinalMixer, ColorMixer.ColorARatioProperty, ForeFinalMixer.ColorARatio, 1, 300 * UseAnim, 0).StartAnimation();
            }
        }
    }
}
