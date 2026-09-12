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
using XeF4Core.WPF;

namespace PCL.CS.Controls
{
    public class MyTextBox : TextBox
    {
        public string Hint
        {
            get => (string)GetValue(HintProperty);
            set => SetValue(HintProperty, value);
        }
        public static readonly DependencyProperty HintProperty = DependencyProperty.Register("Hint", typeof(string), typeof(MyTextBox));



        public Color BorderColorNormal
        {
            get { return (Color)GetValue(BorderColorNormalProperty); }
            set { SetValue(BorderColorNormalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BorderColorNormal.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BorderColorNormalProperty =
            DependencyProperty.Register(nameof(BorderColorNormal), typeof(Color), typeof(MyTextBox));



        public Color BorderColorHighlight
        {
            get { return (Color)GetValue(BorderColorHighlightProperty); }
            set { SetValue(BorderColorHighlightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BorderColorHighlight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BorderColorHighlightProperty =
            DependencyProperty.Register(nameof(BorderColorHighlight), typeof(Color), typeof(MyTextBox));


        public Color BackColorNormal
        {
            get { return (Color)GetValue(BackColorNormalProperty); }
            set { SetValue(BackColorNormalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackColorNormal.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackColorNormalProperty =
            DependencyProperty.Register(nameof(BackColorNormal), typeof(Color), typeof(MyTextBox));


        public Color BackColorSelect
        {
            get { return (Color)GetValue(BackColorSelectProperty); }
            set { SetValue(BackColorSelectProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackColorSelect.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackColorSelectProperty =
            DependencyProperty.Register(nameof(BackColorSelect), typeof(Color), typeof(MyTextBox));


        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BorderColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BorderColorProperty =
            DependencyProperty.Register(nameof(BorderColor), typeof(Color), typeof(MyTextBox));



        public Color BackColor
        {
            get { return (Color)GetValue(BackColorProperty); }
            set { SetValue(BackColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackColorProperty =
            DependencyProperty.Register(nameof(BackColor), typeof(Color), typeof(MyTextBox));

        private readonly ColorMixer BorderColorMixer;
        private readonly ColorMixer BorderFinalMixer;
        private readonly ColorMixer BackColorMixer;

        private TextBlock LabHint = null;
        private Border LabBorder = null;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            LabHint = Template.FindName("PART_Hint", this) as TextBlock;
            if (string.IsNullOrEmpty(this.Text))
                LabHint.Visibility = Visibility.Visible;
            else
                LabHint.Visibility = Visibility.Hidden;
            LabBorder = Template.FindName("PART_Border", this) as Border;
            var borderBrush = new SolidColorBrush();
            borderBrush.SetBinding(SolidColorBrush.ColorProperty, new Binding(nameof(BorderColor)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            var backBrush = new SolidColorBrush();
            backBrush.SetBinding(SolidColorBrush.ColorProperty, new Binding(nameof(BackColor)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            LabBorder.BorderBrush = borderBrush;
            LabBorder.Background = backBrush;
        }
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            if (string.IsNullOrEmpty(Text))
            {
                if (LabHint != null)
                    LabHint.Visibility = Visibility.Visible;
            }
            else
            {
                if (LabHint != null)
                    LabHint.Visibility = Visibility.Hidden;
            }
        }
        public MyTextBox()
        {
            BorderColorMixer = new ColorMixer();
            BorderFinalMixer = new ColorMixer();
            BackColorMixer = new ColorMixer();
            this.IsEnabledChanged += (s, e) => ColorRefresh();
            OnInit();
        }

        private void OnInit()
        {
            BorderColorMixer.SetBinding(ColorMixer.ColorAProperty, new Binding(nameof(BorderColorHighlight)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            BorderColorMixer.SetBinding(ColorMixer.ColorBProperty, new Binding(nameof(BorderColorNormal)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            BorderFinalMixer.SetResourceReference(ColorMixer.ColorAProperty, "ColorObjectGray4");
            BorderColorMixer.ColorARatio = 0;
            BorderFinalMixer.ColorARatio = 0;
            BorderFinalMixer.SetBinding(ColorMixer.ColorBProperty, new Binding(nameof(ColorMixer.ColorResult)) { Source = BorderColorMixer, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            BackColorMixer.SetBinding(ColorMixer.ColorAProperty, new Binding(nameof(BackColorSelect)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            BackColorMixer.SetBinding(ColorMixer.ColorBProperty, new Binding(nameof(BackColorNormal)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            BackColorMixer.ColorARatio = 0;
            this.SetBinding(BorderColorProperty, new Binding(nameof(ColorMixer.ColorResult)) { Source = BorderFinalMixer, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            this.SetBinding(BackColorProperty, new Binding(nameof(ColorMixer.ColorResult)) { Source = BackColorMixer, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            if (!IsEnabled) BorderFinalMixer.ColorARatio = 1;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            ColorRefresh();
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            ColorRefresh();
        }
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            ColorRefresh();
        }
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            ColorRefresh();
        }
        private void ColorRefresh()
        {
            double BorderOpacity;
            short BackColor;
            double IsUnEnabled;
            short IsAnimEnable = 1;
            double TotalTime = 400;
            if (!this.IsEnabled)
            {
                BorderOpacity = 0;
                IsUnEnabled = 1;
                BackColor = 0;
                TotalTime = 300;
            }
            else if (this.IsFocused)
            {
                BorderOpacity = 1.0;
                IsUnEnabled = 0;
                BackColor = 1;
                TotalTime = 100;
            }
            else if (this.IsMouseOver)
            {
                BorderOpacity = 0.9;
                IsUnEnabled = 0;
                BackColor = 1;
                TotalTime = 200;
            }
            else
            {
                BorderOpacity = 0;
                IsUnEnabled = 0;
                BackColor = 0;
                TotalTime = 200;
            }
            if (!this.IsLoaded || !this.IsVisible) IsAnimEnable = 0;

            new DoubleAnimation(BorderColorMixer, ColorMixer.ColorARatioProperty, BorderColorMixer.ColorARatio, BorderOpacity, TotalTime * IsAnimEnable).StartAnimation();
            new DoubleAnimation(BorderFinalMixer, ColorMixer.ColorARatioProperty, BorderFinalMixer.ColorARatio, IsUnEnabled, 300 * IsAnimEnable).StartAnimation();
            new DoubleAnimation(BackColorMixer, ColorMixer.ColorARatioProperty, BackColorMixer.ColorARatio, BackColor, 200 * IsAnimEnable).StartAnimation();
        }
    }
}
