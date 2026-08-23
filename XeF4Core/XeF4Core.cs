using System;
using System.Collections.Generic;
using System.Text;

namespace XeF4Core
{
    /// <summary>
    /// 代表核心
    /// </summary>
    public static class Core
    {
        /// <summary>
        /// 启动器完整名称
        /// </summary>
        public const string LauncherName = "PCLC#Edition";
        /// <summary>
        /// 启动器缩写
        /// </summary>
        public const string LauncherShort = "PCLCS";
        /// <summary>
        /// 当记录日志时触发
        /// </summary>
        public static event LogWritter? OnLogsReceives;
        internal static void Log(string Log) => OnLogsReceives?.Invoke(Log);
    }
    /// <summary>
    /// 表示一个记录日志的委托
    /// </summary>
    /// <param name="Log"></param>
    public delegate void LogWritter(string Log);
}
