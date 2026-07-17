using PCL.CS.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using XeF4Core;

namespace PCL.CS.Controls
{
    /// <summary>
    /// TitleLeft.xaml 的交互逻辑
    /// </summary>
    public partial class TitleLeft : UserControl
    {
        public string TitleText
        {
            get { return LabTitleLogo.Text; }
            set { LabTitleLogo.Text = value; }
        }
        public enum TType
        {
            Path,
            Text,
            [Obsolete("未实现")]
            Image
        }
        public TType TitleType
        {
            get { return _TitleType; }
            set
            {
                _TitleType = value;
                ShapeTitleLogo.Opacity = 0;
                LabTitleLogo.Opacity = 0;
                switch (value)
                {
                    case TType.Path:
                        ShapeTitleLogo.Opacity = 1;
                        break;
                    case TType.Text:
                        LabTitleLogo.Opacity = 1;
                        break;
                }
            }
        }
        private TType _TitleType = TType.Path;
        public string InnerTitleText
        {
            get { return LabTitleInner.Text; }
            set { LabTitleInner.Text = value; }
        }
        /// <summary>
        /// 是否是处于内页标题状态
        /// </summary>
        public bool IsInner
        {
            get { return _IsInner; }
        }
        private bool _IsInner;
        private AnimationGroup ChangeInnerAnim;
        public void ChangeTitleState(bool isInner, string InnerText = null)
        {
            if (!IsInner && !isInner) return;
            Animation.Stop(ChangeInnerAnim);
            ChangeInnerAnim = new AnimationGroup();
            ChangeInnerAnim.TotalTime = TimeSpan.FromMilliseconds(500);
            if (IsInner && isInner)
            {
                ChangeInnerAnim = new AnimationGroup
                {
                    new DoubleAnimation(LabTitleInner, OpacityProperty, LabTitleInner.Opacity, 0, 150, 0),
                    new EventAnimation(TimeSpan.FromMilliseconds(150), () => { LabTitleInner.Text = InnerText; }),
                    new DoubleAnimation(LabTitleInner, OpacityProperty, 0, 1, 150, 150),
                    new DoubleAnimation(TitleTranslate, TranslateTransform.XProperty, TitleTranslate.X, 16, 150, 0, new AniEaseInFluent(2)),
                    new DoubleAnimation(Title, Grid.OpacityProperty, Title.Opacity, 0, 150, 0),
                    new DoubleAnimation(InnerTitleTranslate, TranslateTransform.XProperty, InnerTitleTranslate.X, 0, 350, 150, new AniEaseOutBack(2)),
                    new DoubleAnimation(PanTitleInner, Grid.OpacityProperty, PanTitleInner.Opacity, 1, 150, 150)
                };
            }
            else
            {
                var animList = new AnimationGroup();
                animList.Add(new DoubleAnimation(LabTitleInner, OpacityProperty, LabTitleInner.Opacity, 1, 150, 150));
                if (IsInner)
                {
                    PanTitleInner.IsHitTestVisible = false;
                    animList.Add(new DoubleAnimation(InnerTitleTranslate, TranslateTransform.XProperty, InnerTitleTranslate.X, -16, 150, 0, new AniEaseInFluent(2)));
                    animList.Add(new DoubleAnimation(PanTitleInner, Grid.OpacityProperty, PanTitleInner.Opacity, 0, 150, 0));
                    animList.Add(new DoubleAnimation(TitleTranslate, TranslateTransform.XProperty, TitleTranslate.X, 0, 350, 150, new AniEaseOutBack(2)));
                    animList.Add(new DoubleAnimation(Title, Grid.OpacityProperty, Title.Opacity, 1, 150, 150));
                    animList.Add(new EventAnimation(TimeSpan.FromMilliseconds(150), () => { PanTitleInner.Visibility = Visibility.Collapsed; }));
                }
                if (!IsInner)
                {
                    InnerTitleText = InnerText;
                    PanTitleInner.Visibility = Visibility.Visible;
                    PanTitleInner.IsHitTestVisible = true;
                    animList.Add(new DoubleAnimation(TitleTranslate, TranslateTransform.XProperty, TitleTranslate.X, 16, 150, 0, new AniEaseInFluent(2)));
                    animList.Add(new DoubleAnimation(Title, Grid.OpacityProperty, Title.Opacity, 0, 150, 0));
                    animList.Add(new DoubleAnimation(InnerTitleTranslate, TranslateTransform.XProperty, InnerTitleTranslate.X, 0, 350, 150, new AniEaseOutBack(2)));
                    animList.Add(new DoubleAnimation(PanTitleInner, Grid.OpacityProperty, PanTitleInner.Opacity, 1, 150, 150));
                }
                _IsInner = isInner;
                ChangeInnerAnim = animList;
            }
            Animation.Start(ChangeInnerAnim);
        }
        public TitleLeft()
        {
            InitializeComponent();
            LabTitleInner.Text = "次级页面";
            BtnTitleInner.Click += BtnTitleInner_Click;
#pragma warning disable CS0162
            switch (Base.BuildType)
            {
                case BuildType.Alpha:
                    TitleHintText.Text = "Alpha";
                    break;
                case BuildType.Beta:
                    TitleHintText.Text = "Beta";
                    break;
                case BuildType.Release:
                    TitleHintBorder.Visibility = Visibility.Collapsed;
                    break;
                case BuildType.Debug:
                    TitleHintText.Text = "Debug";
                    TitleHintBorder.Background = Brushes.Orange;
                    break;
            }
#pragma warning restore CS0162
        }

        private void BtnTitleInner_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(BackEvent));
        }

        public static readonly RoutedEvent BackEvent = EventManager.RegisterRoutedEvent(
            "Back",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(TitleLeft)
        );
        public event RoutedEventHandler Back
        {
            add { AddHandler(BackEvent, value); }
            remove { RemoveHandler(BackEvent, value); }
        }
    }
}