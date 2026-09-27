using Newtonsoft.Json.Linq;
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
using System.Windows.Markup;
using System.Windows.Media;
using XeF4Core.WPF;

namespace PCL.CS.Controls
{
    /// <summary>
    /// 列表项
    /// </summary>
    public class MyListItem : ContentControl,IMyButton,IMyRadio
    {
        private Border BackgroundBorder = null;
        private UIElement MainElement = null;
        private RowDefinition RowDefinition = null;

        private ScaleTransform AnimScale = new ScaleTransform();

        private ScaleTransform MainScale = new ScaleTransform();

        private RatioControl Ratio;

        public MyListItem()
        {
            BackColorMixer = new ColorMixer();
            ForeColorMixer = new ColorMixer();
            Init();
        }
        public MyRadioGroup RadioGroup
        {
            get { return (MyRadioGroup)GetValue(RadioGroupProperty); }
            set { SetValue(RadioGroupProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RadioGroup.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RadioGroupProperty =
            DependencyProperty.Register(nameof(RadioGroup), typeof(MyRadioGroup), typeof(MyListItem), new PropertyMetadata((s, e) => (s as MyListItem)?.OnRadioGroupChanged(e)));

        private void OnRadioGroupChanged(DependencyPropertyChangedEventArgs e)
        {
            (e.OldValue as MyRadioGroup)?.RemoveChild(this);
            (e.NewValue as MyRadioGroup)?.AddChild(this);
        }

        public Color BackColorNormal
        {
            get { return (Color)GetValue(BackColorNormalProperty); }
            set { SetValue(BackColorNormalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackColorNormal.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackColorNormalProperty =
            DependencyProperty.Register(nameof(BackColorNormal), typeof(Color), typeof(MyListItem));

        public Color BackColorSelect
        {
            get { return (Color)GetValue(BackColorSelectProperty); }
            set { SetValue(BackColorSelectProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackColorSelect.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackColorSelectProperty =
            DependencyProperty.Register(nameof(BackColorSelect), typeof(Color), typeof(MyListItem));
        public Color BackColor
        {
            get { return (Color)GetValue(BackColorProperty); }
            set { SetValue(BackColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackColorProperty =
            DependencyProperty.Register(nameof(BackColor), typeof(Color), typeof(MyListItem));


        public Color ForeColorNormal
        {
            get { return (Color)GetValue(ForeColorNormalProperty); }
            set { SetValue(ForeColorNormalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ForeColorNormal.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForeColorNormalProperty =
            DependencyProperty.Register(nameof(ForeColorNormal), typeof(Color), typeof(MyListItem));


        public Color ForeColorSelect
        {
            get { return (Color)GetValue(ForeColorSelectProperty); }
            set { SetValue(ForeColorSelectProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ForeColorSelect.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForeColorSelectProperty =
            DependencyProperty.Register(nameof(ForeColorSelect), typeof(Color), typeof(MyListItem));


        public Color ForeColor
        {
            get { return (Color)GetValue(ForeColorProperty); }
            set { SetValue(ForeColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ForeColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForeColorProperty =
            DependencyProperty.Register(nameof(ForeColor), typeof(Color), typeof(MyListItem));

        private readonly ColorMixer BackColorMixer;
        private readonly ColorMixer ForeColorMixer;

        private void Init()
        {
            BackColorMixer.SetBinding(ColorMixer.ColorBProperty, new Binding(nameof(BackColorNormal)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            BackColorMixer.SetBinding(ColorMixer.ColorAProperty, new Binding(nameof(BackColorSelect)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            this.SetBinding(BackColorProperty, new Binding(nameof(ColorMixer.ColorResult)) { Source = BackColorMixer, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            ForeColorMixer.SetBinding(ColorMixer.ColorBProperty, new Binding(nameof(ForeColorNormal)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            ForeColorMixer.SetBinding(ColorMixer.ColorAProperty, new Binding(nameof(ForeColorSelect)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            this.SetBinding(ForeColorProperty, new Binding(nameof(ColorMixer.ColorResult)) { Source = ForeColorMixer, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            BackColorMixer.ColorARatio = 0;
            ForeColorMixer.ColorARatio = 0;

            var ForeBrush = new SolidColorBrush();
            ForeBrush.SetBinding(SolidColorBrush.ColorProperty, new Binding(nameof(ForeColor)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            this.Foreground = ForeBrush;
        }

        protected virtual double RectBackCornerRadius { get => 6; }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            BackgroundBorder = Template.FindName("PART_Background", this) as Border;
            MainElement = Template.FindName("PART_Main", this) as UIElement;
            Ratio = Template.FindName("PART_Ratio", this) as RatioControl;
            BackgroundBorder.RenderTransformOrigin = new Point(0.5, 0.5);
            BackgroundBorder.RenderTransform = AnimScale;
            MainElement.RenderTransformOrigin = new Point(0.5, 0.5);
            MainElement.RenderTransform = MainScale;
            BackgroundBorder.Opacity = 0;
            BackgroundBorder.BorderThickness = new Thickness(0.3);
            BackgroundBorder.CornerRadius = new CornerRadius(RectBackCornerRadius);
            var backBrush = new SolidColorBrush();
            backBrush.SetBinding(SolidColorBrush.ColorProperty, new Binding(nameof(BackColor)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            BackgroundBorder.Background = backBrush;

            return;


#pragma warning disable CS0162 // 检测到无法访问的代码
            this.SetResourceReference(BackColorProperty, "ColorObject7");
#pragma warning restore CS0162 // 检测到无法访问的代码
            if (IsChecked) RowDefinition.Height = new GridLength(6, GridUnitType.Star);
            else RowDefinition.Height = new GridLength(0, GridUnitType.Star);
            AnimScale.ScaleX = AnimScale.ScaleY = DefaultAnimScale;
            SolidColorBrush Fore = new SolidColorBrush();
            this.Foreground = Fore;
            Fore.Color = (Color)(IsChecked ? App.Current.FindResource("ColorObject3") : App.Current.FindResource("ColorObject1"));
            this.SetResourceReference(ForeColorProperty, IsChecked ? "ColorObject4" : "ColorObject1");
        }
        //private AnimationGroup AnimationDownUp;

        private bool IsMouseDown = false;
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (IsMouseDown) RaiseEvent();
            IsMouseDown = false;
            AnimationRefresh();
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            IsMouseDown = true;
            AnimationRefresh();
        }
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            AnimationRefresh();
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            IsMouseDown = false;
            AnimationRefresh();
        }


        protected virtual double MouseOverScale { get => 0.985; } 

        protected virtual double DefaultAnimScale { get => 0.992; }



        private void AnimationRefresh()
        {
            if (_IsMouseIn != IsMouseOver)
            {
                _IsMouseIn = IsMouseOver;
                InOutAnimation?.StopAnimation();
                InOutAnimation = new AnimationGroup();
                if (IsMouseOver)
                {
                    InOutAnimation.Add(new DoubleAnimation(AnimScale, ScaleTransform.ScaleXProperty, AnimScale.ScaleX, 1.0, 200, 0, new AniEaseOutFluent(2)));
                    InOutAnimation.Add(new DoubleAnimation(AnimScale, ScaleTransform.ScaleYProperty, AnimScale.ScaleY, 1.0, 200, 0, new AniEaseOutFluent(2)));
                    InOutAnimation.Add(new DoubleAnimation(BackgroundBorder, Border.OpacityProperty, BackgroundBorder.Opacity, 0.7, 200, 0));
                }
                else
                {
                    InOutAnimation.Add(new DoubleAnimation(AnimScale, ScaleTransform.ScaleXProperty, AnimScale.ScaleX, 0.992, 400, 0, new AniEaseOutFluent(2)));
                    InOutAnimation.Add(new DoubleAnimation(AnimScale, ScaleTransform.ScaleYProperty, AnimScale.ScaleY, 0.992, 400, 0, new AniEaseOutFluent(2)));
                    InOutAnimation.Add(new DoubleAnimation(BackgroundBorder, Border.OpacityProperty, BackgroundBorder.Opacity, 0, 400, 0));
                }
                Animation.Start(InOutAnimation);
            }
            if (_IsMouseDown != IsMouseDown)
            {
                _IsMouseDown = IsMouseDown;
                Animation.Stop(MouseDownAnim);
                MouseDownAnim = new AnimationGroup();
                if (IsMouseDown)
                {
                    MouseDownAnim.Add(new DoubleAnimation(MainScale, ScaleTransform.ScaleXProperty, MainScale.ScaleX, MouseOverScale, 200, 0, new AniEaseOutFluent(2)));
                    MouseDownAnim.Add(new DoubleAnimation(MainScale, ScaleTransform.ScaleYProperty, MainScale.ScaleY, MouseOverScale, 200, 0, new AniEaseOutFluent(2)));
                    //this.SetResourceReference(BackColorProperty, "ColorObject5");
                    MouseDownAnim.Add(new DoubleAnimation(BackColorMixer, ColorMixer.ColorARatioProperty, BackColorMixer.ColorARatio, 1, 200, 0));
                }
                else
                {
                    MouseDownAnim.Add(new DoubleAnimation(MainScale, ScaleTransform.ScaleXProperty, MainScale.ScaleX, 1.0, 400, 0, new AniEaseOutFluent(2)));
                    MouseDownAnim.Add(new DoubleAnimation(MainScale, ScaleTransform.ScaleYProperty, MainScale.ScaleY, 1.0, 400, 0, new AniEaseOutFluent(2)));
                    //this.SetResourceReference(BackColorProperty, "ColorObject6");
                    MouseDownAnim.Add(new DoubleAnimation(BackColorMixer, ColorMixer.ColorARatioProperty, BackColorMixer.ColorARatio, 0, 200, 0));
                }
                Animation.Start(MouseDownAnim);
            }
        }
        private bool _IsMouseIn = false;
        private bool _IsMouseDown = false;
        private AnimationGroup InOutAnimation;
        private AnimationGroup MouseDownAnim;

        protected virtual bool AutoCheckResult => !IsChecked;

        private void RaiseEvent()
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent));
            if (AutoCheck) IsChecked = AutoCheckResult;
        }
        
        //private class AnimCheck : Animation
        //{
        //    public double StartValue { get; set; }
        //    public double EndValue { get; set; }
        //    public MyListItem Obj { get; set; }
        //    public AniEase Ease { get; set; }
        //    public override object GetValue(TimeSpan t)
        //    {
        //        return GetValue(t.TotalMilliseconds / TotalTime.TotalMilliseconds);
        //    }
        //    public object GetValue(double t)
        //    {
        //        return (EndValue - StartValue) * Ease.GetValue(t) + StartValue;
        //    }
        //    public override void SetValue(object value)
        //    {
        //        double Value = (double)value;
        //        if (Math.Abs(Value - 1) < 0.05) return;
        //        double ActuV = Value / (1 - Value) * 2;
        //        GridLength gridLength = new GridLength(ActuV, GridUnitType.Star);
        //        Obj.RowDefinition.Height = gridLength;
        //    }
        //}



        private AnimationGroup CheckAnim { get; set; }
        public bool AutoCheck { get; set; } = false;

        private static readonly AniEase EaseInOutSine = new AniEaseInOut(new AniEaseInSine(), new AniEaseOutSine());



        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsChecked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(MyListItem), new PropertyMetadata((d, e) => (d as MyListItem).OnIsCheckedChanged(e)));

        private void OnIsCheckedChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is not bool value) return;
            if (e.OldValue is not bool OldValue)
                OldValue = false;
            if (value == OldValue) return;
            if (Ratio is null || !this.IsLoaded) return;
            if (value && !OldValue) RaiseEvent(new RoutedEventArgs(CheckedEvent));
            if (value)
            {
                Animation.Stop(CheckAnim);
                CheckAnim = new AnimationGroup();
                CheckAnim.Add(new DoubleAnimation(Ratio, RatioControl.HeightRatioProperty, Ratio.HeightRatio, 0.8, 150, 0, new AniEaseOutFluent(3)));
                CheckAnim.Add(new DoubleAnimation(Ratio, RatioControl.HeightRatioProperty, 0.8, 0.75, 50, 150, EaseInOutSine));
                CheckAnim.Add(new DoubleAnimation(ForeColorMixer, ColorMixer.ColorARatioProperty, ForeColorMixer.ColorARatio, 1, 200, 0));
                Animation.Start(CheckAnim);
            }
            else
            {
                Animation.Stop(CheckAnim);
                CheckAnim = new AnimationGroup();
                CheckAnim.Add(new DoubleAnimation(Ratio, RatioControl.HeightRatioProperty, Ratio.HeightRatio, 0, 200, 0, new AniEaseInFluent(3)));
                CheckAnim.Add(new DoubleAnimation(ForeColorMixer, ColorMixer.ColorARatioProperty, ForeColorMixer.ColorARatio, 0, 200, 0));
                Animation.Start(CheckAnim);
            }
        }
        //public bool aIsChecked
        //{
        //    get => _IsChecked;
        //    set
        //    {
        //        if(RowDefinition is null||!this.IsLoaded)
        //        {
        //            _IsChecked = value;
        //            return;
        //        }
        //        if (value && !_IsChecked)
        //        {
        //            RaiseEvent(new RoutedEventArgs(CheckedEvent));
        //        }
        //        if (_IsChecked == value) return;
        //        _IsChecked = value;
        //        switch (value)
        //        {
        //            case true:
        //                Animation.Stop(CheckAnim);
        //                CheckAnim = new AnimationGroup();
        //                CheckAnim.Add(new AnimCheck() { Obj = this, StartValue = (RowDefinition.Height.Value / 2) / (RowDefinition.Height.Value / 2 + 1), EndValue = 0.8, Ease = new AniEaseOutFluent(3), TotalTime = TimeSpan.FromMilliseconds(150) });
        //                CheckAnim.Add(new AnimCheck() { Obj = this, StartValue = 0.8, EndValue = 0.75, Ease = EaseInOutSine, TotalTime = TimeSpan.FromMilliseconds(50), After = TimeSpan.FromMilliseconds(150) });
        //                this.SetResourceReference(ForeColorProperty, "ColorObject3");
        //                Animation.Start(CheckAnim);
        //                break;
        //            case false:
        //                Animation.Stop(CheckAnim);
        //                CheckAnim = new AnimationGroup();
        //                CheckAnim.Add(new AnimCheck() { Obj = this, StartValue = (RowDefinition.Height.Value / 2) / (RowDefinition.Height.Value / 2 + 1), EndValue = 0, Ease = new AniEaseInFluent(3), TotalTime = TimeSpan.FromMilliseconds(200) });
        //                this.SetResourceReference(ForeColorProperty, "ColorObject1");
        //                Animation.Start(CheckAnim);
        //                break;
        //        }
        //    }
        //}
        //private bool _IsChecked;
        public static readonly RoutedEvent ClickEvent = MyButton.ClickEvent;
        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }
        public static readonly RoutedEvent CheckedEvent = MyRadioButton.CheckedEvent;
        public event RoutedEventHandler Checked
        {
            add => AddHandler(CheckedEvent, value);
            remove => RemoveHandler(CheckedEvent, value);
        }
    }
}
