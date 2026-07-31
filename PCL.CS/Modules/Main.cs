using PCL.CS.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PCL.CS.Modules
{
    public enum HintColorState
    {
        Normal,
        Green,
        Red
    }
    public static class Main
    {
        public static MainWindow MainWnd { get => MainWindow.Current; }

        static Main()
        {
            HintInit();
        }
        public static Stopwatch Stopwatch { get; } = Stopwatch.StartNew();

        #region 弹窗

        public static MyMsg CreateMsg(string Title,string Content)
        {
            return new MyMsg() { Title = Title, Content = Content, Btn1Text = "关闭", BtnCount = 1 };
        }
        public static MyMsg CreateMsg(string Content)
        {
            return new MyMsg() { Title = "提示", Content = Content, Btn1Text = "关闭", BtnCount = 1 };
        }

        private static Queue<MyMsg> MessageList { get; } = new Queue<MyMsg>();

        private static MyMsg ShowingMsg { get; set; } = null;

        public static async Task<object> ShowMessage(MyMsg Message)
        {
            return await await Base.UIDispatcher.InvokeAsync(async () => await showMessage(Message));
        }
        private static async Task<object> showMessage(MyMsg Message)
        {
            Base.Log("[Message]加入弹窗队列");
            MessageList.Enqueue(Message);
            Base.Log("[Message]尝试显示弹窗");
            await TryShowMessage();
            Base.Log("[Message]等待弹窗返回...");
            await MainWindow.Current.CloseMessage(Message);
            object Result = await Message.ResultTask.Task;
            Base.Log($"[Message]弹窗已返回，返回值{Result??"Null"}");
            lock (MessageList)
            {
                if (MessageList.Any())
                {
                    Base.Log($"[Message]显示下一个弹窗...");
                    ShowingMsg = MessageList.Dequeue();
                    MainWindow.Current.ShowMessage(ShowingMsg);
                }
                else ShowingMsg = null;
            }
            return Result;
        }

        /// <summary>
        /// 尝试显示一个弹窗
        /// </summary>
        private static async Task TryShowMessage()
        {
            lock (MessageList)
            {
                if (!MessageList.Any()) return;
                if (ShowingMsg is null)
                {
                    var msg = MessageList.Dequeue();
                    ShowingMsg = msg;
                }
                else return;
            }
            await MainWindow.LoadedTask.Task;
            Base.Log("[Message]尝试显示弹窗成功");
            MainWindow.Current.ShowMessage(ShowingMsg);
        }

        #endregion

        #region 提示


        public static void Hint(string Text,HintColorState State=HintColorState.Normal)
        {
            Text = Text.Replace("\n", "").Replace("\r", "");
            if (MainWindow.Current is null)
            {
                Func<Task> a = async () =>
                {
                    await MainWindow.LoadedTask.Task;
                    Base.UIDispatcher.Invoke(() => RunHint(Text, State));
                };
                _ = a();
            }
            else Base.UIDispatcher.Invoke(() => RunHint(Text, State));
        }

        private static void RunHint(string Text,HintColorState State)
        {
            HintControl control = new HintControl() { Text = Text, ColorType = State };
            control.HorizontalAlignment = HorizontalAlignment.Left;
            control.TrueHeight = 0;
            control.Margin = new Thickness(0, -26, 0, 0);
            control.Opacity = 0;
            control.Tag = Stopwatch.Elapsed;

            Animation.Start(new AnimationGroup
            {
                new DoubleAnimation(control,HintControl.TrueHeightProperty,0,26,300,0,new AniEaseOutFluent(2)),
                new DoubleAnimation(control,HintControl.LeftMarginProperty,-20,0,400,0,new AniEaseOutElastic(1.5)),
                new DoubleAnimation(control,Control.OpacityProperty,0,1,200,0)
            });


            MainWindow.Current.PanHint.Children.Add(control);
        }
        private static void RemoveHint(HintControl control)
        {
            control.Tag = null;
            Animation.Start(new AnimationGroup
            {
                new DoubleAnimation(control,HintControl.TrueHeightProperty,26,0,150,200,new AniEaseInFluent(2)),
                new DoubleAnimation(control,HintControl.LeftMarginProperty,control.LeftMargin,-10,200,0,new AniEaseInFluent(2)),
                new DoubleAnimation(control,Control.OpacityProperty,control.Opacity,0,200,0),
                new EventAnimation(350,()=>MainWindow.Current?.PanHint.Children.Remove(control))
            });
        }

        private static void HintInit()
        {
            Task.Run(async () =>
            {
                await MainWindow.LoadedTask.Task;
                var Timer = new DispatcherTimer(DispatcherPriority.Normal, Base.UIDispatcher)
                {
                    Interval = TimeSpan.FromMilliseconds(20)
                };
                Timer.Tick += (s, e) =>
                {
                    TimeSpan Now = Stopwatch.Elapsed;
                    foreach (HintControl control in MainWindow.Current.PanHint.Children)
                    {
                        if (control.Tag is null) continue;
                        if (Now - ((TimeSpan)control.Tag) > TimeSpan.FromSeconds(2))
                            RemoveHint(control);
                    }
                };
                Timer.Start();
            });
        }

        #endregion

    }
}
