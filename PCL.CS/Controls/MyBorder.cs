using PCL.CS.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PCL.CS.Controls
{
    public class MyBorder : ContentControl
    {
        private double AnimLenth = 150;
        private static DependencyProperty BorderColorTg = DependencyProperty.Register("BorderColorTg", typeof(Color), typeof(MyBorder),
            new PropertyMetadata((d, e) => (d as MyBorder).DoColorAnim((Color)e.NewValue)));
        private AnimationGroup BorderColorAnim;
        private void DoColorAnim(Color NewValue)
        {
            Animation.Stop(BorderColorAnim);
            BorderColorAnim = new AnimationGroup();
            BorderColorAnim.TotalTime = TimeSpan.FromMilliseconds(AnimLenth);
            BorderColorAnim.Add(new ColorAnimation(this, BorderColorProperty, this.BorderColor, NewValue, AnimLenth, 0));
            Animation.Start(BorderColorAnim);
        }
        public Color BorderColor
        {
            get => (Color)GetValue(BorderColorProperty);
        }
        public static readonly DependencyProperty BorderColorProperty = DependencyProperty.Register("BorderColor", typeof(Color), typeof(MyBorder));
        public double ChromeOpacity
        {
            get => (double)GetValue(ChromeOpacityProperty);
            set => SetValue(ChromeOpacityProperty, value);
        }
        public static readonly DependencyProperty ChromeOpacityProperty = DependencyProperty.Register("ChromeOpacity", typeof(double), typeof(MyBorder));
        public MyBorder()
        {
            this.Background = new SolidColorBrush(Color.FromArgb(245, 255, 255, 255));
            this.Loaded += (s, e) => ThisLoad();
        }
        public MyBorder(object Content)
        {
            this.Content = Content;
            this.Background = new SolidColorBrush(Color.FromArgb(245, 255, 255, 255));
            this.Loaded += (s, e) => ThisLoad();
        }
        private void ThisLoad()
        {
            this.SetResourceReference(BorderColorTg, "ColorObject1");
            this.ChromeOpacity = 0.07;
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var MainBorder = GetTemplateChild("PART_MainBorder") as Border;
            MainBorder.MouseEnter += (s, e) => MouseIn();
            MainBorder.MouseLeave += (s, e) => MouseOut();
            
        }
        private AnimationGroup ColorAnim;
        private void MouseIn()
        {
            Animation.Stop(ColorAnim);
            ColorAnim = new AnimationGroup();
            ColorAnim.TotalTime = TimeSpan.FromMilliseconds(AnimLenth);
            ColorAnim.Add(new DoubleAnimation(this, ChromeOpacityProperty, this.ChromeOpacity, 0.4, AnimLenth, 0));
            Animation.Start(ColorAnim);
            this.SetResourceReference(BorderColorTg, "ColorObject4");
        }
        private void MouseOut()
        {
            Animation.Stop(ColorAnim);
            ColorAnim = new AnimationGroup();
            ColorAnim.TotalTime = TimeSpan.FromMilliseconds(AnimLenth);
            ColorAnim.Add(new DoubleAnimation(this, ChromeOpacityProperty, this.ChromeOpacity, 0.07, AnimLenth, 0));
            Animation.Start(ColorAnim);
            this.SetResourceReference(BorderColorTg, "ColorObject1");
        }
    }
}
