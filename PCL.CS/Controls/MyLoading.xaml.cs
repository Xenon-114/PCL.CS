using PCL.CS.Controls;
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
    /// 一个加载图标（只有图标）
    /// </summary>
    public partial class MyLoading : UserControl
    {
        /// <summary>
        /// 文本
        /// </summary>
        public string Text
        {
            get { return LabText.Text; }
            set { LabText.Text = value; }
        }
        /// <summary>
        /// 文本依赖属性
        /// </summary>
        public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(MyLoading),
            new PropertyMetadata("加载中...", OnTextChanged));
        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MyLoading)d).Text = (string)e.NewValue;
        }
        /// <summary>
        /// 前景笔刷
        /// </summary>
        public new Color Foreground
        {
            get { return (Color)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }
        /// <summary>
        /// 前景笔刷依赖属性
        /// </summary>
        public static new DependencyProperty ForegroundProperty = DependencyProperty.Register("Foreground", typeof(Color), typeof(MyLoading),
            new PropertyMetadata(App.Current.Resources["ColorObject3"], OnForegroundChanged));
        private static void OnForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MyLoading)d).ForegroundChange((Color)e.NewValue);
        }
        private AnimationGroup ForegroundAnim;
        public void ForegroundChange(Color NewValue)
        {
            Animation.Stop(ForegroundAnim);
            ForegroundAnim = new AnimationGroup();
            ForegroundAnim.TotalTime = TimeSpan.FromMilliseconds(300);
            ForegroundAnim.Add(new ColorAnimation(LabText.Foreground, SolidColorBrush.ColorProperty, ((SolidColorBrush)LabText.Foreground).Color, NewValue, 300, 0));
            Animation.Start(ForegroundAnim);
        }
        /// <summary>
        /// 加载状态
        /// </summary>
        public enum MyLoadingState
        {
            /// <summary>
            /// 正在跑
            /// </summary>
            Running = 0,
            /// <summary>
            /// 出了问题
            /// </summary>
            Error = 1
        }
        /// <summary>
        /// 加载图标的状态
        /// </summary>
        public MyLoadingState State
        {
            get { return _State; }
            set
            {
                if (value == _State) return;
                _State = value;
                RefreshState();
            }
        }
        private MyLoadingState _State = MyLoadingState.Running;
        private void RefreshState()
        {
            //这里就要切动画了
            switch (State)
            {
                case MyLoadingState.Running:
                    Animation.Stop(OnErrorAnim);
                    Animation.Stop(RunningWaitAnim);
                    OnErrorAnim = new AnimationGroup
                        {
                            new DoubleAnimation(PathError,OpacityProperty,PathError.Opacity,0,100,0,null),
                            new DoubleAnimation(ErrorScale,ScaleTransform.ScaleXProperty,ErrorScale.ScaleX,0.6,200,0,null),
                            new DoubleAnimation(ErrorScale,ScaleTransform.ScaleYProperty,ErrorScale.ScaleY,0.6,200,0,null),
                        };
                    this.SetResourceReference(ForegroundProperty, "ColorObject3");
                    Animation.Start(new DoubleAnimation(PickaxeRotate, RotateTransform.AngleProperty, PickaxeRotate.Angle, 55, 100, 0, new AniEaseOutFluent(3)));
                    PathLeft.Opacity = 0;
                    PathRight.Opacity = 0;
                    Animation.Start(OnErrorAnim);
                    Animation.Start(OnRunningAnim);
                    break;
                case MyLoadingState.Error:
                    double Wait = 1400 - Animation.GetTime(OnRunningAnim).TotalMilliseconds;
                    if (Wait < 0) Wait = 0;
                    if (Wait > 1250) Wait = 0;
                    Animation.Stop(OnErrorAnim);
                    OnErrorAnim = new AnimationGroup
                        {
                            new DoubleAnimation(PathError,OpacityProperty,PathError.Opacity,1,100,0,null),
                            new DoubleAnimation(ErrorScale,ScaleTransform.ScaleXProperty,ErrorScale.ScaleX,1,200,0,new AniEaseOutBack()),
                            new DoubleAnimation(ErrorScale,ScaleTransform.ScaleYProperty,ErrorScale.ScaleY,1,200,0,new AniEaseOutBack())
                        };
                    this.SetResourceReference(ForegroundProperty, "ColorObjectRedLight");
                    if (RunningWaitAnim is null)
                        RunningWaitAnim = new AnimationGroup();
                    RunningWaitAnim.Clear();
                    RunningWaitAnim.Add(
                        (Modules.Animation)new EventAnimation
                        (
                            action: () =>
                            {
                                Modules.Animation.Start(OnErrorAnim);
                            },
                            After: TimeSpan.FromMilliseconds(Math.Max(Wait - 200, 0))
                        ));
                    RunningWaitAnim.Add(new EventAnimation
                        (
                            action: () =>
                            {
                                Modules.Animation.Stop(OnRunningAnim);
                            },
                            After: TimeSpan.FromMilliseconds(Wait)
                        ));
                    RunningWaitAnim.TotalTime = TimeSpan.FromMilliseconds(Wait);
                    Animation.Start(RunningWaitAnim);
                    break;
            }
        }
        private AnimationGroup OnRunningAnim;
        private AnimationGroup RunningWaitAnim;
        private AnimationGroup OnErrorAnim;
        public MyLoading()
        {
            InitializeComponent();
            LabText.Foreground =
                new SolidColorBrush((Color)App.Current.Resources["ColorObject3"]);
            this.SetResourceReference(ForegroundProperty, "ColorObject3");
            OnRunningAnim = new AnimationGroup
                {
                    new DoubleAnimation(PickaxeRotate,RotateTransform.AngleProperty,55,-20,350,250,new AniEaseInBack(2)),
                    new DoubleAnimation(PickaxeRotate,RotateTransform.AngleProperty,-20,55,900,600,new AniEaseAdd(new AniEaseOutFluent(2),new AniEaseOutElastic(2),2,1)),
                    //石块动画
                    new DoubleAnimation(RockLeft,TranslateTransform.XProperty,0,-5,180,600,new AniEaseOutFluent(2)),
                    new DoubleAnimation(RockLeft,TranslateTransform.YProperty,0,-6,180,600,new AniEaseOutFluent(2)),
                    new DoubleAnimation(PathLeft,Path.OpacityProperty,1,0,100,600),
                    new DoubleAnimation(RockRight,TranslateTransform.XProperty,0,5,180,600,new AniEaseOutFluent(2)),
                    new DoubleAnimation(RockRight,TranslateTransform.YProperty,0,-6,180,600,new AniEaseOutFluent(2)),
                    new DoubleAnimation(PathRight,Path.OpacityProperty,1,0,100,600)
                };
            OnRunningAnim.TotalTime = TimeSpan.FromMilliseconds(1500);
            OnRunningAnim.Repeat = -1;
            this.Loaded += MyLoading_Loaded;
            this.Unloaded += MyLoading_Unloaded;
        }

        private void MyLoading_Unloaded(object sender, RoutedEventArgs e)
        {
            Animation.Stop(OnRunningAnim);
        }

        private void MyLoading_Loaded(object sender, RoutedEventArgs e)
        {
            Animation.Stop(OnRunningAnim);
            //this.Loaded -= MyLoading_Loaded;
            PickaxeRotate.Angle = 55;
            PathLeft.Opacity = 0;
            PathRight.Opacity = 0;
            RefreshState();
        }
    }
}
