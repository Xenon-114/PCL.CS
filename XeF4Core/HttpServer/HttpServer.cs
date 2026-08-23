using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace XeF4Core;

/// <summary>
/// Http服务
/// </summary>
public static class HttpServer
{
    /// <summary>
    /// 默认署名头
    /// </summary>
    public static string DefaultHeaderSign { get; set; } = Core.LauncherName;
    /// <summary>
    /// 从指定URL下载文件到缓存文件夹。
    /// </summary>
    /// <param name="url">指定URL</param>
    /// <param name="AllowCache">是否允许缓存。若允许，将尽量使用缓存。</param>
    /// <returns></returns>
    /// <exception cref="WebException"></exception>
    public static async Task<string> DownloadFile(string url, bool AllowCache = false)
    {
        string localpath = Path.Combine(Path.GetTempPath(), Core.LauncherShort, url.GetMd5Hash());
        if (AllowCache && File.Exists(localpath) && DateTime.Now - File.GetLastWriteTime(localpath) < TimeSpan.FromDays(1))
            return localpath;
        else
        {
            await DownloadFile(url, localpath);
            return localpath;
        }
    }
    /// <summary>
    /// 从指定URL下载文件到目标路径
    /// </summary>
    /// <param name="url">指定URL</param>
    /// <param name="localPath">目标路径</param>
    /// <returns></returns>
    /// <exception cref="WebException"></exception>
    public static async Task DownloadFile(string url, string localPath)
    {
        string directory = Path.GetDirectoryName(localPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        int Retry = 3;
        Exception? InnerEx = null;
        while (Retry-- > 0)
        {
            try
            {
                byte[] bytes = await HttpServerCore.SendRequest(url, HttpMethod.Get, simulateBrowserHeaders: true);
                await Extensions.WriteAllBytesToFileAsync(localPath,bytes);
                return;
            }
            catch (ThreadInterruptedException)
            {
                File.Delete(localPath);
                throw;
            }
            catch (Exception Ex)
            {
                InnerEx = Ex;
            }
        }
        File.Delete(localPath);
        throw new WebException($"从指定路径下载文件失败！\n{url} -> {localPath}", InnerEx);
    }
}
