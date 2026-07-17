using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using XeF4Core;

namespace PCL.CS.Modules
{
    public static class Net
    {
        public static readonly Downloader NetDownloader = new Downloader();

        public static DownloadTask Download(string url,string localPath)
        {
            return NetDownloader.Download(url, localPath);
        }
    }
}
