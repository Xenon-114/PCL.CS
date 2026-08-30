using PCL.CS.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PCL.CS.Controls
{
    [TypeConverter(typeof(MyImageSourceConverter))]
    public class MyImageSource
    {
        public ImageSource GetImageSource()
        {
            LoadingTask.Wait();
            return Bitmap;
        }
        public async Task<ImageSource> GetImageSourceAsync()
        {
            await LoadingTask;
            return Bitmap;
        }
        
        private BitmapImage Bitmap { get; set; }
        private Task LoadingTask { get; set; }
        private static bool IsPathRooted(string path)
        {
            if (string.IsNullOrEmpty(path) || path.Length < 2)
                return false;

            char first = path[0];
            char second = path[1];

            // 首字母是大写或小写字母，第二个字符是冒号
            return (first >= 'A' && first <= 'Z' || first >= 'a' && first <= 'z')
                && second == ':';
        }
        private void LoadBitmap()
        {
            if (SourcePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                SourcePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                LoadingTask = LoadBitmapFromInternet();
                return;
            }
            LoadingTask = Task.CompletedTask;
            if (IsPathRooted(SourcePath))
            {
                LoadBitmapFromFilePath(SourcePath);
                return;
            }
            if (SourcePath.StartsWith("./"))
            {
                var FullPath = Path.Combine(Base.Path, SourcePath.Substring(1));
                LoadBitmapFromFilePath(FullPath);
                return;
            }
            string ResourcePath = SourcePath.TrimStart('/');
            ResourcePath = $"PCL.CS.{ResourcePath.Replace('/', '.')}";
            Assembly assembly = Assembly.GetExecutingAssembly();
            using var ResourceStream = assembly.GetManifestResourceStream(ResourcePath);
            if (ResourceStream is null)
            {
                Main.Hint($"资源{ResourcePath}不可用！");
                return;
            }
            LoadBitmapFromStream(ResourceStream);
            return;
        }
        private async Task LoadBitmapFromInternet()
        {
            string LocalPath = await Net.Download(SourcePath, true);
            LoadBitmapFromFilePath(LocalPath);
        }
        private void LoadBitmapFromFilePath(string Path)
        {
            using var Stream = File.OpenRead(Path);
            LoadBitmapFromStream(Stream);
            return;
        }
        private void LoadBitmapFromStream(Stream Stream)
        {
            if (Stream is null) throw new ArgumentNullException(nameof(Stream));
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = Stream;
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();
            Bitmap = bmp;
        }
        public MyImageSource(string Path)
        {
            if(Path is null)throw new ArgumentNullException(nameof(Path));
            SourcePath = Path;
        }
        public MyImageSource() { }
        private void SetSourcePath(string Path)
        {
            Path = Path.Trim();
            Path = Path.Trim('"', '\'', '“', '”', '‘', '’');
            Path = Path.Replace('\\', '/');
            LoadBitmap();
        }
        private bool HasSetSourcePath = false;
        private string _sourcePath = null;
        public string SourcePath
        {
            get => _sourcePath;
            set
            {
                if (HasSetSourcePath) 
                    throw new InvalidOperationException("无法再次设置对象的SourcePath！");
                _sourcePath = value;
                SetSourcePath(value);
                HasSetSourcePath = true;
            }
        }
        public static implicit operator MyImageSource(string Path) => new MyImageSource(Path);
    }
    public class MyImageSourceConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if(value is string str)
            {
                return new MyImageSource(str);
            }
            return base.ConvertFrom(context, culture, value);
        }

    }
}
