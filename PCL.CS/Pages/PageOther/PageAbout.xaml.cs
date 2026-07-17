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
using PCL.CS.Controls;
using PCL.CS.Modules;

namespace PCL.CS.Pages
{
    /// <summary>
    /// PageAbout.xaml 的交互逻辑
    /// </summary>
    public partial class PageAbout : MyPageRight
    {
        public PageAbout()
        {
            InitializeComponent();
        }
        public override AnimationGroup AnimationIn()
        {
            AnimationGroup aniGroup;
            aniGroup = new AnimationGroup();
            double TotalTime = 0;
            aniGroup.Add(new DoubleAnimation(ControlOver.RenderTransform, TranslateTransform.YProperty, -20, 0, 300, 0, new AniEaseOutBack(2)));
            aniGroup.Add(new DoubleAnimation(ControlOver, OpacityProperty, 0, 1, 250, 0));
            aniGroup.Add(MyScrollBarAnimationIn(MainAScrollViewer));
            TotalTime += 30;
            for(int i = 0; i < MainScrollViewer.Children.Count; i++)
            {
                UIElement Element = MainScrollViewer.Children[i];
                TranslateTransform translate = Element.RenderTransform as TranslateTransform;
                if (translate is null)
                {
                    translate = new TranslateTransform();
                    Element.RenderTransform = translate;
                }
                aniGroup.Add(new DoubleAnimation(translate, TranslateTransform.YProperty, -20, 0, 300, TotalTime, new AniEaseOutBack(2)));
                aniGroup.Add(new DoubleAnimation(Element, OpacityProperty, 0, 1, 250, TotalTime));
                TotalTime += 30;
            }
            TotalTime += 270;
            aniGroup.TotalTime = TimeSpan.FromMilliseconds(TotalTime);
            return aniGroup;
        }
        public override AnimationGroup AnimationOut()
        {
            AnimationGroup aniGroup;
            aniGroup = new AnimationGroup();
            double TotalTime = 0;
            aniGroup.Add(new DoubleAnimation(ControlOver.RenderTransform, TranslateTransform.YProperty, ((TranslateTransform)ControlOver.RenderTransform).Y, -20, 200, 0, new AniEaseInFluent(3)));
            aniGroup.Add(new DoubleAnimation(ControlOver, OpacityProperty, ControlOver.Opacity, 0, 200, 0));
            aniGroup.Add(MyScrollBarAnimationOut(MainAScrollViewer));
            for (int i = 0; i < MainScrollViewer.Children.Count; i++)
            {
                UIElement Element = MainScrollViewer.Children[i];
                TranslateTransform translate = Element.RenderTransform as TranslateTransform;
                if (translate is null)
                {
                    translate = new TranslateTransform();
                    Element.RenderTransform = translate;
                }
                aniGroup.Add(new DoubleAnimation(translate, TranslateTransform.YProperty, translate.Y, -20, 200, TotalTime, new AniEaseInFluent(3)));
                aniGroup.Add(new DoubleAnimation(Element, OpacityProperty, Element.Opacity, 0, 200, TotalTime));
                TotalTime += 100 / MainScrollViewer.Children.Count;
            }
            TotalTime = 300;
            aniGroup.TotalTime = TimeSpan.FromMilliseconds(TotalTime);
            return aniGroup;
        }
        public override void Reset()
        {
            MainAScrollViewer.ScrollToVerticalOffset(0);
            ControlOver.Opacity = 0;
            for (int i = 0; i < MainScrollViewer.Children.Count; i++)
            {
                UIElement Element = MainScrollViewer.Children[i];
                Element.Opacity = 0;
            }
        }

        private void MyButton_Click(object sender, RoutedEventArgs e)
        {
            MyLoading myLoading = Loadingaaa.Content as MyLoading;
            myLoading.State = myLoading.State is MyLoading.MyLoadingState.Running ? MyLoading.MyLoadingState.Error : MyLoading.MyLoadingState.Running;
        }

        private void MyIconButton_Click(object sender, RoutedEventArgs e)
        {
            var Btn = e.Source as MyIconButton;
            Btn.IsEnabled = false;
        }
    }
}
