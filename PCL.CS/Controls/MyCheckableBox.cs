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
    public abstract class MyCheckableBox : Control
    {

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(MyCheckableBox));


        public Color ForeColor
        {
            get { return (Color)GetValue(ForeColorProperty); }
            set { SetValue(ForeColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ForeColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForeColorProperty =
            DependencyProperty.Register(nameof(ForeColor), typeof(Color), typeof(MyCheckableBox));


        public Color ForeColorNormal
        {
            get { return (Color)GetValue(ForeColorNormalProperty); }
            set { SetValue(ForeColorNormalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ForeColorNormal.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForeColorNormalProperty =
            DependencyProperty.Register(nameof(ForeColorNormal), typeof(Color), typeof(MyCheckableBox));


        public Color ForeColorOver
        {
            get { return (Color)GetValue(ForeColorOverProperty); }
            set { SetValue(ForeColorOverProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ForeColorOver.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForeColorOverProperty =
            DependencyProperty.Register(nameof(ForeColorOver), typeof(Color), typeof(MyCheckableBox));



        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BorderColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BorderColorProperty =
            DependencyProperty.Register(nameof(BorderColor), typeof(Color), typeof(MyCheckableBox));



        private Border PartBorder;
        private Shape PartDot;
        private TextBlock PartText;


        private readonly ColorMixer ForeColorMixer;
        private readonly ColorMixer ForeFinalMixer;
        private readonly ColorMixer BorderColorMixer;
        private readonly ColorMixer BorderFinalMixer;
        public MyCheckableBox()
        {
            ForeColorMixer = new ColorMixer();
            ForeFinalMixer = new ColorMixer();
            BorderColorMixer = new ColorMixer();
            BorderFinalMixer = new ColorMixer();

            Init();

            this.IsEnabledChanged += (s, e) =>
            {
                IsMouseDown = false;
                AnimRefresh();
            };
        }
        private void Init()
        {
            ForeColorMixer.SetBinding(ColorMixer.ColorBProperty, new Binding(nameof(ForeColorNormal)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            ForeColorMixer.SetBinding(ColorMixer.ColorAProperty, new Binding(nameof(ForeColorOver)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            ForeColorMixer.ColorARatio = 0;
            ForeFinalMixer.SetBinding(ColorMixer.ColorBProperty, new Binding(nameof(ColorMixer.ColorResult)) { Source = ForeColorMixer, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            ForeFinalMixer.SetResourceReference(ColorMixer.ColorAProperty, "ColorObjectGray4");
            this.SetBinding(ForeColorProperty, new Binding(nameof(ColorMixer.ColorResult)) { Source = ForeFinalMixer, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            ForeFinalMixer.ColorARatio = 0;

            BorderColorMixer.SetBinding(ColorMixer.ColorBProperty, new Binding(nameof(ColorMixer.ColorResult)) { Source = ForeColorMixer, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            BorderColorMixer.SetBinding(ColorMixer.ColorAProperty, new Binding(nameof(ForeColorOver)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            BorderFinalMixer.SetBinding(ColorMixer.ColorBProperty, new Binding(nameof(ColorMixer.ColorResult)) { Source = BorderColorMixer, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            BorderFinalMixer.SetResourceReference(ColorMixer.ColorAProperty, "ColorObjectGray4");
            BorderFinalMixer.SetBinding(ColorMixer.ColorARatioProperty, new Binding(nameof(ColorMixer.ColorARatio)) { Source = ForeFinalMixer, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            this.SetBinding(BorderColorProperty, new Binding(nameof(ColorMixer.ColorResult)) { Source = BorderFinalMixer, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            PartBorder = Template.FindName("PART_Border", this) as Border;
            PartDot = Template.FindName("PART_Dot", this) as Shape;
            PartText = Template.FindName("PART_Text", this) as TextBlock;

            SolidColorBrush Fore = new SolidColorBrush();
            Fore.SetBinding(SolidColorBrush.ColorProperty, new Binding(nameof(ForeColor)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            SolidColorBrush Border = new SolidColorBrush();
            Border.SetBinding(SolidColorBrush.ColorProperty, new Binding(nameof(BorderColor)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });

            PartBorder.BorderBrush = Border;
            PartDot.Fill = Border;
            PartText.Foreground = Fore;

            var BorderScaleTransform = new ScaleTransform();
            BorderScaleTransform.SetBinding(ScaleTransform.ScaleXProperty, new Binding(nameof(BorderScale)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            BorderScaleTransform.SetBinding(ScaleTransform.ScaleYProperty, new Binding(nameof(BorderScale)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            var AnimScaleTransform = new ScaleTransform();
            AnimScaleTransform.SetBinding(ScaleTransform.ScaleXProperty, new Binding(nameof(OverAnimScale)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            AnimScaleTransform.SetBinding(ScaleTransform.ScaleYProperty, new Binding(nameof(OverAnimScale)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });

            PartBorder.RenderTransform = new TransformGroup { Children = { BorderScaleTransform, AnimScaleTransform } };

            var InnerScaleTransform = new ScaleTransform();
            InnerScaleTransform.SetBinding(ScaleTransform.ScaleXProperty, new Binding(nameof(DotScale)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            InnerScaleTransform.SetBinding(ScaleTransform.ScaleYProperty, new Binding(nameof(DotScale)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });

            PartDot.RenderTransform = InnerScaleTransform;

            PartDot.SetBinding(Shape.OpacityProperty, new Binding(nameof(DotOpacity)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });

            DotOpacity = IsChecked ? 1 : 0;
            DotScale = IsChecked ? 1 : 0;
            BorderColorMixer.ColorARatio = IsChecked ? 1 : 0;
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
            if (IsMouseDown) AutomaticCheck();
            IsMouseDown = false;
            AnimRefresh();
        }

        public double OverAnimScale
        {
            get { return (double)GetValue(OverAnimScaleProperty); }
            set { SetValue(OverAnimScaleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OverAnimScale.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OverAnimScaleProperty =
            DependencyProperty.Register(nameof(OverAnimScale), typeof(double), typeof(MyCheckableBox), new PropertyMetadata(1.0));


        public double BorderScale
        {
            get { return (double)GetValue(BorderScaleProperty); }
            set { SetValue(BorderScaleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BorderScale.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BorderScaleProperty =
            DependencyProperty.Register(nameof(BorderScale), typeof(double), typeof(MyCheckableBox), new PropertyMetadata(1.0));

        public double DotScale
        {
            get { return (double)GetValue(DotScaleProperty); }
            set { SetValue(DotScaleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DotScale.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DotScaleProperty =
            DependencyProperty.Register(nameof(DotScale), typeof(double), typeof(MyCheckableBox), new PropertyMetadata(1.0));



        public double DotOpacity
        {
            get { return (double)GetValue(DotOpacityProperty); }
            set { SetValue(DotOpacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DotOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DotOpacityProperty =
            DependencyProperty.Register(nameof(DotOpacity), typeof(double), typeof(MyCheckableBox), new PropertyMetadata(1.0));


        private bool _MouseOver = false;
        private bool _MouseDown = false;
        private bool _Enabled = true;

        private bool IsMouseDown = false;
        private Animation MouseOverAnimation;
        private Animation MouseDownAnimation;
        private void AnimRefresh()
        {
            double UseAnim = this.IsVisible ? 1 : 0;
            bool IsMouseOver = this.IsMouseOver;
            bool IsMouseDown = this.IsMouseDown;
            if (!IsEnabled) IsMouseDown = false;
            if (IsMouseOver != _MouseOver)
            {
                _MouseOver = IsMouseOver;
                MouseOverAnimation?.StopAnimation();
                if (IsMouseOver)
                    MouseOverAnimation = new DoubleAnimation(ForeColorMixer, ColorMixer.ColorARatioProperty, ForeColorMixer.ColorARatio, 1, 150 * UseAnim, 0);
                else
                    MouseOverAnimation = new DoubleAnimation(ForeColorMixer, ColorMixer.ColorARatioProperty, ForeColorMixer.ColorARatio, 0, 150 * UseAnim, 0);
                MouseOverAnimation.StartAnimation();
            }
            if (IsMouseDown != _MouseDown)
            {
                _MouseDown = IsMouseDown;
                MouseDownAnimation?.StopAnimation();
                if (IsMouseDown)
                    MouseDownAnimation = new DoubleAnimation(this, OverAnimScaleProperty, OverAnimScale, 0.92, 600 * UseAnim, 0, new AniEaseOutFluent(4));
                else
                    MouseDownAnimation = new DoubleAnimation(this, OverAnimScaleProperty, OverAnimScale, 1, 300 * UseAnim, 0, new AniEaseOutFluent(4));
                MouseDownAnimation.StartAnimation();
            }
            if (IsEnabled != _Enabled)
            {
                _Enabled = IsEnabled;
                if (IsEnabled)
                    new DoubleAnimation(ForeFinalMixer, ColorMixer.ColorARatioProperty, ForeFinalMixer.ColorARatio, 0, 300 * UseAnim, 0).StartAnimation();
                else
                    new DoubleAnimation(ForeFinalMixer, ColorMixer.ColorARatioProperty, ForeFinalMixer.ColorARatio, 1, 300 * UseAnim, 0).StartAnimation();
            }
        }

        private void AutomaticCheck()
        {
            if (CanAutoUnSelect) IsChecked = !IsChecked;
            else IsChecked = true;
        }


        protected abstract bool CanAutoUnSelect { get; }
        public static readonly RoutedEvent CheckedEvent = MyRadioButton.CheckedEvent;
        public event RoutedEventHandler Checked
        {
            add => AddHandler(CheckedEvent, value);
            remove => RemoveHandler(CheckedEvent, value);
        }
        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsChecked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(MyCheckableBox), new PropertyMetadata((d, e) => (d as MyCheckableBox).OnIsCheckedChanged(e)));

        private void OnIsCheckedChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is not bool value) return;
            if (e.OldValue is not bool OldValue)
                OldValue = false;
            if (value == OldValue) return;
            if (PartDot is null || !this.IsLoaded)
            {
                if (PartDot is not null)
                {
                    PartDot.Opacity = value ? 1 : 0;
                    DotScale = value ? 1 : 0.2;
                    BorderColorMixer.ColorARatio = value ? 1 : 0;
                }
                return;
            }
            if (value && !OldValue) RaiseEvent(new RoutedEventArgs(CheckedEvent));

            IsCheckAnimRefresh(value);
        }
        private AnimationGroup CheckAnimation;
        private void IsCheckAnimRefresh(bool IsChecked)
        {
            //Base.Log($"播放{(IsChecked ? "点击" : "取消")}动画");
            double UseAnim = this.IsVisible ? 1 : 0;
            const double CheckAnimLenth = 100;
            CheckAnimation?.StopAnimation();
            CheckAnimation = new AnimationGroup();
            if (IsChecked)
            {
                CheckAnimation.Add(new DoubleAnimation(this, BorderScaleProperty, BorderScale, 0.7, CheckAnimLenth * UseAnim, 0, new AniEaseOutFluent(2)));
                CheckAnimation.Add(new DoubleAnimation(this, BorderScaleProperty, 0.7, 1.1, CheckAnimLenth * 1.8 * UseAnim, CheckAnimLenth * UseAnim, new AniEaseInOut(new AniEaseInFluent(2), new AniEaseOutFluent(2))));
                CheckAnimation.Add(new DoubleAnimation(this, BorderScaleProperty, 1.1, 1, CheckAnimLenth * 0.3 * Math.PI * UseAnim, CheckAnimLenth * 2.8 * UseAnim, new AniEaseSine()));
                CheckAnimation.Add(new DoubleAnimation(this, DotScaleProperty, DotScale, 1, 390 * UseAnim, 0, new AniEaseOutBack(3)));
                CheckAnimation.Add(new DoubleAnimation(this, DotOpacityProperty, DotOpacity, 1, 75 * UseAnim, 20 * UseAnim));
                CheckAnimation.Add(new DoubleAnimation(BorderColorMixer, ColorMixer.ColorARatioProperty, BorderColorMixer.ColorARatio, 1, 150 * UseAnim, 0));
            }
            else
            {
                CheckAnimation.Add(new DoubleAnimation(this, BorderScaleProperty, BorderScale, 0.9, 150 * UseAnim, 0, new AniEaseOutFluent(3)));
                CheckAnimation.Add(new DoubleAnimation(this, BorderScaleProperty, 0.9, 1, 150 * UseAnim, 150 * UseAnim, AniEaseSine.Shared));
                CheckAnimation.Add(new DoubleAnimation(this, DotScaleProperty, DotScale, 0, 150 * UseAnim, 0, new AniEaseInFluent(3)));
                CheckAnimation.Add(new DoubleAnimation(this, DotOpacityProperty, DotOpacity, 0, 75 * UseAnim, 30 * UseAnim));
                CheckAnimation.Add(new DoubleAnimation(BorderColorMixer, ColorMixer.ColorARatioProperty, BorderColorMixer.ColorARatio, 0, 150 * UseAnim, 0));
            }
            CheckAnimation.StartAnimation();
        }
    }
}