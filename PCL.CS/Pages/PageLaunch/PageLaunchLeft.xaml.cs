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
            Minecraft.OnSelectInstanceChanged += (s, e) => Refresh();
            Refresh();
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
            Refresh();
        }
        private void Refresh()
        {
            if (Minecraft.SelectedInstance is null)
            {
                TextLaunch.Text = "下载游戏";
                TextVersion.Text = "没有可用版本";
            }
            else
            {
                TextLaunch.Text = "启动游戏";
                TextVersion.Text = Minecraft.SelectedInstance.VersionName;
            }
        }
        private void BtnVersion_Click(object sender, RoutedEventArgs e)
        {
            PagesContent.ChangePage(5);
        }

        private void BtnLaunch_Click(object sender, RoutedEventArgs e)
        {
            if (Minecraft.SelectedInstance is null)
                PagesContent.ChangePage(1);
            else
                LaunchMinecraft();
        }
        private class AddOn : XeF4Core.MinecraftCore.IMinecraftRunnerExtra
        {
            KeyValuePair<string, string>[] XeF4Core.MinecraftCore.IMinecraftRunnerExtra.MacroReplacement => null;
            KeyValuePair<string, bool>[] XeF4Core.MinecraftCore.IMinecraftRunnerExtra.Features => null;
            string[] XeF4Core.MinecraftCore.IMinecraftRunnerExtra.GameArgsAdd => null;
            string[] XeF4Core.MinecraftCore.IMinecraftRunnerExtra.JvmArgsAdd { get; } = { "-Dorg.lwjgl.util.Debug=true", "-Dorg.lwjgl.util.DebugLoader=true" };
        }
        private async void LaunchMinecraft()
        {
            BtnLaunch.IsEnabled = false;
            try
            {
                string PlayerName = PlayerNameBox.Text;
                await Task.Run(async () =>
                {
                    var Runner = Minecraft.SelectedInstance.CreateRunner();
                    Runner.Login(PlayerName, new Guid(PlayerName.GetMd5Hash()).ToString(), "0", "legacy", "0");
                    await Runner.FileChecks();
                    Runner.PrepairNativeLibs();
                    Runner.GameJava = java;
                    Runner.Extras.Add(new AddOn());
                    Runner.OutputDataReceived += (s, e) => Main.Hint(e);
                    Runner.ErrorDataReceived += (s, e) => Main.Hint(e);
                    Runner.Start();
                    await Runner.WaitForExitAsync();
                    //Runner.CleanUp();
                });
            }
            finally
            {
                Main.Hint("游戏已退出......");
                BtnLaunch.IsEnabled = true;
            }
        }
        private Java java;

        private async void JavaButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                java = await Java.FromFile(JavaBox.Text);
                Main.Hint("Java已使用！");
            }
            catch (Exception ex)
            {
                Main.Hint(ex.ToString());
            }
        }
    }
}
