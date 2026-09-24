using Downloader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DLStatus = Downloader.DownloadStatus;

namespace XeF4Core;

/// <summary>
/// 下载状态
/// </summary>
public enum DownloadStatus
{
    /// <summary>
    /// 准备
    /// </summary>
    Preparing = DLStatus.Created,
    /// <summary>
    /// 下载中
    /// </summary>
    Downloading = DLStatus.Running,
    /// <summary>
    /// 已完成
    /// </summary>
    Finished = DLStatus.Completed,
    /// <summary>
    /// 错误
    /// </summary>
    Error = DLStatus.Failed,
    /// <summary>
    /// 已取消
    /// </summary>
    Canceled = DLStatus.Stopped,
    /// <summary>
    /// 暂停中
    /// </summary>
    Paused = DLStatus.Paused,
}

/// <summary>
/// 下载任务
/// </summary>
public sealed class DownloadTask : IMyTask
{
    /// <summary>
    /// 任务名称
    /// </summary>
    public string Name { get; set; } = "下载任务";
    /// <summary>
    /// 目标链接
    /// </summary>
    public string Url { get; }
    /// <summary>
    /// 本地路径
    /// </summary>
    public string LocalPath { get; }
    private DownloadService? _service;
    /// <summary>
    /// 下载状态
    /// </summary>
    public DownloadStatus Status
    {
        get
        {
            if(CancellationTokenSource.IsCancellationRequested)
                return DownloadStatus.Canceled;
            if (_service is null)
                return DownloadStatus.Preparing;
            if (_service.Status is DLStatus.None)
                return DownloadStatus.Preparing;
            return (DownloadStatus)_service.Status;
        }
    }
    /// <summary>
    /// 已下载字节数
    /// </summary>
    public long DownloadedBytes { get; private set; }
    /// <summary>
    /// 文件总大小
    /// </summary>
    public long FileSize { get; private set; }
    /// <summary>
    /// 下载进度
    /// </summary>
    public double Progress => (double)DownloadedBytes / FileSize;
    /// <summary>
    /// 出现的错误。若未出错，为null。
    /// </summary>
    public Exception? Error { get; internal set; } = null;
    private Task InnerTask { get; }
    private CancellationTokenSource CancellationTokenSource = new();

    #region 内部封闭信息
    internal Downloader Owner { get; }
    #endregion
    internal bool SingleThreadOnly { get; }

    internal DownloadTask(string url, string localPath, Downloader owner, bool singleThreadOnly = false)
    {
        Url = url;
        LocalPath = localPath;
        Owner = owner;
        SingleThreadOnly = singleThreadOnly;
        InnerTask = Download();
    }
    internal TaskCompletionSource<object?> Allows = new();
    private MyWorkingList WaitingDownloadTasks
    {
        get => Owner.WorkingList;
    }
    private async Task Download()
    {
        var apply = WaitingDownloadTasks.Apply();
        await apply.WaitAsync();
        try
        {
            if (CancellationTokenSource.IsCancellationRequested)
                return;
            var downloadOpt = new DownloadConfiguration()
            {
                ChunkCount = SingleThreadOnly ? 1 : (int)Owner.ChunkCount,
                ParallelDownload = true,
                ParallelCount = SingleThreadOnly ? 1 : (int)Owner.ChunkCount,
                MaxTryAgainOnFailure = SingleThreadOnly ? 5 : 20,
                EnableAutoResumeDownload = true,
                MaximumMemoryBufferBytes = 50 * 1024 * 1024,
                CheckDiskSizeBeforeDownload = true,
                MaximumBytesPerSecond = Owner.DownloadSpeedLimit,
                BlockTimeout = 1000,
                HttpClientTimeout = 100 * 1000,

                RequestConfiguration =
                {
                    UserAgent = $"{HttpServer.DefaultHeaderSign} Mozilla/5.0 AppleWebKit/537.36 Chrome/63.0.3239.132 Safari/537.36",
                    Headers = ["Referer: http://C#.PCL.Alpha/"]
                },

                CustomHttpClientFactory = () => Owner.HttpClient
            };
            _service = new DownloadService(downloadOpt);
            _service.DownloadStarted += (s, e) =>
            {
                Core.Log($"开始下载:{e.FileName},大小：{e.TotalBytesToReceive}字节");
                this.FileSize = e.TotalBytesToReceive;
            };
            _service.DownloadProgressChanged += (s, e) => this.DownloadedBytes = e.ReceivedBytesSize;
            _service.DownloadFileCompleted += (s, e) =>
            {
                if (e.Error is not null)
                    Error = e.Error;
                else if (e.Cancelled) DownloadedBytes = 0;
                else DownloadedBytes = FileSize;
                
            };
            await _service.DownloadFileTaskAsync(Url, LocalPath, CancellationTokenSource.Token);

            if (Status is DownloadStatus.Error && Error is not null) throw Error;
        }
        finally
        {
            apply.Finish();
        }
    }
    /// <inheritdoc/>
    public TaskAwaiter GetAwaiter() => InnerTask.GetAwaiter();
    /// <inheritdoc/>
    public Task WaitAsync()
    {
        return this.InnerTask;
    }
    /// <inheritdoc/>
    public void Wait()
    {
        InnerTask.Wait();
    }
    /// <summary>
    /// 取消任务
    /// </summary>
    public void Cancel() => CancellationTokenSource.Cancel();
}
/// <summary>
/// 下载器。需要实例化。
/// </summary>
public class Downloader
{

    internal readonly object locker = new();
    /// <summary>
    /// 最大同步下载数
    /// </summary>
    public uint MaxAsyncDownloadLimit
    {
        get => WorkingList.AsyncWorkLimit;
        set => WorkingList.AsyncWorkLimit = value;
    }
    /// <summary>
    /// 每文件分块数
    /// </summary>
    public uint ChunkCount { get; set; } = 8;
    /// <summary>
    /// 下载限速
    /// </summary>
    public int DownloadSpeedLimit
    {
        get;
        set;
    }
    /// <summary>
    /// 下载器。需要实例化。
    /// </summary>
    public Downloader()
    {
        WorkingList.AsyncWorkLimit = 8u;
    }
    internal readonly MyWorkingList WorkingList = new();
    #region 公共方法
    /// <summary>
    /// 从指定URL下载文件到缓存文件夹。
    /// </summary>
    /// <param name="url">指定URL</param>
    /// <param name="AllowCache">是否允许缓存。若允许，将尽量使用缓存。</param>
    /// <returns></returns>
    /// <exception cref="WebException"></exception>
    public async Task<string> Download(string url, bool AllowCache = false)
    {
        string localpath = Path.Combine(Path.GetTempPath(), Core.LauncherShort, url.GetMd5Hash());
        if (AllowCache && File.Exists(localpath) && DateTime.Now - File.GetLastWriteTime(localpath) < TimeSpan.FromDays(1))
            return localpath;
        else
        {
            await Download(url, localpath);
            return localpath;
        }
    }
    /// <summary>
    /// 下载。
    /// </summary>
    /// <param name="url"></param>
    /// <param name="localPath"></param>
    /// <returns></returns>
    public DownloadTask Download(string url, string localPath)
    {
        if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
        (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException($"URL 不合法：{url}", nameof(url));
        }
        var Task = new DownloadTask(url, localPath, this);
        return Task;
    }
    #endregion

    #region 私有定义与方法
    internal HttpClient HttpClient { get; } = new() { Timeout = TimeSpan.FromMinutes(5) };
    internal static long ParseTotalSizeFromResponse(HttpResponseMessage response)
    {
        // 从 Content-Range 头解析总大小
        if (response.Content.Headers.TryGetValues("Content-Range", out var values))
        {
            var contentRange = values.FirstOrDefault();
            if (!string.IsNullOrEmpty(contentRange))
            {
                var parts = contentRange.Split('/');
                if (parts.Length == 2 && long.TryParse(parts[1], out var size))
                    return size;
            }
        }
        // 降级：使用 Content-Length（通常 206 也会带）
        return response.Content.Headers.ContentLength ?? throw new Exception("无法获取文件总大小");
    }
    /// <summary>
    /// 最大下载线程数限制
    /// </summary>
    public uint DownloadThreadLimit { get => TasksWorkingList.AsyncWorkLimit; set => TasksWorkingList.AsyncWorkLimit = value; }
    private readonly MyWorkingList TasksWorkingList = new() { AsyncWorkLimit = 64 };
    /// <summary>
    /// 已使用线程数
    /// </summary>
    public int TasksUsing => TasksWorkingList.WorkingTaskCount;

    internal event Action? OnLimitRefresh;
    ///
    protected async Task DoInTasksLimit(Action action)
    {
        var apply = TasksWorkingList.Apply();
        await apply;
        action.Invoke();
        apply.Finish();
        OnLimitRefresh?.Invoke();
        return;
    }
    ///
    protected async Task DoInTasksLimit(Func<Task> action)
    {
        var apply = TasksWorkingList.Apply();
        await apply;
        await action.Invoke();
        apply.Finish();
        OnLimitRefresh?.Invoke();
        return;
    }
    ///
    protected async Task DoInTasksLimit(Func<ITask> action)
    {
        var apply = TasksWorkingList.Apply();
        await apply;
        await action.Invoke();
        apply.Finish();
        OnLimitRefresh?.Invoke();
        return;
    }
    internal async Task<(HttpResponseMessage response, HttpRequestMessage request)> SendRequest(
        string url,
        HttpMethod? method = null,
        RangeHeaderValue? range = null,
        object? content = null,
        KeyValuePair<string, string>[]? headers = null,
        Encoding? encoding = null,
        CancellationToken ct = default)
    {
        method ??= HttpMethod.Get;
        encoding ??= Encoding.UTF8;

        var request = new HttpRequestMessage(method, url);

        try
        {

            if (range is not null) request.Headers.Range = range;

            // 处理请求体
            if (content != null && method != HttpMethod.Get && method != HttpMethod.Head)
            {
                request.Content = content switch
                {
                    HttpContent httpContent => httpContent,
                    byte[] bytes => new ByteArrayContent(bytes),
                    _ => new ByteArrayContent(encoding.GetBytes(content.ToString()))
                };
            }

            // 添加自定义请求头
            if (headers != null)
            {
                foreach (var keyValuePair in headers)
                {
                    if (string.Equals(keyValuePair.Key, "content-type", StringComparison.OrdinalIgnoreCase))
                        request.Content?.Headers.TryAddWithoutValidation(keyValuePair.Key, keyValuePair.Value);
                    else
                        request.Headers.TryAddWithoutValidation(keyValuePair.Key, keyValuePair.Value);
                }
            }

            // 模拟浏览器头（自定义方法）
            HttpServerCore.HeadersSign(url, request, true);

            // 发送请求
            var response = await HttpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                ct).ConfigureAwait(false);

            return (response, request);
        }
        catch
        {
            request.Dispose();
            throw;
        }
    }

    #endregion


}
