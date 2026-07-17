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
    public class MyWinMain:Control
    {
        public Brush TitleBarBrush
        {
            get => (Brush)GetValue(TitleBarBrushProperty);
            set => SetValue(TitleBarBrushProperty, value);
        }
        public static readonly DependencyProperty TitleBarBrushProperty = DependencyProperty.Register("TitleBarBrush", typeof(Brush), typeof(MyWinMain),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public Brush PgLeftBrush
        {
            get => (Brush)GetValue(PgLeftBrushProperty);
            set => SetValue(PgLeftBrushProperty, value);
        }
        public static readonly DependencyProperty PgLeftBrushProperty = DependencyProperty.Register("PgLeftBrush", typeof(Brush), typeof(MyWinMain),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public Brush LeftChrome
        {
            get => (Brush)GetValue(LeftChromeProperty);
            set => SetValue(LeftChromeProperty, value);
        }
        public static readonly DependencyProperty LeftChromeProperty = DependencyProperty.Register("LeftChrome", typeof(Brush), typeof(MyWinMain),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public double PageLeftBackWidth
        {
            get => (double)GetValue(PageLeftBackWidthProperty);
            set => SetValue(PageLeftBackWidthProperty, value);
        }
        public static readonly DependencyProperty PageLeftBackWidthProperty = DependencyProperty.Register("PageLeftBackWidth", typeof(double), typeof(MyWinMain),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            drawingContext.DrawRectangle(Background, null, new Rect(RenderSize));
            drawingContext.DrawRectangle(TitleBarBrush, null, new Rect(0, 0, RenderSize.Width, 48));
            drawingContext.DrawRectangle(PgLeftBrush, null, new Rect(0, 48, PageLeftBackWidth, RenderSize.Height - 48));
            drawingContext.PushOpacity(Math.Min(PageLeftBackWidth / 10, 0.4));
            drawingContext.DrawRectangle(LeftChrome, null, new Rect(PageLeftBackWidth, 48, 4, RenderSize.Height - 48));
            drawingContext.Pop();
        }
    }
}
