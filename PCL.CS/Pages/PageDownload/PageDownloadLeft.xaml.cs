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
    /// 下载界面导航栏
    /// </summary>
    public partial class PageDownloadLeft : MyPageLeft
    {
        public PageDownloadLeft()
        {
            InitializeComponent();
        }
        private readonly Dictionary<UIElement, TranslateTransform> TranslateDictionary = new Dictionary<UIElement, TranslateTransform>();
        public override AnimationGroup AnimationIn()
        {
            var AniGroup = new AnimationGroup();
            foreach (UIElement Element in MainStackPanel.Children)
            {
                int Index = MainStackPanel.Children.IndexOf(Element);
                TranslateTransform Translate;
                if (TranslateDictionary.TryGetValue(Element, out TranslateTransform TranslateTransform))
                    Translate = TranslateTransform;
                else
                {
                    Translate = new TranslateTransform();
                    TranslateDictionary[Element] = Translate;
                    if (Element.RenderTransform is null)
                        Element.RenderTransform = Translate;
                    else
                    {
                        var TransGroup = new TransformGroup();
                        TransGroup.Children.Add(Translate);
                        TransGroup.Children.Add(Element.RenderTransform);
                        Element.RenderTransform = TransGroup;
                    }
                }
                AniGroup.Add(new DoubleAnimation(Translate, TranslateTransform.XProperty, -25, 0, 300, Index * 50, new AniEaseOutBack(2)));
                AniGroup.Add(new DoubleAnimation(Element, UIElement.OpacityProperty, 0, 1, 200, Index * 50));
            }
            return AniGroup;
        }
        public override AnimationGroup AnimationOut()
        {
            var AniGroup = new AnimationGroup();
            foreach (UIElement Element in MainStackPanel.Children)
            {
                int Index = MainStackPanel.Children.IndexOf(Element);
                TranslateTransform Translate;
                if (TranslateDictionary.TryGetValue(Element, out TranslateTransform TranslateTransform))
                    Translate = TranslateTransform;
                else
                {
                    Translate = new TranslateTransform();
                    TranslateDictionary[Element] = Translate;
                    if (Element.RenderTransform is null)
                        Element.RenderTransform = Translate;
                    else
                    {
                        var TransGroup = new TransformGroup();
                        TransGroup.Children.Add(Translate);
                        TransGroup.Children.Add(Element.RenderTransform);
                        Element.RenderTransform = TransGroup;
                    }
                }
                AniGroup.Add(new DoubleAnimation(Translate, TranslateTransform.XProperty, Translate.X, -25, 150, Index/MainStackPanel.Children.Count * 300,new AniEaseInFluent(3)));
                AniGroup.Add(new DoubleAnimation(Element, UIElement.OpacityProperty, Element.Opacity, 0, 150, Index / MainStackPanel.Children.Count * 300));
            }
            return AniGroup;
        }
        public override void Reset()
        {}
        public override void OnLoaded()
        {
            base.OnLoaded();
            foreach (UIElement Element in MainStackPanel.Children)
            {
                Element.Opacity = 0;
            }
        }
        public override void OnUnloaded()
        {
            base.OnUnloaded();
            foreach(var Pair in TranslateDictionary)
            {
                Pair.Value.X = 0;
            }
            foreach (UIElement Element in MainStackPanel.Children)
            {
                Element.Opacity = 1;
            }
        }

        private void Refresh(object sender,RoutedEventArgs e)
        {

        }
    }
}
