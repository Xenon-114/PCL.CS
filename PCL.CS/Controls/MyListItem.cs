using PCL.CS.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

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

        public MyListItem()
        { }

        public MyRadioGroup RadioGroup
        {
            get => _RadioGroup;
            set
            {
                if (_RadioGroup == value) return;
                if (_RadioGroup != null)
                {
                    _RadioGroup.RemoveChild(this);
                }
                if (value != null)
                {
                    _RadioGroup.AddChild(this);
                }
                _RadioGroup = value;
            }
        }
        private MyRadioGroup _RadioGroup;

        protected virtual double RectBackCornerRadius { get => 6; }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            BackgroundBorder = Template.FindName("PART_Background", this) as Border;
            MainElement = Template.FindName("PART_Main", this) as UIElement;
            RowDefinition = Template.FindName("PART_RowDef", this) as RowDefinition;
            BackgroundBorder.RenderTransformOrigin = new Point(0.5, 0.5);
            BackgroundBorder.RenderTransform = AnimScale;
            MainElement.RenderTransform = MainScale;
            MainElement.RenderTransformOrigin = new Point(0.5, 0.5);
            BackgroundBorder.Opacity = 0;
            this.MouseEnter += MyListItem_MouseEnter;
            this.MouseLeave += MyListItem_MouseLeave;
            this.Background = new SolidColorBrush((Color)App.Current.Resources["ColorObject6"]);
            //BackgroundBorder.SetResourceReference(Border.BorderBrushProperty, "ColorBrushBg0");
            BackgroundBorder.BorderThickness = new Thickness(0.3);
            BackgroundBorder.CornerRadius = new CornerRadius(RectBackCornerRadius);
            this.MouseLeftButtonDown += MyListItem_MouseDown;
            this.MouseLeftButtonUp += MyListItem_MouseUp;
            this.SetResourceReference(BackColorProperty, "ColorObject7");
            if (IsChecked) RowDefinition.Height = new GridLength(6, GridUnitType.Star);
            else RowDefinition.Height = new GridLength(0, GridUnitType.Star);
            AnimScale.ScaleX = AnimScale.ScaleY = DefaultAnimScale;
            SolidColorBrush Fore = new SolidColorBrush();
            this.Foreground = Fore;
            Fore.Color = (Color)(IsChecked ? App.Current.FindResource("ColorObject3") : App.Current.FindResource("ColorObject1"));
            this.SetResourceReference(ForeColorProperty, IsChecked ? "ColorObject4" : "ColorObject1");
        }
        //private AnimationGroup AnimationDownUp;

        private bool IsMouseDown { get; set; }
        private void MyListItem_MouseUp(object sender, EventArgs e)
        {
            if (IsMouseDown) RaiseEvent();
            IsMouseDown = false;
            AnimationRefresh();
        }
        
        private void MyListItem_MouseDown(object sender, EventArgs e)
        {
            IsMouseDown = true;
            AnimationRefresh();
        }

        private static readonly DependencyProperty BackColorProperty = DependencyProperty.Register("BackColor", typeof(Color), typeof(MyListItem),
            new PropertyMetadata((d, e) => (d as MyListItem).OnBackColorChanged((Color)e.NewValue)));
        private void OnBackColorChanged(Color NewValue)
        {
            Animation.Start(new ColorAnimation(this.Background, SolidColorBrush.ColorProperty, (Background as SolidColorBrush).Color, NewValue, 200, 0));
        }

        private void MyListItem_MouseEnter(object sender, MouseEventArgs e)
        {
            AnimationRefresh();
        }

        private void MyListItem_MouseLeave(object sender, MouseEventArgs e)
        {
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
                Animation.Stop(InOutAnimation);
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
                    this.SetResourceReference(BackColorProperty, "ColorObject5");
                }
                else
                {
                    MouseDownAnim.Add(new DoubleAnimation(MainScale, ScaleTransform.ScaleXProperty, MainScale.ScaleX, 1.0, 400, 0, new AniEaseOutFluent(2)));
                    MouseDownAnim.Add(new DoubleAnimation(MainScale, ScaleTransform.ScaleYProperty, MainScale.ScaleY, 1.0, 400, 0, new AniEaseOutFluent(2)));
                    this.SetResourceReference(BackColorProperty, "ColorObject6");
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
        
        private class AnimCheck : Animation
        {
            public double StartValue { get; set; }
            public double EndValue { get; set; }
            public MyListItem Obj { get; set; }
            public AniEase Ease { get; set; }
            public override object GetValue(double t)
            {
                return (EndValue - StartValue) * Ease.GetValue(t) + StartValue;
            }
            public override void SetValue(object value)
            {
                double Value = (double)value;
                if (Math.Abs(Value - 1) < 0.05) return;
                double ActuV = Value / (1 - Value) * 2;
                GridLength gridLength = new GridLength(ActuV, GridUnitType.Star);
                Obj.RowDefinition.Height = gridLength;
            }
        }


        private static readonly DependencyProperty ForeColorProperty = DependencyProperty.Register("ForeColor", typeof(Color), typeof(MyListItem),
            new PropertyMetadata((d, e) => (d as MyListItem).ChangeForeColor((Color)e.NewValue)));

        private void ChangeForeColor(Color NewValue)
        {
            Animation.Start(new ColorAnimation(this.Foreground, SolidColorBrush.ColorProperty, (Color)this.Foreground.GetValue(SolidColorBrush.ColorProperty), NewValue, 200));
        }


        private AnimationGroup CheckAnim { get; set; }
        public bool AutoCheck { get; set; } = false;

        private static readonly AniEase EaseInOutSine = new AniEaseInOut(new AniEaseInSine(), new AniEaseOutSine());
        public bool IsChecked
        {
            get => _IsChecked;
            set
            {
                if(RowDefinition is null||!this.IsLoaded)
                {
                    _IsChecked = value;
                    return;
                }
                if (value && !_IsChecked)
                {
                    RaiseEvent(new RoutedEventArgs(CheckedEvent));
                }
                if (_IsChecked == value) return;
                _IsChecked = value;
                switch (value)
                {
                    case true:
                        Animation.Stop(CheckAnim);
                        CheckAnim = new AnimationGroup();
                        CheckAnim.Add(new AnimCheck() { Obj = this, StartValue = (RowDefinition.Height.Value / 2) / (RowDefinition.Height.Value / 2 + 1), EndValue = 0.8, Ease = new AniEaseOutFluent(3), TotalTime = TimeSpan.FromMilliseconds(150) });
                        CheckAnim.Add(new AnimCheck() { Obj = this, StartValue = 0.8, EndValue = 0.75, Ease = EaseInOutSine, TotalTime = TimeSpan.FromMilliseconds(50), After = TimeSpan.FromMilliseconds(150) });
                        this.SetResourceReference(ForeColorProperty, "ColorObject3");
                        Animation.Start(CheckAnim);
                        break;
                    case false:
                        Animation.Stop(CheckAnim);
                        CheckAnim = new AnimationGroup();
                        CheckAnim.Add(new AnimCheck() { Obj = this, StartValue = (RowDefinition.Height.Value / 2) / (RowDefinition.Height.Value / 2 + 1), EndValue = 0, Ease = new AniEaseInFluent(3), TotalTime = TimeSpan.FromMilliseconds(200) });
                        this.SetResourceReference(ForeColorProperty, "ColorObject1");
                        Animation.Start(CheckAnim);
                        break;
                }
            }
        }
        private bool _IsChecked;
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
