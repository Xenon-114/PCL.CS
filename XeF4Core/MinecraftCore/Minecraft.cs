using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XeF4Core.MinecraftCore;



/// <summary>
/// Minecraft实例
/// </summary>
public class Minecraft : IDisposable
{
    /// <summary>
    /// 用于进行文件下载的默认下载器。
    /// </summary>
    public static Downloader? DefaultDownloader;

    #region 实例成员
    private bool _disposed = false;

    private void ThrowIfNotAvailable()
    {
        if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        if (!IsAvailable) throw new InvalidOperationException("实例不可用！");
    }
    /// <summary>
    /// 版本名称
    /// </summary>
    public string VersionName
    {
        get
        {
            ThrowIfNotAvailable();
            return MinecraftJson.Id ?? Path.GetFileNameWithoutExtension(JsonPath.Name);
        }
        set
        {
            ThrowIfNotAvailable();
            MinecraftJson.Id = value;
            ChangeJson();
        }
    }
    /// <summary>
    /// Json文件路径
    /// </summary>
    public FileInfo JsonPath { get; private set; }
    /// <summary>
    /// Jar文件路径
    /// </summary>
    public FileInfo CorePath { get; private set; }
    /// <summary>
    /// 公共库路径，用于存储全局字典
    /// </summary>
    public DirectoryInfo PublicPath { get; set; }
    /// <summary>
    /// 私有库文件路径，用于调整版本隔离
    /// </summary>
    public DirectoryInfo PrivatePath { get; set; }
    /// <summary>
    /// 游戏版本文件夹
    /// </summary>
    public DirectoryInfo VersionPath { get; private set; }
    /// <summary>
    /// 版本Json信息
    /// </summary>
    public MinecraftJsonInfomation MinecraftJson
    {
        get
        {
            ThrowIfNotAvailable();
            if (_LastJsonChanged != JsonPath.LastWriteTime) RefreshJson();
            return _Json;
        }
    }
    private void ChangeJson()
    {
        File.WriteAllText(JsonPath.FullName, JsonConvert.SerializeObject(MinecraftJson, Formatting.Indented));
        _LastJsonChanged = JsonPath.LastWriteTime;
    }
    private void RefreshJson()
    {
        using FileStream file = JsonPath.OpenRead();
        var json = Extensions.DeserializeJson<MinecraftJsonInfomation>(file) ?? throw new JsonException($"无法解析位于 {JsonPath.FullName} 的Json！");
        _Json = json;
    }
    private MinecraftJsonInfomation _Json;
    private DateTime _LastJsonChanged;
    /// <summary>
    /// 游戏启动所需的Java
    /// </summary>
    public Java? Java { get; set; }
    /// <summary>
    /// 游戏版本
    /// </summary>
    public string Version { get => MinecraftJson.ClientVersion ?? throw new JsonException($"无法解析位于 {JsonPath.FullName} 的Json！"); }
    /// <summary>
    /// 游戏发布号
    /// </summary>
    public BuildType BuildType { get; private set; }
    /// <summary>
    /// 返回该实例是否可用
    /// </summary>
    /// <returns></returns>
    public bool IsAvailable => JsonPath.Exists && Path.GetExtension(JsonPath.Name) is ".json" && !_disposed && Minecrafts.ContainsValue(this);
    private Minecraft(FileInfo jsonPath)
    {
        VersionPath = jsonPath.Directory;
        JsonPath = jsonPath;
        CorePath = new FileInfo(Path.Combine(jsonPath.DirectoryName, Path.GetFileNameWithoutExtension(jsonPath.Name) + ".jar"));
        RefreshJson();
        _ = VersionName ?? throw new JsonException($"无法解析位于 {JsonPath.FullName} 的Json！");
        _ = _Json ?? throw new JsonException($"无法解析位于 {JsonPath.FullName} 的Json！");
        PublicPath = jsonPath.Directory;
        PrivatePath = jsonPath.Directory;
        BuildType = MinecraftJson.Type switch
        {
            "release" => BuildType.Release,
            "snapshot" => BuildType.Snapshot,
            "old_alpha" => BuildType.Alpha,
            "old_beta" => BuildType.Beta,
            _ => BuildType.Release
        };
    }
    private static Dictionary<string, Minecraft> Minecrafts = new();
    /// <summary>
    /// 销毁该Minecraft对象。
    /// </summary>
    public void Dispose()
    {
        Minecrafts.Remove(JsonPath.FullName);
        _disposed = true;
    }

    /// <summary>
    /// 创建<see cref="XeF4Core.MinecraftCore.MinecraftRunner"/>
    /// </summary>
    /// <returns></returns>
    public MinecraftRunner CreateRunner()
    {
        ThrowIfNotAvailable();
        return MinecraftRunner.CreateRunner(this);
    }

    /// <summary>
    /// 判断一个版本是否是愚人节版本
    /// </summary>
    /// <returns></returns>
    public bool IsAprilFool() => IsAprilFool(this);

    /// <summary>
    /// 进行文件校验补全
    /// </summary>
    /// <returns></returns>
    /// <exception cref="JsonException"></exception>
    public IMyTask FileChecks() => FileChecks(DefaultDownloader ?? throw new NullReferenceException("请使用FileChecks(Downloader)函数"));
    #region 下载任务
    private class FileCheckTask : IMyTask
    {
        public double Progress
        {
            get
            {
                double progress = 0;
                lock (downloads)
                {
                    if (downloads.Count == 0) return 1;
                    foreach (var download in downloads)
                    {
                        if (download.Progress is not double.NaN)
                            progress += download.Progress;
                    }
                    progress /= downloads.Count;
                }
                return progress;
            }
        }
        public List<DownloadTask> downloads { get; } = new();
        public string Name { get; } = "文件校验";
        public Task Task
        {
            get
            {
                lock (downloads)
                    if (_Task is null)
                    {
                        List<Task> tasks = new();
                        foreach (var download in downloads)
                        {
                            tasks.Add(download.WaitAsync());
                        }
                        _Task = Task.WhenAll(tasks);
                    }
                return _Task;
            }
        }
        private Task? _Task = null;
        public TaskAwaiter GetAwaiter()
        {
            if (Task is null) throw new NullReferenceException();
            return Task.GetAwaiter();
        }
        public void Wait()
        {
            if (Task is null) throw new NullReferenceException();
            Task.Wait();
        }
        public Task WaitAsync()
        {
            if (Task is null) throw new NullReferenceException();
            return Task;
        }
    }
    private class AssetsCheckTask : IMyTask
    {
        public double Progress
        {
            get
            {
                double progress = 0;
                lock (downloads)
                {
                    if (downloads.Count == 0) return 1;
                    foreach (var download in downloads)
                    {
                        if (download.Progress is not double.NaN)
                            progress += download.Progress;
                    }
                    progress /= downloads.Count;
                }
                return progress;
            }
        }
        public List<DownloadTask> downloads { get; } = new();
        public string Name { get; } = "文件校验";
        public Task Task
        {
            get
            {
                lock (downloads)
                    if (_Task is null)
                    {
                        List<Task> tasks = new();
                        foreach (var download in downloads)
                        {
                            tasks.Add(download.WaitAsync());
                        }
                        _Task = Task.WhenAll(tasks);
                    }
                return _Task;
            }
        }
        private Task? _Task = null;
        public TaskAwaiter GetAwaiter()
        {
            if (Task is null) throw new NullReferenceException();
            return Task.GetAwaiter();
        }
        public void Wait()
        {
            if (Task is null) throw new NullReferenceException();
            Task.Wait();
        }
        public Task WaitAsync()
        {
            if (Task is null) throw new NullReferenceException();
            return Task;
        }
    }
    #endregion
    /// <summary>
    /// 进行文件校验补全
    /// </summary>
    /// <returns></returns>
    /// <exception cref="JsonException"></exception>
    public IMyTask FileChecks(Downloader downloader)
    {
        ThrowIfNotAvailable();
        FileCheckTask tasks = new();
        if (MinecraftJson.Downloads?.Client is not null)
        {
            Artifact client = MinecraftJson.Downloads.Client;
            string Sha1 = client.Sha1 ?? throw new JsonException("读取Json失败！");
            string url = client.Url ?? throw new JsonException("读取Json失败！");
            if (!CorePath.Exists || Minecraft.ComputeFileSha1(CorePath) != Sha1)
            {
                CorePath.Delete();
                var dltsk = downloader.Download(url, CorePath.FullName);
                Core.Log($"开始下载游戏本体：{url} => {CorePath.FullName}");
                tasks.downloads.Add(dltsk);
            }
        }
        foreach (var lib in MinecraftJson.Libraries)
        {
            if (lib == null) continue;
            {
                bool IsEnable = true;
                if (lib.Rules is not null)
                    foreach (Rule Rule in lib.Rules)
                    {
                        IsEnable &= IsRulesEnable(Rule);
                        if (!IsEnable) break;
                    }

                Core.Log($"检查库{lib.Name}，启用：{IsEnable}");
                if (!IsEnable) continue;
            }
            string LocalPath = GetLocalLibraryPath(lib, PublicPath.FullName);
            if (!Directory.Exists(Path.GetDirectoryName(LocalPath))) Directory.CreateDirectory(Path.GetDirectoryName(LocalPath));
            //检查
            bool needsDownload = !File.Exists(LocalPath);
            if (!needsDownload && lib.Downloads?.Artifact?.Sha1 != null)
            {
                try
                {
                    string actualSha1 = ComputeFileSha1(LocalPath);
                    needsDownload = actualSha1 != lib.Downloads.Artifact.Sha1;
                }
                catch
                {
                    needsDownload = true; // 读不了就当需要重下
                }

            }
            if (needsDownload)
            {
                if (File.Exists(LocalPath))
                    File.Delete(LocalPath);
                //不给我下就跳过
                if (lib.Downloads?.Artifact?.Url is not null)
                {
                    var dltsk = downloader.Download(lib.Downloads.Artifact.Url, LocalPath);
                    tasks.downloads.Add(dltsk);
                }
            }
        }
        return tasks;
    }
    /// <summary>
    /// 进行资源文件补全
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public IMyTask AssetsChecks() => AssetsChecks(DefaultDownloader ?? throw new NullReferenceException("请使用FileChecks(Downloader)函数"));
    /// <summary>
    /// 进行资源文件补全
    /// </summary>
    /// <param name="downloader"></param>
    /// <returns></returns>
    /// <exception cref="JsonException"></exception>
    public IMyTask AssetsChecks(Downloader downloader)
    {
        ThrowIfNotAvailable();
        AssetsCheckTask assetsCheck = new();
        if (MinecraftJson.AssetIndex is not null)
        {
            //1、解析映射
            AssetIndex assetIndex = MinecraftJson.AssetIndex; // 来自 version.json
            string? indexUrl = assetIndex.Url;
            string? indexSha1 = assetIndex.Sha1;

            async Task CheckAssets()
            {
                string indexPath = Path.Combine(PublicPath.FullName, "assets", "indexes", $"{assetIndex.Id}.json");
                //Core.Log($"开始下载资源索引文件：{indexUrl} => {indexPath}");
                if (!File.Exists(indexPath) || Minecraft.ComputeFileSha1(indexPath) != indexSha1)
                    await downloader.Download(indexUrl, indexPath);
                //Core.Log($"下载资源索引文件完成");
                using var fStream = File.OpenRead(indexPath);
                var Index = Extensions.DeserializeJson<MinecraftAssetsIndex>(fStream);
                if (Index is null) return;
                var objectPath = Path.Combine(PublicPath.FullName, "assets", "objects");
                if (!Directory.Exists(objectPath)) Directory.CreateDirectory(objectPath);
                foreach (var assetIndex in Index.Objects)
                {
                    var Hash = assetIndex.Value.Hash;
                    string subDir = Hash.Substring(0, 2);
                    string localPath = Path.Combine(PublicPath.FullName, "assets", "objects", subDir, Hash);
                    if (!File.Exists(localPath) || new FileInfo(localPath).Length != assetIndex.Value.Size)
                    {
                        // 下载：https://resources.download.minecraft.net/<hash前两位>/<hash>
                        string url = $"https://resources.download.minecraft.net/{subDir}/{Hash}";
                        //Core.Log($"下载资源文件：{url} => {localPath}");
                        assetsCheck.downloads.Add(downloader.Download(url, localPath));
                    }
                }
            }
            if (indexUrl is not null && indexSha1 is not null)
            {
                CheckAssets().Wait();
                return assetsCheck;
            }
            else throw new JsonException($"未能解析以下两者之一的值：{nameof(MinecraftJson.AssetIndex.Url)}/{nameof(MinecraftJson.AssetIndex.Sha1)}");
        }
        else throw new JsonException($"未能解析以下值：{nameof(MinecraftJson.AssetIndex)}");
    }

    private static bool IsRulesEnable(Rule rule)
    {
        //{
        //    Console.WriteLine("规则");
        //    if (rule.Os is not null) Console.WriteLine($"适用于平台{rule.Os.Name.ToString()}，架构{rule.Os.Arch?.ToString()??"任意"}");
        //}
        bool Used = true;
        if (rule.Os is not null)
            if (rule.Os.Name is not null && !RuntimeInformation.IsOSPlatform(((OSPlatform)rule.Os.Name).ToOsPlatform())) Used = false;
            else if (!Extensions.IsArchMatch(rule.Os.Arch)) Used = false;

        if (rule.Action is "disallow") Used = !Used;
        return Used;
    }

    /// <summary>
    /// 删除该实例
    /// </summary>
    public void Delete()
    {
        ThrowIfNotAvailable();
        VersionPath.Delete(true);
        Dispose();
    }

    #endregion

    #region 静态方法
    /// <summary>
    /// 从一个指定文件夹创建Minecraft
    /// </summary>
    /// <param name="jsonPath"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static Minecraft FromJsonPath(string jsonPath)
    {
        if (jsonPath is null) throw new ArgumentNullException(nameof(jsonPath));
        var Info = new FileInfo(jsonPath);
        if (Minecrafts.TryGetValue(Info.FullName, out var MC))
        {
            if (!MC.IsAvailable)
            {
                MC.Dispose();
                throw new Exception("创建指定实例失败");
            }
            else return MC;
        }
        else
        {
            if (!Info.Exists || Path.GetExtension(jsonPath) is not ".json") throw new Exception("创建指定实例失败");
        }
        var mc = new Minecraft(Info);
        return mc;
    }
    /// <summary>
    /// 获取文件SHA-1，用于MC标准文件校验
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string ComputeFileSha1(string filePath)
    {
        using var sha1 = SHA1.Create();
        using var stream = File.OpenRead(filePath);
        byte[] hashBytes = sha1.ComputeHash(stream);
        return hashBytes.ToHex().ToLowerInvariant();
    }
    /// <summary>
    /// 获取文件SHA-1，用于MC标准文件校验
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string ComputeFileSha1(FileInfo filePath)
    {
        using var sha1 = SHA1.Create();
        using var stream = filePath.OpenRead();
        byte[] hashBytes = sha1.ComputeHash(stream);
        return hashBytes.ToHex().ToLowerInvariant();
    }
    /// <summary>
    /// 获得指定库文件的绝对路径
    /// </summary>
    /// <param name="lib"></param>
    /// <param name="minecraftRoot"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static string GetLocalLibraryPath(Library lib, string minecraftRoot)
    {
        // 1. 优先取 downloads.artifact.path（如果有）
        if (lib.Downloads?.Artifact?.Path != null)
        {
            return Path.Combine(minecraftRoot, "libraries", lib.Downloads.Artifact.Path);
        }

        // 2. 回退：手动按 Maven 规则生成路径
        return BuildMavenPath(lib.Name ?? throw new NullReferenceException());
    }
    private static string BuildMavenPath(string mavenCoordinate)
    {
        // 格式：groupId:artifactId:version[:classifier]
        var parts = mavenCoordinate.Split(':');
        string groupId = parts[0];
        string artifactId = parts[1];
        string version = parts[2];
        string? classifier = parts.Length > 3 ? parts[3] : null;

        string groupPath = groupId.Replace('.', Path.DirectorySeparatorChar);
        string fileName = $"{artifactId}-{version}";
        if (!string.IsNullOrEmpty(classifier))
            fileName += $"-{classifier}";
        fileName += ".jar";

        return Path.Combine(groupPath, artifactId, version, fileName);
    }
    /// <summary>
    /// 判断一个版本是否是愚人节版本
    /// </summary>
    /// <param name="mc"></param>
    /// <returns></returns>
    public static bool IsAprilFool(Minecraft mc)
    {
        if (!DateTimeOffset.TryParse(mc.MinecraftJson.ReleaseTime, out var result)) return false;
        DateTimeOffset Date = result;
        if (Date.Month is 4 && Date.Day is 1) return true;
        else return false;
    }
    #endregion

    #region 下载
    /// <summary>
    /// 下载任务
    /// </summary>
    public ITask? Downloading { get; private set; }
    /// <summary>
    /// 表示当前实例是否在下载
    /// </summary>
    public bool IsDownloading => Downloading is not null;
    /// <summary>
    /// 取消下载
    /// </summary>
    public void DownloadCancel()
    {
        Cancellation?.Cancel();
    }
    private CancellationTokenSource Cancellation { get; set; } = new();

    /// <summary>
    /// 下载任务
    /// </summary>
    public class MinecraftDownloadTask : IMyTaskList
    {
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public IReadOnlyList<IMyTask> Children => _tasks;
        internal readonly List<IMyTask> _tasks = [];
        public double Progress { get; }
        public string Name { get; set; } = "下载Minecraft";
        internal readonly TaskCompletionSource<object?> Finishing = new();
        private Task? Waiter = null;
        public TaskAwaiter GetAwaiter() => (Finishing.Task as Task).GetAwaiter();

        public void Wait() => Finishing.Task.Wait();
        public Task WaitAsync()
        {
            Waiter ??= Task.Run(async () => { await Finishing.Task; });
            return Waiter;
        }
        internal readonly TaskCompletionSource<Minecraft> Mc = new();
        public Minecraft Minecraft => Mc.Task.Result;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
    }
    /// <summary>
    /// 下载一个版本
    /// </summary>
    /// <param name="version">版本</param>
    /// <param name="Name">名称</param>
    /// <param name="downloader">所使用的下载器</param>
    /// <param name="directory">目标文件夹</param>
    /// <returns></returns>
    public static MinecraftDownloadTask Download(MinecraftVersion version, string Name, Downloader downloader, DirectoryInfo directory) =>
        Download(version, Name, downloader, directory, directory);
    /// <summary>
    /// 下载一个版本
    /// </summary>
    /// <param name="version">版本</param>
    /// <param name="downloader">所使用的下载器</param>
    /// <param name="directory">目标文件夹</param>
    /// <param name="PublicPath">目标实例的公共目录</param>
    /// <returns></returns>
    public static MinecraftDownloadTask Download(MinecraftVersion version, Downloader downloader, DirectoryInfo directory, DirectoryInfo PublicPath) =>
        Download(version, version.Id ?? throw new ArgumentNullException(nameof(version.Id)), downloader, directory, PublicPath);
    /// <summary>
    /// 下载一个版本
    /// </summary>
    /// <param name="version">版本</param>
    /// <param name="downloader">所使用的下载器</param>
    /// <param name="directory">目标文件夹</param>
    /// <returns></returns>
    public static MinecraftDownloadTask Download(MinecraftVersion version,Downloader downloader,DirectoryInfo directory)=>
        Download(version, version.Id ?? throw new ArgumentNullException(nameof(version.Id)), downloader, directory, directory);

    /// <summary>
    /// 下载一个版本
    /// </summary>
    /// <param name="version">版本</param>
    /// <param name="Name">名称</param>
    /// <param name="downloader">所使用的下载器</param>
    /// <param name="directory">目标文件夹</param>
    /// <param name="PublicPath">目标实例的公共目录</param>
    /// <returns></returns>
    public static MinecraftDownloadTask Download(MinecraftVersion version, string Name, Downloader downloader, DirectoryInfo directory,DirectoryInfo PublicPath)
    {
        string JsonPath = Path.Combine(directory.FullName, $"{Name}.json");

        if (File.Exists(JsonPath)) File.Delete(JsonPath);
        if (Minecrafts.TryGetValue(JsonPath, out var result)) result.Dispose();
        
        MinecraftDownloadTask task = new();

        var dwnldtask1 = downloader.Download(version.Url ?? throw new ArgumentException($"{nameof(version)}中的参数不合法！"), JsonPath);

        task._tasks.Add(dwnldtask1);

        CheckTask checkTaskFile = new();
        checkTaskFile.Name = "下载原版库文件";
        Minecraft? mc = null;
        async Task FileDownload()
        {
            try
            {
                await dwnldtask1;
            }
            catch(Exception ex) 
            {
                task.Finishing.SetException(ex);
                throw;
            }
            //1、预先修改Json文件
            {
                MinecraftJsonInfomation jsonInfomation = JsonConvert.DeserializeObject<MinecraftJsonInfomation>(File.ReadAllText(JsonPath)) ?? new();
                jsonInfomation.ClientVersion = jsonInfomation.Id;
                jsonInfomation.Id = Name;
                File.WriteAllText(JsonPath, JsonConvert.SerializeObject(jsonInfomation));
            }
            mc = FromJsonPath(JsonPath);
            mc.PublicPath = PublicPath;
            task.Mc.SetResult(mc);
            int FailCounter = 0;
            while (true)
                try
                {
                    var checkTask = mc.FileChecks(downloader);
                    checkTaskFile.GetProgress = () => checkTask.Progress;
                    await checkTask;
                    return;
                }
                catch
                {
                    if (mc.Cancellation.IsCancellationRequested) return;
                    FailCounter++;
                    if (FailCounter > 9) return;
                    await Task.Delay(600);
                }
        }
        Task FileTask = FileDownload();
        checkTaskFile.Task = FileTask;

        CheckTask checkTaskAssets = new();
        async Task AssetsDownload()
        {
            await FileTask;
            //此时mc已由上面的函数完成
            if (mc is null)
            {
                task.Finishing.SetException( new NullReferenceException());
                return;
            }
            int FailCounter = 0;
            if (mc.Cancellation.IsCancellationRequested) return;
            while (true)
                try
                {
                    var checkTask = mc.AssetsChecks(downloader);
                    checkTaskAssets.GetProgress = () => checkTask.Progress;
                    await checkTask;
                    break;
                }
                catch
                {
                    if (mc.Cancellation.IsCancellationRequested) return;
                    FailCounter++;
                    if (FailCounter > 20) break;
                    await Task.Delay(600);
                }
            task.Finishing.SetResult(null);
        }
        checkTaskAssets.Task = AssetsDownload();
        task._tasks.Add(checkTaskFile);
        task._tasks.Add(checkTaskAssets);
        return task;
    }
    private class CheckTask : IMyTask
    {
        public double Progress { get => GetProgress?.Invoke() ?? 0; }
        public Func<double>? GetProgress;
        public string? Name { get; set; }

        public Task? Task;
        public TaskAwaiter GetAwaiter() => Task?.GetAwaiter()??throw new NullReferenceException();
        public void Wait() => Task?.Wait();
        public Task WaitAsync() => Task ?? throw new NullReferenceException();
    }

    #endregion

}


/// <summary>
/// 启动器拓展
/// </summary>
public interface IMinecraftRunnerExtra
{
    /// <summary>
    /// 拓展实例提供的宏替换。后加入的会替换前面的。
    /// </summary>
    public KeyValuePair<string, string>[]? MacroReplacement { get; }
    /// <summary>
    /// 拓展实例提供的Jvm参数。小心重复参数。
    /// </summary>
    public string[]? JvmArgsAdd { get; }
    /// <summary>
    /// 拓展实例提供的游戏参数。小心重复参数。
    /// </summary>
    public string[]? GameArgsAdd { get; }
    /// <summary>
    /// 拓展中的特性。可标明启用或不启用。
    /// </summary>
    public KeyValuePair<string, bool>[]? Features { get; }
}