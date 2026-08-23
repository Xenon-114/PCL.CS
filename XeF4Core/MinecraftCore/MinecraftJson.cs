using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace XeF4Core.MinecraftCore;

/// <summary>
/// 表示 Minecraft 版本 JSON 文件的根对象。
/// 包含启动游戏所需的所有元数据、依赖、参数和下载信息。
/// </summary>
public class MinecraftJsonInfomation
{
    /// <summary>
    /// 获取或设置启动参数（包括 JVM 参数和游戏参数）。
    /// 可能为 null（旧版本可能使用顶层的 <see cref="MinecraftJsonInfomation.MainClass"/> 和独立参数）。
    /// </summary>
    [JsonProperty("arguments")]
    public Arguments? Arguments { get; set; }

    /// <summary>
    /// 获取或设置资源索引信息，用于下载和校验游戏资源文件（纹理、音效等）。
    /// </summary>
    [JsonProperty("assetIndex")]
    public AssetIndex? AssetIndex { get; set; }

    /// <summary>
    /// 获取或设置资源版本标识符（字符串形式，如 "1.20" 或 "29"）。
    /// 用于确定使用哪个资源索引。
    /// </summary>
    [JsonProperty("assets")]
    public string? Assets { get; set; }

    /// <summary>
    /// 获取或设置合规性级别，用于指示游戏所需的 Java 版本最低要求。
    /// </summary>
    [JsonProperty("complianceLevel")]
    public int ComplianceLevel { get; set; }

    /// <summary>
    /// 获取或设置游戏核心文件（client.jar）和服务端文件的下载信息。
    /// </summary>
    [JsonProperty("downloads")]
    public DownloadsRoot? Downloads { get; set; }

    /// <summary>
    /// 获取或设置版本 ID，如 "1.20.4"。
    /// </summary>
    [JsonProperty("id")]
    public string? Id { get; set; }

    /// <summary>
    /// 获取或设置推荐的 Java 运行时版本信息。
    /// </summary>
    [JsonProperty("javaVersion")]
    public JavaVersion? JavaVersion { get; set; }

    /// <summary>
    /// 获取或设置所有依赖库的列表（包括第三方库和 natives）。
    /// </summary>
    [JsonProperty("libraries")]
    public List<Library> Libraries { get; set; } = [];

    /// <summary>
    /// 获取或设置日志配置信息（如 log4j2 配置文件）。
    /// </summary>
    [JsonProperty("logging")]
    public Logging? Logging { get; set; }

    /// <summary>
    /// 获取或设置游戏主入口类的完全限定名，如 "net.minecraft.client.main.Main"。
    /// </summary>
    [JsonProperty("mainClass")]
    public string? MainClass { get; set; }

    /// <summary>
    /// 获取或设置最低要求的启动器版本（整数）。
    /// 如果启动器版本低于此值，可能无法正确启动。
    /// </summary>
    [JsonProperty("minimumLauncherVersion")]
    public int MinimumLauncherVersion { get; set; }

    /// <summary>
    /// 获取或设置版本的发布时间（ISO 8601 格式字符串）。
    /// </summary>
    [JsonProperty("releaseTime")]
    public string? ReleaseTime { get; set; }

    /// <summary>
    /// 获取或设置版本的更新时间（ISO 8601 格式字符串）。
    /// </summary>
    [JsonProperty("time")]
    public string? Time { get; set; }

    /// <summary>
    /// 获取或设置版本类型，如 "release"（正式版）、"snapshot"（快照）或 "old_alpha"。
    /// </summary>
    [JsonProperty("type")]
    public string? Type { get; set; }

    /// <summary>
    /// 获取或设置客户端版本号（通常与 <see cref="Id"/> 相同）。
    /// </summary>
    [JsonProperty("clientVersion")]
    public string? ClientVersion { get; set; }

    /// <summary>
    /// Patches
    /// </summary>
    [JsonProperty("patches")]
    public List<Patch> Patches { get; set; } = [];

}

/// <summary>
/// 表示启动参数集合，包含 JVM 参数和游戏参数。
/// 参数可以是纯字符串或带规则（rules）的条件对象。
/// </summary>
public class Arguments
{
    /// <summary>
    /// 获取或设置游戏参数列表（传递给主类的参数）。
    /// 每个元素可以是 string 或包含 rules 和 value 的 JObject。
    /// </summary>
    [JsonProperty("game")]
    public List<JToken> GameArgs { get; set; } = [];

    /// <summary>
    /// 获取或设置 JVM 参数列表（传递给 Java 虚拟机的参数）。
    /// 每个元素可以是 string 或包含 rules 和 value 的 JObject。
    /// </summary>
    [JsonProperty("jvm")]
    public List<JToken> JvmArgs { get; set; } = [];
}

/// <summary>
/// 表示资源索引信息，用于下载和校验游戏资源文件。
/// </summary>
public class AssetIndex
{
    /// <summary>
    /// 获取或设置资源索引的标识符（通常与版本 ID 相同）。
    /// </summary>
    [JsonProperty("id")]
    public string? Id { get; set; }

    /// <summary>
    /// 获取或设置资源索引文件的 SHA-1 哈希值。
    /// </summary>
    [JsonProperty("sha1")]
    public string? Sha1 { get; set; }

    /// <summary>
    /// 获取或设置资源索引文件的大小（字节）。
    /// </summary>
    [JsonProperty("size")]
    public long Size { get; set; }

    /// <summary>
    /// 获取或设置所有资源文件的总大小（字节），用于进度显示。
    /// </summary>
    [JsonProperty("totalSize")]
    public long TotalSize { get; set; }

    /// <summary>
    /// 获取或设置资源索引文件的下载 URL。
    /// </summary>
    [JsonProperty("url")]
    public string? Url { get; set; }
}

/// <summary>
/// 表示游戏核心文件（client/server）的下载信息根对象。
/// </summary>
public class DownloadsRoot
{
    /// <summary>
    /// 获取或设置客户端 JAR 文件的下载信息。
    /// </summary>
    [JsonProperty("client")]
    public Artifact? Client { get; set; }

    /// <summary>
    /// 获取或设置客户端映射文件（混淆映射）的下载信息。
    /// </summary>
    [JsonProperty("client_mappings")]
    public Artifact? ClientMappings { get; set; }

    /// <summary>
    /// 获取或设置服务端 JAR 文件的下载信息。
    /// </summary>
    [JsonProperty("server")]
    public Artifact? Server { get; set; }

    /// <summary>
    /// 获取或设置服务端映射文件的下载信息。
    /// </summary>
    [JsonProperty("server_mappings")]
    public Artifact? ServerMappings { get; set; }
}

/// <summary>
/// 表示 Java 运行时版本要求。
/// </summary>
public class JavaVersion
{
    /// <summary>
    /// 获取或设置 Java 组件的名称，如 "java-runtime-beta"。
    /// </summary>
    [JsonProperty("component")]
    public string? Component { get; set; }

    /// <summary>
    /// 获取或设置主版本号，如 17 表示 Java 17。
    /// </summary>
    [JsonProperty("majorVersion")]
    public int MajorVersion { get; set; }
}

/// <summary>
/// 表示一个依赖库（library），包含其下载信息、适用规则和 natives 提取信息。
/// </summary>
public class Library
{
    /// <summary>
    /// 获取或设置该库的下载信息（包括 artifact 和 classifiers）。
    /// </summary>
    [JsonProperty("downloads")]
    public LibraryDownloads? Downloads { get; set; }

    /// <summary>
    /// 获取或设置库的 Maven 坐标，如 "org.lwjgl:lwjgl:3.3.1"。
    /// </summary>
    [JsonProperty("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 获取或设置该库适用的系统规则列表（用于平台过滤）。
    /// </summary>
    [JsonProperty("rules")]
    public List<Rule> Rules { get; set; } = [];
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool ShouldSerializeRules() => Rules.Count > 0;

    /// <summary>
    /// 获取或设置 natives 的映射表（键为操作系统名，值为对应的 classifier）。
    /// </summary>
    [JsonProperty("natives")]
    public Dictionary<string, string> Natives { get; set; } = [];

    /// <summary>
    /// 获取或设置解压配置（如需要排除的文件）。
    /// </summary>
    [JsonProperty("extract")]
    public Extract? Extract { get; set; }
}

/// <summary>
/// 表示库的下载信息，包含主 artifact 和可选的分类器（classifiers）。
/// </summary>
public class LibraryDownloads
{
    /// <summary>
    /// 获取或设置主 artifact（即核心 JAR 文件）。
    /// </summary>
    [JsonProperty("artifact")]
    public Artifact? Artifact { get; set; }

    /// <summary>
    /// 获取或设置分类器下载信息（如 natives 特定平台的 JAR）。
    /// </summary>
    [JsonProperty("classifiers")]
    public Dictionary<string, Artifact> Classifiers { get; set; } = [];
}

/// <summary>
/// 表示一个可下载的文件（JAR 或其他资源），包含路径、哈希、大小和 URL。
/// </summary>
public class Artifact
{
    /// <summary>
    /// 获取或设置文件在本地 .minecraft 中的相对路径（如 "libraries/..."）。
    /// </summary>
    [JsonProperty("path")]
    public string? Path { get; set; }

    /// <summary>
    /// 获取或设置文件的 SHA-1 哈希值（用于校验）。
    /// </summary>
    [JsonProperty("sha1")]
    public string? Sha1 { get; set; }

    /// <summary>
    /// 获取或设置文件的大小（字节）。
    /// </summary>
    [JsonProperty("size")]
    public long Size { get; set; }

    /// <summary>
    /// 获取或设置文件的下载 URL。
    /// </summary>
    [JsonProperty("url")]
    public string? Url { get; set; }
}

/// <summary>
/// 表示一个应用规则（rule），用于条件性地包含或排除某个参数或库。
/// </summary>
public class Rule
{
    /// <summary>
    /// 获取或设置操作类型："allow" 表示允许（满足条件时包含），"disallow" 表示禁止。
    /// </summary>
    [JsonProperty("action")]
    public string? Action { get; set; }

    /// <summary>
    /// 获取或设置操作系统条件（如名称和架构）。
    /// </summary>
    [JsonProperty("os")]
    public Os? Os { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool ShouldSerializeOs() => Os is not null;

    /// <summary>
    /// 获取或设置特性条件（如 "is_demo_user"）。
    /// </summary>
    [JsonProperty("features")]
    public Dictionary<string, bool> Features { get; set; } = [];
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool ShouldSerializeFeatures() => Features.Count > 0;
}

/// <summary>
/// 表示操作系统信息，用于规则匹配。
/// </summary>
public class Os
{
    /// <summary>
    /// 获取或设置操作系统名称："windows"、"osx" 或 "linux"。
    /// </summary>
    [JsonProperty("name")]
    public OSPlatform? Name { get; set; } = null;

    /// <summary>
    /// 获取或设置 CPU 架构："x86"、"x86_64" 或 "arm64" 等。
    /// </summary>
    [JsonProperty("arch")]
    public string? Arch { get; set; }
}

/// <summary>
/// 表示解压配置，用于指定 natives 提取时需要排除的文件模式。
/// </summary>
public class Extract
{
    /// <summary>
    /// 获取或设置需要排除的文件名或通配符列表（如 "META-INF/"）。
    /// </summary>
    [JsonProperty("exclude")]
    public List<string> Exclude { get; set; } = [];
}

/// <summary>
/// 表示日志配置的根对象。
/// </summary>
public class Logging
{
    /// <summary>
    /// 获取或设置客户端的日志配置（如 log4j2）。
    /// </summary>
    [JsonProperty("client")]
    public LoggingClient? Client { get; set; }
}

/// <summary>
/// 表示客户端日志配置的详细信息。
/// </summary>
public class LoggingClient
{
    /// <summary>
    /// 获取或设置日志参数，如 "-Dlog4j.configurationFile=${path}"。
    /// 其中 ${path} 会被替换为实际配置文件路径。
    /// </summary>
    [JsonProperty("argument")]
    public string? Argument { get; set; }

    /// <summary>
    /// 获取或设置日志配置文件（如 log4j2.xml）的下载信息。
    /// </summary>
    [JsonProperty("file")]
    public Artifact? File { get; set; }

    /// <summary>
    /// 获取或设置日志类型，如 "log4j2-xml"。
    /// </summary>
    [JsonProperty("type")]
    public string? Type { get; set; }
}
/// <summary>
/// MC资源索引文件
/// </summary>
public class MinecraftAssetsIndex
{
    /// <summary>
    /// 项目
    /// </summary>
    [JsonProperty("objects")]
    public Dictionary<string, AssetsObjects> Objects { get; set; } = new();
}
/// <summary>
/// 资源对象
/// </summary>
public class AssetsObjects
{
    /// <summary>
    /// 哈希值
    /// </summary>
    [JsonProperty("hash")]
    public string Hash { get; set; } = "";
    /// <summary>
    /// 大小
    /// </summary>
    [JsonProperty("size")]
    public int Size { get; set; }
}

/// <summary>
/// 补丁，如Forge
/// </summary>
public class Patch
{
    /// <summary>
    /// Id
    /// </summary>
    [JsonProperty("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Version
    /// </summary>
    [JsonProperty("version")]
    public string? Version { get; set; }

    /// <summary>
    /// Arguments
    /// </summary>
    [JsonProperty("arguments")]
    public Arguments? Arguments { get; set; }

    /// <summary>
    /// MainClass
    /// </summary>
    [JsonProperty("mainClass")]
    public string? MainClass { get; set; }

    /// <summary>
    /// Libraries
    /// </summary>
    [JsonProperty("libraries")]
    public List<Library> Libraries { get; set; } = [];

    /// <summary>
    /// Type
    /// </summary>
    [JsonProperty("type")]
    public string? Type { get; set; }
}

/// <summary>
/// 版本列表数据
/// </summary>
public class MinecraftDataList
{

    /// <summary>
    /// 从指定的链接拉取Minecraft元数据
    /// </summary>
    /// <param name="Url"></param>
    /// <returns></returns>
    public static async Task<MinecraftDataList?> GetDataList(string Url)
    {
        string local = await HttpServer.DownloadFile(Url);
        string Content = File.ReadAllText(local);
        var result = JsonConvert.DeserializeObject<MinecraftDataList>(Content);
        return result;
    }
    /// <summary>
    /// 从Mojang官方源拉取Minecraft元数据
    /// </summary>
    /// <returns></returns>
    public static Task<MinecraftDataList?> GetDataList()
    {
        return GetDataList("https://piston-meta.mojang.com/mc/game/version_manifest.json");
    }
    /// <summary>
    /// 最新版本
    /// </summary>
    [JsonProperty("latest")]
    public Dictionary<string, string> Latest { get; } = new();

    /// <summary>
    /// 版本列表
    /// </summary>
    [JsonProperty("versions")]
    public List<MinecraftVersion> Versions { get; } = new();
}

/// <summary>
/// Minecraft版本数据，用于下载
/// </summary>
public class MinecraftVersion
{
    /// <summary>
    /// Id
    /// </summary>
    [JsonProperty("id")]
    public string? Id { get; set; }
    /// <summary>
    /// Type
    /// </summary>
    [JsonProperty("type")]
    public string? Type { get; set; }
    /// <summary>
    /// Url
    /// </summary>
    [JsonProperty("url")]
    public string? Url { get; set; }
    /// <summary>
    /// 获取或设置版本的发布时间（ISO 8601 格式字符串）。
    /// </summary>
    [JsonProperty("releaseTime")]
    public string? ReleaseTime { get; set; }

    /// <summary>
    /// 获取或设置版本的更新时间（ISO 8601 格式字符串）。
    /// </summary>
    [JsonProperty("time")]
    public string? Time { get; set; }
}

