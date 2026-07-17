using PCL.CS.Controls;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using System.Xml.Linq;
using XeF4Core;

namespace PCL.CS.Modules
{
    /// <summary>
    /// 为一些操作提供支持
    /// </summary>
    public static class Base
    {

        public const BuildType BuildType =
#if Alpha
            BuildType.Alpha
#elif Beta
            BuildType.Beta
#elif DEBUG
            BuildType.Debug
#elif TRACE
            BuildType.Release
#endif
            ;
        public static readonly VersionCode Version = VersionCode.FromVersion(Assembly.GetEntryAssembly().GetName().Version);
        public static bool OnRunning { get; private set; } = false;
        public static void Initialize()
        {
            UIDispatcher = Dispatcher.CurrentDispatcher;
            OnRunning = true;
        }
        public static Dispatcher UIDispatcher;
        private static readonly int UiThreadId = Thread.CurrentThread.ManagedThreadId;
        public static void RunInUiWait(Action Action)
        {
            if (Thread.CurrentThread.ManagedThreadId == UiThreadId)
                Action();
            else
                UIDispatcher.Invoke(Action);
        }
        /// <summary>
        /// 让逻辑在独立的线程上运行
        /// 注意：请不要滥用该函数
        /// </summary>
        /// <param name="Threadname"></param>
        /// <param name="Function"></param>
        /// <param name="Priority"></param>
        /// <returns></returns>
        public static Thread RunInNewThread(Action Function, string Threadname = "BackgroundThread", ThreadPriority Priority = ThreadPriority.Normal)
        {
            Thread NewThread = new Thread(
                () =>
                {
                    try
                    {
                        Function();
                    }
                    catch (ThreadInterruptedException ex)
                    {
                        Log("[Thread]" + Threadname + "：线程已中止\n详细信息：" + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Log("[Thread]" + Threadname + "：线程执行失败\n详细信息：" + ex.ToString());
                    }
                }
            );

            NewThread.Name = Threadname;
            NewThread.IsBackground = true;
            NewThread.Priority = Priority;
            NewThread.Start();
            return NewThread;
        }
        /// <summary>
        /// 让逻辑在独立的线程上运行（包含错误重试逻辑）
        /// 注意：请不要滥用该函数
        /// </summary>
        /// <param name="Threadname"></param>
        /// <param name="Function"></param>
        /// <param name="Priority"></param>
        /// <returns></returns>
        public static Thread RunInNewThread(Action Function, string Threadname = "BackgroundThread", ThreadPriority Priority = ThreadPriority.Normal,bool Retry=false)
        {
            Thread NewThread = new Thread(
                () =>
                {
                Try:
                    try
                    {
                        Function();
                    }
                    catch (ThreadInterruptedException ex)
                    {
                        Log("[Thread]" + Threadname + "：线程已中止\n详细信息：" + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        if (Retry)
                        {
                            Log("[Thread]" + Threadname + "：线程执行失败\n详细信息：" + ex.ToString() + "\n重试已开始。");
                            goto Try;
                        }
                        else
                        {
                            Log("[Thread]" + Threadname + "：线程执行失败\n详细信息：" + ex.ToString());
                        }
                    }
                }
            );

            NewThread.Name = Threadname;
            NewThread.IsBackground = true;
            NewThread.Priority = Priority;
            NewThread.Start();
            return NewThread;
        }

        #region 基础数据
        //程序启动路径
        public static string Path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        #endregion

        #region 文件操作

        #endregion

        #region 日志
        private static bool canLog = false;
        private struct Logs
        {
            public DateTime LogTime;
            public string LogThread;
            public string LogContent;
        };

        //日志打印队列
        private static Queue<Logs> LogList = new Queue<Logs>();
        public static Thread LogThread;
        public static void StartLog()
        {
            LogThread = new Thread(new ThreadStart(LogMain));
            LogThread.Name = "Log";
            LogThread.IsBackground = true;
            LogThread.Start();
            return;
        }
        private static StreamWriter LogWriter;
        private static void LogMain()
        {
            
            try
            {
                if (!Directory.Exists(Path + "PCL\\"))
                    Directory.CreateDirectory(Path + "PCL\\");
                for (int i = 4; i >= 1; i--)
                {
                    if (File.Exists(Path + "PCL\\Log" + (i + 1) + ".txt"))
                        File.Delete(Path + "PCL\\Log" + (i + 1) + ".txt");
                    if (File.Exists(Path + "PCL\\Log" + (i) + ".txt"))
                        File.Copy(Path + "PCL\\Log" + i + ".txt",
                            Path + "PCL\\Log" + (i + 1) + ".txt"
                            );
                }
                File.Create(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "PCL\\Log1.txt").Close();
                canLog = true;
                LogWriter = new StreamWriter(Path + "PCL\\Log1.txt", true);
                LogWriter.AutoFlush = true;
                //LogWriter = File.AppendText(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "PCL\\Log1.txt");
            }
            catch (IOException Ex)
            {
                canLog = false;
                Main.Hint($"[Log]无法写入日志！请检查权限！消息：{Ex.Message}", HintColorState.Red);
                //return;
            }
            Log("[Log]日志打印开始");
            while (true)
            {
                if (canLog)
                    WriteAllLogs(LogWriter);
                else
                    LogList.Clear();
                if (!OnRunning) return;
                Thread.Sleep(50);
            }
        }
        public static void WriteAllLogs(StreamWriter LogWriter)
        {
            while (LogList.Count > 0)
            {

                Logs log;
                log = LogList.Dequeue();
                LogWriter.Write($"[{log.LogTime.ToString("HH':'mm'.'ss'.'fff")}][{log.LogThread}]{log.LogContent}\n");
                //if (!File.Exists(Path + "CreateUndo.txt"))
                //    File.Create(Path + "CreateUndo.txt").Close();
                //File.AppendAllText(Path + "CreateUndo.txt", $"Log:[{log.LogTime.ToString("HH':'mm'.'ss'.'fff")}][{log.LogThread}]{log.LogContent}\n");
            }
            return;
        }
        public static void End()
        {
            if (canLog)
                WriteAllLogs(LogWriter);
            OnRunning = false;
        }
        public static void Log(string log)
        {

            DateTime whenLog = DateTime.Now;
            Logs logs;
            logs.LogTime = whenLog;
            logs.LogThread = (Thread.CurrentThread.Name == null ? "Main" : Thread.CurrentThread.Name);
            logs.LogContent = log;
            LogList.Enqueue(logs);
            //LogList.Append(logs);
            //LogWriter.Write($"[{log.LogTime.ToString("HH':'mm'.'ss'.'fff")}][{log.LogThread}]{log.LogContent}\n");
            //if (!File.Exists(Path + "CreateUndo.txt"))
            //    File.Create(Path + "CreateUndo.txt").Close();
            //File.AppendAllText(Path + "CreateUndo.txt", $"Log:{LogList.Count.ToString()},{logs.LogContent}");

        }
        public static void Log(Exception ex)
        {
            DateTime whenLog = DateTime.Now;
            Logs logs;
            logs.LogTime = whenLog;
            logs.LogThread = (Thread.CurrentThread.Name == null ? "Main" : Thread.CurrentThread.Name);
            logs.LogContent = $"[Exception]运行出现错误！\n详细信息：\n";
            string ExStr = "";
            ExStr += "引发错误：" + ex.GetType().ToString() + "\n";
            ExStr += "发生自线程：" + (Thread.CurrentThread.Name == null ? "Main" : Thread.CurrentThread.Name)+"\n";
            ExStr += ex.ToString();
            logs.LogContent += ExStr;
            LogList.Enqueue(logs);
        }
        #endregion

        public static void throwwarn()
        {

        }
    }

    /// <summary>
    /// 提供数学的支持
    /// </summary>
    public static class MathHelper
    {
        public static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
        private static Random Rand = new Random();
        /// <summary>
        /// 随机选择其一
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static char RandOne(char[] value)
        {
            return value[Rand.Next(0, value.Length)];
        }
        public static int Random(int max)
        {
            return Rand.Next(0, max);
        }
    }

    public static class ColorHelper
    {
        /// <summary>
        /// 将 WPF Color 转换为 HSL 值（色相、饱和度、亮度）
        /// </summary>
        /// <param name="color">RGB 颜色</param>
        /// <returns>包含 Hue（0-360）、Saturation（0-1）、Lightness（0-1）的元组</returns>
        public static (double hue, double saturation, double lightness) ToHsl(this Color color)
        {
            //alpha = color.A;

            // 归一化 RGB 分量到 [0, 1]
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            double hue = 0;
            double saturation = 0;
            double lightness = (max + min) / 2.0;

            if (delta != 0)
            {
                // 计算饱和度
                saturation = lightness <= 0.5
                    ? delta / (max + min)
                    : delta / (2.0 - max - min);

                // 计算色相
                if (max == r)
                    hue = (g - b) / delta + (g < b ? 6 : 0);
                else if (max == g)
                    hue = (b - r) / delta + 2;
                else // max == b
                    hue = (r - g) / delta + 4;

                hue *= 60; // 转换为角度
            }

            // 确保色相在 [0, 360)
            hue = (hue + 360) % 360;

            return (hue, saturation, lightness);
        }

        /// <summary>
        /// 将 HSL 值转换为 WPF Color
        /// </summary>
        /// <param name="hue">色相 (0-360)</param>
        /// <param name="saturation">饱和度 (0-1)</param>
        /// <param name="lightness">亮度 (0-1)</param>
        /// <param name="alpha">Alpha 通道值 (0-255)，默认 255 不透明</param>
        /// <returns>对应的 Color 结构</returns>
        public static Color HslToColor(this (double hue, double saturation, double lightness) hsl, byte alpha = 255)
        {
            var (hue, saturation, lightness) = hsl;
            // 将色相规范到 [0, 360)
            hue = hue % 360;
            if (hue < 0) hue += 360;

            double r, g, b;

            if (saturation == 0)
            {
                // 灰色（无色相）
                r = g = b = lightness;
            }
            else
            {
                double q = lightness < 0.5
                    ? lightness * (1 + saturation)
                    : lightness + saturation - lightness * saturation;
                double p = 2 * lightness - q;

                r = HueToRgb(p, q, hue / 360 + 1.0 / 3);
                g = HueToRgb(p, q, hue / 360);
                b = HueToRgb(p, q, hue / 360 - 1.0 / 3);
            }

            // 将 [0,1] 分量转换为 0-255 字节
            return Color.FromArgb(
                alpha,
                (byte)Math.Round(r * 255),
                (byte)Math.Round(g * 255),
                (byte)Math.Round(b * 255)
            );
        }

        private static double HueToRgb(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6) return p + (q - p) * 6 * t;
            if (t < 1.0 / 2) return q;
            if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
            return p;
        }
    }
}
