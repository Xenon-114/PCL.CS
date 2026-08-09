using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace XeF4Core.WPF;

public class MyTooltip : DependencyObject
{
    #region 样式画刷

    public Brush Background
    {
        get => (Brush)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(MyTooltip), new PropertyMetadata(SystemColors.WindowBrush));


    public Brush BorderBrush
    {
        get => (Brush)GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    public static readonly DependencyProperty BorderBrushProperty =
        DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(MyTooltip), new PropertyMetadata(SystemColors.WindowFrameBrush));




    #endregion

    #region 常量值

    private const double ScaleClosed = 0.9;
    private const double ShadowBlur = 18;
    private const double ShadowAlpha = 0.15;
    private const int MaxContentWidth = 676;
    private const double TipFontSize = 12.5;
    private const double TipLineHeight = 17;
    private const int AnimLength = 300;
    private const int AnimExit = 80;

    private readonly Thickness _innerPad = new(12, 10, 12, 10);
    private readonly DropShadowEffect _shadow = new()
    {
        Opacity = ShadowAlpha,
        BlurRadius = ShadowBlur,
        ShadowDepth = 0,
        Color = Colors.Black
    };
    private readonly object locker = new();

    #endregion

    #region 元素附加属性

    private static readonly DependencyProperty _keyCombo = DependencyProperty.RegisterAttached(
        "KeyCombo", typeof(bool), typeof(MyTooltip), new PropertyMetadata(false));

    #endregion

    #region 实例状态与字段

    private readonly FrameworkElement _root;
    private bool _isDisposed;
    private bool _closing;
    private Point _cursor;
    private Popup? _flyout;
    private Border? _shell;
    private ScaleTransform? _scaler;
    private Storyboard? OpenStory;
    private Storyboard? CloseStory;
    private DispatcherTimer? _latch;
    private HwndSourceHook? _transparentHook;
    private bool _layoutSubscribed;

    #endregion

    #region 构建函数

    public MyTooltip(FrameworkElement root)
    {
        _root = root ?? throw new ArgumentNullException(nameof(root));



        _OnEnterHandler = OnMouseEnter;
        _OnMoveHandler = OnMouseMove;
        _OnLeaveHandler= OnMouseLeave;
        _OnUnloadedHandler = OnUnload;


        _shadow.Freeze();
        // 预建 Storyboard
        _BuildUi();
        _PrebuildStoryboards();

        // 挂载事件（仅当前根元素范围内）
        _AttachRootEvents();
    }

    #endregion

    #region 事件注册

    private readonly MouseEventHandler _OnEnterHandler;
    private readonly MouseEventHandler _OnMoveHandler;
    private readonly MouseEventHandler _OnLeaveHandler;
    private readonly RoutedEventHandler _OnUnloadedHandler;

    private void _AttachRootEvents()
    {
        // 在根元素上拦截事件（handledEventsToo = true）
        EventManager.RegisterClassHandler(typeof(FrameworkElement),
            UIElement.MouseEnterEvent, _OnEnterHandler, true);
        EventManager.RegisterClassHandler(typeof(FrameworkElement),
            UIElement.MouseMoveEvent, _OnMoveHandler);
        EventManager.RegisterClassHandler(typeof(FrameworkElement),
            UIElement.MouseLeaveEvent, _OnLeaveHandler);
        EventManager.RegisterClassHandler(typeof(FrameworkElement),
            FrameworkElement.UnloadedEvent, _OnUnloadedHandler);
        EventManager.RegisterClassHandler(typeof(FrameworkElement),
            ToolTipService.ToolTipOpeningEvent, new ToolTipEventHandler((s, e) => { e.Handled = true; }), true);
    }

    #endregion

    #region 事件处理
    private void OnMouseEnter(object sender,MouseEventArgs e)
    {
        if (sender is not FrameworkElement FElement) return;
        FElement.Dispatcher.BeginInvoke(() => TryShow(FElement));
    }
    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not FrameworkElement FElement) return;
        var Over = _SeekOwner(_Over());
        if (Over is not null)
            TryShow(Over);

        _PlaceNear();
    }
    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        lock (locker)
        {
            if (sender is not FrameworkElement fe ||  !ReferenceEquals(fe, UsingTarget) ) return;

            if (_PointInside(fe, Mouse.GetPosition(fe)))
            {
                e.Handled = true;
                return;
            }

            var next = _SeekOwner(_Over());
            if (next is not null && !ReferenceEquals(next, Target))
            {
                TryShow(next);
                return;
            }
            else
            {
                CloseToolTip();
            }
        }
    }
    private void OnUnload(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement FElement) return;
        FElement.Dispatcher.BeginInvoke(() =>
        {
            if (ReferenceEquals(FElement, Target))
                CloseToolTip();
        });
    }
    private FrameworkElement? UsingTarget => (State is ToolTipState.ChangingOrClosing || State is ToolTipState.Waiting) ? NewTarget : Target;
    #endregion

    #region 设置Storyboard

    private void _PrebuildStoryboards()
    {
        OpenStory = new Storyboard();
        CloseStory = new Storyboard();

        // 打开动画：透明度 0→1，缩放 0.97→1
        void AddOpenAnim(double to, string prop, IEasingFunction? ease = null)
        {
            var anim = new DoubleAnimation(to, new Duration(TimeSpan.FromMilliseconds(AnimLength)))
            {
                EasingFunction = ease ?? new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            // 显式设置目标，避免依赖隐式传递
            Storyboard.SetTarget(anim, _shell);
            Storyboard.SetTargetProperty(anim, new PropertyPath(prop));
            OpenStory.Children.Add(anim);
        }

        // 关闭动画：透明度 1→0，缩放 1→0.97
        void AddCloseAnim(double to, string prop)
        {
            var anim = new DoubleAnimation(to, new Duration(TimeSpan.FromMilliseconds(AnimExit)))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(anim, _shell);
            Storyboard.SetTargetProperty(anim, new PropertyPath(prop));
            CloseStory.Children.Add(anim);
        }

        BackEase backEase = new() { EasingMode = EasingMode.EaseOut, Amplitude = 0.4 };

        AddOpenAnim(1, nameof(UIElement.Opacity));
        AddOpenAnim(1, "RenderTransform.ScaleX", backEase);
        AddOpenAnim(1, "RenderTransform.ScaleY", backEase);

        AddCloseAnim(0, nameof(UIElement.Opacity));
        AddCloseAnim(ScaleClosed, "RenderTransform.ScaleX");
        AddCloseAnim(ScaleClosed, "RenderTransform.ScaleY");
        CloseStory.Completed += (s, e) => OnToolTipClosed();
    }

    #endregion

    #region 所有者解析

    private void TryShow(FrameworkElement Over)
    {
        lock (locker)
        {
            //Debug.WriteLine("尝试显示ToolTip");
            // 获取鼠标下方实际元素
            var leaf = Over;
            var owner = _SeekOwner(leaf);
            if (owner is null)
            {
                //Debug.WriteLine("没有找到ToolTip");
                if (UsingTarget is not null) CloseToolTip();
                return;
            }

            // 提取内容
            var content = _FetchContentObject(owner);
            bool empty = content is null || (content is string s && string.IsNullOrEmpty(s));

            if (empty)
            {
                //Debug.WriteLine("ToolTip为空");
                if (UsingTarget is not null) CloseToolTip();
                return;
            }

            bool targetChanged = !ReferenceEquals(owner, UsingTarget);

            if (targetChanged)
            {
                //Debug.WriteLine("ToolTip开始改变");
                ChangeToolTip(owner);
            }
            else
            {
                if (_flyout is { IsOpen: true })
                    _PlaceNear();
                //应对ToolTip意外关闭
                else if (Target is not null) _ShowToolTip();
            }
        }
    }

    private DependencyObject? _Over() => Mouse.DirectlyOver as DependencyObject;

    private FrameworkElement? _SeekOwner(DependencyObject? leaf)
    {
        for (var cur = leaf; cur is not null; cur = _GetTreeParent(cur))
        {
            if (cur is FrameworkElement fe && _Eligible(fe) && _FetchContent(fe))
                return fe;
        }
        return null;
    }

    private static DependencyObject? _GetTreeParent(DependencyObject current)
    {
        // 优先 VisualTree，失败则 LogicalTree（支持 ContentElement）
        var visualParent = VisualTreeHelper.GetParent(current);
        if (visualParent is not null)
            return visualParent;
        return LogicalTreeHelper.GetParent(current);
    }

    private bool _Eligible(FrameworkElement fe) =>
        ToolTipService.GetIsEnabled(fe) &&
        (fe.IsEnabled || ToolTipService.GetShowOnDisabled(fe));

    private bool _FetchContent(FrameworkElement src)
    {
        return _FetchContentObject(src) is not null;
    }

    private object? _FetchContentObject(FrameworkElement src)
    {
        var raw = src.ToolTip;
        if (raw is null) return null;
        var payload = raw is ToolTip tip ? tip.Content : raw;
        return payload;
    }

    private bool _PointInside(FrameworkElement el, Point p) =>
        p.X >= 0 && p.Y >= 0 && p.X <= el.ActualWidth && p.Y <= el.ActualHeight;

    #endregion

    #region 运行周期

    private void CloseToolTip()
    {
        lock (locker)
        {
            if (State is ToolTipState.ChangingOrClosing)
                NewTarget = null;
            if (State is ToolTipState.Waiting)
            {
                StopShowAfter();
                NewTarget = null;
            }
            if(State is ToolTipState.Showing)
            {
                NewTarget = null;
                State = ToolTipState.ChangingOrClosing;
                CloseStory?.Begin();
            }
        }
    }
    private void ChangeToolTip(FrameworkElement element)
    {
        lock (locker)
        {
            //if (element == NewTarget) return;
            //Debug.WriteLine($"改变ToolTip，当前状态{State}");
            if (State is ToolTipState.ChangingOrClosing)
                NewTarget = element;
            if (State is ToolTipState.Waiting || State is ToolTipState.Nothing)
                ShowAfter(element);
            if (State is ToolTipState.Showing)
            {
                //Debug.WriteLine("关闭已有的ToolTip");
                NewTarget = element;
                State = ToolTipState.ChangingOrClosing;
                CloseStory?.Begin();
            }
        }
    }
    private void OnToolTipClosed()
    {
        lock (locker)
        {
            State = ToolTipState.Nothing;
            if (NewTarget is not null)
                _ShowToolTip();
            else
            {
                _flyout!.IsOpen = false;
                Target = null;
            }
        }
    }

    private enum ToolTipState
    {
        Nothing,
        Waiting,
        ChangingOrClosing,
        Showing
    }
    private FrameworkElement? Target { get; set; }
    private FrameworkElement? NewTarget { get; set; }
    private ToolTipState State { get; set; } = ToolTipState.Nothing;
    private void _ShowToolTip()
    {
        lock (locker)
        {
            Target = NewTarget;
            //Debug.WriteLine("ToolTip已显示");
            if (Target is not null)
            {
                _RenderInside(Target);
                _PlaceNear();
                _flyout!.IsOpen = true;
                OpenStory?.Begin();
                State = ToolTipState.Showing;
            }
        }
    }
    private int watchgen = 0;
    private void StopShowAfter()
    {
        lock (locker)
        {
            watchgen++;
            _latch?.Stop();
            State = ToolTipState.Nothing;
            NewTarget = null;
            Target = null;
        }
    }
    private void ShowAfter(FrameworkElement target)
    {
        lock (locker)
        {
            if (NewTarget == target) return;
            int gen = ++watchgen;
            NewTarget = target;
            _latch?.Stop();
            var ms = Math.Max(0, ToolTipService.GetInitialShowDelay(NewTarget));
            //Debug.WriteLine($"ToolTip将在200毫秒后显示，代{gen}");
            State = ToolTipState.Waiting;
            _latch = new DispatcherTimer(
            TimeSpan.FromMilliseconds(ms),
            DispatcherPriority.Normal,
            (_, _) =>
            {
                lock (locker)
                {
                    _latch?.Stop();
                    if (gen == watchgen && NewTarget is not null)
                        _ShowToolTip();
                }
            },
            NewTarget!.Dispatcher);
        }
    }

    #endregion

    #region 构建UI

    private void _BuildUi()
    {
        _scaler = new ScaleTransform(ScaleClosed, ScaleClosed);
        _shell = new Border
        {
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            MaxWidth = 700,
            SnapsToDevicePixels = true,
            UseLayoutRounding = true,
            RenderTransform = _scaler,
            RenderTransformOrigin = new Point(0, 0),
            Effect = _shadow
        };

        _shell.SetBinding(Border.BackgroundProperty, new Binding(nameof(Background)) { Source = this });
        _shell.SetBinding(Border.BorderBrushProperty, new Binding(nameof(BorderBrush)) { Source = this });

        var wrap = new Grid
        {
            Margin = new Thickness(ShadowBlur + 1),
            SnapsToDevicePixels = true,
            UseLayoutRounding = true
        };
        wrap.Children.Add(_shell);

        _flyout = new Popup
        {
            AllowsTransparency = true,
            IsHitTestVisible = false,
            StaysOpen = true,
            PopupAnimation = PopupAnimation.None,
            Placement = PlacementMode.Relative,
            Child = wrap
        };

        // 鼠标穿透钩子
        const int WM_NCHITTEST = 0x0084;
        const int HTTRANSPARENT = -1;

        static nint transparentHook(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
        {
            if (msg == WM_NCHITTEST)
            {
                handled = true;
                return HTTRANSPARENT;
            }
            return 0;
        }
        _transparentHook = transparentHook;

        // 强制打开以创建 HwndSource，挂钩子后关闭
        _flyout.IsOpen = true;
        _AttachTransparentHook();
        _flyout.IsOpen = false;
        _flyout.Opened += (_, _) => _AttachTransparentHook();
    }

    private void _AttachTransparentHook()
    {
        if (_transparentHook is null || _flyout?.Child is not UIElement child) return;
        var src = PresentationSource.FromVisual(child) as HwndSource;
        if (src is not null)
        {
            src.RemoveHook(_transparentHook);
            src.AddHook(_transparentHook);
        }
    }

    #endregion

    #region 内容渲染

    private void _RenderInside(FrameworkElement owner)
    {
        _shell!.Child = null;

        var raw = owner.ToolTip;
        var tip = raw as ToolTip;
        var content = tip?.Content ?? raw;

        if (content is null || content is string { Length: 0 })
            return;

        var hasTpl = tip?.ContentTemplate is not null || tip?.ContentTemplateSelector is not null;
        var tipW = tip is { Width: > 0 } && !double.IsNaN(tip.Width) ? tip.Width : MaxContentWidth;

        if (content is string text && !hasTpl)
        {
            var tb = new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                Margin = _innerPad,
                FontSize = TipFontSize,
                LineHeight = TipLineHeight,
                MaxWidth = tipW,
                Foreground = SystemColors.ControlTextBrush
            };
            _shell.Child = tb;
        }
        else
        {
            _shell.Child = new ContentPresenter
            {
                Content = content,
                ContentTemplate = tip?.ContentTemplate,
                ContentTemplateSelector = tip?.ContentTemplateSelector,
                ContentStringFormat = tip?.ContentStringFormat,
                Margin = _innerPad,
                MaxWidth = tipW
            };
        }
    }

    #endregion

    #region 定位

    private PlacementMode LastPlctMode;
    private double LastHorizontalOffset;
    private double LastVerticalOffset;

    private void _PlaceNear()
    {
        if (_flyout is null) return;
        var target = Target;
        PlacementMode mode;
        if (Target is not null)
        {
            mode = ToolTipService.GetPlacement(target);
            if (LastPlctMode != mode) LastPlctMode = mode;
        }
        else mode = LastPlctMode;
        

        if (mode == PlacementMode.Mouse)
        {
            _flyout.Placement = PlacementMode.Relative;
            _flyout.PlacementTarget = _root;
            var pt = Mouse.GetPosition(_root);
            _flyout.PlacementRectangle = default;
            if(target is not null)
            {
                LastHorizontalOffset = ToolTipService.GetHorizontalOffset(target);
                LastVerticalOffset= ToolTipService.GetVerticalOffset(target);
            }
            _flyout.HorizontalOffset = Math.Round(pt.X + 15 + LastHorizontalOffset);
            _flyout.VerticalOffset = Math.Round(pt.Y + 25 + LastVerticalOffset);
        }
        else if (mode == PlacementMode.MousePoint)
        {
            _flyout.Placement = PlacementMode.Relative;
            _flyout.PlacementTarget = _root;
            var pt = Mouse.GetPosition(_root);
            _flyout.PlacementRectangle = default;
            if (target is not null)
            {
                LastHorizontalOffset = ToolTipService.GetHorizontalOffset(target);
                LastVerticalOffset = ToolTipService.GetVerticalOffset(target);
            }
            _flyout.HorizontalOffset = Math.Round(pt.X + LastHorizontalOffset);
            _flyout.VerticalOffset = Math.Round(pt.Y + LastVerticalOffset);
        }
        else
        {
            if (target is null) return;
            _flyout.PlacementTarget = target;
            _flyout.Placement = mode;
            _flyout.HorizontalOffset = ToolTipService.GetHorizontalOffset(target);
            _flyout.VerticalOffset = ToolTipService.GetVerticalOffset(target);
            _flyout.PlacementRectangle = ToolTipService.GetPlacementRectangle(target);

            double offset = _flyout.HorizontalOffset;
            // 给它一个极小的增量，触发重定位
            _flyout.HorizontalOffset = offset + 0.000001;
            // 立即恢复原值
            _flyout.HorizontalOffset = offset;
        }
    }

    #endregion
}