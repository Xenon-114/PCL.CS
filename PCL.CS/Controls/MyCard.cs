using PCL.CS.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PCL.CS.Controls
{
    public class MyCard : ContentControl
    {
        #region 公共属性
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(MyCard),
            new PropertyMetadata((d, e) => { (d as MyCard).MainRefresh(); }));

        public double ContentAnimHeight
        {
            get => (double)GetValue(ContentAnimHeightProperty);
            private set => SetValue(ContentAnimHeightProperty, value);
        }
        public static readonly DependencyProperty ContentAnimHeightProperty = DependencyProperty.Register("ContentAnimHeight", typeof(double), typeof(MyCard));

        public bool IsSwapped
        {
            get => _Swapped;
            set {
                _Swapped = value;
                if (ContentPre != null)
                    ContentPre.IsHitTestVisible = !value;
                MainRefresh();
            }
        }

        public bool CanSwap
        {
            get => _CanSwap;
            set
            {
                _CanSwap = value;
                MainRefresh();
            }
        }

        public RotateTransform BtnRotate
        {
            get => (RotateTransform)GetValue(BtnRotateProperty);
            private set => SetValue(BtnRotateProperty, value);
        }
        public static readonly DependencyProperty BtnRotateProperty = DependencyProperty.Register("BtnRotate", typeof(RotateTransform), typeof(MyCard));

        #endregion

        private UIElement PART_Title { get; set; }

        private UIElement Arrow { get; set; }

        #region 主刷新逻辑
        private double _ContentHeight;
        private void MainRefresh(bool UsingAnim = true)
        {
            if (!this.IsLoaded || !this.IsVisible) UsingAnim = false;
            if (PART_Title != null)
            {
                if (!this.CanSwap && (this.Title == null || this.Title == ""))
                    PART_Title.Visibility = Visibility.Collapsed;
                else PART_Title.Visibility = Visibility.Visible;
            }
            Animation.Stop(BtnRotateAnim);
            if (!this.CanSwap)
            {
                BtnRotate.Angle = 0;
                if (Arrow != null)
                    Arrow.Visibility = Visibility.Hidden;
            }
            else
            {
                if (Arrow != null)
                    Arrow.Visibility = Visibility.Visible;
                if (UsingAnim)
                    BtnRotateChange(this.IsSwapped ? 0 : 180);
                else
                {
                    BtnRotate.Angle = this.IsSwapped ? 0 : 180;
                }
            }
            if (_ContentHeight != ContentHeight)
            {
                if (UsingAnim)
                {
                    ContentSizeChange();
                    _ContentHeight = ContentHeight;
                }
                else
                {
                    Animation.Stop(HeightChangeAnim);
                    _ContentHeight = ContentHeight;
                    ContentAnimHeight = ContentHeight;
                }
            }
        }
        #endregion

        private bool _Swapped = false;
        private bool _CanSwap = true;

        private ContentPresenter ContentPre = null;

        private double HeightChangeAnimLenth
        {
            get => Math.Sqrt(Math.Sqrt(Math.Abs(ContentAnimHeight - ContentHeight))) * 100;
        }
        
        private double ContentHeight
        {
            get
            {
                if (this.IsSwapped || ContentPre is null) return 0;
                return ContentPre.ActualHeight;
            }
        }
        public MyCard()
        {
            this.Loaded += (s, e) =>
            {
                if (Foreground is null || Foreground.IsFrozen)
                    this.Foreground = new SolidColorBrush();
                ContentAnimHeight = ContentHeight;
                BtnRotate = new RotateTransform();
                MainRefresh(false);
                Arrow.Visibility = CanSwap ? Visibility.Visible : Visibility.Hidden;
            };
            this.BtnRotate = new RotateTransform();
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            MyBorder BackBorder = GetTemplateChild("PART_BackBorder") as MyBorder;
            var Des = DependencyPropertyDescriptor.FromProperty(MyBorder.BorderColorProperty, typeof(MyBorder));
            Des.AddValueChanged(BackBorder, (s, e) =>
            {
                var Foreground = this.Foreground as SolidColorBrush;
                if (Foreground is null || Foreground.IsFrozen) this.Foreground = Foreground = new SolidColorBrush();
                Foreground.Color = (s as MyBorder).BorderColor;
            });
            ContentPre = GetTemplateChild("PART_Content") as ContentPresenter;
            ContentPre.SizeChanged += (s,e)=>
            {
                if (e.NewSize.Height != e.PreviousSize.Height) ContentSizeChange();
            };
            PART_Title = GetTemplateChild("PART_Title") as UIElement;
            Arrow = GetTemplateChild("PART_Arrow") as UIElement;
            (GetTemplateChild("PART_BtnExpand") as Button).Click += EventUnSwap;
            (GetTemplateChild("PART_BtnSwap") as Button).Click += EventSwap;
        }

        #region 主事件处理
        private void EventUnSwap(object sender,EventArgs e)
        {
            if (!this.CanSwap) return;
            if (this.IsSwapped)
                this.IsSwapped = false;
        }
        private void EventSwap(object sender, EventArgs e)
        {
            if (!this.CanSwap) return;
            if (!this.IsSwapped)
                this.IsSwapped = true;
            else EventUnSwap(sender, e);
        }
        #endregion

        private AnimationGroup HeightChangeAnim;
        private void ContentSizeChange()
        {
            if (!this.IsLoaded) return;
            Animation.Stop(HeightChangeAnim);
            HeightChangeAnim = new AnimationGroup();
            HeightChangeAnim.TotalTime = TimeSpan.FromMilliseconds(HeightChangeAnimLenth);
            HeightChangeAnim.Add(new DoubleAnimation(this, ContentAnimHeightProperty, this.ContentAnimHeight, this.ContentHeight, HeightChangeAnimLenth, 0, new AniEaseOutFluent(3)));
            Animation.Start(HeightChangeAnim);
        }
        private AnimationGroup BtnRotateAnim;
        private void BtnRotateChange(double Target)
        {
            if (!this.IsLoaded) return;
            Animation.Stop(BtnRotateAnim);
            BtnRotateAnim = new AnimationGroup();
            BtnRotateAnim.TotalTime = TimeSpan.FromMilliseconds(400);
            BtnRotateAnim.Add(new DoubleAnimation(BtnRotate, RotateTransform.AngleProperty, BtnRotate.Angle, Target, 400, 0, new AniEaseOutFluent(3)));
            Animation.Start(BtnRotateAnim);
        }
    }
}
