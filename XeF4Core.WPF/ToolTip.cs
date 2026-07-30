using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

using nint = System.IntPtr;

namespace XeF4Core.WPF;

public partial class MyToolTip : DependencyObject
{
    #region 依赖属性
    public Brush Background
    {
        get { return (Brush)GetValue(BackgroundProperty); }
        set { SetValue(BackgroundProperty, value); }
    }

    public static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(MyToolTip), new PropertyMetadata(Brushes.White));

    public Brush BorderBrush
    {
        get { return (Brush)GetValue(BorderBrushProperty); }
        set { SetValue(BorderBrushProperty, value); }
    }

    public static readonly DependencyProperty BorderBrushProperty =
        DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(MyToolTip), new PropertyMetadata(Brushes.Gray));

    public Brush Foreground
    {
        get { return (Brush)GetValue(ForegroundProperty); }
        set { SetValue(ForegroundProperty, value); }
    }

    public static readonly DependencyProperty ForegroundProperty =
        DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(MyToolTip), new PropertyMetadata(Brushes.Black));



    #endregion

    private FrameworkElement Root { get; set; }
    public MyToolTip(FrameworkElement rootElement)
    {
        Root = rootElement;
        AttendEvents();
        ToolTipBox = new Popup();
        CreatePopup();
        _ = ContentCtrl ?? throw new NullReferenceException();
        _ = ContentBase ?? throw new NullReferenceException();
    }
    private readonly ScaleTransform ToolTipScale = new();
    private ContentControl ContentCtrl;
    private UIElement ContentBase;
    private Popup CreatePopup()
    {
        Popup popup = new()
        {
            AllowsTransparency = true,
            IsHitTestVisible = false,
            StaysOpen = true,
            PopupAnimation = PopupAnimation.None
        };
        
        popup.IsOpen = true;
        var src = PresentationSource.FromVisual(popup) as HwndSource;
        popup.IsOpen = false;
        if (src is null) throw new ArithmeticException("src转换失败");

        src.AddHook(_transparentHook);

        static nint _transparentHook(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
        {
            const int WM_NCHITTEST = 0x0084;
            const int HTTRANSPARENT = -1;

            if (msg == WM_NCHITTEST)
            {
                handled = true;
                return (nint)HTTRANSPARENT;
            }
            return nint.Zero;
        }

        Grid Child = new();
        Child.RenderTransformOrigin = new Point(0.1, 0.1);
        Child.RenderTransform = ToolTipScale;

        (popup as IAddChild).AddChild(Child);

        {
            var border = new Border();
            border.SetBinding(Border.BackgroundProperty, new Binding(nameof(Background)) { Source = this });
            border.SetBinding(Border.BorderBrushProperty, new Binding(nameof(BorderBrush)) { Source = this });

            var content = new ContentControl();
            border.Child = content;
            ContentCtrl = content;
            ContentBase = Child;

            Child.Children.Add(border);

        }

        return popup;
    }
    private Popup ToolTipBox;
    private void AttendEvents()
    {
        Root.AddHandler(UIElement.MouseEnterEvent, MouseEvents, handledEventsToo: true);
        Root.AddHandler(UIElement.MouseMoveEvent, OnToolTipMove, handledEventsToo: true);
        Root.LayoutUpdated += OnToolTipMove;
        Root.AddHandler(UIElement.MouseLeaveEvent, MouseEvents, handledEventsToo: true);
    }
    private void MouseEvents(object sender, MouseEventArgs e) =>
        Refresh();    
    private void OnToolTipMove(object sender, EventArgs e) =>
        RefreshToolTip();
    private FrameworkElement? GetToolTipFather(IInputElement MouseDirectlyOver)
    {
        if (MouseDirectlyOver is not DependencyObject Element) return null;
        while (true)
        {
            if (Element is FrameworkElement Felement && IsValidToolTipTarget(Felement))
                return Felement;
            Element = GetParent(Element);
        }
    }
    private DependencyObject GetParent(DependencyObject Object)
    {
        var Parent = VisualTreeHelper.GetParent(Object);
        Parent ??= LogicalTreeHelper.GetParent(Object);
        return Parent;
    }


    
}


public partial class MyToolTip
{
    private bool IsValidToolTipTarget(FrameworkElement fe)
    {
        // 1. 必须启用 ToolTipService（和你的附加属性 IsEnabled）
        if (!ToolTipService.GetIsEnabled(fe)) return false;
        // 如果你有自定义的附加属性，也在这里检查
        // if (!GetIsEnabled(fe)) return false;

        // 2. 内容必须存在
        var content = GetToolTipContent(fe);
        if (content is null) return false;
        if (content is string str && string.IsNullOrEmpty(str)) return false;

        return true;
    }
    private object? GetToolTipContent(FrameworkElement fe)
    {
        var raw = fe.ToolTip;
        if (raw is null) return null;
        // 如果用户写的是 <Button.ToolTip><ToolTip>内容</ToolTip></Button.ToolTip>
        if (raw is ToolTip tip) return tip.Content;
        return raw;
    }
}
public enum MyToolTipState
{
    /// <summary>
    /// 空闲，没有任务
    /// </summary>
    Nothing,
    /// <summary>
    /// 等待弹出
    /// </summary>
    Waiting,
    /// <summary>
    /// 已弹出
    /// </summary>
    Showing,
    /// <summary>
    /// 正在改变，但没有应用新值
    /// </summary>
    Changing,
    /// <summary>
    /// 正在关闭
    /// </summary>
    Closing
}

public partial class MyToolTip
{
    private readonly struct TooltipInfo(PlacementMode placement, double horizontalOffset, double verticalOffset, object content)
    {
        public PlacementMode Placement { get; } = placement;
        public double HorizontalOffset { get; } = horizontalOffset;
        public double VerticalOffset { get; } = verticalOffset;
        public object Content { get; } = content;
    }
    private TooltipInfo? NowToolTip = null;
    private UIElement? NowToolTipObject;
    private (UIElement NewToolTipObject, TooltipInfo NewToolTip)? NewToolTip;
    private MyToolTipState State = MyToolTipState.Nothing;
    private IInputElement? LastElement = null;
    private TooltipInfo? GetToolTip(FrameworkElement? felement)
    {
        if (felement is null) return null;
        var Content = felement.ToolTip;
        if (Content is null) return null;
        var placement = ToolTipService.GetPlacement(felement);
        var HorOffset = ToolTipService.GetHorizontalOffset(felement);
        var VerOffset = ToolTipService.GetVerticalOffset(felement);
        if (Content is ToolTip tip) Content = tip.Content;
        return new TooltipInfo(placement, HorOffset, VerOffset, Content);
    }
    private int _gen = 0;
    private Storyboard? StartAnim;
    private Storyboard? CloseAnim;
    private void Refresh()
    {
        var Element = Mouse.DirectlyOver;
        if (Element == LastElement) return;
        LastElement = Element;
        var ToolTipElement = GetToolTipFather(Element);
        if (ToolTipElement == NowToolTipObject) return;
        TooltipInfo? TipInfo;
        //1、获取ToolTip内容
        //状态机：关闭
        if (ToolTipElement is null || ((TipInfo = GetToolTip(ToolTipElement)) is null))
        {
            if (State is MyToolTipState.Nothing) return;
            if (State is MyToolTipState.Waiting) { _gen++; return; }
            if (State is MyToolTipState.Showing)
            {
                State = MyToolTipState.Closing;
                CloseAnim ??= GetCloseAnimation();
                CloseAnim.Begin();
                return;
            }
            if (State is MyToolTipState.Changing) { State = MyToolTipState.Closing; return; }
            if (State is MyToolTipState.Closing) return;
            return;
        }
        //状态机：开启
        if (State is MyToolTipState.Nothing || State is MyToolTipState.Waiting)
        {

        }
    }
    private DispatcherTimer? Timer = null;
    private readonly static BackEase EaseStart = new() { Amplitude = 1.1, EasingMode = EasingMode.EaseOut };
    private Storyboard GetShowAnimation()
    {
        Storyboard storyboard = new();
        var AniScaleX = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(200)) { EasingFunction = EaseStart };
        Storyboard.SetTarget(AniScaleX, ToolTipScale);
        Storyboard.SetTargetProperty(AniScaleX, new PropertyPath(nameof(ToolTipScale.ScaleX)));
        var AniScaleY = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(200)) { EasingFunction = EaseStart };
        Storyboard.SetTarget(AniScaleY, ToolTipScale);
        Storyboard.SetTargetProperty(AniScaleY, new PropertyPath(nameof(ToolTipScale.ScaleY)));
        storyboard.Children.Add(AniScaleX);
        storyboard.Children.Add(AniScaleY);
        var AniOpac = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(150));
        Storyboard.SetTarget(AniOpac, this);
        Storyboard.SetTargetProperty(AniOpac, new PropertyPath(nameof(ContentBase.Opacity)));
        
        return storyboard;
    }
    private Storyboard GetCloseAnimation()
    {
        Storyboard storyboard = new();
        var AniScaleX = new DoubleAnimation(0.9, TimeSpan.FromMilliseconds(150));
        Storyboard.SetTarget(AniScaleX, ToolTipScale);
        Storyboard.SetTargetProperty(AniScaleX, new PropertyPath(nameof(ToolTipScale.ScaleX)));
        var AniScaleY = new DoubleAnimation(0.9, TimeSpan.FromMilliseconds(150));
        Storyboard.SetTarget(AniScaleY, ToolTipScale);
        Storyboard.SetTargetProperty(AniScaleY, new PropertyPath(nameof(ToolTipScale.ScaleY)));
        storyboard.Children.Add(AniScaleX);
        storyboard.Children.Add(AniScaleY);
        var AniOpac = new DoubleAnimation(0.0, TimeSpan.FromMilliseconds(150));
        Storyboard.SetTarget(AniOpac, this);
        Storyboard.SetTargetProperty(AniOpac, new PropertyPath(nameof(ContentBase.Opacity)));

        storyboard.Completed += (s, e) =>
        {
            if(this.State is MyToolTipState.Changing)
            {
                if (NewToolTip is null)
                {
                    State = MyToolTipState.Nothing;
                    return;
                }
                NowToolTip = NewToolTip?.NewToolTip;
                NowToolTipObject = NewToolTip?.NewToolTipObject;
                StartAnim ??= GetShowAnimation();
                StartAnim.Begin();
            }
            if (this.State is not MyToolTipState.Closing) return;
            this.State = MyToolTipState.Nothing;
            NowToolTip = null;
            NowToolTipObject = null;
            return;
        };

        return storyboard;
    }
    private void CloseToolTip()
    {

    }
    private void ShowToolTip()
    {
        DispatcherTimer timer = new() { };
    }
    private void RefreshToolTip()
    {
        if (NowToolTipObject is null) return;
        if (NowToolTip is null) return;
        PlacementMode mode = NowToolTip?.Placement ?? PlacementMode.Mouse;

        ContentCtrl.Content = NowToolTip?.Content;

        Point screenPos = Root.PointToScreen(Mouse.GetPosition(Root));

        if (mode == PlacementMode.Mouse || mode == PlacementMode.MousePoint)
        {
            // 3a. 鼠标跟随模式：使用 Relative 模式，并手动计算偏移
            ToolTipBox.Placement = PlacementMode.Relative;
            ToolTipBox.PlacementRectangle = Rect.Empty; // 清空矩形区域

            // 计算偏移：鼠标坐标 + 用户设置的偏移量
            double offsetX = screenPos.X + ToolTipService.GetHorizontalOffset(NowToolTipObject);
            double offsetY = screenPos.Y + ToolTipService.GetVerticalOffset(NowToolTipObject);

            // 如果是 Mouse 模式，额外增加一个默认偏移，让提示出现在鼠标右下角
            if (mode == PlacementMode.Mouse)
            {
                offsetX += 15;
                offsetY += 25;
            }

            ToolTipBox.HorizontalOffset = Math.Round(offsetX);
            ToolTipBox.VerticalOffset = Math.Round(offsetY);
        }
        else
        {
            ToolTipBox.Placement = mode;
            ToolTipBox.PlacementTarget = NowToolTipObject;
            ToolTipBox.PlacementRectangle = ToolTipService.GetPlacementRectangle(NowToolTipObject);
            ToolTipBox.HorizontalOffset = ToolTipService.GetHorizontalOffset(NowToolTipObject);
            ToolTipBox.VerticalOffset = ToolTipService.GetVerticalOffset(NowToolTipObject);
        }
    }
}