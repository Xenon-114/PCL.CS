using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace XeF4Core.MinecraftCore;

/// <summary>
/// 一组Minecraft，对应一个Minecraft文件夹
/// </summary>
public class MinecraftGroup : IDisposable,IEnumerable<Minecraft>,IEnumerable
{
    #region 迭代器
    IEnumerator<Minecraft> IEnumerable<Minecraft>.GetEnumerator() => minecrafts.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => minecrafts.GetEnumerator();
    #endregion
    private bool _disposed = false;
    private readonly List<Minecraft> minecrafts = [];
    /// <summary>
    /// 游戏
    /// </summary>
    public IReadOnlyList<Minecraft> Minecrafts { get => minecrafts; }
    /// <summary>
    /// 下载器
    /// </summary>
    public Downloader? MinecraftDownloader { get; set; }

    internal Downloader Downloader
    {
        get => MinecraftDownloader ?? Minecraft.DefaultDownloader ?? throw new NullReferenceException();
        set => MinecraftDownloader = value;
    }
    /// <summary>
    /// 文件夹名称
    /// </summary>
    public string Name { get; set; } = "游戏文件夹";

    /// <summary>
    /// 游戏文件夹的目录
    /// </summary>
    public DirectoryInfo MinecraftDirectory { get; }

    /// <summary>
    /// 确认当前实例是否可用
    /// </summary>
    public bool IsAvailable => MinecraftDirectory.Exists && !_disposed && MinecraftMaps.ContainsValue(this);
    private void ThrowIfNotAvailable()
    {
        if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        if (!IsAvailable) throw new InvalidOperationException("实例不可用！");
    }

    /// <summary>
    /// 刷新Minecraft列表
    /// </summary>
    public void Refresh()
    {
        ThrowIfNotAvailable();
        minecrafts.Clear();
        var DirVersion = new DirectoryInfo(Path.Combine(MinecraftDirectory.FullName, "versions"));
        foreach (var dir in DirVersion.EnumerateDirectories())
        {
            string JsonName = dir.Name;
            string JsonPath = Path.Combine(dir.FullName, JsonName + ".json");
            try
            {
                Minecraft Mc = Minecraft.FromJsonPath(JsonPath);
                Mc.PublicPath = MinecraftDirectory;
                minecrafts.Add(Mc);
            }
            catch { }
        }
    }
    /// <summary>
    /// 删除实例
    /// </summary>
    public void Delete()
    {
        ThrowIfNotAvailable();
        MinecraftDirectory.Delete(true);
        Dispose();
    }
    private MinecraftGroup(DirectoryInfo directory,string fullPath)
    {
        MinecraftMaps[fullPath] = this;
        MinecraftDirectory = directory;
        var DirVersion = new DirectoryInfo(Path.Combine(directory.FullName, "versions"));
        if (DirVersion.Exists)
        {
            foreach (var dir in DirVersion.EnumerateDirectories())
            {
                Core.Log($"扫描{dir.FullName}");
                string JsonName = dir.Name;
                string JsonPath = Path.Combine(dir.FullName, JsonName + ".json");
                try
                {
                    Core.Log($"尝试从{JsonPath}创建......");
                    Minecraft Mc = Minecraft.FromJsonPath(JsonPath);
                    Mc.PublicPath = directory;
                    minecrafts.Add(Mc);
                }
                catch(Exception ex)
                {
                    Core.Log($"出现错误：\n{ex.ToString()}");
                }
            }
        }
    }
    private static readonly Dictionary<string, MinecraftGroup> MinecraftMaps = [];
    /// <summary>
    /// 从.minecraft文件夹创建
    /// </summary>
    /// <param name="Path"></param>
    /// <returns></returns>
    public static MinecraftGroup FromMinecraftPath(string Path)
    {
        Core.Log($"尝试从{Path}创建...");
        Path = Path.Trim(' ', '\"');
        Path = FileExtensions.GetCanonicalPath(Path);
        if (MinecraftMaps.TryGetValue(Path, out var minecraftGroup))
        {
            if (!minecraftGroup.IsAvailable)
            {
                minecraftGroup.Dispose();
                throw new DirectoryNotFoundException($"没有找到位于{Path}处的.minecraft文件夹");
            }
            else return minecraftGroup;
        }
        DirectoryInfo directory = new(Path);
        if (!directory.Exists) throw new DirectoryNotFoundException($"没有找到位于{Path}处的.minecraft文件夹");
        return new MinecraftGroup(directory,Path);
    }
    /// <summary>
    /// 删除该文件夹
    /// </summary>
    public void Dispose()
    {
        MinecraftMaps.Remove(MinecraftDirectory.FullName);
        _disposed = true;
    }
    /// <summary>
    /// 将一个版本下载至文件夹中
    /// </summary>
    /// <param name="version"></param>
    /// <param name="Name"></param>
    /// <param name="downloader"></param>
    /// <returns></returns>
    public IMyTask Download(MinecraftVersion version, string Name, Downloader downloader)
    {
        ThrowIfNotAvailable();
        DirectoryInfo directory = new(Path.Combine(MinecraftDirectory.FullName, Name));
        Minecraft.MinecraftDownloadTask result = Minecraft.Download(version, Name, downloader, directory, MinecraftDirectory);
        result.Mc.Task.GetAwaiter().OnCompleted(() => this.minecrafts.Add(result.Minecraft));
        return result;
    }
}
