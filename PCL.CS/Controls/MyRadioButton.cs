using PCL.CS.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PCL.CS.Controls
{
    public class MyRadioButton : Control, IMyButton, IMyRadio
    {
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(MyRadioButton));
        public Geometry Logo
        {
            get => (Geometry)GetValue(LogoProperty);
            set => SetValue(LogoProperty, value);
        }
        public static readonly DependencyProperty LogoProperty = DependencyProperty.Register("Logo", typeof(Geometry), typeof(MyRadioButton));
        public double LogoScale
        {
            get => (double)GetValue(LogoScaleProperty);
            set => SetValue(LogoScaleProperty, value);
        }
        public static readonly DependencyProperty LogoScaleProperty = DependencyProperty.Register("LogoScale", typeof(double), typeof(MyRadioButton));

        private double ColorAnimLenth = 300;
        private static readonly DependencyProperty BackColor = DependencyProperty.Register("BackColor", typeof(Color), typeof(MyRadioButton),
            new PropertyMetadata((d, e) => (d as MyRadioButton).BackColorChange((Color)e.NewValue)));

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
        private static readonly DependencyProperty ForeColor = DependencyProperty.Register("ForeColor", typeof(Color), typeof(MyRadioButton),
            new PropertyMetadata((d, e) => (d as MyRadioButton).ForeColorChange((Color)e.NewValue)));
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
            White,
            Color
        }
        public ColorState ColorType
        {
            get;
            set;
        }

        private string GetColor(bool IsBackground, bool IsChecked)
        {
            if (IsBackground)
            {
                return ColorType is ColorState.Color ? "ColorObject3" : "ColorObjectWhite";
            }
            if (IsChecked)
            {
                switch (ColorType)
                {
                    case ColorState.Color:
                        return IsBackground ? "ColorObject3" : "ColorObjectWhite";
                    case ColorState.White:
                        return IsBackground ? "ColorObjectWhite" : "ColorObject3";
                    default:
                        return "";
                }
            }
            else
            {
                switch (ColorType)
                {
                    case ColorState.Color:
                        return "ColorObject3";
                    case ColorState.White:
                        return "ColorObjectWhite";
                    default:
                        return "";
                }
            }
        }

        public MyRadioButton()
        {
            this.Loaded += (s, e) =>
            {
                this.Background = new SolidColorBrush((Color)App.Current.Resources[GetColor(true, IsChecked)]);
                this.Foreground = new SolidColorBrush((Color)App.Current.Resources[GetColor(false, IsChecked)]);
                this.SetResourceReference(BackColor, GetColor(true, IsChecked));
                this.SetResourceReference(ForeColor, GetColor(false, IsChecked));
                this.Background.Opacity = this.IsChecked ? 1 : 0;
            };
            this.MouseEnter += (s, e) => ColorUpdate();
            this.MouseLeave += (s, e) =>
            {
                this.IsMouseDown = false;
                ColorUpdate();
            };
            this.MouseLeftButtonDown += (s, e) =>
            {
                this.IsMouseDown = true;
                ColorUpdate();
            };
            this.MouseLeftButtonUp += (s, e) =>
            {
                if (!this.IsMouseDown) return;
                this.IsMouseDown = false;
                RaiseEvent(new RoutedEventArgs(ClickEvent));
                if (AutoChosen) this.IsChecked = true;
                ColorUpdate();
            };
        }
        private bool IsMouseDown = false;
        private void ColorUpdate()
        {
            if (!this.IsLoaded) return;
            string BackColorName;
            string ForeColorName;
            double TargetOpa;
            if (this.IsChecked)
            {
                BackColorName = GetColor(true, true);
                ForeColorName = GetColor(false, true);
                TargetOpa = 1.0;
                ColorAnimLenth = 120;
            }
            else if (this.IsMouseDown)
            {
                BackColorName = GetColor(true, false);
                ForeColorName = GetColor(false, false);
                TargetOpa = 0.5;
                ColorAnimLenth = 120;
            }
            else if (this.IsMouseOver)
            {
                BackColorName = GetColor(true, false);
                ForeColorName = GetColor(false, false);
                TargetOpa = 0.2;
                ColorAnimLenth = 90;
            }
            else
            {
                BackColorName = GetColor(true, false);
                ForeColorName = GetColor(false, false);
                TargetOpa = 0;
                ColorAnimLenth = 150;
            }
            if (!this.IsVisible) ColorAnimLenth = 0;
            this.SetResourceReference(ForeColor, ForeColorName);
            this.SetResourceReference(BackColor, BackColorName);
            this.OpacTo(TargetOpa, ColorAnimLenth);
            ColorAnimLenth = 300;
        }
        private AnimationGroup OpacAnim;
        private void OpacTo(double Value, double Time)
        {
            Animation.Stop(OpacAnim);
            if (!this.IsLoaded) return;
            if (!this.IsVisible)
            {
                this.Background.Opacity = Value;
            }
            OpacAnim = new AnimationGroup();
            OpacAnim.TotalTime = TimeSpan.FromMilliseconds(Time);
            OpacAnim.Add(new DoubleAnimation(this.Background, Brush.OpacityProperty, this.Background.Opacity, Value, Time, 0));
            Animation.Start(OpacAnim);
        }

        public bool AutoChosen { get; set; } = false;

        public bool IsChecked
        {
            get => _Checked;
            set
            {
                if (value)
                {
                    RadioGroup.SelectedItem = this;
                }
                _Checked = value;
                if (value) RaiseEvent(new RoutedEventArgs(CheckedEvent));
                if (!this.IsLoaded || this.Background is null || this.Foreground is null) return;
                ColorUpdate();
            }
        }
        private bool _Checked = false;


        public static readonly RoutedEvent CheckedEvent = RadioButton.CheckedEvent;
        public event RoutedEventHandler Checked
        {
            add => AddHandler(CheckedEvent, value);
            remove => RemoveHandler(CheckedEvent, value);
        }
        public static readonly RoutedEvent ClickEvent = MyButton.ClickEvent;
        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

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
        private MyRadioGroup _RadioGroup = new MyRadioGroup();
    }
}
