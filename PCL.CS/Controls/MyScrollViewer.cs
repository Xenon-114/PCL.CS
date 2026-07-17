using PCL.CS.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace PCL.CS.Controls
{
    public class MyScrollViewer:ScrollViewer
    {
        public double StepDelta
        {
            get { return (double)GetValue(StepDeltaProperty); }
            set { SetValue(StepDeltaProperty, value); }
        }
        public static readonly DependencyProperty StepDeltaProperty = DependencyProperty.Register("StepDelta", typeof(double), typeof(MyScrollViewer),
            new PropertyMetadata(1.0));
        //private AnimationGroup OffsetChangeAnim;
        public double RealOffset
        {
            get { return (double)GetValue(RealOffsetProperty); }
            set { SetValue(RealOffsetProperty, value); }
        }
        public static readonly DependencyProperty RealOffsetProperty = DependencyProperty.Register("RealOffset", typeof(double), typeof(MyScrollViewer),
            new PropertyMetadata(0.0));
        public TranslateTransform BarTranslate
        {
            get => GetValue(BarTranslateProperty) as TranslateTransform;
            set => SetValue(BarTranslateProperty, value);
        }
        public static readonly DependencyProperty BarTranslateProperty = DependencyProperty.Register("BarTranslate", typeof(TranslateTransform), typeof(MyScrollViewer));
        public MyScrollViewer()
        {
            this.ScrollChanged += MyScrollViewer_ScrollChanged;
            this.Loaded += MyScrollViewer_Loaded;
            BarTranslate = new TranslateTransform();
        }
        private MyScrollBar ScrollBar;
        private void MyScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollBar = (MyScrollBar)GetTemplateChild("PART_VerticalScrollBar");
        }

        private void MyScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            RealOffset = VerticalOffset;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            ScrollDelta(-e.Delta);
        }
        private void ScrollDelta(double Delta)
        {
            Animation.Start(new MyScrollAnim(DoScroll, Delta * StepDelta));
        }
        private void DoScroll(double Delta)
        {
            RealOffset = MathHelper.Clamp(RealOffset + Delta, 0, ExtentHeight - ActualHeight);
            ScrollToVerticalOffset(RealOffset);
        }
        private class MyScrollAnim : Animation
        {
            public double Value { get; }
            public AniEase Ease { get; }
            public Action<double> Action { get; }
            private double LastValue { get; set; }
            public override object GetValue(double t)
            {
                return Ease.GetValue(t) * Value;
            }
            public override void SetValue(object Va)
            {
                double Value = (double)Va;
                double LastValue = this.LastValue;
                var Act = Action;
                Act(Value - LastValue);
                this.LastValue = Value;
            }
            public MyScrollAnim(Action<double> Act,double Value)
            {
                this.Value = Value;
                this.After = TimeSpan.Zero;
                this.TotalTime = TimeSpan.FromMilliseconds(300);
                this.Ease = new AniEaseOutFluent(6);
                this.Action = Act;
                this.LastValue = 0;
            }
        }

    }
}
