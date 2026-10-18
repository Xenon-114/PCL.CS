using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace XeF4Core.HttpService;

internal class DownloadFileManager
{
    public FileInfo File { get; }
    public DownloadTaskSettings Settings { get; }
    private string _downloadFilePath;
    private string _filePath;
    private FileStream? Stream { get; }
    private DownloadPackage Package { get; }
    public DownloadFileManager(string path,DownloadTaskSettings settings)
    {
        var dirurl = Path.GetDirectoryName(path);
        var fileName = Path.GetFileName(path);
        _filePath = path;
        _downloadFilePath = Path.Combine(dirurl, fileName + "." + settings.Extension);
        File = new FileInfo(_downloadFilePath);
        Package = new();
        Settings = settings;
    }
    public DownloadFileManager(FileInfo file,DownloadTaskSettings settings)
    {
        File = file;
        _downloadFilePath = file.FullName;
        var Dir = Path.GetDirectoryName(_downloadFilePath);
        var fName = Path.GetFileNameWithoutExtension(_downloadFilePath);
        _filePath = Path.Combine(Dir, fName);
        Stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        Settings = settings;
    }
    private DownloadPackage GetPackage(Stream stream)
    {
        stream.Seek(0, SeekOrigin.End);

    }
}
