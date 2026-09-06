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
    /// PageGamesRight.xaml 的交互逻辑
    /// </summary>
    public partial class PageGamesRight : MyPageRight
    {
        public PageGamesRight()
        {
            InitializeComponent();
            WithGroupUI = MainCard.Child;
            NoGroupUI = BuildNoGroupUI();
            Minecraft.OnSelectGroupChanged += RefreshSelectGroup;
            InstancesStack.AddHandler(MyListItem.ClickEvent, (RoutedEventHandler)OnListItemClicked);
        }

        private void OnListItemClicked(object sender, RoutedEventArgs args)
        {
            if (args.Source is not FrameworkElement Item) return;
            if (Item.Tag is not XeF4Core.MinecraftCore.Minecraft Mc)
            {
                PagesContent.ChangePage(1);
            }
            else
            {
                Minecraft.SelectInstance(Mc);
                PagesContent.PageBack();
            }
        }

        private void RefreshSelectGroup(object sender, XeF4Core.MinecraftCore.MinecraftGroup e)
        {
            Refresh();
        }
        public void Refresh()
        {
            if (PagesContent.PageIndex == 5)
                PagesContent.Refresh(false, true);
        }

        private static UIElement BuildNoGroupUI()
        {
            var card = new MyBorder()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            StackPanel ContentPanel = new StackPanel();
            ContentPanel.Margin = new Thickness(20, 17, 20, 17);
            card.Content = ContentPanel;

            TextBlock LabNoGroupTitle = new TextBlock
            {
                Margin = new Thickness(0, 0, 0, 9),
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = "选择文件夹",
                FontSize = 19,
                UseLayoutRounding = true,
                SnapsToDevicePixels = true
            };
            LabNoGroupTitle.SetResourceReference(TextBlock.ForegroundProperty, "ColorBrush3");
            ContentPanel.Children.Add(LabNoGroupTitle);

            var rect = new Rectangle
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 2
            };
            rect.SetResourceReference(Rectangle.FillProperty, "ColorBrush3");
            ContentPanel.Children.Add(rect);

            TextBlock LabEmptyContent = new TextBlock();
            LabEmptyContent.Margin = new Thickness(10, 15, 10, 5);
            LabEmptyContent.Text = "尚未选中任意文件夹。请在右侧手动添加文件夹。";
            LabEmptyContent.FontSize = 13;
            LabEmptyContent.TextWrapping = TextWrapping.Wrap;
            LabEmptyContent.SetResourceReference(TextBlock.ForegroundProperty, "ColorBrushGray1");
            ContentPanel.Children.Add(LabEmptyContent);

            return card;
        }
        private UIElement WithGroupUI { get; }
        private UIElement NoGroupUI { get; }
        public override void Reset()
        {
            MainCard.Opacity = 0;
        }
        public override void OnLoaded()
        {
            base.OnLoaded();
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (Minecraft.SelectedGroup is null) MainCard.Child = NoGroupUI;
            else
            {
                MainCard.Child = WithGroupUI;
                AddMinecrafts();
            }
        }
        private UIElement BtnDownload { get; } = BuildDownloadButton();
        private static GeometryConverter GeometryConverter { get; } = new GeometryConverter();
        private static MyClickableItem BuildDownloadButton()
        {
            var Btn = new MyClickableItem
            {
                Height = 50,
                UseImage = false,
                Logo =
                #region Logo字符串
                        (Geometry)GeometryConverter.ConvertFromString("M512.277 954.412c-118.89 0-230.659-46.078-314.73-129.73S67.12 629.666 67.12 511.222s46.327-229.744 130.398-313.427 195.82-129.73 314.73-129.73 230.659 46.078 314.72 129.73S957.397 392.81 957.397 511.183 911.078 740.96 826.97 824.642s-195.8 129.77-314.692 129.77z m0-822.784c-101.972 0-197.809 39.494-269.865 111.222s-111.7 166.997-111.7 268.373 39.653 196.695 111.67 268.335S410.246 890.78 512.248 890.78s197.809-39.484 269.865-111.222 111.7-166.998 111.67-268.374c-0.03-101.375-39.654-196.665-111.67-268.303S614.22 131.628 512.277 131.628z m222.585 347.8H544.073V288.64c-0.76-17.561-15.613-31.18-33.173-30.419-16.495 0.714-29.704 13.924-30.419 30.419v190.787H289.703c-17.56 0.761-31.179 15.614-30.419 33.174 0.715 16.494 13.924 29.703 30.42 30.418H480.48v190.788c0.761 17.56 15.614 31.179 33.174 30.419 16.494-0.715 29.703-13.925 30.418-30.42V543.02h190.788c17.56 0.762 32.413-12.857 33.173-30.418 0.762-17.561-12.858-32.414-30.419-33.174a31.683 31.683 0 0 0-2.753 0z")
                #endregion
                ,
                LogoScale = 0.9,
                Title = "下载游戏",
                Info = "前往下载页面并下载一个Minecraft",
                AutoCheck = false
            };
            return Btn;
        }
        private void AddMinecrafts()
        {
            InstancesStack.Children.Clear();
            if (Minecraft.SelectedGroup is not null)
                Main.Hint($"游戏文件夹位于{Minecraft.SelectedGroup.MinecraftDirectory.FullName},拥有{Minecraft.SelectedGroup.Minecrafts.Count}个游戏");
            foreach (var instance in Minecraft.SelectedGroup.Minecrafts)
            {
                var Btn = new MyClickableItem
                {
                    Height = 50,
                    UseImage = true,
                    LogoScale = 0.9,
                    Title = instance.VersionName,
                    Info = $"{instance.Version} {instance.BuildType}",
                    AutoCheck = false
                };
                Btn.ImagePath = App.Current.Resources["Image.Blocks.Grass.png"] as MyImageSource;
                Btn.Tag = instance;
                InstancesStack.Children.Add(Btn);
            }
            InstancesStack.Children.Add(BtnDownload);
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
