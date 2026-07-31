using System;
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

//==================================================================================================
//  代码来源：PCL-CE(PCL.Core)
//    源码地址：https://github.com/PCL-Community/PCL-CE/blob/dev/PCL.Core/UI/Controls/Tooltip.cs
//==================================================================================================

namespace XeF4Core.WPF;

public class MyTooltip : DependencyObject, IDisposable
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

    #endregion

    #region 元素附加属性

    private static readonly DependencyProperty _keyCombo = DependencyProperty.RegisterAttached(
        "KeyCombo", typeof(bool), typeof(MyTooltip), new PropertyMetadata(false));

    #endregion

    #region 实例状态机

    private readonly FrameworkElement _root;
    private bool _isDisposed;
    private int _gen;
    private bool _closing;
    private Point _cursor;
    private FrameworkElement? _target;
    private object? _lastContent;
    private Popup? _flyout;
    private Border? _shell;
    private ScaleTransform? _scaler;
    private Storyboard? _openStory;
    private Storyboard? _closeStory;
    private DispatcherTimer? _latch;
    private HwndSourceHook? _transparentHook;
    private bool _layoutSubscribed;

    // 预创建的委托缓存
    private readonly MouseEventHandler _onEnterHandler;
    private readonly MouseEventHandler _onMoveHandler;
    private readonly MouseEventHandler _onLeaveHandler;
    private readonly MouseButtonEventHandler _onReleaseHandler;
    private readonly RoutedEventHandler _onUnloadedHandler;
    private readonly RoutedEventHandler _onComboLoadedHandler;
    private readonly MouseButtonEventHandler _onComboMouseDownHandler;
    private readonly EventHandler _onLayoutUpdatedHandler;

    #endregion

    #region 构建函数

    public MyTooltip(FrameworkElement root)
    {
        _root = root ?? throw new ArgumentNullException(nameof(root));

        _shadow.Freeze();

        // 初始化委托
        _onEnterHandler = OnEnter;
        _onMoveHandler = OnMove;
        _onLeaveHandler = OnLeave;
        _onReleaseHandler = OnRelease;
        _onUnloadedHandler = OnUnloaded;
        _onComboLoadedHandler = OnComboInit;
        _onComboMouseDownHandler = OnComboInit;
        _onLayoutUpdatedHandler = OnLayoutUpdated;

        // 预建 Storyboard
        _PrebuildStoryboards();

        // 挂载事件（仅当前根元素范围内）
        _AttachRootEvents();
    }

    #endregion

    #region 事件注册

    private void _AttachRootEvents()
    {
        // 在根元素上拦截事件（handledEventsToo = true）
        _root.AddHandler(UIElement.MouseEnterEvent, _onEnterHandler, true);
        _root.AddHandler(UIElement.MouseMoveEvent, _onMoveHandler, true);
        _root.AddHandler(UIElement.MouseLeaveEvent, _onLeaveHandler, true);
        _root.AddHandler(UIElement.PreviewMouseUpEvent, _onReleaseHandler, true);
        _root.AddHandler(FrameworkElement.UnloadedEvent, _onUnloadedHandler, true);

        // ComboBox 特殊处理
        _root.AddHandler(ComboBox.LoadedEvent, _onComboLoadedHandler, true);
        _root.AddHandler(ComboBox.PreviewMouseDownEvent, _onComboMouseDownHandler, true);
    }

    private void _DetachRootEvents()
    {
        _root.RemoveHandler(UIElement.MouseEnterEvent, _onEnterHandler);
        _root.RemoveHandler(UIElement.MouseMoveEvent, _onMoveHandler);
        _root.RemoveHandler(UIElement.MouseLeaveEvent, _onLeaveHandler);
        _root.RemoveHandler(UIElement.PreviewMouseUpEvent, _onReleaseHandler);
        _root.RemoveHandler(FrameworkElement.UnloadedEvent, _onUnloadedHandler);
        _root.RemoveHandler(ComboBox.LoadedEvent, _onComboLoadedHandler);
        _root.RemoveHandler(ComboBox.PreviewMouseDownEvent, _onComboMouseDownHandler);
        _root.LayoutUpdated -= _onLayoutUpdatedHandler;
        _layoutSubscribed = false;
    }

    #endregion

    #region 设置Storyboard

    private void _PrebuildStoryboards()
    {
        _openStory = new Storyboard();
        _closeStory = new Storyboard();

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
            _openStory.Children.Add(anim);
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
            _closeStory.Children.Add(anim);
        }

        BackEase backEase = new() { EasingMode = EasingMode.EaseOut, Amplitude = 0.4 };

        AddOpenAnim(1, nameof(UIElement.Opacity));
        AddOpenAnim(1, "RenderTransform.ScaleX", backEase);
        AddOpenAnim(1, "RenderTransform.ScaleY", backEase);

        AddCloseAnim(0, nameof(UIElement.Opacity));
        AddCloseAnim(ScaleClosed, "RenderTransform.ScaleX");
        AddCloseAnim(ScaleClosed, "RenderTransform.ScaleY");
    }

    #endregion

    #region 事件处理器

    private void OnEnter(object sender, MouseEventArgs e)
    {
        if (_isDisposed) return;
        _root.Dispatcher.BeginInvoke(() => _TryClaim());
    }

    private bool _IsCursorPlaced(FrameworkElement el) =>
        ToolTipService.GetPlacement(el) is PlacementMode.Mouse or PlacementMode.MousePoint;

    private void OnMove(object sender, MouseEventArgs e)
    {
        if (_isDisposed) return;

        // 左键按下时只更新位置，不触发重置
        if (Mouse.LeftButton == MouseButtonState.Pressed)
        {
            if (_target is not null)
            {
                _cursor = Mouse.GetPosition(_target);
                if (_IsCursorPlaced(_target) && _flyout is { IsOpen: true })
                    _PlaceNear(_target, _cursor);
            }
            else if (_flyout is { IsOpen: true, PlacementTarget: FrameworkElement ft } && _IsCursorPlaced(ft))
            {
                _cursor = Mouse.GetPosition(ft);
                _PlaceNear(ft, _cursor);
            }
            return;
        }

        _TryClaim();

        if (_target is not null)
        {
            _cursor = Mouse.GetPosition(_target);
            if (_IsCursorPlaced(_target) && _flyout is { IsOpen: true })
                _PlaceNear(_target, _cursor);
        }
        else if (_flyout is { IsOpen: true, PlacementTarget: FrameworkElement ft } && _IsCursorPlaced(ft))
        {
            _cursor = Mouse.GetPosition(ft);
            _PlaceNear(ft, _cursor);
        }
    }

    private void OnLeave(object sender, MouseEventArgs e)
    {
        if (_isDisposed) return;
        if (sender is not FrameworkElement fe || !ReferenceEquals(fe, _target)) return;

        if (_PointInside(fe, Mouse.GetPosition(fe)))
        {
            e.Handled = true;
            return;
        }

        // 检查鼠标是否进入了其他有效元素
        var next = _SeekOwner(_Over());
        if (next is not null && !ReferenceEquals(next, _target))
        {
            _StartCycle(next, Mouse.GetPosition(next));
            return;
        }

        _WindDown();
    }

    private void OnRelease(object sender, MouseButtonEventArgs e)
    {
        if (_isDisposed) return;
        _root.Dispatcher.BeginInvoke(() =>
        {
            if (_target is null) return;
            var owner = _SeekOwner(_Over() ?? sender as DependencyObject);
            if (owner is null)
                _WindDown();
            else
                _StartCycle(owner, Mouse.GetPosition(owner));
        }, DispatcherPriority.Input);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_isDisposed) return;
        if (sender is FrameworkElement fe && ReferenceEquals(fe, _target))
            _WindDown();
    }

    #endregion

    #region 所有者解析

    private void _TryClaim()
    {
        // 获取鼠标下方实际元素
        var leaf = Mouse.DirectlyOver as DependencyObject;
        var owner = _SeekOwner(leaf);
        if (owner is null)
        {
            if (_target is not null) _WindDown();
            _lastContent = null; // 清除缓存
            return;
        }

        // 提取内容
        var content = _FetchContentObject(owner);
        bool empty = content is null || (content is string s && string.IsNullOrEmpty(s));

        if (empty)
        {
            if (_target is not null) _WindDown();
            _lastContent = null;
            return;
        }

        // 内容对比
        bool targetChanged = !ReferenceEquals(owner, _target);

        if (targetChanged)
        {
            // 唯一重置条件：目标变
            _lastContent = content;
            if (_target is not null && _flyout is { IsOpen: true })
            {
                _WindDown(); // 关闭旧内容（_WindDown 不会清除 _lastContent）
            }
            _target = owner;
            _cursor = Mouse.GetPosition(owner);
            _latch?.Stop();
            _KickTimer(owner);
        }
        else
        {
            // 目标未变：仅更新光标位置
            _cursor = Mouse.GetPosition(owner);
            if (_flyout is { IsOpen: true })
                _PlaceNear(owner, _cursor);
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

    #region 生命周期

    private void _StartCycle(FrameworkElement target, Point pt)
    {
        if (!_Eligible(target) || !_FetchContent(target)) return;

        // 如果是 ComboBox，特殊处理
        _Stitch(target as ComboBox);

        // 如果目标未变，只更新光标和可能的定时器
        if (ReferenceEquals(_target, target))
        {
            _cursor = pt;
            if (_flyout is not { IsOpen: true } && _latch is null)
                _KickTimer(target);
            return;
        }

        // 目标改变，但 _TryClaim 已处理内容对比，此处主要处理正在显示时的切换
        // 但 _TryClaim 已经调用了 _WindDown，所以这里只处理尚未显示但需要切换的情况
        if (_flyout is { IsOpen: true })
        {
            // 如果 Tooltip 正在显示，则关闭并重新弹出（但 _TryClaim 已处理）
            // 这里是额外保护
            _closing = false;
            _target = target;
            _cursor = pt;
            var mark = ++_gen;
            var sb = _closeStory!.Clone();
            sb.Completed += (_, _) =>
            {
                if (mark == _gen)
                {
                    _flyout.IsOpen = false;
                    _PopUp(target, pt);
                }
                sb.Remove(_shell!);
            };
            _shell!.BeginStoryboard(sb);
            return;
        }

        // 正常切换（未打开）
        _Hush();
        _target = target;
        _cursor = pt;
        _KickTimer(target);
    }

    private void _KickTimer(FrameworkElement target)
    {
        _latch?.Stop();

        var ms = Math.Max(0, ToolTipService.GetInitialShowDelay(target));
        if (ms == 0)
        {
            _PopUp(target, _cursor);
            return;
        }

        var mark = ++_gen;
        _latch = new DispatcherTimer(
            TimeSpan.FromMilliseconds(ms),
            DispatcherPriority.Normal,
            (_, _) =>
            {
                _latch?.Stop();
                if (mark == _gen && _target is not null)
                    _PopUp(_target, _cursor);
            },
            target.Dispatcher);
    }

    private void _PopUp(FrameworkElement target, Point pt)
    {
        if (!ReferenceEquals(target, _target)) return;

        if (_flyout is null)
            _BuildUi();

        _flyout!.PlacementTarget = target;
        _PlaceNear(target, pt);

        _shell!.DataContext = (target.ToolTip as ToolTip)?.DataContext ?? target.DataContext;
        _shell.FlowDirection = target.FlowDirection;

        _RenderInside(target);

        // 增加代标记，使之前的关闭动画失效
        _gen++;
        _shell.BeginStoryboard(_openStory!);
        _flyout.IsOpen = true;

        // 订阅布局更新（只在 Tooltip 打开时）
        _SubscribeLayoutUpdates();
    }

    private void _WindDown()
    {
        if (_closing) return;
        _closing = true;

        _latch?.Stop();
        _latch = null;

        // 注意：不清除 _target 和 _lastContent，由 _Hush 清理

        if (_flyout is not { IsOpen: true } || _shell is null)
        {
            _Hush();
            return;
        }

        var mark = ++_gen;
        var sb = _closeStory!.Clone();
        sb.Completed += (_, _) =>
        {
            if (mark == _gen) _Hush();
            sb.Remove(_shell);
        };
        _shell.BeginStoryboard(sb);
    }

    private void _Hush()
    {
        _latch?.Stop();
        _latch = null;
        _closing = false;
        _target = null;
        // 保留 _lastContent，仅在 _TryClaim 中遇到空内容或退出时清空
        _gen++;

        if (_flyout is not null)
            _flyout.IsOpen = false;

        if (_shell is not null)
        {
            _shell.BeginAnimation(UIElement.OpacityProperty, null);
            _shell.Child = null;
        }

        if (_scaler is not null)
        {
            _scaler.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            _scaler.BeginAnimation(ScaleTransform.ScaleYProperty, null);
        }

        // 取消布局更新订阅
        _UnsubscribeLayoutUpdates();
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

    private void _PlaceNear(FrameworkElement target, Point pt)
    {
        _flyout!.PlacementTarget = target;
        var mode = ToolTipService.GetPlacement(target);

        if (mode == PlacementMode.Mouse)
        {
            _flyout.Placement = PlacementMode.Relative;
            _flyout.PlacementRectangle = default;
            _flyout.HorizontalOffset = Math.Round(pt.X + 15 + ToolTipService.GetHorizontalOffset(target));
            _flyout.VerticalOffset = Math.Round(pt.Y + 25 + ToolTipService.GetVerticalOffset(target));
        }
        else if (mode == PlacementMode.MousePoint)
        {
            _flyout.Placement = PlacementMode.Relative;
            _flyout.PlacementRectangle = default;
            _flyout.HorizontalOffset = Math.Round(pt.X + ToolTipService.GetHorizontalOffset(target));
            _flyout.VerticalOffset = Math.Round(pt.Y + ToolTipService.GetVerticalOffset(target));
        }
        else
        {
            _flyout.Placement = mode;
            _flyout.HorizontalOffset = ToolTipService.GetHorizontalOffset(target);
            _flyout.VerticalOffset = ToolTipService.GetVerticalOffset(target);
            _flyout.PlacementRectangle = ToolTipService.GetPlacementRectangle(target);
        }

        double offset = _flyout.HorizontalOffset;
        // 给它一个极小的增量，触发重定位
        _flyout.HorizontalOffset = offset + 0.000001;
        // 立即恢复原值
        _flyout.HorizontalOffset = offset;
    }

    #endregion

    #region 布局更新

    private void _SubscribeLayoutUpdates()
    {
        if (_layoutSubscribed) return;
        _root.LayoutUpdated += _onLayoutUpdatedHandler;
        _layoutSubscribed = true;
    }

    private void _UnsubscribeLayoutUpdates()
    {
        if (!_layoutSubscribed) return;
        _root.LayoutUpdated -= _onLayoutUpdatedHandler;
        _layoutSubscribed = false;
    }

    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        if (_flyout is not { IsOpen: true } || _target is null) return;

        var mode = ToolTipService.GetPlacement(_target);
        //if (mode != PlacementMode.Mouse && mode != PlacementMode.MousePoint && !GetFollowCursor(_target))
        //    return;

        try
        {
            Point newPos = Mouse.GetPosition(_target);
            if (Math.Abs(newPos.X - _cursor.X) > 0.1 || Math.Abs(newPos.Y - _cursor.Y) > 0.1)
            {
                _cursor = newPos;
                _PlaceNear(_target, _cursor);
            }
        }
        catch
        {
            // 目标可能已被移除，忽略
        }
    }

    #endregion

    #region ComboBox钩子

    private void OnComboInit(object s, RoutedEventArgs e) => _Stitch(s as ComboBox);
    private void OnComboInit(object s, MouseButtonEventArgs e) => _Stitch(s as ComboBox);

    private void _Stitch(ComboBox? box)
    {
        if (box is null || (bool)box.GetValue(_keyCombo)) return;
        box.SetValue(_keyCombo, true);
        box.DropDownOpened += (_, _) =>
        {
            if (_target is not null) _WindDown();
        };
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        // 关闭 Tooltip
        _Hush();

        // 移除事件钩子
        _DetachRootEvents();

        // 移除 HWND 钩子
        if (_transparentHook is not null && _flyout?.Child is UIElement child)
        {
            var src = PresentationSource.FromVisual(child) as HwndSource;
            src?.RemoveHook(_transparentHook);
        }

        // 清理引用
        _flyout = null;
        _shell = null;
        _scaler = null;
        _openStory = null;
        _closeStory = null;
        _latch = null;
        _transparentHook = null;
        _target = null;
        _lastContent = null;
    }

    #endregion
}