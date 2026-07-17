using PCL.CS.Controls;
using PCL.CS.Modules;
using PCL.CS.Pages;
using XeF4Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using nint = System.IntPtr;

namespace PCL.CS
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Currect
        {
            get => CurrectWindow;
        }
        private static MainWindow CurrectWindow = null;

        public static MyMsgBox CurrectMsgBox { get; } = new MyMsgBox();

        private ScaleTransform MainScale = new ScaleTransform();
        private RotateTransform MainRotate = new RotateTransform();
        private TranslateTransform MainTranslate = new TranslateTransform();

        public static TaskCompletionSource<object> LoadedTask { get; } = new TaskCompletionSource<object>();

        private UIElement PageLeft
        {
            get => PgLeftBorder.Child;
            set => PgLeftBorder.Child = value;
        }
        private UIElement PageRight
        {
            get => PgRightBorder.Child;
            set => PgRightBorder.Child = value;
        }
        public MainWindow()
        {
            InitializeComponent();
            CurrectWindow = this;
            MWindow.TitleBarDrag += (s, e) => DragMove();
            PanMsg.MouseLeftButtonDown += (s, e) =>
            {
                if (PanMsg.IsMouseDirectlyOver) DragMove();
            };
            this.PanMsg.Child = CurrectMsgBox;
            this.Loaded += MainWindow_Loaded;
            PgLeftBorder.SizeChanged += PgLeftAnim;
            BtnTitleClose.Click += MainWindow_Closed;
            MainGrid.RenderTransformOrigin = new Point(0.5, 0.5);

            this.SizeChanged += (s, e) =>
            {
                if (WindowState == WindowState.Maximized)
                    WindowState = WindowState.Normal;
            };
            BtnTitleMin.Click += (s, e) =>
            {
                this.WindowState = WindowState.Minimized;
            };
            MainGrid.RenderTransform = new TransformGroup
            {
                Children =
                {
                    MainTranslate,
                    MainRotate,
                    MainScale
                }
            };
            MainGrid.Opacity = 0;

            {
                PageLeft = PagesContent.Pages[0].PageLeft;
                PageRight = PagesContent.Pages[0].PageRight[0];
                PgLeftBorder.ClipToBounds = true;
                PgRightBorder.ClipToBounds = true;
                Base.Log($"[Loading]初始化页面完成");
            }
            
            RadioStackMain.SelectIndexChanged += RadioStackMain_SelectedIndexChanged;
            PanTitleLeft.Back += PanTitleLeft_Back;

            //MainWindow.CurrectMsgBox = this.MsgBox;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (IsShowingMessage) CurrectMsgBox.OnThisKeyDown(e.Key);
        }
        private void PgLeftAnim(object s,SizeChangedEventArgs e)
        {
            if (!this.IsLoaded) return;
            if (e.NewSize.Width == e.PreviousSize.Width) return;
            Animation.Start(new DoubleAnimation(MWindow, MyMainWindow.PageLeftBackWidthProperty, MWindow.PageLeftBackWidth, e.NewSize.Width, 180, 0, new AniEaseOutFluent(4)));
        }
        private AnimationGroup MainWindowAnim;
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Animation.Stop(MainWindowAnim);
            MainWindowAnim = new AnimationGroup();
            MainWindowAnim.TotalTime = TimeSpan.FromMilliseconds(600);
            MainWindowAnim.Add(new DoubleAnimation(MainRotate, RotateTransform.AngleProperty, -4, 0, 500, 0, new AniEaseOutBack(2)));
            MainWindowAnim.Add(new DoubleAnimation(MainTranslate, TranslateTransform.YProperty, 60, 0,600, 0, new AniEaseOutBack(2)));
            MainWindowAnim.Add(new DoubleAnimation(MainGrid, Grid.OpacityProperty, 0, 1, 250, 0));
            //MainGrid.Opacity = 1;
            Animation.Start(MainWindowAnim);
            MWindow.PageLeftBackWidth = PgLeftBorder.ActualWidth;
            LoadedTask.SetResult(null);
        }
        private void MainWindow_Closed(object sender,RoutedEventArgs e)
        {
            this.IsHitTestVisible = false;
            Animation.Stop(MainWindowAnim);
            DoubleAnimation OutRenderAnim = new DoubleAnimation(MainRotate, RotateTransform.AngleProperty, MainRotate.Angle, MainRotate.Angle + 0.6, 180, 0, new AniEaseOutFluent(2));
            DoubleAnimation OutSizeXAnim = new DoubleAnimation(MainScale, ScaleTransform.ScaleXProperty, MainScale.ScaleX, MainScale.ScaleX * 0.88, 180, 0, new AniEaseOutFluent(2));
            DoubleAnimation OutSizeYAnim = new DoubleAnimation(MainScale, ScaleTransform.ScaleYProperty, MainScale.ScaleY, MainScale.ScaleY * 0.88, 180, 0, new AniEaseOutFluent(2));
            DoubleAnimation OutOpacityAnim = new DoubleAnimation(MainGrid, OpacityProperty, MainGrid.Opacity, 0, 180, 0, new AniEaseOutFluent(2));
            EventAnimation Event = new EventAnimation(TimeSpan.FromMilliseconds(180), () =>
            {
                Base.Log("程序已退出(Success)");
                //this.Close();
                Animator.IsRunning = false;
                Base.UIDispatcher.BeginInvoke(new Action(() =>
                {
                    Animator.StopThread();
                    Base.End();
                    this.Close();
                    Base.LogThread.Join();
                }));
                Base.UIDispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
            });
            AnimationGroup OutAnimGroup= new AnimationGroup(){ OutRenderAnim, OutSizeXAnim, OutSizeYAnim, OutOpacityAnim, Event };
            Animation.Start(OutAnimGroup);
        }

        #region 页面管理
        private AnimationGroup PageLeftChangeAnimMain;
        private AnimationGroup PageLeftChangeAnim;
        /// <summary>
        /// 触发左页面刷新动画并更新为一个控件
        /// </summary>
        public void ChangePageLeft(MyPageLeft NewPageLeft)
        {
            Animation.Stop(PageLeftChangeAnimMain);
            Animation.Stop(PageLeftChangeAnim);
            //Base.Log($"切换右页面，已停止动画（Is Null? {PageRightChangeAnim is null}）");
            PgLeftBorder.IsHitTestVisible = false;
            PageLeftChangeAnimMain = new AnimationGroup();
            PageLeftChangeAnimMain.TotalTime = TimeSpan.FromMilliseconds(300);
            MyPageLeft OldPageLeft = PageLeft as MyPageLeft;
            if (OldPageLeft != null)
                PageLeftChangeAnim = OldPageLeft.AnimationOut();
            else
                PageLeftChangeAnim = null;
            //Base.Log($"切换右页面，已启用右页面动画");
            Animation.Start(PageLeftChangeAnim);
            Animation AnimIn = PageLeftChangeAnim;
            PageLeftChangeAnim = NewPageLeft?.AnimationIn();
            EventAnimation Eventa = new EventAnimation(TimeSpan.FromMilliseconds(300), () =>
            {
                Animation.Stop(AnimIn);
                OldPageLeft?.OnUnloaded();
                if (OldPageLeft != null) OldPageLeft.Visibility = Visibility.Collapsed;
                PageLeft = NewPageLeft;
                NewPageLeft?.OnLoaded();
                if (NewPageLeft != null) NewPageLeft.Visibility = Visibility.Visible;
                PgLeftBorder.IsHitTestVisible = true;
                Animation.Start(PageLeftChangeAnim);
            });
            PageLeftChangeAnimMain.Add(Eventa);
            Animation.Start(PageLeftChangeAnimMain);
        }
        private AnimationGroup PageRightChangeAnimMain;
        private Animation PageRightChangeAnim;
        /// <summary>
        /// 触发右页面刷新动画并更新为一个控件
        /// </summary>
        public void ChangePageRight(MyPageRight NewPageRight)
        {
            Animation.Stop(PageRightChangeAnimMain);
            Animation.Stop(PageRightChangeAnim);
            //Base.Log($"切换右页面，已停止动画（Is Null? {PageRightChangeAnim is null}）");
            PgRightBorder.IsHitTestVisible = false;
            PageRightChangeAnimMain = new AnimationGroup();
            PageRightChangeAnimMain.TotalTime = TimeSpan.FromMilliseconds(300);
            MyPageRight OldPageRight = PageRight as MyPageRight;
            if (OldPageRight != null)
                PageRightChangeAnim = OldPageRight.AnimationOut();
            else
                PageRightChangeAnim = null;
            //Base.Log($"切换右页面，已启用右页面动画");
            Animation.Start(PageRightChangeAnim);
            Animation AnimIn = PageRightChangeAnim;
            
            EventAnimation Eventa = new EventAnimation(TimeSpan.FromMilliseconds(300), () =>
            {
                PageRightChangeAnim = NewPageRight?.AnimationIn();
                Animation.Stop(AnimIn);
                OldPageRight?.OnUnloaded();
                if (OldPageRight != null) OldPageRight.Visibility = Visibility.Collapsed;
                PageRight = NewPageRight;
                NewPageRight?.OnLoaded();
                if (NewPageRight != null) NewPageRight.Visibility = Visibility.Visible;
                PgRightBorder.IsHitTestVisible = true;
                Animation.Start(PageRightChangeAnim);
            });
            PageRightChangeAnimMain.Add(Eventa);
            Animation.Start(PageRightChangeAnimMain);
        }
        private void RadioStackMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            int PageIndex = int.Parse((RadioStackMain.SelectItem as FrameworkElement).Tag as string);
            if (PageIndex == PagesContent.PageIndex) return;
            PagesContent.ChangePage(PageIndex);
        }
        private AnimationGroup RadioStackAnim;
        public void TitleLeftChange(bool IsSub, string Title)
        {
            PanTitleLeft.ChangeTitleState(IsSub, Title);
            if (IsSub)
            {
                Animation.Stop(RadioStackAnim);
                RadioStackAnim = new AnimationGroup();
                RadioStackAnim.TotalTime = TimeSpan.FromMilliseconds(200);
                RadioStackMain.IsHitTestVisible = false;
                RadioStackAnim.Add(new DoubleAnimation(RadioStackMainTranslate, TranslateTransform.XProperty, RadioStackMainTranslate.X, 20, 200, 0, new AniEaseInFluent(3)));
                RadioStackAnim.Add(new DoubleAnimation(RadioStackMain, MyRadioStack.OpacityProperty, RadioStackMain.Opacity, 0, 200, 0, new AniEaseLinear()));
                Animation.Start(RadioStackAnim);
            }
            else
            {
                Animation.Stop(RadioStackAnim);
                RadioStackAnim = new AnimationGroup();
                RadioStackAnim.TotalTime = TimeSpan.FromMilliseconds(400);
                RadioStackMain.IsHitTestVisible = true;
                RadioStackAnim.Add(new DoubleAnimation(RadioStackMainTranslate, TranslateTransform.XProperty, RadioStackMainTranslate.X, 0, 400, 0, new AniEaseOutBack(2)));
                RadioStackAnim.Add(new DoubleAnimation(RadioStackMain, MyRadioStack.OpacityProperty, RadioStackMain.Opacity, 1, 200, 0, new AniEaseLinear()));
                Animation.Start(RadioStackAnim);
            }
        }
        private void PanTitleLeft_Back(object sender, RoutedEventArgs e)
        {
            PagesContent.PageBack();
        }
        #endregion

        #region 弹窗

        private bool IsShowingMessage = false;
        public void ShowMessage(MyMsg msg)
        {
            CurrectMsgBox.Message = msg;
            CurrectMsgBox.Show();
            PanMsg.Visibility = Visibility.Visible;
            Animation.Start(new DoubleAnimation(PanMsgBrush, Brush.OpacityProperty, PanMsgBrush.Opacity, 0.4, 200, 0));
            IsShowingMessage = true;
        }
        public async Task<object> CloseMessage(MyMsg msg)
        {
            IsShowingMessage = false;
            object Result = await msg.ResultTask.Task;
            Animation.Start(new AnimationGroup
            {
                new DoubleAnimation(PanMsgBrush,Brush.OpacityProperty,PanMsgBrush.Opacity,0,150,0),
                new EventAnimation(150,()=>PanMsg.Visibility=Visibility.Collapsed)
            });
            await CurrectMsgBox.CloseAndWait();
            return Result;
        }
        #endregion

        #region 大小调整
        public DpiScale DPI => VisualTreeHelper.GetDpi(this);

        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern void DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

        protected override void OnSourceInitialized(EventArgs e)
        {

            base.OnSourceInitialized(e);

            var hwnd = new WindowInteropHelper(this).Handle;
            var source = HwndSource.FromHwnd(hwnd);
            if (source != null)
            {
                // 渲染层允许 Alpha 通道通过
                source.CompositionTarget.BackgroundColor = Colors.Transparent;
                // 魔改窗口边缘判定
                source.AddHook(_SizeWndProc);
            }

            // 设置 DWM 窗口框架
            try
            {
                var margins = new MARGINS
                {
                    leftWidth = -1,
                    rightWidth = -1,
                    topHeight = -1,
                    bottomHeight = -1
                };
                DwmExtendFrameIntoClientArea(hwnd, ref margins);
            }
            catch (Exception ex)
            {
                Base.Log("DWM 窗口框架应用失败: " + ex.Message);
            }
        }

        private nint _SizeWndProc(nint hWnd, int msg, nint wParam, nint lParam, ref bool handled)
        {
            // 窗口活动常量

            const int WM_NCHITTEST = 0x84;
            const int HTCLIENT = 1;
            const int HTLEFT = 10;
            const int HTRIGHT = 11;
            const int HTTOP = 12;
            const int HTTOPLEFT = 13;
            const int HTTOPRIGHT = 14;
            const int HTBOTTOM = 15;
            const int HTBOTTOMLEFT = 16;
            const int HTBOTTOMRIGHT = 17;

            DpiScale DPI = this.DPI;

            // 过滤非 WM_NCHITTEST 事件
            if (msg != WM_NCHITTEST)
                return nint.Zero;

            // 提取鼠标坐标
            var xMouse = (short)(lParam.ToInt32() & 0xffff);
            var yMouse = (short)(lParam.ToInt32() >> 16);

            var lxMouse = xMouse / DPI.DpiScaleX;
            var lyMouse = yMouse / DPI.DpiScaleY;

            // 获取窗口参数
            Rect windowRect;
            try
            {
                var windowRect_ = Windows.GetWindowRect(hWnd);
                windowRect = new Rect(windowRect_.Left / DPI.DpiScaleX, windowRect_.Top / DPI.DpiScaleY, (windowRect_.Right - windowRect_.Left) / DPI.DpiScaleX, (windowRect_.Bottom - windowRect_.Top) / DPI.DpiScaleY);
            }
            catch
            {
                return nint.Zero;
            }
            


            // 判断鼠标是否在窗口范围内
            var isInWindow = lxMouse >= windowRect.Left && lxMouse <= windowRect.Right && lyMouse >= windowRect.Top &&
                             lyMouse <= windowRect.Bottom;

            // 过滤不在窗口内的请求
            if (!isInWindow)
                return nint.Zero;

            // 如果 CanResize 为 False，直接返回 HTCLIENT
            if (this.ResizeMode != ResizeMode.CanResize)
                return new nint(HTCLIENT);

            // 计算鼠标相对于窗口左上角的物理像素位置
            var relX = lxMouse - windowRect.Left;
            var relY = lyMouse - windowRect.Top;

            // 判定是否命中热区
            var inLeft = (relX >= 6 && relX <= 11 && relY >= 6 && relY <= windowRect.Height - 16);
            var inRight = (relX >= windowRect.Width - 11 && relX <= windowRect.Width - 6 && relY >= 6 && relY <= windowRect.Height - 16);
            var inTop = (relY >= 6 && relY <= 11 && relX >= 6 && relX <= windowRect.Width - 6);
            var inBottom = (relY >= windowRect.Height - 16 && relY <= windowRect.Height - 11 && relX >= 6 && relX <= windowRect.Width - 6);

            handled = true; // 接管该区域的消息

            // 返回结果
            if (inTop && inLeft)
                return new nint(HTTOPLEFT);
            if (inTop && inRight)
                return new nint(HTTOPRIGHT);
            if (inBottom && inLeft)
                return new nint(HTBOTTOMLEFT);
            if (inBottom && inRight)
                return new nint(HTBOTTOMRIGHT);
            if (inLeft)
                return new nint(HTLEFT);
            if (inRight)
                return new nint(HTRIGHT);
            if (inTop)
                return new nint(HTTOP);
            if (inBottom)
                return new nint(HTBOTTOM);

            // 如果在 0-offset 范围内，返回 HTCLIENT 杀掉默认缩放
            return new nint(HTCLIENT);
        }

        #endregion


    }
}
