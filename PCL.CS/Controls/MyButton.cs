using PCL.CS.Modules;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using XeF4Core;
using XeF4Core.WPF;

namespace PCL.CS.Controls
{
    public class MyButton : Control
    {
        public Color ColorBorder
        {
            get { return (Color)GetValue(ColorBorderProperty); }
            set => SetValue(ColorBorderProperty, value);
        }

        public static readonly DependencyProperty ColorBorderProperty = DependencyProperty.Register(nameof(ColorBorder), typeof(Color), typeof(MyButton));
        public Color ColorBorderHighlight
        {
            get { return (Color)GetValue(ColorBorderHighlightProperty); }
            set { SetValue(ColorBorderHighlightProperty, value); }
        }

        public static readonly DependencyProperty ColorBorderHighlightProperty =
            DependencyProperty.Register(nameof(ColorBorderHighlight), typeof(Color), typeof(MyButton));

        public Color ColorBorderNormal
        {
            get { return (Color)GetValue(ColorBorderNormalProperty); }
            set { SetValue(ColorBorderNormalProperty, value); }
        }

        public static readonly DependencyProperty ColorBorderNormalProperty =
            DependencyProperty.Register(nameof(ColorBorderNormal), typeof(Color), typeof(MyButton));

        private readonly ColorMixer BorderColorMixer = new ColorMixer();
        private readonly ColorMixer BorderFinalMixer = new ColorMixer();


        public Color ColorBackNormal
        {
            get { return (Color)GetValue(ColorBackNormalProperty); }
            set { SetValue(ColorBackNormalProperty, value); }
        }

        public static readonly DependencyProperty ColorBackNormalProperty =
            DependencyProperty.Register(nameof(ColorBackNormal), typeof(Color), typeof(MyButton));


        public Color ColorBackHighlight
        {
            get { return (Color)GetValue(ColorBackHighlightProperty); }
            set { SetValue(ColorBackHighlightProperty, value); }
        }

        public static readonly DependencyProperty ColorBackHighlightProperty =
            DependencyProperty.Register(nameof(ColorBackHighlight), typeof(Color), typeof(MyButton));

        public Color ColorBack
        {
            get { return (Color)GetValue(ColorBackProperty); }
            set { SetValue(ColorBackProperty, value); }
        }

        public static readonly DependencyProperty ColorBackProperty =
            DependencyProperty.Register(nameof(ColorBack), typeof(Color), typeof(MyButton));

        private readonly ColorMixer BackColorMixer = new ColorMixer();


        public double AnimateScale
        {
            get { return (double)GetValue(AnimateScaleProperty); }
            set { SetValue(AnimateScaleProperty, value); }
        }

        public static readonly DependencyProperty AnimateScaleProperty =
            DependencyProperty.Register(nameof(AnimateScale), typeof(double), typeof(MyButton), new PropertyMetadata((double)1));


        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(MyButton),
            new PropertyMetadata("按钮"));

        public Thickness TextPadding
        {
            get { return (Thickness)GetValue(TextPaddingProperty); }
            set { SetValue(TextPaddingProperty, value); }
        }

        public static readonly DependencyProperty TextPaddingProperty =
            DependencyProperty.Register(nameof(TextPadding), typeof(Thickness), typeof(MyButton), new PropertyMetadata(new Thickness()));
        public enum ColorState
        {
            Normal,
            HighLight,
            Red
        }


        public ColorState ColorType
        {
            get { return (ColorState)GetValue(ColorTypeProperty); }
            set { SetValue(ColorTypeProperty, value); }
        }
        
        public static readonly DependencyProperty ColorTypeProperty =
            DependencyProperty.Register(nameof(ColorType), typeof(ColorState), typeof(MyButton), new PropertyMetadata(ColorState.Normal));


        public MyButton()
        {
            OnMyButtonLoaded();
            BorderColorMixer.ColorARatio = 1;
            BackColorMixer.ColorARatio = 1;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var border = Template.FindName("PART_Border", this) as Border;
            var textBlock = Template.FindName("PART_TextBlock", this) as TextBlock;
            var brush = new SolidColorBrush();
            brush.SetBinding(SolidColorBrush.ColorProperty, new Binding(nameof(ColorBorder))
            {
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.OneWay
            });
            var backBrush= new SolidColorBrush();
            backBrush.SetBinding(SolidColorBrush.ColorProperty, new Binding(nameof(ColorBack))
            {
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.OneWay
            });
            backBrush.Opacity = 0.7;
            var scale = new ScaleTransform();
            var binding = new Binding(nameof(AnimateScale))
            {
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.OneWay
            };
            scale.SetBinding(ScaleTransform.ScaleXProperty, binding);
            scale.SetBinding(ScaleTransform.ScaleYProperty, binding);
            border.RenderTransform = scale;
            border.BorderBrush = brush;
            border.Background = backBrush;
            textBlock.Foreground = brush;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            ColorUpdate();
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            IsMousePressed = false;
            ColorUpdate();
            ScaleTo(1.0, 300, new AniEaseOutFluent(2));
        }
        private bool IsMousePressed = false;
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            IsMousePressed = true;
            ColorUpdate();
            ScaleTo(0.955, 80, new AniEaseOutFluent(4));
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (!IsMousePressed) return;
            ScaleTo(1.0, 300, new AniEaseOutFluent(2));
            IsMousePressed = false;
            RaiseEvent(new RoutedEventArgs(ClickEvent));
        }
        private Animation ScaleAnim;
        private void ScaleTo(double scale, double Time, AniEase Ease)
        {
            Animation.Stop(ScaleAnim);
            if (!this.IsVisible)
            {
                this.AnimateScale = 1;
                return;
            }
            ScaleAnim = new DoubleAnimation(this, AnimateScaleProperty, this.AnimateScale, scale, Time, 0, Ease);
            Animation.Start(ScaleAnim);
        }
        private AnimationGroup ColorAnimation;
        private void ColorUpdate()
        {
            if (!this.IsLoaded) return;
            ColorAnimation?.StopAnimation();
            ColorAnimation = new AnimationGroup();
            if (!this.IsEnabled)
            {
                ColorAnimation.Add(new DoubleAnimation(BorderFinalMixer, ColorMixer.ColorARatioProperty, BorderFinalMixer.ColorARatio, 0, 200, 0));
                ColorAnimation.Add(new DoubleAnimation(BackColorMixer, ColorMixer.ColorARatioProperty, BackColorMixer.ColorARatio, 1, 200, 0));
            }
            else if (this.IsMouseOver)
            {
                ColorAnimation.Add(new DoubleAnimation(BorderFinalMixer, ColorMixer.ColorARatioProperty, BorderFinalMixer.ColorARatio, 1, 200, 0));
                ColorAnimation.Add(new DoubleAnimation(BorderColorMixer, ColorMixer.ColorARatioProperty, BorderColorMixer.ColorARatio, 0, 100, 0));
                ColorAnimation.Add(new DoubleAnimation(BackColorMixer, ColorMixer.ColorARatioProperty, BackColorMixer.ColorARatio, 0, 100, 0));
            }
            else
            {
                ColorAnimation.Add(new DoubleAnimation(BorderFinalMixer, ColorMixer.ColorARatioProperty, BorderFinalMixer.ColorARatio, 1, 200, 0));
                ColorAnimation.Add(new DoubleAnimation(BorderColorMixer, ColorMixer.ColorARatioProperty, BorderColorMixer.ColorARatio, 1, 300, 0));
                ColorAnimation.Add(new DoubleAnimation(BackColorMixer, ColorMixer.ColorARatioProperty, BackColorMixer.ColorARatio, 1, 300, 0));
            }
            ColorAnimation.StartAnimation();
        }
        private void OnMyButtonLoaded()
        {
            BorderColorMixer.SetBinding(ColorMixer.ColorAProperty, new Binding(nameof(ColorBorderNormal)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            BorderColorMixer.SetBinding(ColorMixer.ColorBProperty, new Binding(nameof(ColorBorderHighlight)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            BorderFinalMixer.SetBinding(ColorMixer.ColorAProperty, new Binding(nameof(ColorMixer.ColorResult)) { Source = BorderColorMixer, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            BorderFinalMixer.SetResourceReference(ColorMixer.ColorBProperty, "ColorObjectGray4");
            BorderFinalMixer.ColorARatio = 1.0;
            this.SetBinding(ColorBorderProperty, new Binding(nameof(ColorMixer.ColorResult)) { Source = BorderFinalMixer, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            BackColorMixer.SetBinding(ColorMixer.ColorAProperty, new Binding(nameof(ColorBackNormal)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            BackColorMixer.SetBinding(ColorMixer.ColorBProperty, new Binding(nameof(ColorBackHighlight)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            this.SetBinding(ColorBackProperty, new Binding(nameof(ColorMixer.ColorResult)) { Source = BackColorMixer, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
        }
        public static readonly RoutedEvent ClickEvent = Button.ClickEvent;
        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }
    }
}