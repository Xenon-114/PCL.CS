using PCL.CS.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

        private TextBlock LabHint = null;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            LabHint = Template.FindName("labHint", this) as TextBlock;
            if (this.Text == "" || this.Text is null)
                LabHint.Visibility = Visibility.Visible;
            else
                LabHint.Visibility = Visibility.Hidden;
            
        }
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            if (this.Text == "" || this.Text is null)
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
            BorderBrush = new SolidColorBrush((Color)App.Current.Resources["ColorObject3"]) { Opacity = 0.6 };
            this.Loaded += (s, e) =>
            {
                BorderBrush.Opacity = 0.6;
            };
            this.GotFocus += (s, e) => ColorRefresh();
            this.LostFocus += (s, e) => ColorRefresh();
            this.MouseEnter += (s, e) => ColorRefresh();
            this.MouseLeave += (s, e) => ColorRefresh();
            this.IsEnabledChanged += (s, e) => ColorRefresh();
        }
        private Animation OpacityAnimation;
        private void ColorRefresh()
        {
            double BorderOpacity;
            double TotalTime = 400;
            if (!this.IsEnabled)
            {
                BorderOpacity = 0.4;
                TotalTime = 300;
            }
            else if (this.IsFocused)
            {
                BorderOpacity = 1.0;
                TotalTime = 100;
            }
            else if (this.IsMouseOver)
            {
                BorderOpacity = 0.9;
                TotalTime = 200;
            }
            else
            {
                BorderOpacity = 0.6;
                TotalTime = 200;
            }
            Animation.Stop(OpacityAnimation);
            OpacityAnimation = new DoubleAnimation(this.BorderBrush, Brush.OpacityProperty, this.BorderBrush.Opacity, BorderOpacity, TotalTime, 0);
            Animation.Start(OpacityAnimation);
        }
    }
}
