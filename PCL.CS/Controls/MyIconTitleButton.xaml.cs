using PCL.CS.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PCL.CS.Controls
{
    /// <summary>
    /// 标题栏专用的图标按钮
    /// </summary>
    public partial class MyIconTitleButton : UserControl,IMyButton 
    {
        public static readonly DependencyProperty LogoProperty =
            DependencyProperty.Register("Logo", typeof(string), typeof(MyIconTitleButton),
                new PropertyMetadata("", OnPathChanged));
        public static readonly DependencyProperty LogoScaleProperty =
            DependencyProperty.Register("LogoScale", typeof(double), typeof(MyIconTitleButton),
                new PropertyMetadata(1.0, OnScaleChanged));
        private static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (MyIconTitleButton)d;
            ((ScaleTransform)button.Path.RenderTransform).ScaleX = (double)e.NewValue;
            ((ScaleTransform)button.Path.RenderTransform).ScaleY = (double)e.NewValue;
        }
        public double LogoScale
        {
            get { return (double)GetValue(LogoScaleProperty); }
            set { SetValue(LogoScaleProperty, value); }
        }
        public string Logo
        {
            get { return (string)GetValue(LogoProperty); }
            set { SetValue(LogoProperty, value); }
        }

        // Logo属性变化时的处理
        private static void OnPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (MyIconTitleButton)d;
            button.Path.Data = (Geometry)(new GeometryConverter().ConvertFromString((string)e.NewValue)); ;
        }
        public MyIconTitleButton()
        {
            InitializeComponent();
            PanBack.Background = new SolidColorBrush { Color = Color.FromArgb(50, 255, 255, 255),Opacity=0 };
            Path.Fill = new SolidColorBrush { Color = Color.FromArgb(255, 255, 255, 255) };
        }
        //动画
        private Animation ColorAni;
        private Animation DownAni;
        private bool MouseisIn = false;
        private bool MouseisDown = false;
        private void TitleIcon_MouseIn(object sender, MouseEventArgs e)
        {
            MouseisIn = true;
            Animation.Stop(ColorAni);
            DoubleAnimation BackGroundAni = new DoubleAnimation(
                PanBack.Background,
                SolidColorBrush.OpacityProperty,
                PanBack.Background.Opacity,
                1,
                200,
                0,
                new AniEaseLinear()
            );
            ColorAni = BackGroundAni;
            Animation.Start(ColorAni);
        }
        private void TitleIcon_MouseOut(object sender, MouseEventArgs e)
        {
            MouseisIn = false;
            if (MouseisDown)
            {
                Animation.Stop(DownAni);
                DoubleAnimation AniScaleX = new DoubleAnimation(
                    PanBack.RenderTransform,
                    ScaleTransform.ScaleXProperty,
                    ((ScaleTransform)PanBack.RenderTransform).ScaleX,
                    1.0,
                    400,
                    0,
                    new AniEaseOutFluent(2)
                );
                DoubleAnimation AniScaleY = new DoubleAnimation(
                    PanBack.RenderTransform,
                    ScaleTransform.ScaleYProperty,
                    ((ScaleTransform)PanBack.RenderTransform).ScaleY,
                    1.0,
                    400,
                    0,
                    new AniEaseOutFluent(2)
                );
                DownAni = new AnimationGroup { AniScaleX, AniScaleY };
                //DownAni.Animations = new List<Animation> { AniScaleX, AniScaleY };
                DownAni.TotalTime = TimeSpan.FromMilliseconds(400);
                Animation.Start(DownAni);
            }
            MouseisDown = false;
            Animation.Stop(ColorAni);
            //防止动画时间超出
            DoubleAnimation BackGroundAni = new DoubleAnimation(
                PanBack.Background,
                SolidColorBrush.OpacityProperty,
                PanBack.Background.Opacity,
                0,
                200,
                0,
                new AniEaseLinear()
            );
            ColorAni = BackGroundAni;
            //ColorAni.Animations = new List<Animation> { BackGroundAni };

            Animation.Start(ColorAni);
        }
        private void TitleIcon_MouseDown(object sender, MouseEventArgs e)
        {
            if (!MouseisIn) return;
            MouseisDown = true;
            Animation.Stop(DownAni);
            DoubleAnimation AniScaleX = new DoubleAnimation(
                PanBack.RenderTransform,
                ScaleTransform.ScaleXProperty,
                ((ScaleTransform)PanBack.RenderTransform).ScaleX,
                0.8,
                250,
                0,
                new AniEaseOutFluent(4)
            );
            DoubleAnimation AniScaleY = new DoubleAnimation(
                PanBack.RenderTransform,
                ScaleTransform.ScaleYProperty,
                ((ScaleTransform)PanBack.RenderTransform).ScaleY,
                0.8,
                250,
                0,
                new AniEaseOutFluent(4)
            );
            DownAni = new AnimationGroup { AniScaleX, AniScaleY };
            //DownAni.Animations = new List<Animation> { AniScaleX, AniScaleY };
            DownAni.TotalTime = TimeSpan.FromMilliseconds(250);
            Animation.Start(DownAni);
        }
        private void TitleIcon_MouseUp(object sender, MouseEventArgs e)
        {
            if (!MouseisDown) return;
            MouseisDown = false;
            RaiseEvent(new RoutedEventArgs(ClickEvent));
            Animation.Stop(DownAni);
            DoubleAnimation AniScaleX = new DoubleAnimation(
                PanBack.RenderTransform,
                ScaleTransform.ScaleXProperty,
                ((ScaleTransform)PanBack.RenderTransform).ScaleX,
                1.0,
                400,
                0,
                new AniEaseOutFluent(2)
            );
            DoubleAnimation AniScaleY = new DoubleAnimation(
                PanBack.RenderTransform,
                ScaleTransform.ScaleYProperty,
                ((ScaleTransform)PanBack.RenderTransform).ScaleY,
                1.0,
                400,
                0,
                new AniEaseOutFluent(2)
            );
            DownAni = new AnimationGroup { AniScaleX, AniScaleY };
            //DownAni.Animations = new List<Animation> { AniScaleX, AniScaleY };
            DownAni.TotalTime = TimeSpan.FromMilliseconds(400);
            Animation.Start(DownAni);
        }
        //=====Click=====
        public static readonly RoutedEvent ClickEvent = MyButton.ClickEvent;

        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }
    }
}