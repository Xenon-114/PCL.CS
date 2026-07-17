using PCL.CS.Modules;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace PCL.CS
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Animator.AniFPS = 100;
            Animator.StartThread();
            Base.Initialize();
            Base.StartLog();

            //这行代码是测试用的

            //Animator.AniSpeed = 0.1;
            

            ToolTipService.InitialShowDelayProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(300));
            ToolTipService.BetweenShowDelayProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(400));
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(9999999));
            ToolTipService.PlacementProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(PlacementMode.Bottom));
            ToolTipService.HorizontalOffsetProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(8.0));
            ToolTipService.VerticalOffsetProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(4.0));
        }
    }
}
