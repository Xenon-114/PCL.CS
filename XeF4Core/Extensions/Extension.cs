using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XeF4Core;

/// <summary>
/// 扩展
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 异步形式的File.WriteAllBytes
    /// </summary>
    /// <param name="path">指定路径</param>
    /// <param name="bytes">字节数组</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    public static async Task WriteAllBytesToFileAsync(string path ,byte[] bytes, CancellationToken cancellationToken = default)
    {
        var fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
        using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 81920, fileOptions);
        await fileStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
        await fileStream.FlushAsync(cancellationToken);
    }
    #region SHA256
    /// <summary>
    /// 获得字符串的Sha256哈希
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string GetSha256Hash(this string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);

        byte[] hashBytes = inputBytes.GetSha256Hash();

        return hashBytes.ToHex().ToLower();
    }
    /// <summary>
    /// 获得指定连续字节的Sha256哈希
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static byte[] GetSha256Hash(this byte[] data)
    {
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(data);
    }
    /// <summary>
    /// 获得指定文件的Sha256哈希
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static byte[] GetSha256Hash(this FileInfo file)
    {
        using var sha256 = SHA256.Create();
        using var stream = file.OpenRead();
        byte[] hashBytes = sha256.ComputeHash(stream);
        return hashBytes;
    }
    #endregion

    #region SHA512
    /// <summary>
    /// 获得字符串的Sha512哈希
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string GetSha512Hash(this string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);

        byte[] hashBytes = inputBytes.GetSha512Hash();

        return hashBytes.ToHex().ToLower();
    }
    /// <summary>
    /// 获得指定连续字节的Sha512哈希
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static byte[] GetSha512Hash(this byte[] data)
    {
        using var sha512 = SHA512.Create();
        return sha512.ComputeHash(data);
    }
    /// <summary>
    /// 获得指定文件的Sha512哈希
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static byte[] GetSha512Hash(this FileInfo file)
    {
        using var sha512 = SHA512.Create();
        using var stream = file.OpenRead();
        byte[] hashBytes = sha512.ComputeHash(stream);
        return hashBytes;
    }
    #endregion
    /// <summary>
    /// 获得字符串的MD5哈希值
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string GetMd5Hash(this string input)
    {
        using var md5 = MD5.Create();
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = md5.ComputeHash(inputBytes);
        // 转换为十六进制字符串（32 个字符）
        return hashBytes.ToHex().ToLowerInvariant();
    }
    internal static string ToHex(this byte[] bytes)
    {
        char[] hex = new char[bytes.Length * 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            byte b = bytes[i];
            hex[i * 2] = GetHexValue(b >> 4);
            hex[i * 2 + 1] = GetHexValue(b & 0x0F);
        }
        return new string(hex);
    }

    internal static char GetHexValue(int nibble)
    {
        return (char)(nibble < 10 ? nibble + '0' : nibble - 10 + 'a');
    }
    /// <summary>
    /// 反序列化Json
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static T? DeserializeJson<T>(Stream stream)
    {
        using var sr = new StreamReader(stream);
        using var jsonTextReader = new JsonTextReader(sr);

        var serializer = new JsonSerializer();
        return serializer.Deserialize<T>(jsonTextReader);
    }
    /// <summary>
    /// 判断一个数组是不是空的
    /// </summary>
    /// <typeparam name="T">数组储存的类型</typeparam>
    /// <param name="Array">数组</param>
    /// <returns></returns>
    public static bool IsNullOrEmpty<T>(this T[]? Array) => Array is null || Array.Length == 0;
    /// <summary>
    /// 修改VersionCode
    /// </summary>
    /// <param name="version"></param>
    /// <param name="Major"></param>
    /// <param name="Minor"></param>
    /// <param name="Patch"></param>
    /// <param name="Revision"></param>
    public static void With(ref this VersionCode version, int? Major = null, int? Minor = null, int? Patch = null, int? Revision = null) =>
        version = new VersionCode(Major ?? version.Major, Minor ?? version.Minor, Patch ?? version.Patch, Revision ?? version.Revision);

    private static readonly Dictionary<System.Runtime.InteropServices.OSPlatform, OSPlatform> OsPlatformDic = new()
    {
        [System.Runtime.InteropServices.OSPlatform.Windows] = OSPlatform.Windows,
        [System.Runtime.InteropServices.OSPlatform.Linux] = OSPlatform.Linux,
        [System.Runtime.InteropServices.OSPlatform.OSX] = OSPlatform.OSX
    };
    /// <summary>
    /// 将<see cref="System.Runtime.InteropServices.OSPlatform"/>转换为<see cref="XeF4Core.OSPlatform"/>
    /// </summary>
    /// <param name="platform"></param>
    /// <returns></returns>
    public static OSPlatform ToOsPlatform(this System.Runtime.InteropServices.OSPlatform platform) => OsPlatformDic[platform];
    /// <summary>
    /// 将<see cref="XeF4Core.OSPlatform"/>转换为<see cref="System.Runtime.InteropServices.OSPlatform"/>
    /// </summary>
    /// <param name="platform"></param>
    /// <returns></returns>
    public static System.Runtime.InteropServices.OSPlatform ToOsPlatform(this OSPlatform platform)
    {
        if ((int)platform is 1) return System.Runtime.InteropServices.OSPlatform.Linux;
        if ((int)platform is 2) return System.Runtime.InteropServices.OSPlatform.OSX;
        return System.Runtime.InteropServices.OSPlatform.Windows;
    }
    /// <summary>
    /// 检查MC字符串架构是否匹配
    /// </summary>
    /// <param name="ruleArch"></param>
    /// <returns></returns>
    public static bool IsArchMatch(string? ruleArch)
    {

        // 获取当前系统的处理器架构
        Architecture currentArch = (Architecture)System.Runtime.InteropServices.RuntimeInformation.OSArchitecture;

        // 将规则中的字符串与当前架构进行匹配
        return ruleArch switch
        {
            "x86" => currentArch == Architecture.x86,
            "x86_64" or "amd64" => currentArch == Architecture.x64,
            "arm64" or "aarch64" => currentArch == Architecture.Arm64,
            "arm" or "arm32" => currentArch == Architecture.Arm32,
            _ => true, // 未知的架构字符串，视为匹配
        };
    }

    #region MyTask
    private class BoxedTask(Task task) : ITask
    {
        private Task Task { get; } = task;
        public TaskAwaiter GetAwaiter() => Task.GetAwaiter();
        public void Wait() => Task.Wait();
        public Task WaitAsync() => Task;
    }
    /// <summary>
    /// 将Task包装为ITask
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static ITask ToITask(this Task task) => new BoxedTask(task);
    #endregion
}

/// <summary>
/// 表示一个版本号
/// </summary>
public readonly struct VersionCode: IComparable<VersionCode>, IComparable, IEquatable<VersionCode>
{
    /// <summary>
    /// 主版本号
    /// </summary>
    public int Major { get; }
    /// <summary>
    /// 次版本号
    /// </summary>
    public int Minor { get; }
    /// <summary>
    /// 修订号
    /// </summary>
    public int Patch { get; }
    /// <summary>
    /// 构建号
    /// </summary>
    public int Revision { get; }
    /// <summary>
    /// 返回该实例的字符串表达形式
    /// </summary>
    /// <returns>该实例的字符串表达形式</returns>
    public readonly override string ToString()
    {
        if (Revision == 0) return $"{Major}.{Minor}.{Patch}";
        return $"{Major}.{Minor}.{Patch}.{Revision}";
    }
    /// <summary>
    /// 将一个<see cref="System.Version"/>转换为<see cref="XeF4Core.VersionCode"/>
    /// </summary>
    /// <param name="VersionObject"></param>
    /// <returns></returns>
    public static VersionCode FromVersion(Version VersionObject)
    {
        return new VersionCode(VersionObject.Major, VersionObject.Minor, VersionObject.Build, VersionObject.Revision);
    }
    /// <summary>
    /// 从一个字符串创建版本号
    /// </summary>
    /// <param name="VersionStr">以字符串表达的版本号</param>
    /// <returns>版本号</returns>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public static VersionCode FromString(string VersionStr)
    {
        if (VersionStr is null) throw new ArgumentNullException(nameof(VersionStr));
        string[] parts = VersionStr.Split('.');
        if (parts.Length <= 1 || parts.Length > 4) throw new FormatException($"版本字符串{VersionStr}格式错误");
        VersionCode version;
        try
        {
            int Major = int.Parse(parts[0]);
            int Minor = int.Parse(parts[1]);
            int Patch = 0;
            if (parts.Length >= 3) Patch = int.Parse(parts[2]);
            int Revision = 0;
            if (parts.Length == 4) Revision = int.Parse(parts[3]);
            version = new VersionCode(Major, Minor, Patch, Revision);
        }
        catch (FormatException Ex)
        {
            throw new FormatException($"版本字符串{VersionStr}格式错误", Ex);
        }
        catch (OverflowException Ex)
        {
            throw new FormatException($"版本字符串{VersionStr}的版本数过大", Ex);
        }
        return version;
    }
    /// <summary>
    /// 创建一个新的<see cref="VersionCode"/>实例
    /// </summary>
    /// <param name="Major"></param>
    /// <param name="Minor"></param>
    /// <param name="Patch"></param>
    /// <param name="Revision"></param>
    public VersionCode(int Major, int Minor, int Patch, int Revision)
    {
        this.Major = Major;
        this.Minor = Minor;
        this.Patch = Patch;
        this.Revision = Revision;
    }
    /// <summary>
    /// 创建一个新的<see cref="VersionCode"/>实例
    /// </summary>
    /// <param name="Major"></param>
    /// <param name="Minor"></param>
    /// <param name="Patch"></param>
    public VersionCode(int Major, int Minor, int Patch)
    {
        this.Major = Major;
        this.Minor = Minor;
        this.Patch = Patch;
        this.Revision = 0;
    }
    /// <summary>
    /// 创建一个新的<see cref="VersionCode"/>实例
    /// </summary>
    /// <param name="Major"></param>
    /// <param name="Minor"></param>
    public VersionCode(int Major, int Minor)
    {
        this.Major = Major;
        this.Minor = Minor;
        this.Patch = 0;
        this.Revision = 0;
    }
    /// <summary>
    /// 比较两个<see cref="VersionCode"/>的大小
    /// </summary>
    /// <param name="Left">第一个<see cref="VersionCode"/></param>
    /// <param name="Right">第二个<see cref="VersionCode"/></param>
    /// <returns></returns>
    public static bool operator >(VersionCode Left, VersionCode Right)
    {
        if (Left.Major != Right.Major) return Left.Major > Right.Major;
        if (Left.Minor != Right.Minor) return Left.Minor > Right.Minor;
        if (Left.Patch != Right.Patch) return Left.Patch > Right.Patch;
        return Left.Revision > Right.Revision;
    }
    /// <summary>
    /// 比较两个<see cref="VersionCode"/>的大小
    /// </summary>
    /// <param name="Left">第一个<see cref="VersionCode"/></param>
    /// <param name="Right">第二个<see cref="VersionCode"/></param>
    /// <returns></returns>
    public static bool operator <(VersionCode Left, VersionCode Right)
    {
        if (Left.Major != Right.Major) return Left.Major < Right.Major;
        if (Left.Minor != Right.Minor) return Left.Minor < Right.Minor;
        if (Left.Patch != Right.Patch) return Left.Patch < Right.Patch;
        return Left.Revision < Right.Revision;
    }
    /// <summary>
    /// 比较两个<see cref="VersionCode"/>是否相等
    /// </summary>
    /// <param name="Left">第一个<see cref="VersionCode"/></param>
    /// <param name="Right">第二个<see cref="VersionCode"/></param>
    /// <returns></returns>
    public static bool operator ==(VersionCode Left, VersionCode Right)
    {
        return (Left.Major == Right.Major) && (Left.Minor == Right.Minor) && (Left.Patch == Right.Patch) && (Left.Revision == Right.Revision);
    }
    /// <summary>
    /// 比较两个<see cref="VersionCode"/>是否不相等
    /// </summary>
    /// <param name="Left">第一个<see cref="VersionCode"/></param>
    /// <param name="Right">第二个<see cref="VersionCode"/></param>
    /// <returns></returns>
    public static bool operator !=(VersionCode Left, VersionCode Right)
    {
        return !(Left == Right);
    }
    /// <summary>
    /// 确认两个实例是否相等
    /// </summary>
    /// <returns></returns>
    public bool Equals(VersionCode other) => this == other;
    /// <summary>
    /// 确认两个实例是否相等
    /// </summary>
    /// <returns></returns>
    public override bool Equals(object obj) => obj is VersionCode other && Equals(other);
    /// <summary>
    /// 返回该<see cref="VersionCode"/>的哈希代码
    /// </summary>
    /// <returns>32位有符号整数哈希代码</returns>
    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 31 + Major.GetHashCode();
        hash = hash * 31 + Minor.GetHashCode();
        hash = hash * 31 + Patch.GetHashCode();
        hash = hash * 31 + Revision.GetHashCode();
        return hash;
    }
    /// <summary></summary>
    /// <returns></returns>
    public static bool operator >=(VersionCode Left, VersionCode Right) => !(Left < Right);
    /// <summary></summary>
    /// <returns></returns>
    public static bool operator <=(VersionCode Left, VersionCode Right) => !(Left > Right);

    /// <summary></summary>
    /// <returns></returns>
    public int CompareTo(VersionCode other)
    {
        if (Major != other.Major) return Major.CompareTo(other.Major);
        if (Minor != other.Minor) return Minor.CompareTo(other.Minor);
        if (Patch != other.Patch) return Patch.CompareTo(other.Patch);
        return Revision.CompareTo(other.Revision);
    }
    /// <summary></summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public int CompareTo(object obj)
    {
        if (obj is null) return 1;
        if (obj is VersionCode other) return CompareTo(other);
        throw new ArgumentException($"Object must be of type {nameof(VersionCode)}");
        
    }
}

/// <summary>
/// 值类型包装类
/// </summary>
/// <typeparam name="T">指定的值</typeparam>
public class Reference‌<T> where T : struct
{
    /// <summary>
    /// 值
    /// </summary>
    public T Value;

    /// <summary>
    /// 值类型包装类
    /// </summary>
    /// <param name="Value">初始值</param>
    public Reference(T Value)
    {
        this.Value = Value;
    }
    /// <summary>
    /// 装箱
    /// </summary>
    /// <param name="value">值</param>
    public static implicit operator Reference<T>(T value) => new(value);

    /// <summary>
    /// 拆箱
    /// </summary>
    /// <param name="Reference">对象</param>
    public static implicit operator T(Reference<T> Reference) => Reference.Value;
}

/// <summary>
/// 发布信息
/// </summary>
public enum BuildType
{
    /// <summary>
    /// 排错
    /// </summary>
    Debug,
    /// <summary>
    /// 内部测试
    /// </summary>
    Alpha,
    /// <summary>
    /// 公共测试
    /// </summary>
    Beta,
    /// <summary>
    /// 正式
    /// </summary>
    Release,
    /// <summary>
    /// 快照
    /// </summary>
    Snapshot,
    /// <summary>
    /// 预览
    /// </summary>
    Preview
}
/// <summary>
/// 操作系统平台信息
/// </summary>
public enum OSPlatform
{
    /// <summary>
    /// Windows
    /// </summary>
    [JsonProperty("windows")]
    Windows = 0,
    /// <summary>
    /// Linux
    /// </summary>
    [JsonProperty("linux")]
    Linux = 1,
    /// <summary>
    /// MacOS
    /// </summary>
    [JsonProperty("osx")]
    OSX = 2,
    /// <summary>
    /// MacOS
    /// </summary>
    [JsonProperty("macos")]
    MacOS = 2
}
/// <summary>
/// 标识程序架构
/// </summary>
public enum Architecture
{
    /// <summary>
    /// 未知
    /// </summary>
    Unknown = -1,
    /// <summary>
    /// 32位架构
    /// </summary>
    x86 = System.Runtime.InteropServices.Architecture.X86,
    /// <summary>
    /// 64位架构
    /// </summary>
    x64 = System.Runtime.InteropServices.Architecture.X64,
    /// <summary>
    /// Arm-x86架构
    /// </summary>
    Arm32 = System.Runtime.InteropServices.Architecture.Arm,
    /// <summary>
    /// Arm-x64架构
    /// </summary>
    Arm64 = System.Runtime.InteropServices.Architecture.Arm64
}
