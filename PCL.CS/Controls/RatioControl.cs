using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PCL.CS.Controls
{
    public class RatioControl : Decorator
    {
        public static readonly DependencyProperty WidthRatioProperty =
        DependencyProperty.Register("WidthRatio", typeof(double), typeof(RatioControl),
            new FrameworkPropertyMetadata(1.0,
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsArrange));

        public double WidthRatio
        {
            get => (double)GetValue(WidthRatioProperty);
            set => SetValue(WidthRatioProperty, value);
        }

        // 高度比例（0~1，默认 1 占满）
        public static readonly DependencyProperty HeightRatioProperty =
            DependencyProperty.Register("HeightRatio", typeof(double), typeof(RatioControl),
                new FrameworkPropertyMetadata(1.0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange));

        public double HeightRatio
        {
            get => (double)GetValue(HeightRatioProperty);
            set => SetValue(HeightRatioProperty, value);
        }
        protected override Size MeasureOverride(Size constraint)
        {
            if (Child is null)
                return base.MeasureOverride(constraint);

            double childWidth = double.IsPositiveInfinity(constraint.Width)
                ? constraint.Width                              
                : constraint.Width * WidthRatio;

            double childHeight = double.IsPositiveInfinity(constraint.Height)
                ? constraint.Height
                : constraint.Height * HeightRatio;

            Size ChildMeasureSize = new Size(childWidth,childHeight);
            Child.Measure(ChildMeasureSize);
            double DesWidth = Child.DesiredSize.Width / WidthRatio;
            double DesHeight = Child.DesiredSize.Height / HeightRatio;
            if (WidthRatio is 0) DesWidth = Child.DesiredSize.Width;
            if (HeightRatio is 0) DesHeight = Child.DesiredSize.Height;
            return new Size(DesWidth, DesHeight);
        }
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if(Child is null)
                return base.ArrangeOverride(arrangeSize);
            HorizontalAlignment Horo = HorizontalAlignment.Center;
            VerticalAlignment Vert = VerticalAlignment.Center;
            if(Child is FrameworkElement element)
            {
                Horo = element.HorizontalAlignment;
                Vert = element.VerticalAlignment;
            }
            double HoroOffset;
            double VertOffset;
            Size ChildSize = Child.DesiredSize;
            switch (Vert)
            {
                case VerticalAlignment.Top:
                    VertOffset = 0;
                    break;
                case VerticalAlignment.Bottom:
                    VertOffset = arrangeSize.Height - Child.DesiredSize.Height;
                    break;
                case VerticalAlignment.Center:
                    VertOffset = (arrangeSize.Height - Child.DesiredSize.Height) / 2;
                    break;
                case VerticalAlignment.Stretch:
                    VertOffset = (1 - HeightRatio) * arrangeSize.Height / 2;
                    ChildSize.Height = arrangeSize.Height * HeightRatio;
                    break;
                default:
                    VertOffset = 0;
                    break;
            }
            switch (Horo)
            {
                case HorizontalAlignment.Left:
                    HoroOffset = 0;
                    break;
                case HorizontalAlignment.Right:
                    HoroOffset = arrangeSize.Width - Child.DesiredSize.Width;
                    break;
                case HorizontalAlignment.Center:
                    HoroOffset = (arrangeSize.Width - Child.DesiredSize.Width) / 2;
                    break;
                case HorizontalAlignment.Stretch:
                    HoroOffset = (1 - WidthRatio) * arrangeSize.Width / 2;
                    ChildSize.Width = arrangeSize.Width * WidthRatio;
                    break;
                default:
                    HoroOffset = 0;
                    break;
            }
            VertOffset -= VerticalOffset * arrangeSize.Height;
            HoroOffset -= HorizontalOffset * arrangeSize.Width;

            Child.Arrange(new Rect(new Point(HoroOffset, VertOffset), ChildSize));
            return arrangeSize;
        }
        public double HorizontalOffset
        {
            get => (double)GetValue(HorizontalOffsetProperty);
            set => SetValue(HorizontalOffsetProperty, value);
        }

        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register("HorizontalOffset", typeof(double), typeof(RatioControl),
            new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange));

        public double VerticalOffset
        {
            get => (double)GetValue(VerticalOffsetProperty);
            set => SetValue(VerticalOffsetProperty, value);
        }

        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset", typeof(double), typeof(RatioControl),
            new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange));
    }
}
