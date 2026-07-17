using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace PCL.CS.Controls
{
    public class MyDropShadow: Decorator
    {
        private GuidelineSet guidelineSet = null;
        private PathFigure figure = null;
        private Queue<LineSegment> LineSegments = new Queue<LineSegment>();
        private void ClearPathSegs(PathSegmentCollection Segs)
        {
            foreach(PathSegment seg in Segs)
            {
                LineSegments.Enqueue(seg as LineSegment);
            }
            Segs.Clear();
        }
        PathGeometry geometry = null;
        private LineSegment newLineSegment(Point point, bool isStroked)
        {
            LineSegment TargetSegs;
            if (LineSegments.Any())
            {
                TargetSegs = LineSegments.Dequeue();
                TargetSegs.Point = point;
                TargetSegs.IsStroked = isStroked;
            }
            else
            {
                TargetSegs = new LineSegment(point, isStroked);
            }
            return TargetSegs;
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            CornerRadius cornerRadius = this.CornerRadius;
            Rect shadowBounds = new Rect(0, 0, RenderSize.Width, RenderSize.Height);
            Color color = this.Color;
            if (shadowBounds.Width < 0 || shadowBounds.Height < 0 || color.A < 0) return;
            // 基本检查：确保有绘制内容

            // 计算中心区域（去除阴影半径影响后的内部区域）
            double centerWidth = shadowBounds.Right - shadowBounds.Left - 2 * ShadowRadius;
            double centerHeight = shadowBounds.Bottom - shadowBounds.Top - 2 * ShadowRadius;

            // 限制圆角大小不超过控件中心区域的一半
            double maxRadius = Math.Min(centerWidth * 0.5, centerHeight * 0.5);
            cornerRadius.TopLeft = Math.Min(cornerRadius.TopLeft, maxRadius);
            cornerRadius.TopRight = Math.Min(cornerRadius.TopRight, maxRadius);
            cornerRadius.BottomLeft = Math.Min(cornerRadius.BottomLeft, maxRadius);
            cornerRadius.BottomRight = Math.Min(cornerRadius.BottomRight, maxRadius);

            // 获取画笔
            if (this.IsChanged)
            {
                this.IsChanged = false;
                CreateBrush(color, cornerRadius);
            }

            // 计算九宫格各个区域的坐标线
            double centerTop = shadowBounds.Top + ShadowRadius;
            double centerLeft = shadowBounds.Left + ShadowRadius;
            double centerRight = shadowBounds.Right - ShadowRadius;
            double centerBottom = shadowBounds.Bottom - ShadowRadius;

            // X方向坐标线：定义了九宫格在水平方向的分隔点
            double[] guidelineSetX = new double[]
            {
                centerLeft,
                centerLeft + cornerRadius.TopLeft,
                centerRight - cornerRadius.TopRight,
                centerLeft + cornerRadius.BottomLeft,
                centerRight - cornerRadius.BottomRight,
                centerRight
            };

            // Y方向坐标线：定义了九宫格在垂直方向的分隔点
            double[] guidelineSetY = new double[]
            {
                centerTop,
                centerTop + cornerRadius.TopLeft,
                centerTop + cornerRadius.TopRight,
                centerBottom - cornerRadius.BottomLeft,
                centerBottom - cornerRadius.BottomRight,
                centerBottom
            };

            // 应用像素对齐指南，防止渲染时出现模糊边缘
            if (guidelineSet is null)
                guidelineSet = new GuidelineSet();
            guidelineSet.GuidelinesX = new DoubleCollection(guidelineSetX);
            guidelineSet.GuidelinesY = new DoubleCollection(guidelineSetY);
            drawingContext.PushGuidelineSet(guidelineSet);
            

            // 为了绘制角区域，需要将圆角半径加上阴影半径
            // 这样角区域就能包含阴影模糊部分
            cornerRadius.TopLeft += ShadowRadius;
            cornerRadius.TopRight += ShadowRadius;
            cornerRadius.BottomLeft += ShadowRadius;
            cornerRadius.BottomRight += ShadowRadius;

            // 1. 绘制左上角（圆形渐变）
            Rect topLeft = new Rect(shadowBounds.Left, shadowBounds.Top,
                                    cornerRadius.TopLeft, cornerRadius.TopLeft);
            drawingContext.DrawRectangle(TLBrush, null, topLeft);

            // 2. 绘制上边（线性渐变）
            double topWidth = guidelineSetX[2] - guidelineSetX[1];
            if (topWidth > 0)
            {
                Rect top = new Rect(guidelineSetX[1], shadowBounds.Top, topWidth, ShadowRadius);
                drawingContext.DrawRectangle(TopBrush, null, top);
            }

            // 3. 绘制右上角（圆形渐变）
            Rect topRight = new Rect(guidelineSetX[2], shadowBounds.Top,
                                     cornerRadius.TopRight, cornerRadius.TopRight);
            drawingContext.DrawRectangle(TRBrush, null, topRight);

            // 4. 绘制左边（线性渐变）
            double leftHeight = guidelineSetY[3] - guidelineSetY[1];
            if (leftHeight > 0)
            {
                Rect left = new Rect(shadowBounds.Left, guidelineSetY[1], ShadowRadius, leftHeight);
                drawingContext.DrawRectangle(LeftBrush, null, left);
            }

            // 5. 绘制右边（线性渐变）
            double rightHeight = guidelineSetY[4] - guidelineSetY[2];
            if (rightHeight > 0)
            {
                Rect right = new Rect(guidelineSetX[5], guidelineSetY[2], ShadowRadius, rightHeight);
                drawingContext.DrawRectangle(RightBrush, null, right);
            }

            // 6. 绘制左下角（圆形渐变）
            Rect bottomLeft = new Rect(shadowBounds.Left, guidelineSetY[3],
                                       cornerRadius.BottomLeft, cornerRadius.BottomLeft);
            drawingContext.DrawRectangle(BLBrush, null, bottomLeft);

            // 7. 绘制下边（线性渐变）
            double bottomWidth = guidelineSetX[4] - guidelineSetX[3];
            if (bottomWidth > 0)
            {
                Rect bottom = new Rect(guidelineSetX[3], guidelineSetY[5], bottomWidth, ShadowRadius);
                drawingContext.DrawRectangle(BottomBrush, null, bottom);
            }

            // 8. 绘制右下角（圆形渐变）
            Rect bottomRight = new Rect(guidelineSetX[4], guidelineSetY[4],
                                        cornerRadius.BottomRight, cornerRadius.BottomRight);
            drawingContext.DrawRectangle(BRBrush, null, bottomRight);

            // 9. 绘制中心区域
            // 判断是否所有圆角都等于阴影半径（即是否有自定义圆角）
            if (cornerRadius.TopLeft == ShadowRadius &&
                cornerRadius.TopLeft == cornerRadius.TopRight &&
                cornerRadius.TopLeft == cornerRadius.BottomLeft &&
                cornerRadius.TopLeft == cornerRadius.BottomRight)
            {
                // 情况1：所有角都是相同的标准圆角，直接绘制矩形
                Rect center = new Rect(guidelineSetX[0], guidelineSetY[0], centerWidth, centerHeight);
                drawingContext.DrawRectangle(MidBrush, null, center);
            }
            else
            {
                // 情况2：有自定义圆角，需要绘制不规则的中心区域
                if (figure is null)
                    figure = new PathFigure();
                else
                    figure.IsClosed = true;
                ClearPathSegs(figure.Segments);
                // 根据每个角的情况，构建路径
                if (cornerRadius.TopLeft > ShadowRadius)
                {
                    // 左上角有额外圆角，从内部点开始
                    figure.StartPoint = new Point(guidelineSetX[1], guidelineSetY[0]);
                    figure.Segments.Add(newLineSegment(new Point(guidelineSetX[1], guidelineSetY[1]), true));
                    figure.Segments.Add(newLineSegment(new Point(guidelineSetX[0], guidelineSetY[1]), true));
                }
                else
                {
                    // 左上角是标准圆角，直接从顶点开始
                    figure.StartPoint = new Point(guidelineSetX[0], guidelineSetY[0]);
                }

                // 处理左下角
                if (cornerRadius.BottomLeft > ShadowRadius)
                {
                    figure.Segments.Add(newLineSegment(new Point(guidelineSetX[0], guidelineSetY[3]), true));
                    figure.Segments.Add(newLineSegment(new Point(guidelineSetX[3], guidelineSetY[3]), true));
                    figure.Segments.Add(newLineSegment(new Point(guidelineSetX[3], guidelineSetY[5]), true));
                }
                else
                {
                    figure.Segments.Add(newLineSegment(new Point(guidelineSetX[0], guidelineSetY[5]), true));
                }

                // 处理右下角
                if (cornerRadius.BottomRight > ShadowRadius)
                {
                    figure.Segments.Add(newLineSegment(new Point(guidelineSetX[4], guidelineSetY[5]), true));
                    figure.Segments.Add(newLineSegment(new Point(guidelineSetX[4], guidelineSetY[4]), true));
                    figure.Segments.Add(newLineSegment(new Point(guidelineSetX[5], guidelineSetY[4]), true));
                }
                else
                {
                    figure.Segments.Add(newLineSegment(new Point(guidelineSetX[5], guidelineSetY[5]), true));
                }

                // 处理右上角
                if (cornerRadius.TopRight > ShadowRadius)
                {
                    figure.Segments.Add(newLineSegment(new Point(guidelineSetX[5], guidelineSetY[2]), true));
                    figure.Segments.Add(newLineSegment(new Point(guidelineSetX[2], guidelineSetY[2]), true));
                    figure.Segments.Add(newLineSegment(new Point(guidelineSetX[2], guidelineSetY[0]), true));
                }
                else
                {
                    figure.Segments.Add(newLineSegment(new Point(guidelineSetX[5], guidelineSetY[0]), true));
                }

                figure.IsClosed = true;

                if (geometry is null)
                    geometry = new PathGeometry();
                geometry.Figures.Clear();
                geometry.Figures.Add(figure);

                drawingContext.DrawGeometry(MidBrush, null, geometry);
            }

            // 恢复之前的绘制状态
            drawingContext.Pop();

        }
        /// <summary>
        /// 阴影颜色
        /// </summary>
        public Color Color
        {
            get { return _Color; }
            set {  SetValue(ColorProperty, value);_Color = value; }
        }
        private Color _Color= Color.FromArgb(0x71, 0x0, 0x0, 0x0);
        /// <summary>
        /// 阴影颜色依赖属性
        /// </summary>
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(MyDropShadow),
                new PropertyMetadata(Color.FromArgb(0x71,0x0,0x0,0x0), ReSetBrush));
        /// <summary>
        /// 阴影模糊半径
        /// </summary>
        public double ShadowRadius
        {
            get { return _ShadowRadius; }
            set { SetValue(ShadowRadiusProperty, value);_ShadowRadius = value; }
        }
        private double _ShadowRadius=5.0;
        /// <summary>
        /// 阴影模糊半径依赖属性
        /// </summary>
        public static readonly DependencyProperty ShadowRadiusProperty =
            DependencyProperty.Register("ShadowRadius", typeof(double), typeof(MyDropShadow),
                new PropertyMetadata(5.0, ReSetBrush));
        /// <summary>
        /// 圆角半径
        /// </summary>
        public CornerRadius CornerRadius
        {
            get { return _CornerRadius; }
            set { SetValue(CornerRadiusProperty, value);_CornerRadius = value; }
        }
        private CornerRadius _CornerRadius = new CornerRadius();
        /// <summary>
        /// 圆角半径依赖属性
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(MyDropShadow),
                new PropertyMetadata(new CornerRadius(), ReSetBrush));
        private static void ReSetBrush(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            switch (e.NewValue)
            {
                case Color value:
                    ((MyDropShadow)d)._Color = value;
                    break;
                case CornerRadius value:
                    ((MyDropShadow)d)._CornerRadius = value;
                    break;
                case double value:
                    ((MyDropShadow)d)._ShadowRadius = value;
                    break;
            }
            ((MyDropShadow)d).IsChanged = true;
            ((MyDropShadow)d).InvalidateVisual();
        }
        private bool IsChanged = false;
        private Queue<GradientStop> StopsQueue = new Queue<GradientStop>();//保存对象以便复用
        private GradientStop GetGradientStop(Color color,double Offset)
        {
            if (StopsQueue.Any())
            {
                GradientStop gradientStop = StopsQueue.Dequeue();
                gradientStop.Offset = Offset;
                gradientStop.Color = color;
                return gradientStop;
            }
            else
            {
                return new GradientStop(color, Offset);
            }
        }
        private GradientStopCollection GradientStops(Color C,double cornerRadius,GradientStopCollection GSC = null)
        {
            if (GSC is null) GSC = new GradientStopCollection();
            double gradientScale = 1 / (ShadowRadius + cornerRadius);
            Color thisColor=C;
            ClearGStops(GSC);
            //加入一个渐变点
            GSC.Add(GetGradientStop(thisColor, (ShadowRadius * 0.1 + cornerRadius) * gradientScale));
            thisColor.A = (byte)(C.A * 0.74336);
            //后面也是一样的道理
            GSC.Add(GetGradientStop(thisColor, (ShadowRadius * 0.3 + cornerRadius) * gradientScale));
            thisColor.A = (byte)(C.A * 0.38053);
            GSC.Add(GetGradientStop(thisColor, (ShadowRadius * 0.5 + cornerRadius) * gradientScale));
            thisColor.A = (byte)(C.A * 0.12389);
            GSC.Add(GetGradientStop(thisColor, (ShadowRadius * 0.7 + cornerRadius) * gradientScale));
            thisColor.A = (byte)(C.A * 0.02654);
            GSC.Add(GetGradientStop(thisColor, (ShadowRadius * 0.9 + cornerRadius) * gradientScale));
            thisColor.A = 0;
            GSC.Add(GetGradientStop(thisColor, (ShadowRadius + cornerRadius) * gradientScale));
            return GSC;
        }
        private void ClearGStops(GradientStopCollection TargetGSC)
        {
            if (TargetGSC is null) return;
            foreach(GradientStop Gstop in TargetGSC)
            {
                StopsQueue.Enqueue(Gstop);
            }
            TargetGSC.Clear();
        }
        
        private void CreateBrush(Color C, CornerRadius cornerRadius)
        {
            //先保存一下我刚生成的渐变点
            GradientStopCollection sideGSC = TopBrush?.GradientStops;
            if(sideGSC is null)
                sideGSC = new GradientStopCollection();
            sideGSC = GradientStops(C, 0, sideGSC);
            if (MidBrush is null)
                MidBrush = new SolidColorBrush(C);
            else
                MidBrush.Color = C;
            if (TopBrush is null)
                TopBrush = new LinearGradientBrush(sideGSC, new Point(0, 1), new Point(0, 0));
            if (BottomBrush is null)
                BottomBrush = new LinearGradientBrush(sideGSC, new Point(0, 0), new Point(0, 1));
            if (LeftBrush is null)
                LeftBrush = new LinearGradientBrush(sideGSC, new Point(1, 0), new Point(0, 0));
            if (RightBrush is null)
                RightBrush = new LinearGradientBrush(sideGSC, new Point(0, 0), new Point(1, 0));
            if (TLBrush is null)
                TLBrush = new RadialGradientBrush(cornerRadius.TopLeft == 0 ? sideGSC : GradientStops(C, CornerRadius.TopLeft))
                {
                    RadiusX = 1,
                    RadiusY = 1,
                    Center = new Point(1, 1),
                    GradientOrigin = new Point(1, 1)
                };
            else
                TLBrush.GradientStops = cornerRadius.TopLeft == 0 ? sideGSC : GradientStops(C, CornerRadius.TopLeft, TLBrush.GradientStops);
            if (TRBrush is null)
                TRBrush = new RadialGradientBrush(cornerRadius.TopRight == 0 ? sideGSC : GradientStops(C, CornerRadius.TopRight))
                {
                    RadiusX = 1,
                    RadiusY = 1,
                    Center = new Point(0, 1),
                    GradientOrigin = new Point(0, 1)
                };
            else
                TRBrush.GradientStops = cornerRadius.TopRight == 0 ? sideGSC : GradientStops(C, CornerRadius.TopRight, TLBrush.GradientStops);
            if (BLBrush is null)
                BLBrush = new RadialGradientBrush(cornerRadius.TopRight == 0 ? sideGSC : GradientStops(C, CornerRadius.TopRight))
                {
                    RadiusX = 1,
                    RadiusY = 1,
                    Center = new Point(1, 0),
                    GradientOrigin = new Point(1, 0)
                };
            else
                BLBrush.GradientStops = cornerRadius.TopRight == 0 ? sideGSC : GradientStops(C, CornerRadius.TopRight,BLBrush.GradientStops);
            if (BRBrush is null)
                BRBrush = new RadialGradientBrush(cornerRadius.TopRight == 0 ? sideGSC : GradientStops(C, CornerRadius.TopRight))
                {
                    RadiusX = 1,
                    RadiusY = 1,
                    Center = new Point(0, 0),
                    GradientOrigin = new Point(0, 0)
                };
            else
                BRBrush.GradientStops = cornerRadius.TopRight == 0 ? sideGSC : GradientStops(C, CornerRadius.TopRight,BRBrush.GradientStops);
        }
        //资源缓存
        private SolidColorBrush MidBrush=null;
        private LinearGradientBrush TopBrush=null;
        private LinearGradientBrush BottomBrush = null;
        private LinearGradientBrush LeftBrush = null;
        private LinearGradientBrush RightBrush = null;
        private RadialGradientBrush TLBrush = null;
        private RadialGradientBrush TRBrush = null;
        private RadialGradientBrush BLBrush = null;
        private RadialGradientBrush BRBrush = null;


        public MyDropShadow()
        {
            this.Unloaded += (s, e) => Cleanup();
        }
        private void Cleanup()
        {
            // 清空线段池
            LineSegments.Clear();
            // 清空渐变点池
            StopsQueue.Clear();
            // 释放画刷（设为 null 让 GC 回收）
            MidBrush = null;
            TopBrush = null;
            BottomBrush = null;
            LeftBrush = null;
            RightBrush = null;
            TLBrush = null;
            TRBrush = null;
            BLBrush = null;
            BRBrush = null;
            // 释放几何对象
            figure = null;
            geometry = null;
            guidelineSet = null;
        }
    }
}
