using PCL.CS.Controls;
using PCL.CS.Modules;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// PageGamesLeft.xaml 的交互逻辑
    /// </summary>
    public partial class PageGamesLeft : MyPageLeft
    {
        public PageGamesLeft()
        {
            InitializeComponent();
            DirectorysList.SelectIndexChanged += ChangeSelectMinecraftGroup;
            Minecraft.OnSelectGroupChanged += OnMinecraftSelectGroupChanged;
            BtnCreate.Click += CreateMinecraftFolder;
        }

        private void CreateMinecraftFolder(object sender, RoutedEventArgs e)
        {
            if (Minecraft.Local is not null && Minecraft.Local.IsAvailable) return;
            if(!Directory.Exists(Minecraft.LocalPath))
                Directory.CreateDirectory(Minecraft.LocalPath);
            var McGroup = XeF4Core.MinecraftCore.MinecraftGroup.FromMinecraftPath(Minecraft.LocalPath);
            Minecraft.AddMinecraftGroup(McGroup);
        }

        private void OnMinecraftSelectGroupChanged(object sender, XeF4Core.MinecraftCore.MinecraftGroup e)
        {
            var Selected = DirectorysList.SelectItem as FrameworkElement;
            if (ReferenceEquals(e, Selected?.Tag)) return;
            Refresh();
        }

        private void ChangeSelectMinecraftGroup(object sender, RoutedEventArgs e)
        {
            if (DirectorysList.Children.Count == 0) return;
            var Selected = DirectorysList.SelectItem as FrameworkElement;
            if (Selected is null) return;
            Minecraft.SelectMcGroup(Selected.Tag as XeF4Core.MinecraftCore.MinecraftGroup);
        }
        public override void Reset()
        {
            MainStack.Opacity = 0;
            MainTranslate.X = 0;
        }
        public override void OnLoaded()
        {
            base.OnLoaded();
            Refresh();
        }

        private List<XeF4Core.MinecraftCore.MinecraftGroup> Minecrafts { get; } = new();

        private void Refresh()
        {
            Minecrafts.Clear();
            Minecraft.Refresh();
            if (Minecraft.Local is not null)
                Minecrafts.Add(Minecraft.Local);
            Minecrafts.AddRange(Minecraft.McGroups);
            Main.Hint($"加载中，已加载{Minecrafts.Count}个Minecaft文件夹");
            RefreshUI();
        }
        private void RefreshUI()
        {
            DirectorysList.Children.Clear();
            if (Minecrafts.Count == 0)
            {
                DirectorysTitle.Visibility = Visibility.Collapsed;
                DirectorysList.Visibility = Visibility.Collapsed;
            }
            else
            {
                DirectorysTitle.Visibility = Visibility.Visible;
                DirectorysList.Visibility = Visibility.Visible;
            }
            foreach (var group in Minecrafts)
            {
                var ListItem = new MyRadioBoxItem()
                {
                    Height = 40,
                    Title = group.Name,
                    Info = group.MinecraftDirectory.FullName,
                    IsChecked = ReferenceEquals(Minecraft.SelectedGroup, group),
                    Tag = group
                };
                if (ReferenceEquals(Minecraft.SelectedGroup, group)) Main.Hint($"已选中列表中第{Minecrafts.IndexOf(group)}");
                DirectorysList.Children.Add(ListItem);
            }
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
