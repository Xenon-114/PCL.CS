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
using XeF4Core.MinecraftCore;

namespace PCL.CS.Pages
{
    /// <summary>
    /// PageDownloadMinecraft.xaml 的交互逻辑
    /// </summary>
    public partial class PageDownloadMinecraft : MyPageRight
    {
        public PageDownloadMinecraft()
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
                AniGroup.Add(new DoubleAnimation(Translate, TranslateTransform.YProperty, -20, 0, 300, Index * 50, new AniEaseOutBack(2)));
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
                AniGroup.Add(new DoubleAnimation(Translate, TranslateTransform.YProperty, Translate.Y, -20, 150, (double)Index / MainStackPanel.Children.Count * 300, new AniEaseInFluent(3)));
                AniGroup.Add(new DoubleAnimation(Element, UIElement.OpacityProperty, Element.Opacity, 0, 150, (double)Index / MainStackPanel.Children.Count * 300));
            }
            return AniGroup;
        }
        public override void Reset()
        { }
        public override void OnLoaded()
        {
            base.OnLoaded();
            foreach (UIElement Element in MainStackPanel.Children)
            {
                Element.Opacity = 0;
            }
            //if (!HasList) Load();
        }
        private bool HasList = false;
        private bool IsLoading = false;
        private async void Load()
        {
            if (IsLoading) return;
            IsLoading = true;
            Main.Hint("正在加载版本列表......");
            try
            {
                var List = await MinecraftDataList.GetDataList();
                DataList.Children.Clear();
                MyVirtualPanel<MinecraftVersion> VPanel;
                VPanel = new MyVirtualPanel<MinecraftVersion>()
                {
                    ListItemsHeight = 50,
                    Datas = List.Versions
                };
                VPanel.Conventer = Converter;
                static void Converter(ref MyClickableItem l,MinecraftVersion item)
                {
                    l.Title = item.Id;
                    l.Info = $"{DateTime.Parse(item.ReleaseTime)} | {item.Type}";
                    l.UseImage = true;
                    l.Height = 50;
                    l.AutoCheck = false;

                    l.SetResourceReference(MyClickableItem.ImagePathProperty, item.Type is "release" ? "Image.Blocks.Grass.png" : "Image.Blocks.CommandBlock.png");
                }
                VPanel.ClipParent = MainScroll;
                MainScroll.LayoutUpdated += VPanel.Update;
                DataList.Children.Add(VPanel);
                
                //foreach (var item in List.Versions)
                //{
                //    MyClickableItem clickableItem;
                //    DataList.Children.Add(new MyVirtualElement()
                //    {
                //        Content = clickableItem =
                //        new MyClickableItem()
                //        {
                //            Title = item.Id,
                //            Info = $"{DateTime.Parse(item.ReleaseTime)} | {item.Type}",
                //            UseImage = true,
                //            Height = 50,
                //            AutoCheck = false
                //        },
                //        Height = 50
                //    });
                //    clickableItem.SetResourceReference(MyClickableItem.ImagePathProperty, item.Type is "release" ? "Image.Blocks.Grass.png" : "Image.Blocks.CommandBlock.png");
                //}
                HasList = true;
            }
            catch (Exception ex)
            {
                Main.Hint(ex.Message);
                Base.Log(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }
        public override void OnUnloaded()
        {
            base.OnUnloaded();
            foreach (var Pair in TranslateDictionary)
            {
                Pair.Value.X = 0;
            }
            foreach (UIElement Element in MainStackPanel.Children)
            {
                Element.Opacity = 1;
            }
        }
    }
}
