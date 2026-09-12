using PCL.CS.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using XeF4Core.WPF;

namespace PCL.CS.Controls
{
    public class MyComboBox : ComboBox
    {


        public string Hint
        {
            get { return (string)GetValue(HintProperty); }
            set { SetValue(HintProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Hint.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HintProperty =
            DependencyProperty.Register(nameof(Hint), typeof(string), typeof(MyComboBox), new PropertyMetadata(string.Empty));
        public Color BorderColorNormal
        {
            get { return (Color)GetValue(BorderColorNormalProperty); }
            set { SetValue(BorderColorNormalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BorderColorNormal.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BorderColorNormalProperty =
            DependencyProperty.Register(nameof(BorderColorNormal), typeof(Color), typeof(MyComboBox));



        public Color BorderColorHighlight
        {
            get { return (Color)GetValue(BorderColorHighlightProperty); }
            set { SetValue(BorderColorHighlightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BorderColorHighlight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BorderColorHighlightProperty =
            DependencyProperty.Register(nameof(BorderColorHighlight), typeof(Color), typeof(MyComboBox));


        public Color BackColorNormal
        {
            get { return (Color)GetValue(BackColorNormalProperty); }
            set { SetValue(BackColorNormalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackColorNormal.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackColorNormalProperty =
            DependencyProperty.Register(nameof(BackColorNormal), typeof(Color), typeof(MyComboBox));


        public Color BackColorSelect
        {
            get { return (Color)GetValue(BackColorSelectProperty); }
            set { SetValue(BackColorSelectProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackColorSelect.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackColorSelectProperty =
            DependencyProperty.Register(nameof(BackColorSelect), typeof(Color), typeof(MyComboBox));


        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BorderColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BorderColorProperty =
            DependencyProperty.Register(nameof(BorderColor), typeof(Color), typeof(MyComboBox));



        public Color BackColor
        {
            get { return (Color)GetValue(BackColorProperty); }
            set { SetValue(BackColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackColorProperty =
            DependencyProperty.Register(nameof(BackColor), typeof(Color), typeof(MyComboBox));



        public Brush PopupBackground
        {
            get { return (Brush)GetValue(PopupBackgroundProperty); }
            set { SetValue(PopupBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PopupBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PopupBackgroundProperty =
            DependencyProperty.Register(nameof(PopupBackground), typeof(Brush), typeof(MyComboBox));



        private readonly ColorMixer BorderColorMixer;
        private readonly ColorMixer BorderFinalMixer;
        private readonly ColorMixer BackColorMixer;

        public MyComboBox()
        {
            this.DropDownOpened += MyComboBox_DropDownOpened;
            this.Height = 28;

            BorderColorMixer = new ColorMixer();
            BorderFinalMixer = new ColorMixer();
            BackColorMixer = new ColorMixer();

            this.IsEnabledChanged += (s, e) => ColorRefresh();

            OnInit();

            this.IsEnabledChanged += (s, e) =>
            {
                this.ColorRefresh();
            };
            this.MouseEnter += (s, e) =>
            {
                this.ColorRefresh();
            };
            this.MouseLeave += (s, e) =>
            {
                this.ColorRefresh();
            };
            this.GotKeyboardFocus += (s, e) =>
            {
                this.ColorRefresh();
            };
            this.LostKeyboardFocus += (s, e) => this.ColorRefresh();
            this.GotFocus += (s, e) => this.ColorRefresh();
            this.LostFocus += (s, e) => this.ColorRefresh();
            this.Loaded += (s, e) =>
            {
                if (this.Items.Count > 0 && this.SelectedIndex == -1)
                    this.SelectedIndex = 0;
            };
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
        private Border LabBorder;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            LabBorder = Template.FindName("PART_Border", this) as Border;
            var borderBrush = new SolidColorBrush();
            borderBrush.SetBinding(SolidColorBrush.ColorProperty, new Binding(nameof(BorderColor)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            var backBrush = new SolidColorBrush();
            backBrush.SetBinding(SolidColorBrush.ColorProperty, new Binding(nameof(BackColor)) { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.OneWay });
            LabBorder.BorderBrush = borderBrush;
            LabBorder.Background = backBrush;
        }
        private void MyComboBox_DropDownOpened(object sender, EventArgs e)
        {
            if (thisPopup is null)
                thisPopup = this.GetTemplateChild("PART_Popup") as Popup;
            thisPopup.Width = this.ActualWidth;
        }
        private Popup thisPopup = null;
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.Focus();
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
            else if (IsDropDownOpen || ((UIElement)Template.FindName("PART_EditableTextBox", this)).IsFocused)
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
    public class MyComboBoxItem : ComboBoxItem
    {
        public Color ForegroundColor
        {
            get { return (Color)GetValue(ForegroundColorProperty); }
            set { SetValue(ForegroundColorProperty, value); }
        }
        public static readonly DependencyProperty ForegroundColorProperty = DependencyProperty.Register("Foreground", typeof(Color), typeof(MyComboBoxItem),
            new PropertyMetadata(new Color(), (d, e) => ((MyComboBoxItem)d).ForegroundChanged((Color)e.NewValue)));
        private double ColorAnimLenth = 200;
        private AnimationGroup ColorAnimation = null;
        private void ForegroundChanged(Color NewValue)
        {
            Animation.Stop(ColorAnimation);
            if (ColorAnimation is null)
                ColorAnimation = new AnimationGroup();
            else
                ColorAnimation.Clear();
            ColorAnimation.Add(new ColorAnimation(this.Background, SolidColorBrush.ColorProperty, ((SolidColorBrush)this.Background).Color, NewValue, ColorAnimLenth, 0));
            ColorAnimation.TotalTime = TimeSpan.FromMilliseconds(ColorAnimLenth);
            Animation.Start(ColorAnimation);
        }
        public MyComboBoxItem()
        {
            this.Background = new SolidColorBrush((Color)App.Current.Resources["ColorObjectTransparent"]);

            SetResourceReference(ForegroundColorProperty, "ColorObjectTransparent");

            this.Selected += (s, e) => ColorRefresh();
            this.Unselected += (s, e) => ColorRefresh();
            this.MouseEnter += (s, e) => ColorRefresh();
            this.MouseMove += (s, e) => ColorRefresh();
            this.MouseLeave += (s, e) => ColorRefresh();
            this.IsEnabledChanged += (s, e) => ColorRefresh();
        }
        private void ColorRefresh()
        {
            string newBackColorName;
            int time;

            if (IsSelected)
            {
                newBackColorName = "ColorObject6";
                time = 100;
            }
            else if (IsMouseOver)
            {
                newBackColorName = "ColorObject8";
                time = 100;
            }
            else if (IsEnabled)
            {
                newBackColorName = "ColorObjectTransparent";
                time = 300;
            }
            else
            {
                newBackColorName = "ColorObjectGray5";
                time = 300;
            }


            // 触发颜色动画
            if (IsVisible) // 防止默认属性变更触发动画
            {
                // 有动画
                ColorAnimLenth = time;
                SetResourceReference(ForegroundColorProperty, newBackColorName);
            }
            else
            {
                // 无动画
                time = 0;
                SetResourceReference(ForegroundColorProperty, newBackColorName);
            }
            time = 200;
        }
    }

}
