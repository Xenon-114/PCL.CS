using PCL.CS.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace PCL.CS.Controls
{
    public class MyScrollBar:ScrollBar
    {
        //public static readonly RoutedEvent DragStartedEvent = EventManager.RegisterRoutedEvent(
        //    "DragStarted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MyScrollBar));

        //public static readonly RoutedEvent DragCompletedEvent = EventManager.RegisterRoutedEvent(
        //    "DragCompleted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MyScrollBar));

        // CLR 事件包装器
        //public event RoutedEventHandler DragStarted
        //{
        //    add { AddHandler(DragStartedEvent, value); }
        //    remove { RemoveHandler(DragStartedEvent, value); }
        //}

        //public event RoutedEventHandler DragCompleted
        //{
        //    add { AddHandler(DragCompletedEvent, value); }
        //    remove { RemoveHandler(DragCompletedEvent, value); }
        //}
        public MyScrollBar()
        {
            this.Loaded += (s, e) =>
            {
                MyScrollBar_ColorRefresh();
            };
            this.IsEnabledChanged += (s, e) =>
            {
                MyScrollBar_ColorRefresh();
            };
            this.GotMouseCapture += (s, e)=>
            {
                MyScrollBar_ColorRefresh();
            };
            this.LostMouseCapture += (s, e) =>
            {
                MyScrollBar_ColorRefresh();
            };
            this.MouseEnter += (s, e) =>
            {
                MyScrollBar_ColorRefresh();
            };
            this.MouseLeave += (s, e) =>
            {
                MyScrollBar_ColorRefresh();
            };
            this.IsVisibleChanged += (s, e) =>
            {
                MyScrollBar_ColorRefresh();
            };
            this.Foreground = new SolidColorBrush((Color)App.Current.Resources["ColorObject4"]) { Opacity = 1 };
            this.Opacity = 0.5;
            BarAnimTime = 0;
            SetValue(BarColor, App.Current.Resources["ColorObject4"]);
            BarAnimTime = 150;
            this.Loaded += MyScrollBar_Loaded;
            MyScrollBar_ColorRefresh();
        }
        private void MyScrollBar_Loaded(object sender, RoutedEventArgs e)
        {
            MyScrollBar_ColorRefresh();
        }


        private void MyScrollBar_ColorRefresh()
        {
            string ColorKey;
            double Opac;
            if (!IsVisible)
            {
                BarAnimTime = 0;
                ColorKey = "ColorObject4";
                Opac = 0;
            }else if (IsMouseCaptureWithin)
            {
                BarAnimTime = 100;
                ColorKey = "ColorObject4";
                Opac = 1;
            }else if ( IsMouseOver)
            {
                BarAnimTime = 130;
                ColorKey = "ColorObject3";
                Opac = 0.9;
            }
            else
            {
                BarAnimTime = 180;
                ColorKey = "ColorObject4";
                Opac = 0.5;
            }
            this.SetResourceReference(BarColor, ColorKey);
            AaOpac(Opac);
            BarAnimTime = 150;
        }

        private static readonly DependencyProperty BarColor = DependencyProperty.Register("BarColor", typeof(Color), typeof(MyScrollBar),
            new PropertyMetadata(new Color(), (DependencyObject d, DependencyPropertyChangedEventArgs e) => ((MyScrollBar)d).OnBarColorChange((Color)e.NewValue)));
        private AnimationGroup BarColorAnim;
        private double BarAnimTime = 150;
        private void OnBarColorChange(Color NewValue)
        {
            Animation.Stop(BarColorAnim);
            BarColorAnim = new AnimationGroup();
            BarColorAnim.TotalTime = TimeSpan.FromMilliseconds(BarAnimTime);
            BarColorAnim.Add(new ColorAnimation(this.Foreground, SolidColorBrush.ColorProperty, ((SolidColorBrush)this.Foreground).Color, NewValue, BarAnimTime, 0));
            Animation.Start(BarColorAnim);
        }
        private AnimationGroup BarOpacAnim;
        private void AaOpac(double NewValue)
        {
            Animation.Stop(BarOpacAnim);
            BarOpacAnim = new AnimationGroup();
            BarOpacAnim.TotalTime = TimeSpan.FromMilliseconds(BarAnimTime);
            BarOpacAnim.Add(new DoubleAnimation(this, ScrollBar.OpacityProperty, this.Opacity, NewValue, BarAnimTime, 0));
            Animation.Start(BarOpacAnim);
        }
    }
}
