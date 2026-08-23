using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XeF4Core.MinecraftCore;

/// <summary>
/// Minecraft启动器实例
/// </summary>
public partial class MinecraftRunner
{
    /// <summary>
    /// 从指定Minecraft创建Runner
    /// </summary>
    /// <param name="minecraft"></param>
    /// <returns></returns>
    public static MinecraftRunner CreateRunner(Minecraft minecraft)
    {
        return new MinecraftRunner(minecraft);
    }


    /// <summary>
    /// 启动器用于进行文件补全的指定下载器。
    /// </summary>
    public Downloader MinecraftDownloader
    {
        get => _downloader ?? Minecraft.DefaultDownloader ?? throw new NullReferenceException("没有可用的下载器！");
        set => _downloader = value;
    }
    private Downloader? _downloader = null;
    private MinecraftJsonInfomation Jsons { get; }
    private readonly Minecraft minecraft;
    /// <summary>
    /// 本地临时文件路径
    /// </summary>
    public string NativeDir { get; private set; }
    /// <summary>
    /// 获取或设置启动游戏所用的Java。
    /// </summary>
    public Java? GameJava { get; set; }
    /// <summary>
    /// 创建<see cref="MinecraftRunner"/>
    /// </summary>
    /// <param name="minecraft"></param>
    private MinecraftRunner(Minecraft minecraft)
    {
        //预先反序列化Json
        MinecraftJsonInfomation? V = LoadMinecraftVersion(minecraft.JsonPath);
        GameJava = minecraft.Java;
        Jsons = V ?? throw new JsonException("读取Json失败！");

        if (Jsons.Patches.Count > 0)
        {
            foreach (var patch in V.Patches)
            {
                if (patch.Arguments is not null)
                {
                    Arguments Args = patch.Arguments;
                    if (Args.JvmArgs.Count > 0)
                        Jsons.Arguments?.JvmArgs.AddRange(Args.JvmArgs);
                    if (Args.GameArgs.Count > 0)
                        Jsons.Arguments?.GameArgs.AddRange(Args.GameArgs);
                }
                if(patch.Libraries.Count > 0)
                {
                    Jsons.Libraries.AddRange(patch.Libraries);
                }
                if(!string.IsNullOrEmpty(patch.MainClass))
                    Jsons.MainClass = patch.MainClass;
            }
        }

        this.minecraft = minecraft;
        NativeDir = Path.Combine(minecraft.VersionPath.FullName, Path.GetFileNameWithoutExtension(minecraft.JsonPath.Name) + "-natives");
    }
    /// <summary>
    /// 加入该启动器的拓展。
    /// </summary>
    public List<IMinecraftRunnerExtra> Extras { get; } = new();
    private string? AuthPlayerName;
    private string? AuthUuid;
    private string? AuthAccessToken;
    private string? UserType;
    private bool IsLogin = false;
    private string AuthXuid = "";

    private bool IsRulesEnable(Rule rule, IReadOnlyDictionary<string, bool>? Features)
    {

        //{
        //    Console.WriteLine("规则");
        //    if (rule.Os is not null) Console.WriteLine($"适用于平台{rule.Os.Name.ToString()}，架构{rule.Os.Arch?.ToString()??"任意"}");
        //}
        bool Used = true;
        if (rule.Os is not null)
            if (rule.Os.Name is not null && !RuntimeInformation.IsOSPlatform(((OSPlatform)rule.Os.Name).ToOsPlatform())) Used = false;
            else if (!Extensions.IsArchMatch(rule.Os.Arch)) Used = false;


        if (rule.Features is not null)
            if (Features is null) Used = false;
            else foreach (var f in rule.Features)
                if (!Features.TryGetValue(f.Key, out var value) || f.Value != value)
                    Used = false;
        if (rule.Action is "disallow") Used = !Used;
        return Used;
    }
    /// <summary>
    /// 使用登录信息登录游戏
    /// </summary>
    /// <param name="PlayerName"></param>
    /// <param name="Uuid"></param>
    /// <param name="AccessToken"></param>
    /// <param name="Type"></param>
    /// <param name="Xuid"></param>
    /// <exception cref="Exception"></exception>
    public void Login(string PlayerName, string Uuid, string AccessToken, string Type, string Xuid)
    {
        if (IsLogin) throw new Exception("不能多次登录！");
        IsLogin = true;
        AuthPlayerName = PlayerName;
        AuthUuid = Uuid;
        AuthAccessToken = AccessToken;
        AuthXuid = Xuid;
        UserType = Type;
    }
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
    /// <summary>
    /// 进行文件校验补全
    /// </summary>
    /// <returns></returns>
    /// <exception cref="JsonException"></exception>
    public IMyTaskList FileChecks()
    {
        Dictionary<string, bool> Features = new();
        foreach (var i in Extras)
            if (i.Features is not null)
                foreach (var f in i.Features)
                    Features[f.Key] = f.Value;
        MyTaskList myTaskList = new();
        #region 游戏本体检查
        if (Jsons.Downloads?.Client is not null)
        {
            Artifact client = Jsons.Downloads.Client;
            string Sha1 = client.Sha1 ?? throw new JsonException("读取Json失败！");
            string url = client.Url ?? throw new JsonException("读取Json失败！");
            if (!minecraft.CorePath.Exists || Minecraft.ComputeFileSha1(minecraft.CorePath) != Sha1)
            {
                minecraft.CorePath.Delete();
                var dltsk = MinecraftDownloader.Download(url, minecraft.CorePath.FullName);
                Core.Log($"开始下载游戏本体：{url} => {minecraft.CorePath.FullName}");
                myTaskList.Children.Add(dltsk);
            }
        }
        #endregion
        #region 库检查
        FileCheckTask tasks = new();
        if (Jsons.Downloads?.Client is not null)
        {
            Artifact client = Jsons.Downloads.Client;
            string Sha1 = client.Sha1 ?? throw new JsonException("读取Json失败！");
            string url = client.Url ?? throw new JsonException("读取Json失败！");
            if (!minecraft.CorePath.Exists || Minecraft.ComputeFileSha1(minecraft.CorePath) != Sha1)
            {
                minecraft.CorePath.Delete();
                var dltsk = MinecraftDownloader.Download(url, minecraft.CorePath.FullName);
                Core.Log($"开始下载游戏本体：{url} => {minecraft.CorePath.FullName}");
                tasks.downloads.Add(dltsk);
            }
        }
        foreach (var lib in Jsons.Libraries)
        {
            if (lib == null) continue;
            {
                bool IsEnable = true;
                if (lib.Rules is not null)
                    foreach (Rule Rule in lib.Rules)
                    {
                        IsEnable &= IsRulesEnable(Rule, Features);
                        if (!IsEnable) break;
                    }

                Core.Log($"检查库{lib.Name}，启用：{IsEnable}");
                if (!IsEnable) continue;
            }
            string LocalPath = Minecraft.GetLocalLibraryPath(lib, minecraft.PublicPath.FullName);
            if (!Directory.Exists(Path.GetDirectoryName(LocalPath))) Directory.CreateDirectory(Path.GetDirectoryName(LocalPath));
            //检查
            bool needsDownload = !File.Exists(LocalPath);
            if (!needsDownload && lib.Downloads?.Artifact?.Sha1 != null)
            {
                try
                {
                    string actualSha1 = Minecraft.ComputeFileSha1(LocalPath);
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
                    var dltsk = MinecraftDownloader.Download(lib.Downloads.Artifact.Url, LocalPath);
                    tasks.downloads.Add(dltsk);
                }
            }
        }
        myTaskList.Children.Add(tasks);
        #endregion
        #region 资源文件检查
        myTaskList.Children.Add(minecraft.AssetsChecks());
        #endregion
        return myTaskList;
    }
    private bool HasPrepaired = false;
    /// <summary>
    /// 解压本地库
    /// </summary>
    public void PrepairNativeLibs()
    {
        HasPrepaired = true;
        Dictionary<string, bool> Features = new();
        foreach (var i in Extras)
            if (i.Features is not null)
                foreach (var f in i.Features)
                    Features[f.Key] = f.Value;
        foreach (var lib in Jsons.Libraries)
        {
            if (lib == null) continue;
            {
                bool IsEnable = true;
                if (lib.Rules is not null)
                    foreach (Rule Rule in lib.Rules)
                    {
                        IsEnable &= IsRulesEnable(Rule, Features);
                        if (!IsEnable) break;
                    }
                if (!IsEnable) continue;
            }
            string LocalPath = Minecraft.GetLocalLibraryPath(lib, minecraft.PublicPath.FullName);
            if (lib.Natives.Count != 0 || (lib.Name?.Contains(":natives-") ?? false))
            {
                Core.Log($"解压本地库{lib.Name}");
                if (!File.Exists(LocalPath)) continue;
                UnZIPNativeLib(LocalPath, NativeDir);
            }
        }
    }

    /// <summary>
    /// 当游戏输出日志时触发
    /// </summary>
    public event EventHandler<string>? OutputDataReceived;
    /// <summary>
    /// 当游戏输出错误日志时触发
    /// </summary>
    public event EventHandler<string>? ErrorDataReceived;

    /// <summary>
    /// 启动游戏
    /// </summary>
    /// <exception cref="MinecraftJavaNotFoundException"></exception>
    public void Start()
    {
        if(!HasPrepaired) throw new MinecraftStartingException("必须解压本地库然后启动！");
        if (!IsLogin) throw new MinecraftStartingException("必须登录然后启动！");
        if (AuthPlayerName is null || AuthUuid is null || AuthAccessToken is null || UserType is null) throw new MinecraftStartingException("登录失败！");

        if (GameJava is null) throw new MinecraftJavaNotFoundException("Minecraft必须持有Java实例才能启动");
        if (Jsons.MainClass is null) throw new JsonException("读取Json失败！");
        //1、构建Classpath
        List<string> StartLibrarys = [Path.Combine(minecraft.VersionPath.FullName, minecraft.VersionName + ".jar")];

        if (Jsons.Libraries is null) throw new MinecraftStartingException("解析Minecraft库文件时失败！");

        Directory.CreateDirectory(NativeDir);

        //加载启动设置
        Dictionary<string, bool> Features = new();
        foreach (var i in Extras)
            if (i.Features is not null)
                foreach (var f in i.Features)
                    Features[f.Key] = f.Value;

        //加载库
        foreach (var lib in Jsons.Libraries)
        {
            bool IsEnable = true;
            if (lib.Rules is not null)
                foreach (Rule Rule in lib.Rules)
                    if (!(IsEnable &= IsRulesEnable(Rule, Features))) break;
            if (!IsEnable) continue;
            if (IsEnable)
            {
                string LocalPath = Minecraft.GetLocalLibraryPath(lib, minecraft.PublicPath.FullName);
                StartLibrarys.Add(LocalPath);
            }
        }



        Dictionary<string, string> MacroReplacement = new()
        {
            { "classpath", $"\"{string.Join(Path.PathSeparator.ToString(), StartLibrarys)}\"" },
            { "auth_player_name", AuthPlayerName },
            { "auth_uuid", AuthUuid },
            { "auth_access_token", AuthAccessToken },
            { "user_type", UserType },
            { "auth_xuid", AuthXuid },
            { "clientid", "minecraft" },
            { "version_type", Jsons.Type ?? "Unknown" },
            { "game_directory", minecraft.PrivatePath.FullName },
            { "assets_root", Path.Combine(minecraft.PublicPath.FullName, "assets") },
            { "assets_index_name", Jsons.AssetIndex?.Id ?? "" },
            { "natives_directory", NativeDir },
            { "launcher_name", Core.LauncherName },
            { "launcher_version", "1.0.0.0" },
            { "version_name", Jsons.Id ?? Jsons.ClientVersion ?? "Unknown" }
        };




        List<string> JvmArgs = [];
        List<string> GameArgs = [];
        #region 拼接游戏参数
        foreach (var i in Jsons.Arguments?.JvmArgs ?? throw new MinecraftStartingException("无法解析JVM参数"))
        {
            if (i is JValue Val) JvmArgs.Add(Val.ToString());
            if (i is JObject Object)
            {
                if (Object["rules"] is JArray JArray)
                {
                    bool IsREnable = true;
                    foreach (var rule in JArray)
                    {
                        Rule? R = rule.ToObject<Rule>();
                        if (R is null) continue;
                        IsREnable &= IsRulesEnable(R, Features);
                        if (!IsREnable) continue;
                    }
                    if (!IsREnable) continue;
                }
                var value = Object["value"];
                if (value is null) continue;
                switch (value)
                {
                    case JValue jValue:
                        JvmArgs.Add(jValue.ToString());
                        break;
                    case JArray jArray:
                        foreach (var jValue in jArray)
                        {
                            JvmArgs.Add(jValue.ToString());
                        }
                        break;
                }
            }
        }
        foreach (var i in Jsons.Arguments?.GameArgs ?? throw new MinecraftStartingException("无法解析游戏参数"))
        {
            if (i is JValue Val) GameArgs.Add(Val.ToString());
            if (i is JObject Object)
            {
                if (Object["rules"] is JArray JArray)
                {
                    bool IsREnable = true;
                    foreach (var rule in JArray)
                    {
                        Rule? R = rule.ToObject<Rule>();
                        if (R is null) continue;
                        IsREnable &= IsRulesEnable(R, Features);
                        if (!IsREnable) continue;
                    }
                    if (!IsREnable) continue;
                }
                var value = Object["value"];
                if (value is null) continue;
                switch (value)
                {
                    case JValue jValue:
                        GameArgs.Add(jValue.ToString());
                        break;
                    case JArray jArray:
                        foreach (var jValue in jArray)
                        {
                            GameArgs.Add(jValue.ToString());
                        }
                        break;
                }
            }
        }
        #endregion
        foreach (var Extra in Extras)
        {
            if (Extra.MacroReplacement is not null)
                foreach (var replacement in Extra.MacroReplacement)
                    MacroReplacement[replacement.Key] = replacement.Value;
            if (Extra.JvmArgsAdd is not null)
                JvmArgs.AddRange(Extra.JvmArgsAdd);
            if (Extra.GameArgsAdd is not null)
                GameArgs.AddRange(Extra.GameArgsAdd);
        }
        var GameArgsArray = ReplaceMacrosInArray(GameArgs.ToArray(), MacroReplacement);
        var JvmArgsArray = ReplaceMacrosInArray(JvmArgs.ToArray(), MacroReplacement);
        {
            Core.Log($"启动参数：Jvm={string.Join(" ", JvmArgsArray)}   Game={string.Join(" ", GameArgsArray)}");
        }


        MinecraftProcess = GameJava.RunProcessWithMainClass(JvmArgsArray, null, Jsons.MainClass, GameArgsArray);
        MinecraftProcess.OutputDataReceived += (s, e) => OutputDataReceived?.Invoke(s, e.Data);
        MinecraftProcess.ErrorDataReceived += (s, e) => ErrorDataReceived?.Invoke(s, e.Data);
        MinecraftProcess.BeginOutputReadLine();
        MinecraftProcess.BeginErrorReadLine();
        MinecraftProcess.Exited += (s, e) =>
        {
            IsExited = true;
            MinecraftProcess.Dispose();
            MinecraftProcess = null;
            Complete.TrySetResult(null);
            Directory.Delete(NativeDir, true);
        };
    }
    /// <summary>
    /// 清理临时目录
    /// </summary>
    public void CleanUp()
    {
        Directory.Delete(NativeDir, true);
    }
    private TaskCompletionSource<object?> Complete = new();
    private Process? MinecraftProcess;
    private bool IsExited = false;
    /// <summary>
    /// 杀掉Minecraft进程
    /// </summary>
    /// <exception cref="InvalidOperationException "></exception>
    [Obsolete("你确认要杀掉进程吗？你真的确认吗？")]
    public void KILL()
    {
        if (IsExited) return;
        if (MinecraftProcess is null) throw new InvalidOperationException("无法停止进程，因为进程未开始");
        if (MinecraftProcess.HasExited) return;
        MinecraftProcess.Kill();
        CleanUp();
    }
    /// <summary>
    /// 等待程序退出
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void WaitForExit()
    {
        if (IsExited) return;
        if (MinecraftProcess is null) throw new InvalidOperationException("无法停止进程，因为进程未开始");
        if (MinecraftProcess.HasExited) return;
        MinecraftProcess.WaitForExit();
        CleanUp();
    }
    /// <summary>
    /// 异步等待程序退出
    /// </summary>
    /// <returns></returns>
    public async Task WaitForExitAsync()
    {
        await Complete.Task;
        CleanUp();
    }

    private static void UnZIPNativeLib(string jarPath, string outputDir)
    {
        if (!File.Exists(jarPath))
            throw new FileNotFoundException($"Natives JAR 文件不存在: {jarPath}");
        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);
        //Console.WriteLine($"开始解压本地库{jarPath}");
        using var archive = ZipFile.OpenRead(jarPath);
        foreach (var entry in archive.Entries)
        {
            if (entry.FullName.EndsWith("/") || entry.FullName.EndsWith("\\")) continue;
            string fileName = Path.GetFileName(entry.Name);
            if (string.IsNullOrEmpty(fileName))
                continue;
            string ext = Path.GetExtension(fileName).ToLower();
            if (ext != ".dll" && ext != ".so" && ext != ".dylib" && ext != ".jnilib")
                continue; // 跳过非动态库文件
            string destPath = Path.Combine(outputDir, fileName);
            entry.ExtractToFile(destPath, overwrite: true);
            //Console.WriteLine($"解压{jarPath}内的{fileName}，输出至{destPath}");
        }
        //Console.WriteLine($"解压本地库{jarPath}完成");
    }

    private static string[] ReplaceMacrosInArray(string[] rawArgs, IReadOnlyDictionary<string, string> macros)
    {
        if (rawArgs == null) throw new ArgumentNullException(nameof(rawArgs));

        var result = new string[rawArgs.Length];
        for (int i = 0; i < rawArgs.Length; i++)
        {
            result[i] = ReplaceMacros(rawArgs[i], macros); // 对每个字符串调用 Regex 替换
        }
        return result;
    }
    private static readonly Regex MacroRegex = new Regex(@"\$\{([^}]+)\}", RegexOptions.Compiled);
    private static string ReplaceMacros(string input, IReadOnlyDictionary<string, string> macros)
    {
        return MacroRegex.Replace(input, match =>
        {
            // match.Value 是完整的 ${key}
            // match.Groups[1].Value 是内部的 key
            string key = match.Groups[1].Value;
            // 如果字典里有，替换；否则保留原样
            return macros.TryGetValue(key, out string replacement) ? replacement : match.Value;
        });
    }

    private static MinecraftJsonInfomation LoadMinecraftVersion(FileInfo path)
    {
        using FileStream file = path.OpenRead();
        return Extensions.DeserializeJson<MinecraftJsonInfomation>(file) ?? throw new JsonException("加载Json失败");
    }
}
