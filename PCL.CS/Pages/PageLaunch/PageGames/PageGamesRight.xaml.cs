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
    /// PageGamesRight.xaml 的交互逻辑
    /// </summary>
    public partial class PageGamesRight : MyPageRight
    {
        public PageGamesRight()
        {
            InitializeComponent();
        }
        public override void Reset()
        {
            MainCard.Opacity = 0;
        }
        public override AnimationGroup AnimationIn()
        {
            var anims = new AnimationGroup();
            anims.Add(new DoubleAnimation(MainCard, UIElement.OpacityProperty, 0, 1, 300, 0));
            anims.Add(new DoubleAnimation(MainTranslate, TranslateTransform.YProperty, -20, 0, 300, 0, new AniEaseOutBack(2)));
            anims.TotalTime = TimeSpan.FromMilliseconds(300);
            return anims;
        }
        public override AnimationGroup AnimationOut()
        {
            var anims = new AnimationGroup();
            anims.Add(new DoubleAnimation(MainCard, UIElement.OpacityProperty, MainCard.Opacity, 0, 300, 0));
            anims.Add(new DoubleAnimation(MainTranslate, TranslateTransform.YProperty, MainTranslate.Y, -20, 300, 0, new AniEaseInFluent(3)));
            anims.TotalTime = TimeSpan.FromMilliseconds(300);
            return anims;
        }
    }
}
