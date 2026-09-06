using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static XeF4Core.FileExtensions;

namespace XeF4Core;

/// <summary>
/// 对于文件操作的扩展
/// </summary>
public static class FileExtensions
{
    /// <summary>
    /// 异步形式的File.WriteAllBytes
    /// </summary>
    /// <param name="path">指定路径</param>
    /// <param name="bytes">字节数组</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    public static async Task WriteAllBytesToFileAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
    {
        var fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
        using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 81920, fileOptions);
        await fileStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
        await fileStream.FlushAsync(cancellationToken);
    }
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern uint GetFinalPathNameByHandle(
        IntPtr hFile,
        StringBuilder lpszFilePath,
        uint cchFilePath,
        uint dwFlags
    );
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    private const uint FILE_READ_EA = 0x0008;
    private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
    private const uint OPEN_EXISTING = 3;
    private const uint FILE_SHARE_READ = 0x00000001;
    private const uint FILE_SHARE_WRITE = 0x00000002;
    private const uint FILE_SHARE_DELETE = 0x00000004;


    /// <summary>
    /// 获取文件或目录的规范化的唯一真实路径（解析所有 mklink、目录联接、相对路径）。
    /// </summary>
    /// <param name="path">任意路径（文件或目录）</param>
    /// <returns>规范化的绝对物理路径，如 D:\Minecraft\.minecraft\versions\1.20.1\1.20.1.json</returns>
    /// <exception cref="FileNotFoundException">路径不存在</exception>
    public static string GetCanonicalPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("路径不能为空", nameof(path));

        string fullPath = Path.GetFullPath(path);

        IntPtr hFile = CreateFile(
            fullPath,
            FILE_READ_EA,
            FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE,
            IntPtr.Zero,
            OPEN_EXISTING,
            FILE_FLAG_BACKUP_SEMANTICS, // 关键标志
            IntPtr.Zero);

        if (hFile == new IntPtr(-1))
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

        try
        {
            var sb = new StringBuilder(260);
            uint result = GetFinalPathNameByHandle(hFile, sb, (uint)sb.Capacity, 0);
            if (result == 0)
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

            string finalPath = sb.ToString();
            if (finalPath.StartsWith(@"\\?\"))
                finalPath = finalPath.Substring(4);
            return finalPath;
        }
        finally
        {
            CloseHandle(hFile);
        }
    }
}
