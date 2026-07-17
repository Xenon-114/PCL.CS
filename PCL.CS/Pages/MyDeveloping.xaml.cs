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
    /// MyDeveloping.xaml 的交互逻辑
    /// </summary>
    public partial class MyDeveloping : MyPageRight
    {
        public MyDeveloping()
        {
            InitializeComponent();
            MyDev_MainTranslate = (this.Content as MyBorder).RenderTransform as TranslateTransform;
        }
        private TranslateTransform MyDev_MainTranslate;
        public override AnimationGroup AnimationIn()
        {
            AnimationGroup aniGroup = new AnimationGroup();
            aniGroup.TotalTime = TimeSpan.FromMilliseconds(400);
            aniGroup.Add(new DoubleAnimation(MyDev_MainTranslate, TranslateTransform.YProperty, -20, 0, 400, 0, new AniEaseOutBack(2)));
            aniGroup.Add(new DoubleAnimation(this, OpacityProperty, 0, 1, 150, 0));
            return aniGroup;
        }
        public override AnimationGroup AnimationOut()
        {
            AnimationGroup aniGroup = new AnimationGroup();
            aniGroup.TotalTime = TimeSpan.FromMilliseconds(300);
            aniGroup.Add(new DoubleAnimation(MyDev_MainTranslate, TranslateTransform.YProperty, MyDev_MainTranslate.Y, -20, 300, 0, new AniEaseInFluent(3)));
            aniGroup.Add(new DoubleAnimation(this, OpacityProperty, this.Opacity, 0, 300, 0));
            return aniGroup;
        }
        public override void Reset()
        {
            this.Opacity = 0;
        }
    }
}
