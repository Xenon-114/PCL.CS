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

namespace XeF4Core;

/// <summary>
/// 下载状态
/// </summary>
public enum DownloadStatus
{
    /// <summary>
    /// 准备
    /// </summary>
    Preparing,
    /// <summary>
    /// 下载中
    /// </summary>
    Downloading,
    /// <summary>
    /// 合并分块
    /// </summary>
    Writing,
    /// <summary>
    /// 已完成
    /// </summary>
    Finished,
    /// <summary>
    /// 错误
    /// </summary>
    Error,
    /// <summary>
    /// 已取消
    /// </summary>
    Canceled
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
    /// <summary>
    /// 下载状态
    /// </summary>
    public DownloadStatus Status { get; internal set; } = DownloadStatus.Preparing;
    /// <summary>
    /// 已下载字节数
    /// </summary>
    public long DownloadedBytes => DownloadingTask?.DownloadedBytes ?? 0;
    private IDownloadingTask? DownloadingTask;
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
        Status = DownloadStatus.Preparing;
        SingleThreadOnly = singleThreadOnly;
        InnerTask = Download();
    }
    internal TaskCompletionSource<object?> Allows = new();
    private static readonly Dictionary<WeakReference<Downloader>, MyWorkingList> _WaitingDownloadTasks = new();
    internal static void CleanRef(Downloader downloader)
    {
        if (_WaitingDownloadTasks.TryGetValue(downloader.weakReference, out var list)) downloader.OnLimitRefresh -= list.RefreshAsyncWork;
        _WaitingDownloadTasks.Remove(downloader.weakReference);
    }
    private MyWorkingList WaitingDownloadTasks
    {
        get
        {
            bool IsAllow(int TaskCounter)
            {
                lock (Owner.locker)
                    //至少要有10%线程余量且线程数不能超过32
                    return ((double)Owner.TasksUsing / Owner.DownloadThreadLimit <= 0.9) && (TaskCounter < 48);
            }
            if (!_WaitingDownloadTasks.TryGetValue(Owner.weakReference, out var tasks))
            {
                tasks = new();
                Owner.OnLimitRefresh += tasks.RefreshAsyncWork;
                tasks.LimitChecker = IsAllow;
                _WaitingDownloadTasks[Owner.weakReference] = tasks;
            }
            return tasks;
        }
    }
    private async Task Download()
    {
        var apply = WaitingDownloadTasks.Apply();
        await apply.WaitAsync();

        Core.Log("已发送请求，等待返回......");

        var (response, request) = await Owner.SendRequest(
        Url,
        HttpMethod.Get,
        range: new RangeHeaderValue(0, 0),
        ct: CancellationToken.None);
        var probeResponse = response;
        using var probeRequest = request;

        Core.Log($"请求已返回，大小{probeResponse.Content.Headers.ContentLength ?? 0}");

        // 注意：SendRequest 内部捕获异常时会释放 request，我们只需管理 response
        try
        {
            if (probeResponse.StatusCode == HttpStatusCode.PartialContent)
            {
                //1、支持Range，多线程
                long totalSize = Downloader.ParseTotalSizeFromResponse(probeResponse);
                probeResponse.Dispose(); // 探测响应无数据体，释放
                this.FileSize = totalSize;

                if (!SingleThreadOnly)
                {
                    var task = new Downloader.DownloadTaskByLoader(Url, LocalPath, this, totalSize) { Cancellation = this.CancellationTokenSource.Token };
                    this.DownloadingTask = task;
                    await task.Download();
                    return;
                }
                else
                {
                    var task = new Downloader.DownloadTaskBySingleThread(Url, LocalPath, this, totalSize) { Cancellation = this.CancellationTokenSource.Token };
                    this.DownloadingTask = task;
                    await task.Download();
                    return;
                }
            }
            else if (probeResponse.StatusCode == HttpStatusCode.OK)
            {
                this.FileSize = probeResponse.Content.Headers.ContentLength ?? 0;
                //2、死木服务器返回全部数据，接受并下载
                var task = new Downloader.DownloadTaskByResponse(LocalPath, this, probeResponse, Url) { Cancellation = this.CancellationTokenSource.Token };
                this.DownloadingTask = task;
                await task.Download();
                return;
            }
            else
            {
                //3、单线程
                this.FileSize = probeResponse.Content.Headers.ContentLength ?? 0;
                probeResponse.Dispose();
                var task = new Downloader.DownloadTaskBySingleThread(Url, LocalPath, this, 0);
                this.DownloadingTask = task;
                await task.Download();
                return;
            }
        }
        catch
        {
            probeResponse?.Dispose();
            throw;
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
    internal readonly WeakReference<Downloader> weakReference;

    internal readonly object locker = new();
    /// <summary>
    /// 下载器。需要实例化。
    /// </summary>
    public Downloader()
    {
        weakReference = new(this);
    }
    ///
    ~Downloader()
    {
        DownloadTask.CleanRef(this);
    }
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
    private HttpClient HttpClient { get; } = new() { Timeout = TimeSpan.FromMinutes(5) };
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
    public int DownloadThreadLimit { get => TasksWorkingList.AsyncWorkLimit; set => TasksWorkingList.AsyncWorkLimit = value; }
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

    internal class DownloadTaskByLoader : IDownloadingTask
    {
        public string Url { get; set; }
        public string LocalPath { get; set; }
        public long DownloadedBytes => GetBytesDownload();
        public long TotalBytes { get; set; }
        public DownloadTask Owner { get; set; }
        public CancellationToken Cancellation { get; set; }
        private DownloadStatus Status { get => Owner.Status; set => Owner.Status = value; }
        private List<DownloaderTaskInner> Children { get; } = new();
        private long FinishedBytes = 0;
        private int TotalChunks;
        internal long GetBytesDownload()
        {
            if (this.Status is DownloadStatus.Finished) return TotalBytes;
            if (this.Status is DownloadStatus.Error) return TotalBytes;
            if (this.Status is DownloadStatus.Preparing) return 0;
            if (this.Status is DownloadStatus.Canceled) return 0;
            if (this.Status is DownloadStatus.Writing) return TotalBytes;
            if (TotalChunks is 0) return 0;
            long Downloaded = 0;
            lock (Children)
                foreach (var item in Children)
                {
                    Downloaded += Interlocked.Read(ref item.DownloadedBytes);
                }
            Downloaded += FinishedBytes;
            return Downloaded;
        }
        internal DownloadTaskByLoader(string url, string localPath, DownloadTask owner, long totalBytes)
        {
            Url = url;
            LocalPath = localPath;
            Owner = owner;
            TotalBytes = totalBytes;
        }
        public async Task Download()
        {
            //Core.Log($"已开始多线程下载，{Url} => {LocalPath}");
            //下载最低速度，256KB/s
            const double SpeedLimitLess = 256 * 1024L * 0.2; // 低于256KB/s就加速
            const long MinChunkSize = 64 * 1024L;  // 最小 64KB，防止过碎

            if (Owner.Status is DownloadStatus.Finished) return;
            if (Cancellation.IsCancellationRequested)
            {
                this.Status = DownloadStatus.Canceled;
                return;
            }
            this.Status = DownloadStatus.Preparing;
            int chunkCount = 5;
            long size = this.TotalBytes;
            int chunkLast = 5;
            long FileStarts = 0;
            this.TotalChunks = 5;

            this.TotalChunks = chunkCount;

            var TempDir = Path.Combine(Path.GetTempPath(), Core.LauncherShort, "Downloader", Url.GetMd5Hash());
            if (Directory.Exists(TempDir))
            {
                Directory.Delete(TempDir, true);
                Directory.CreateDirectory(TempDir);
            }

            var tasks = new List<Task>();

            if (Cancellation.IsCancellationRequested)
            {
                this.Status = DownloadStatus.Canceled;
                return;
            }

            this.Status = DownloadStatus.Downloading;

            List<Exception> Exs = new();

            CancellationTokenSource _seekerCts = new();

            async Task DlTask()
            {
                try
                {
                    while (true)
                    {
                        if (this.Cancellation.IsCancellationRequested) break;
                        long size;
                        long start;
                        //领取分块
                        lock (this)
                        {
                            if (chunkLast > 0)
                            {
                                start = FileStarts;
                                var lastBytes = this.TotalBytes - start;
                                size = lastBytes / chunkLast + (lastBytes % chunkLast > 0 ? 1 : 0);

                                chunkLast--;
                                FileStarts += size;
                                if (chunkLast <= 0) _seekerCts.Cancel();

                            }
                            else break;
                        }
                        if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));

                        int WaitMs = 0;
                        while (true)
                        {
                            try
                            {
                                var T = new DownloaderTaskInner(this.Owner.Owner, Url, this.Cancellation)
                                {
                                    StartByte = start,
                                    EndByte = start + size - 1
                                };
                                //Core.Log($"正在下载分块{start}->{start + size - 1}");
                                await T.StartDownload();
                                lock (this) this.FinishedBytes += size;
                                break;
                            }
                            catch (Exception ex)
                            {
                                //throw;
                                if (this.Cancellation.IsCancellationRequested) return;
                                lock (this)
                                {
                                    if (Exs.Count > 40 || ex is ArgumentOutOfRangeException)
                                    {
                                        try
                                        {
                                            if (!_seekerCts.IsCancellationRequested)
                                                _seekerCts.Cancel();
                                        }
                                        catch { }
                                        throw;
                                    }
                                    Exs.Add(ex);
                                }
                            }
                            await Task.Delay(WaitMs);
                            WaitMs += 50;
                        }
                    }
                }
                finally
                {
                }
            }

            //Core.Log("已分块1块");
            tasks.Add(Owner.Owner.DoInTasksLimit(DlTask));

            async Task SpeedSeeker()
            {
                while (true)
                {
                    //分块完成
                    if (FileStarts >= size)
                        return;
                    if (this.Cancellation.IsCancellationRequested) return;
                    long LastDlBytes = this.DownloadedBytes;
                    try
                    {
                        await Task.Delay(200, _seekerCts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    bool NeedAddThread = false;
                    lock (this)
                    {
                        long ThisDlBytes = this.DownloadedBytes;
                        long DlSpeed = ThisDlBytes - LastDlBytes;
                        var lastBytes = this.TotalBytes - FileStarts;
                        var cLast = chunkLast;

                        if (DlSpeed < SpeedLimitLess &&
                            lastBytes / (cLast + 1) > MinChunkSize &&
                            tasks.Count < 48) NeedAddThread = true;
                    }
                    if (NeedAddThread)
                        lock (Owner.Owner.locker)
                            if (Owner.Owner.TasksUsing < Owner.Owner.DownloadThreadLimit)
                            {
                                chunkLast++;
                                this.TotalChunks++;
                                tasks.Add(Owner.Owner.DoInTasksLimit(DlTask));
                                //Core.Log($"已分块{TotalChunks}块");
                            }
                }
            }

            try
            {
                await SpeedSeeker();
                await Task.WhenAll(tasks);
                
            }
            catch (AggregateException)
            {
                if (Cancellation.IsCancellationRequested)
                {
                    this.Status = DownloadStatus.Canceled;
                    if (Directory.Exists(TempDir))
                        Directory.Delete(TempDir, true);
                    return;
                }
                var ex = new AggregateException(Exs);
                this.Status = DownloadStatus.Error;
                Owner.Error = new WebException($"并行下载文件失败：{Url}", ex);
                throw Owner.Error;
            }
            finally
            {
                _seekerCts.Dispose();
            }
            if (Cancellation.IsCancellationRequested)
            {
                this.Status = DownloadStatus.Canceled;
                if (Directory.Exists(TempDir))
                    Directory.Delete(TempDir, true);
                return;
            }
            this.Status = DownloadStatus.Writing;
            //合并文件
            DirectoryInfo DirInfo = new(TempDir);
            var chunkFiles = DirInfo.GetFiles("bytes-*") // 返回 FileInfo[]
                .Select(f =>
                (
                    FileInfo: f,
                    Start: long.Parse(f.Name.Substring(6)) // f.Name 就是 "bytes-12345"
                ))
                .OrderBy(x => x.Start)
                .ToList();

            {
                var Dir = Path.GetDirectoryName(LocalPath);
                if (!Directory.Exists(Dir))
                    Directory.CreateDirectory(Dir);
                if (File.Exists(LocalPath))
                    File.Delete(LocalPath);
            }

            using var outputStream = new FileStream(LocalPath, FileMode.Create, FileAccess.Write, FileShare.None);
            foreach (var chunk in chunkFiles)
            {
                var (file, start) = chunk;
                using var inputStream = file.OpenRead(); // FileInfo 直接提供 OpenRead
                await inputStream.CopyToAsync(outputStream);
            }
            DirInfo.Delete(true);
            if (Cancellation.IsCancellationRequested)
            {
                this.Status = DownloadStatus.Canceled;
                if (File.Exists(LocalPath))
                    File.Delete(LocalPath);
                return;
            }
            this.Status = DownloadStatus.Finished;
            return;
        }
    }

    internal class DownloadTaskBySingleThread : IDownloadingTask
    {
        public string Url { get; set; }
        public string LocalPath { get; set; }
        public long DownloadedBytes => GetBytesDownload();
        public long TotalBytes { get; set; }
        public DownloadTask Owner { get; set; }
        public CancellationToken Cancellation { get; set; }
        private DownloadStatus Status { get => Owner.Status; set => Owner.Status = value; }
        private DownloaderTaskInner? Child { get; set; }
        internal long GetBytesDownload()
        {
            if (this.Status is DownloadStatus.Finished) return TotalBytes;
            if (this.Status is DownloadStatus.Error) return TotalBytes;
            if (this.Status is DownloadStatus.Preparing) return 0;
            if (this.Status is DownloadStatus.Canceled) return 0;
            if (this.Status is DownloadStatus.Writing) return TotalBytes;
            long Downloaded = 0;
            if (Child is null) return 0;
            Downloaded = Interlocked.Read(ref Child.DownloadedBytes);
            return Downloaded;
        }
        internal DownloadTaskBySingleThread(string url, string localPath, DownloadTask owner, long totalBytes)
        {
            Url = url;
            LocalPath = localPath;
            Owner = owner;
            TotalBytes = totalBytes;
        }
        public async Task Download()
        {
            //Core.Log($"已开始单线程下载，{Url} => {LocalPath}");

            if (this.Status is DownloadStatus.Finished) return;
            
            long size = this.TotalBytes;



            if (Cancellation.IsCancellationRequested)
            {
                this.Status = DownloadStatus.Canceled;
                return;
            }

            this.Status = DownloadStatus.Downloading;

            List<Exception> Exs = new();

            async Task func()
            {
                try
                {

                    while (true)
                    {
                        var task = new DownloaderTaskInner(Owner.Owner, Url, this.Cancellation)
                        {
                            StartByte = 0,
                            IsSingleThread = true
                        };
                        lock (this) Child = task;
                        try
                        {
                            await task.StartDownload();
                            return;
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {

                            lock (this)
                            {
                                Exs.Add(ex);
                                if (Exs.Count >= 3) throw new AggregateException(Exs);
                            }
                        }
                    }
                }
                finally
                {
                }
            }
            var task = Owner.Owner.DoInTasksLimit(func);

            try
            {
                await task;
            }
            catch (AggregateException ag)
            {
                if (Cancellation.IsCancellationRequested)
                {
                    this.Status = DownloadStatus.Canceled;
                    Directory.Delete(Path.Combine(Path.GetTempPath(), Core.LauncherShort, "Downloader", Url.GetMd5Hash()), true);
                    return;
                }
                var ex = ag.Flatten();
                this.Status = DownloadStatus.Error;
                Owner.Error = new WebException($"单线程下载文件失败：{Url}", ex);
                throw Owner.Error;
            }
            if (Cancellation.IsCancellationRequested)
            {
                this.Status = DownloadStatus.Canceled;
                Directory.Delete(Path.Combine(Path.GetTempPath(), Core.LauncherShort, "Downloader", Url.GetMd5Hash()), true);
                return;
            }
            this.Status = DownloadStatus.Writing;
            //合并文件
            var TempDir = Path.Combine(Path.GetTempPath(), Core.LauncherShort, "Downloader", Url.GetMd5Hash());
            var FileTemp = Path.Combine(TempDir, "bytes-0");

            {
                var Dir = Path.GetDirectoryName(LocalPath);
                if (!Directory.Exists(Dir))
                    Directory.CreateDirectory(Dir);
            }
            File.Copy(FileTemp, LocalPath, true);

            Directory.Delete(TempDir, true);
            if (Cancellation.IsCancellationRequested)
            {
                this.Status = DownloadStatus.Canceled;
                File.Delete(LocalPath);
                return;
            }
            this.Status = DownloadStatus.Finished;
            return;
        }
    }
    internal class DownloadTaskByResponse : IDownloadingTask
    {
        public string Url { get; set; }
        public string LocalPath { get; set; }
        public long DownloadedBytes => GetBytesDownload();
        public long TotalBytes { get; set; }
        public DownloadTask Owner { get; set; }
        public CancellationToken Cancellation { get; set; }

        private DownloadStatus Status
        {
            get => Owner.Status;
            set => Owner.Status = value;
        }

        private readonly HttpResponseMessage _response;
        private long _downloadBytes;

        internal DownloadTaskByResponse(string localPath, DownloadTask owner, HttpResponseMessage response,string url)
        {
            LocalPath = localPath;
            Owner = owner;
            _response = response;
            Url = url ?? string.Empty;
            TotalBytes = response.Content.Headers.ContentLength ?? 0;
        }

        private long GetBytesDownload()
        {
            if (Status == DownloadStatus.Finished || Status == DownloadStatus.Error || Status == DownloadStatus.Writing)
                return TotalBytes;
            if (Status == DownloadStatus.Preparing || Status == DownloadStatus.Canceled)
                return 0;
            return Interlocked.Read(ref _downloadBytes);
        }

        public async Task Download()
        {
            //Core.Log($"已开始响应流下载，{Url} => {LocalPath}");

            if (Status == DownloadStatus.Finished || Cancellation.IsCancellationRequested)
            {
                if (Cancellation.IsCancellationRequested)
                    Status = DownloadStatus.Canceled;
                return;
            }

            Status = DownloadStatus.Preparing;

            if (Cancellation.IsCancellationRequested)
            {
                Status = DownloadStatus.Canceled;
                return;
            }

            Status = DownloadStatus.Downloading;

            try
            {
                using var response = _response;
                using var networkStream = await response.Content.ReadAsStreamAsync();

                // 确保目录存在
                var dir = Path.GetDirectoryName(LocalPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var fileMode = File.Exists(LocalPath) ? FileMode.Open : FileMode.Create;
                using var fileStream = new FileStream(LocalPath, fileMode, FileAccess.Write, FileShare.None, 81920, true);

                var buffer = new byte[81920];
                int bytesRead;

                while ((bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length, Cancellation)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, Cancellation);
                    Interlocked.Add(ref _downloadBytes, bytesRead);
                }

                // 校验大小（若已知）
                if (TotalBytes > 0 && _downloadBytes != TotalBytes)
                    throw new Exception($"下载大小不一致：预期 {TotalBytes}，实际 {_downloadBytes}");

                Status = DownloadStatus.Writing;  // 保持状态流转
                Status = DownloadStatus.Finished;
            }
            catch (OperationCanceledException)
            {
                Status = DownloadStatus.Canceled;
                try { File.Delete(LocalPath); } catch { }
                throw;
            }
            catch (Exception ex)
            {
                if (Cancellation.IsCancellationRequested)
                {
                    Status = DownloadStatus.Canceled;
                    try { File.Delete(LocalPath); } catch { }
                    return;
                }
                Status = DownloadStatus.Error;
                Owner.Error = new WebException($"流式下载失败：{Url}", ex);
                throw Owner.Error;
            }
            finally
            {
                _response?.Dispose();
            }
        }

        public void Cancel()
        {
            // 取消由 Owner 的 Cts 统一控制，这里无需额外操作
        }
    }

    #endregion

    #region 内部下载器
    private class DownloaderTaskInner
    {
        private CancellationToken Cancellation;
        public DownloaderTaskInner(Downloader owner, string url, CancellationToken Cts)
        {
            Owner = owner; Url = url;
            Cancellation = Cts;
        }
        public string Url { get; set; }
        public long StartByte { get; set; }
        public long DownloadedBytes;
        public long EndByte { get; set; }
        public bool IsSingleThread = false;
        public string LocalPath => Path.Combine(Path.GetTempPath(), Core.LauncherShort , "Downloader", Url.GetMd5Hash(), $"bytes-{StartByte}");
        public Downloader Owner { get; set; }
        public async Task StartDownload()
        {
            try
            {
                var result = await Owner.SendRequest(Url, HttpMethod.Get, !IsSingleThread ? new RangeHeaderValue(StartByte, EndByte) : null, ct: Cancellation);
                using var response = result.response;
                using var request = result.request;

                using var networkStream = await response.Content.ReadAsStreamAsync();

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"下载失败，状态码：{response.StatusCode}，URL：{Url}");

                var localPath = LocalPath;
                {
                    var Dir = Path.GetDirectoryName(localPath);
                    if (!Directory.Exists(Dir))
                        Directory.CreateDirectory(Dir);
                }
                if (File.Exists(localPath)) File.Delete(localPath);
                using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);


                byte[] buffer = new byte[81920];
                int bytesRead;

                while ((bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length, Cancellation)) > 0)
                {
                    // 写入本地磁盘
                    await fileStream.WriteAsync(buffer, 0, bytesRead, Cancellation);

                    // 更新当前的总下载字节数
                    Interlocked.Add(ref DownloadedBytes, bytesRead);
                }

                if (!IsSingleThread && DownloadedBytes != this.EndByte - StartByte + 1) throw new WebException($"下载字节数与期望不符，期望{this.EndByte - this.StartByte + 1},实际{DownloadedBytes}");
            }
            finally
            {
            }
        }
    }
    #endregion

}

internal interface IDownloadingTask
{
    string Url { get; set; }
    string LocalPath {  get; set; }
    long DownloadedBytes { get; }
    long TotalBytes { get; set; }
    CancellationToken Cancellation { set; }
    DownloadTask Owner { set; }
    Task Download();
}