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

namespace PCL.CS.Pages
{
    /// <summary>
    /// PageGamesLeft.xaml 的交互逻辑
    /// </summary>
    public partial class PageGamesLeft : MyPageLeft
    {
        public PageGamesLeft()
        {
            InitializeComponent();
        }
        public override void Reset()
        {
            MainStack.Opacity = 0;
            MainTranslate.X = 0;
        }
        public override AnimationGroup AnimationIn()
        {
            AnimationGroup animations = new();
            animations.Add(new DoubleAnimation(MainStack, UIElement.OpacityProperty, 0, 1, 300, 0));
            animations.Add(new DoubleAnimation(MainTranslate, TranslateTransform.XProperty, -20, 0, 300, 0, new AniEaseOutFluent(3)));
            animations.TotalTime = TimeSpan.FromMilliseconds(300);
            return animations;
        }
        public override AnimationGroup AnimationOut()
        {
            AnimationGroup animations = new();
            animations.Add(new DoubleAnimation(MainStack, UIElement.OpacityProperty, MainStack.Opacity, 0, 300, 0));
            animations.Add(new DoubleAnimation(MainTranslate, TranslateTransform.XProperty, MainTranslate.X, -20, 300, 0, new AniEaseInFluent(3)));
            animations.TotalTime = TimeSpan.FromMilliseconds(300);
            return animations;
        }

    }
}
