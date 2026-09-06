using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace XeF4Core;

#pragma warning disable CS1591
/// <summary>
/// Windows辅助类
/// </summary>
public static class Windows
{
    /// <summary>
    /// Windows矩形
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        /// <summary>
        /// 左
        /// </summary>
        public int Left;
        /// <summary>
        /// 上
        /// </summary>
        public int Top;
        /// <summary>
        /// 右
        /// </summary>
        public int Right;
        /// <summary>
        /// 下
        /// </summary>
        public int Bottom;
    }
    /// <summary>
    /// 获取一个窗口的大小和位置
    /// </summary>
    /// <param name="hwnd">窗口句柄</param>
    /// <returns>窗口信息</returns>
    /// 
    public static RECT GetWindowRect(IntPtr hwnd)
    {
        if (!GetWindowRect(hwnd, out var Rect)) throw new System.ComponentModel.Win32Exception(
            Marshal.GetLastWin32Error(),
            "获取窗口矩形失败");
        return Rect;
    }
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
    /// <summary>
    /// 表示Windows点坐标
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PointAPI
    {
        /// <summary>
        /// X坐标
        /// </summary>
        public int X;
        /// <summary>
        /// Y坐标
        /// </summary>
        public int Y;
    }
    /// <summary>
    /// 获得鼠标坐标
    /// </summary>
    /// <param name="lpPoint">鼠标当前坐标</param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out PointAPI lpPoint);
    //[DllImport("user32.dll")]
    //public static extern short GetAsyncKeyState(int vKey);
    /// <summary>
    /// 
    /// </summary>
    public enum MessageBoxType : uint
    {
        ///
        OkOnly = 0x00000000,
        ///
        OkCancel = 0x00000001,
        ///
        YesNo = 0x00000004,
        ///
        YesNoCancel = 0x00000003,
        ///
        IconError = 0x00000010,
        ///
        IconQuestion = 0x00000020,
        ///
        IconWarning = 0x00000030,
        ///
        IconInformation = 0x00000040,
    }

    /// <summary>
    /// 返回值
    /// </summary>
    public enum Result : int
    {
        ///
        Ok = 1,
        ///
        Cancel = 2,
        ///
        Yes = 6,
        ///
        No = 7,
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int MessageBoxW(IntPtr hWnd, string lpText, string lpCaption, uint uType);

    /// <summary>
    /// 显示一个消息框
    /// </summary>
    /// <param name="text">消息内容</param>
    /// <param name="caption">标题</param>
    /// <param name="type">类型和图标组合</param>
    /// <returns>用户点击的按钮结果</returns>
    public static Result ShowMessageBox(string text, string caption, MessageBoxType type = MessageBoxType.OkOnly | MessageBoxType.IconInformation)
    {
        int result = MessageBoxW(IntPtr.Zero, text, caption, (uint)type);
        return (Result)result;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct FileInfomation
    {
        public uint FileAttributes;
        public System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;
        public uint VolumeSerialNumber;
        public uint FileSizeHigh;
        public uint FileSizeLow;
        public uint NumberOfLinks;
        public uint FileIndexHigh;
        public uint FileIndexLow;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool GetFileInformationByHandle(
        IntPtr hFile,
        out FileInfomation lpFileInformation);
}

#pragma warning restore CS1591