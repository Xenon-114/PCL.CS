using PCL.CS.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace PCL.CS.Controls
{
    public class MyComboBox:ComboBox
    {
        public MyComboBox()
        {
            this.DropDownOpened += MyComboBox_DropDownOpened;
            this.Height = 28;

            this.Background = new SolidColorBrush((Color)App.Current.Resources["ColorObjectHalfWhite"]);
            this.BorderBrush = new SolidColorBrush((Color)App.Current.Resources["ColorObjectBg0"]);

            this.ColorAnimLenth = 0;
            SetResourceReference(BackgroundColor, "ColorObjectHalfWhite");
            SetResourceReference(BorderColor, "ColorObjectBg0");
            this.ColorAnimLenth = 150;

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
            this.Loaded += (s, e) =>
            {
                if (this.Items.Count > 0 && this.SelectedIndex == -1)
                    this.SelectedIndex = 0;
            };
        }
        #region 私有属性
        private double ColorAnimLenth = 150;

        public static readonly DependencyProperty BackgroundColor = DependencyProperty.Register("BackgroundColor", typeof(Color), typeof(MyComboBox),
            new PropertyMetadata(Colors.Blue, (d, e) => ((MyComboBox)d).OnBackgroundColorChange((Color)e.NewValue)));
        private AnimationGroup BackgroundColorAnim;
        private void OnBackgroundColorChange(Color NewValue)
        {
            Animation.Stop(BackgroundColorAnim);
            BackgroundColorAnim = new AnimationGroup();
            BackgroundColorAnim.TotalTime = TimeSpan.FromMilliseconds(ColorAnimLenth);
            BackgroundColorAnim.Add(new ColorAnimation(this.Background, SolidColorBrush.ColorProperty, ((SolidColorBrush)this.Background).Color, NewValue, ColorAnimLenth, 0));
            Animation.Start(BackgroundColorAnim);
        }

        public static readonly DependencyProperty BorderColor = DependencyProperty.Register("ForegroundColor", typeof(Color), typeof(MyComboBox),
            new PropertyMetadata(Colors.Blue, (d, e) => ((MyComboBox)d).OnForegroundColorChange((Color)e.NewValue)));
        private AnimationGroup BorderColorAnim;
        private void OnForegroundColorChange(Color NewValue)
        {
            Animation.Stop(BorderColorAnim);
            BorderColorAnim = new AnimationGroup();
            BorderColorAnim.TotalTime = TimeSpan.FromMilliseconds(ColorAnimLenth);
            BorderColorAnim.Add(new ColorAnimation(this.BorderBrush, SolidColorBrush.ColorProperty, ((SolidColorBrush)this.BorderBrush).Color, NewValue, ColorAnimLenth, 0));
            Animation.Start(BorderColorAnim);
        }

        #endregion

        public SolidColorBrush TextBrush { get; } = new SolidColorBrush((Color)App.Current.Resources["ColorObject1"]);
        private void ColorRefresh()
        {
            string BackgroundColorName = null;
            string ForegroundColorName = null;
            if (IsEnabled)
            {
                if(IsDropDownOpen||(IsDropDownOpen&& ((UIElement)Template.FindName("PART_EditableTextBox", this)).IsFocused))
                {
                    this.ColorAnimLenth = 50;
                    BackgroundColorName = "ColorObject7";
                    ForegroundColorName = "ColorObject3";
                }else if (IsMouseOver)
                {
                    this.ColorAnimLenth = 120;
                    ForegroundColorName = "ColorObject4";
                    BackgroundColorName = "ColorObject7";
                }
                else
                {
                    this.ColorAnimLenth = 150;
                    ForegroundColorName = "ColorObjectBg0";
                    BackgroundColorName = "ColorObjectHalfWhite";
                }
            }
            else
            {
                this.ColorAnimLenth = 200;
                ForegroundColorName = "ColorObjectGray5";
                BackgroundColorName = "ColorObjectGray6";
            }
            SetResourceReference(BorderColor, ForegroundColorName);
            SetResourceReference(BackgroundColor, BackgroundColorName);
            this.ColorAnimLenth = 150;
        }
        private Popup thisPopup = null;
        private void MyComboBox_DropDownOpened(object sender, EventArgs e)
        {
            if (thisPopup is null)
                thisPopup = this.GetTemplateChild("PART_Popup") as Popup;
            thisPopup.Width = this.ActualWidth;
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
            if(ColorAnimation is null)
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
