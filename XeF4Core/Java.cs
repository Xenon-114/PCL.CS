using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XeF4Core;

/// <summary>
/// Java供应商
/// </summary>
public enum JavaVendor
{
    /// <summary>
    /// Oracle
    /// </summary>
    Oracle,
    /// <summary>
    /// Eclipse Adoptium
    /// </summary>
    EclipseAdoptium,
    /// <summary>
    /// Azul Systems
    /// </summary>
    AzulSystems,
    /// <summary>
    /// Amazon
    /// </summary>
    Amazon,
    /// <summary>
    /// Microsoft
    /// </summary>
    Microsoft,
    /// <summary>
    /// 毕昇
    /// </summary>
    Huawei,
    /// <summary>
    /// 其他供应商
    /// </summary>
    Other
}

/// <summary>
/// 表示一个Java
/// </summary>
public class Java
{
    /// <summary>
    /// 可执行文件路径
    /// </summary>
    public string ExecutablePath { get; }
    /// <summary>
    /// Java版本
    /// </summary>
    public VersionCode Version { get; }
    /// <summary>
    /// Java架构
    /// </summary>
    public Architecture Architecture { get; }
    /// <summary>
    /// 程序供应商
    /// </summary>
    public JavaVendor Vendor { get; }
    /// <summary>
    /// 使用Java实例异步执行一个命令
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public async Task<ProcessResult> RunCommandAsync(string args)
    {
        return await RunCommandAsync(ExecutablePath, args);
    }
    /// <summary>
    /// 传递主类来启动一个程序
    /// </summary>
    /// <param name="JvmArgs">Jvm参数</param>
    /// <param name="ClassPaths">主类路径列表</param>
    /// <param name="MainClass">主类</param>
    /// <param name="Args">主类参数列表</param>
    /// <returns></returns>
    public Process RunProcessWithMainClass(string[]? JvmArgs, string[]? ClassPaths, string MainClass, string[]? Args)
    {
        //预先判断
        if (MainClass is null) throw new ArgumentNullException(nameof(MainClass));
        if (JvmArgs.IsNullOrEmpty()) JvmArgs = null;
        if (Args.IsNullOrEmpty()) Args = null;

        var sb = new StringBuilder();
        //拼接JVM参数
        if (JvmArgs is not null)
            sb.Append(string.Join(" ", JvmArgs)).Append(' ');

        if (ClassPaths.IsNullOrEmpty())
        {
            if (!JvmArgs.Contains("-cp")) throw new ArgumentNullException(nameof(ClassPaths));
        }
        else
        {
            if (JvmArgs.Contains("-cp")) throw new ArgumentException($"JVM参数中已包含ClassPaths，请不要重复添加。{ClassPaths.IsNullOrEmpty()}");
            sb.Append($"-cp \"{string.Join(";", ClassPaths)}\" ");
        }
        
        sb.Append(MainClass);
        if (Args is not null)
            sb.Append(' ').Append(string.Join(" ", Args));
        string Command = sb.ToString();

        return StartProcess(Command,true);
    }

    /// <summary>
    /// 传递Jar路径来启动一个程序
    /// </summary>
    /// <param name="JvmArgs">Jvm参数列表</param>
    /// <param name="JarPath">Jar文件路径</param>
    /// <param name="args">主类参数</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public Process RunProcessWithJar(string[]? JvmArgs,string JarPath, string[]? args)
    {
        //预先判断
        if (JarPath is null) throw new ArgumentNullException(nameof(JarPath));
        if (JvmArgs is not null && JvmArgs.Length == 0) JvmArgs = null;
        if (args is not null && args.Length == 0) args = null;

        var builder = new StringBuilder();

        //处理Jvm参数
        if (JvmArgs is not null) builder.Append(string.Join(" ", JvmArgs)).Append(' ');
        builder.Append("-jar ").Append(JarPath);
        if (args is not null) builder.Append(' ').Append(string.Join(" ", args));

        string Command = builder.ToString();
        return StartProcess(Command, true);
    }
    /// <summary>
    /// 使用指定Java异步执行一个命令
    /// </summary>
    /// <param name="file"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public static async Task<ProcessResult>RunCommandAsync(string file,string args)
    {
        if (!File.Exists(file)) throw new FileNotFoundException("无法找到Java主程序");
        var startInfo = new ProcessStartInfo(file, args)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            RedirectStandardError = true
        };
        var process = Process.Start(startInfo);

        var result = new ProcessResult();

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (s, e) =>
        {
            if (e.Data != null)
                outputBuilder.AppendLine(e.Data);
        };
        process.ErrorDataReceived += (s, e) =>
        {
            if (e.Data != null)
                errorBuilder.AppendLine(e.Data);
        };

        try
        {
            // 开始异步读取
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // 等待进程退出（异步等待）
            await Task.Run(() => process.WaitForExit());

            result.ExitCode = process.ExitCode;
            result.StandardOutput = outputBuilder.ToString();
            result.StandardError = errorBuilder.ToString();
        }
        catch (Exception ex)
        {
            result.StandardError = ex.Message;
            result.ExitCode = -1;
        }

        return result;
    }
    /// <summary>
    /// 启动长期进程
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="redirectOutput"></param>
    /// <returns></returns>
    public Process StartProcess(string arguments, bool redirectOutput = false)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = ExecutablePath,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = redirectOutput,
            RedirectStandardError = redirectOutput
        };
        var process = new Process { StartInfo = startInfo };
        process.Start();
        return process;
    }

    private Java(string executablePath,VersionCode version,Architecture architecture,JavaVendor vendor)
    {
        ExecutablePath = executablePath;
        Version = version;
        Architecture = architecture;
        Vendor = vendor;
    }
    private static readonly Dictionary<string, Java> Javas = new();

    /// <summary>
    /// 从某一个可执行文件创建Java
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    public static async Task<Java> FromFile(string file)
    {
        if (string.IsNullOrEmpty(file)) throw new ArgumentNullException(nameof(file));
        file = file.Trim();
        if (file.StartsWith("\"") && file.EndsWith("\"")){
            file = file.Substring(1, file.Length - 2);
        }
        if (!File.Exists(file))
        {
            if (Javas.ContainsKey(file)) Javas.Remove(file);
            throw new FileNotFoundException($"文件不存在：{file}");
        }
        if (Javas.TryGetValue(file, out var java)) return java;

        var result = await RunCommandAsync(file, "-version");

        var strs = result.StandardError.Split('\n');

        string JavaVersion = strs[0];
        string JavaRuntimeEnviroment = strs[1];
        string JavaVM = strs[2];

        string VersionCde;

        var match = Regex.Match(JavaVersion, @"version\s+""([^""]+)""");

        if (match.Success) VersionCde = match.Groups[1].Value;
        else VersionCde = "0.0.0.0";

        VersionCde = VersionCde.Trim().Replace("_", ".");

        VersionCode versionCode = VersionCode.FromString(VersionCde);

        Architecture architecture = GetArchitecture(JavaVM);

        JavaVendor vendor = GetVendor(JavaRuntimeEnviroment);

        return new Java(file, versionCode, architecture, vendor);
    }
    private static JavaVendor GetVendor(string JavaRE)
    {
        if (JavaRE is null) throw new ArgumentNullException(nameof(JavaRE));

        if (JavaRE.StartsWith("Java(TM)")) return JavaVendor.Oracle;

        string lower = JavaRE.ToLowerInvariant();

        if (lower.Contains("temurin") || lower.Contains("adoptopenjdk"))
            return JavaVendor.EclipseAdoptium;
        if (lower.Contains("zulu") || lower.Contains("azul systems"))
            return JavaVendor.AzulSystems;
        if (lower.Contains("corretto") || lower.Contains("amazon"))
            return JavaVendor.Amazon;
        if (lower.Contains("microsoft"))
            return JavaVendor.Microsoft;
        if (lower.Contains("bisheng") || lower.Contains("huawei"))
            return JavaVendor.Huawei;

        return JavaVendor.Other;
    }
    private static Architecture GetArchitecture(string javaVersionOutput)
    {
        if (string.IsNullOrEmpty(javaVersionOutput))
            return Architecture.Unknown;

        // 统一转小写，避免大小写问题
        string lower = javaVersionOutput.ToLowerInvariant();

        // 优先检测架构名（更精确）
        if (lower.Contains("aarch64") || lower.Contains("arm64"))
            return Architecture.Arm64;
        if (lower.Contains("arm") && !lower.Contains("64")) // 排除 arm64
            return Architecture.Arm32;

        // 再检测位数
        if (lower.Contains("64-bit") || lower.Contains("64 bit"))
            return Architecture.x64;
        if (lower.Contains("32-bit") || lower.Contains("32 bit"))
            return Architecture.x86;

        // 最后用架构名兜底
        if (lower.Contains("amd64") || lower.Contains("x86_64") || lower.Contains("x64"))
            return Architecture.x64;
        if (lower.Contains("x86") || lower.Contains("i386") || lower.Contains("i586") || lower.Contains("i686"))
            return Architecture.x86;

        return Architecture.Unknown;
    }
}
/// <summary>
/// 进程结果
/// </summary>
public class ProcessResult
{
    /// <summary>
    /// 退出码
    /// </summary>
    public int ExitCode { get; set; }
    /// <summary>
    /// 输出流
    /// </summary>
    public string StandardOutput { get; set; } = string.Empty;
    /// <summary>
    /// 错误流
    /// </summary>
    public string StandardError { get; set; } = string.Empty;
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success => ExitCode == 0;
}
