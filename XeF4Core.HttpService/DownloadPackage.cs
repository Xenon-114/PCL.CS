using System;
using System.Collections.Generic;
using System.Text;

namespace XeF4Core.HttpService;

internal class DownloadPackage
{
    public string Url { get; set; } = string.Empty;
    public long TotalBytes { get; set; }
    public List<(long, long)> DownloadedChunks { get; } = [];
}
