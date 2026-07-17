using PCL.CS.Controls;
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

namespace PCL.CS.Pages
{
    /// <summary>
    /// PageLaunchLeft.xaml 的交互逻辑
    /// </summary>
    public partial class PageLaunchLeft : MyPageLeft
    {
        private static readonly AniEase EaseInOutSine = new AniEaseInOut(new AniEaseInSine(), new AniEaseOutSine());
        public PageLaunchLeft()
        {
            InitializeComponent();
        }
        public override AnimationGroup AnimationIn()
        {
            AnimationGroup aniGroup = new AnimationGroup();
            aniGroup.TotalTime = TimeSpan.FromMilliseconds(300);
            aniGroup.Add(new DoubleAnimation(MainTranslate, TranslateTransform.XProperty, -20, 0, 300, 0, new AniEaseOutFluent(3)));
            aniGroup.Add(new DoubleAnimation(this, OpacityProperty, 0, 1, 150, 0));
            return aniGroup;
        }
        public override AnimationGroup AnimationOut()
        {
            AnimationGroup aniGroup = new AnimationGroup();
            aniGroup.TotalTime = TimeSpan.FromMilliseconds(300);
            aniGroup.Add(new DoubleAnimation(MainTranslate, TranslateTransform.XProperty, MainTranslate.X, -20, 300, 0, new AniEaseInFluent(3)));
            aniGroup.Add(new DoubleAnimation(this, OpacityProperty, this.Opacity, 0, 300, 0));
            return aniGroup;
        }
        public override void Reset()
        {
            this.Opacity = 0;
        }
        private async void BtnVersion_Click(object sender, RoutedEventArgs e)
        {
            PagesContent.ChangePage(5);
        }



        private async void BtnLaunch_Click(object sender, RoutedEventArgs e)
        {
            Main.Hint("aaa");
        }
    }
}
