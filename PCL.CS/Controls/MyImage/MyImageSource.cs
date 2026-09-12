using PCL.CS.Modules;
using SharpVectors.Renderers.Wpf;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace PCL.CS.Controls
{
    [TypeConverter(typeof(MyImageSourceConverter))]
    public class MyImageSource
    {
        public ImageSource GetImageSource()
        {
            LoadingTask.Wait();
            return Source;
        }
        public async Task<ImageSource> GetImageSourceAsync()
        {
            await LoadingTask;
            return Source;
        }
        
        private ImageSource Source { get; set; }
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
        private void LoadImage()
        {
            if (SourcePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                SourcePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                LoadingTask = LoadImageFromInternet();
                return;
            }
            LoadingTask = Task.Run(GetInner);
            void GetInner()
            {


                if (IsPathRooted(SourcePath))
                {
                    LoadImageFromFilePath(SourcePath);
                    return;
                }
                if (SourcePath.StartsWith("./"))
                {
                    var FullPath = Path.Combine(Base.Path, SourcePath.Substring(1));
                    LoadImageFromFilePath(FullPath);
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
                LoadImageFromStream(ResourceStream);
                return;
            }
            return;
        }
        private async Task LoadImageFromInternet()
        {
            string LocalPath = await Net.Download(SourcePath, true);
            LoadImageFromFilePath(LocalPath);
        }
        private void LoadImageFromFilePath(string Path)
        {
            using var Stream = File.OpenRead(Path);
            LoadImageFromStream(Stream);
            return;
        }
        private void LoadImageFromStream(Stream Stream)
        {
            //MemoryStream MStream;
            //    Stream.CopyTo(MStream);
            if (IsWebPStream(Stream))
                Source = LoadWebP(Stream);
            else if (IsSvgStream(Stream))
                Source = LoadSVG(Stream);
            else
                Source = LoadBitmap(Stream);
        }
        private static ImageSource LoadBitmap(Stream stream)
        {
            if (stream is null) throw new ArgumentNullException(nameof(Stream));
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = stream;
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        private static ImageSource LoadWebP(Stream stream)
        {
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            byte[] webpData = ms.ToArray();
            byte[] pixels = Imazen.WebP.WebPDecoder.Decode(webpData, out int width, out int height, Imazen.WebP.WebPPixelFormat.Bgra);
            var bitmapSource = BitmapSource.Create(
                width, height,
                96, 96,
                System.Windows.Media.PixelFormats.Bgra32,
                null,
                pixels,
                width * 4 
            );
            bitmapSource.Freeze();
            return bitmapSource;
        }
        private static ImageSource LoadSVG(Stream stream)
        {
            using var reader = new System.Xml.XmlTextReader(stream);
            var settings = new WpfDrawingSettings();
            var svgReader = new SharpVectors.Converters.FileSvgReader(settings);
            var dImage = new DrawingImage(svgReader.Read(reader));
            dImage.Freeze();
            return dImage;
        }
        private static bool IsSvgStream(Stream stream)
        {
            if (stream == null || !stream.CanRead) return false;

            // 确保流位置在开头
            if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);

            var settings = new XmlReaderSettings
            {
                ConformanceLevel = ConformanceLevel.Fragment,
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            try
            {
                using var reader = XmlReader.Create(stream, settings);
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        return reader.NamespaceURI == "http://www.w3.org/2000/svg"
                               && reader.LocalName == "svg";
                    }
                }
            }
            catch (XmlException)
            {
                return false;
            }
            finally
            {
                if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);
            }

            return false;
        }
        private static bool IsWebPStream(Stream stream)
        {
            if (stream == null || !stream.CanRead) return false;
            if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);

            // 至少需要 12 个字节来判断
            byte[] header = new byte[12];
            int bytesRead = stream.Read(header, 0, 12);

            // 重置流位置
            if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);

            if (bytesRead < 12) return false;

            // 检查前4字节是否为 "RIFF"，第9-12字节是否为 "WEBP"
            return header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
                && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50;
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
            LoadImage();
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
