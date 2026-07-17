using PCL.CS.Modules; // 假设 ColorHelper 扩展方法在此命名空间
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PCL.CS.Controls
{
    public class MyMainWindow : Control
    {
        static MyMainWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MyMainWindow),
                new FrameworkPropertyMetadata(typeof(MyMainWindow)));
        }

        // 模板内的子元素引用
        private MyWinMain _mainW;
        private Border _titleBar;
        private Rectangle _resizerT, _resizerB, _resizerL, _resizerR;
        private Rectangle _resizerLT, _resizerLB, _resizerRT, _resizerRB;

        // 圆角裁剪几何
        private RectangleGeometry _clipRect;

        // 依赖属性：直接映射到 MyWinMain
        public Brush TitleBarBrush
        {
            get => (Brush)GetValue(TitleBarBrushProperty);
            set => SetValue(TitleBarBrushProperty, value);
        }
        public static readonly DependencyProperty TitleBarBrushProperty =
            DependencyProperty.Register("TitleBarBrush", typeof(Brush), typeof(MyMainWindow),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush PgLeftBrush
        {
            get => (Brush)GetValue(PgLeftBrushProperty);
            set => SetValue(PgLeftBrushProperty, value);
        }
        public static readonly DependencyProperty PgLeftBrushProperty =
            DependencyProperty.Register("PgLeftBrush", typeof(Brush), typeof(MyMainWindow),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush LeftChrome
        {
            get => (Brush)GetValue(LeftChromeProperty);
            set => SetValue(LeftChromeProperty, value);
        }
        public static readonly DependencyProperty LeftChromeProperty =
            DependencyProperty.Register("LeftChrome", typeof(Brush), typeof(MyMainWindow),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public double PageLeftBackWidth
        {
            get => (double)GetValue(PageLeftBackWidthProperty);
            set => SetValue(PageLeftBackWidthProperty, value);
        }
        public static readonly DependencyProperty PageLeftBackWidthProperty =
            DependencyProperty.Register("PageLeftBackWidth", typeof(double), typeof(MyMainWindow),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        // 背景颜色（渐变源）
        public new Color Background
        {
            get => (Color)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }
        public new static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Color), typeof(MyMainWindow),
                new PropertyMetadata(Colors.Transparent, OnBackgroundChanged));

        private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as MyMainWindow)?.UpdateBackground((Color)e.NewValue);
        }

        // 圆角裁剪几何属性（供模板绑定）
        public RectangleGeometry ClipRect
        {
            get => _clipRect;
            private set
            {
                _clipRect = value;
                // 通知模板绑定更新
                OnPropertyChanged(new DependencyPropertyChangedEventArgs());
            }
        }

        // 事件定义
        public event EventHandler TitleBarDrag;
        public event EventHandler ResizeT;
        public event EventHandler ResizeB;
        public event EventHandler ResizeL;
        public event EventHandler ResizeR;
        public event EventHandler ResizeLT;
        public event EventHandler ResizeLB;
        public event EventHandler ResizeRT;
        public event EventHandler ResizeRB;

        public MyMainWindow()
        {
            // 初始化裁剪几何
            _clipRect = new RectangleGeometry(new Rect(0, 0, 0, 0), 6, 6);
            this.SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ActualWidth > 0 && ActualHeight > 0)
            {
                _clipRect.Rect = new Rect(0, 0, ActualWidth, ActualHeight);
                // 强制重新应用模板绑定（可选）
                InvalidateVisual();
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // 获取模板中的命名元素
            _mainW = GetTemplateChild("MainW") as MyWinMain;
            _titleBar = GetTemplateChild("TitleBar") as Border;
            //_pageLeft = GetTemplateChild("PageLeft") as Border;
            //_pageRight = GetTemplateChild("PageRight") as Border;
            //_panMask = GetTemplateChild("PanMask") as Border;
            _resizerT = GetTemplateChild("ResizerT") as Rectangle;
            _resizerB = GetTemplateChild("ResizerB") as Rectangle;
            _resizerL = GetTemplateChild("ResizerL") as Rectangle;
            _resizerR = GetTemplateChild("ResizerR") as Rectangle;
            _resizerLT = GetTemplateChild("ResizerLT") as Rectangle;
            _resizerLB = GetTemplateChild("ResizerLB") as Rectangle;
            _resizerRT = GetTemplateChild("ResizerRT") as Rectangle;
            _resizerRB = GetTemplateChild("ResizerRB") as Rectangle;

            // 绑定 MyWinMain 的属性到当前控件
            if (_mainW != null)
            {
                _mainW.SetBinding(MyWinMain.TitleBarBrushProperty,
                    new System.Windows.Data.Binding("TitleBarBrush") { Source = this });
                _mainW.SetBinding(MyWinMain.PgLeftBrushProperty,
                    new System.Windows.Data.Binding("PgLeftBrush") { Source = this });
                GradientStopCollection LocalGSCs = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(33, 0, 0, 0), 0),
                    new GradientStop(Color.FromArgb(17, 0, 0, 0), 0.3),
                    new GradientStop(Color.FromArgb(0, 0, 0, 0), 1)
                };
                _mainW.LeftChrome = new LinearGradientBrush(LocalGSCs, new Point(0, 0.5), new Point(1, 0.5));
                _mainW.SetBinding(MyWinMain.PageLeftBackWidthProperty,
                    new System.Windows.Data.Binding("PageLeftBackWidth") { Source = this });
                // 初始化背景渐变
                UpdateBackground(Background);
            }

            // 为所有调整大小控件添加鼠标事件（Lambda 表达式）
            AddResizeEvent(_resizerT, ResizeT);
            AddResizeEvent(_resizerB, ResizeB);
            AddResizeEvent(_resizerL, ResizeL);
            AddResizeEvent(_resizerR, ResizeR);
            AddResizeEvent(_resizerLT, ResizeLT);
            AddResizeEvent(_resizerLB, ResizeLB);
            AddResizeEvent(_resizerRT, ResizeRT);
            AddResizeEvent(_resizerRB, ResizeRB);

            // TitleBar 拖拽事件
            if (_titleBar != null)
            {
                _titleBar.MouseLeftButtonDown += (s, e) => TitleBarDrag?.Invoke(this, EventArgs.Empty);
            }
        }

        private void AddResizeEvent(FrameworkElement element, EventHandler handler)
        {
            if (element != null && handler != null)
            {
                element.MouseLeftButtonDown += (s, e) => handler?.Invoke(this, EventArgs.Empty);
            }
        }

        private void UpdateBackground(Color newColor)
        {
            if (_mainW == null) return;

            // 获取或创建 LinearGradientBrush
            var backBrush = _mainW.Background as LinearGradientBrush;
            if (backBrush == null)
            {
                backBrush = new LinearGradientBrush();
                _mainW.Background = backBrush;
            }

            backBrush.StartPoint = new Point(0, 1);
            backBrush.EndPoint = new Point(1, 0);

            var stops = backBrush.GradientStops;
            if (stops == null)
            {
                stops = new GradientStopCollection();
                backBrush.GradientStops = stops;
            }

            // 确保有 3 个色标
            while (stops.Count < 3) stops.Add(new GradientStop());
            stops[0].Offset = -0.1;
            stops[1].Offset = 0.4;
            stops[2].Offset = 1.1;

            // 使用 ColorHelper 转换颜色
            var (h, s, l) = newColor.ToHsl();
            stops[0].Color = (h + 15, s, l).HslToColor();
            stops[1].Color = (h, s, l).HslToColor();
            stops[2].Color = (h - 15, s, l).HslToColor();
        }

        // 为了支持模板中的 Clip 绑定，需要实现一个可绑定的属性
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == BackgroundProperty && _mainW != null)
            {
                UpdateBackground(Background);
            }
        }
    }
}