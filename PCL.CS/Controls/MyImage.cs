using PCL.CS.Modules;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PCL.CS.Controls
{
    public class MyImage : Image
    {
        // 依赖属性：Source（网络或本地路径）
        public new static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(string), typeof(MyImage),
                new PropertyMetadata(null, OnSourceChanged));

        public new string Source
        {
            get => (string)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        // 是否启用缓存（传递给 DownloadFile）
        public static readonly DependencyProperty EnableCacheProperty =
            DependencyProperty.Register("EnableCache", typeof(bool), typeof(MyImage),
                new PropertyMetadata(true));

        public bool EnableCache
        {
            get => (bool)GetValue(EnableCacheProperty);
            set => SetValue(EnableCacheProperty, value);
        }

        // 占位图（下载过程中显示）
        public static readonly DependencyProperty LoadingSourceProperty =
            DependencyProperty.Register("LoadingSource", typeof(string), typeof(MyImage),
                new PropertyMetadata("pack://application:,,,/Image/Icons/NoIcon.png"));

        public string LoadingSource
        {
            get => (string)GetValue(LoadingSourceProperty);
            set => SetValue(LoadingSourceProperty, value);
        }

        // 备用图片（主图下载失败时尝试）
        public static readonly DependencyProperty FallbackSourceProperty =
            DependencyProperty.Register("FallbackSource", typeof(string), typeof(MyImage),
                new PropertyMetadata(null));

        public string FallbackSource
        {
            get => (string)GetValue(FallbackSourceProperty);
            set => SetValue(FallbackSourceProperty, value);
        }

        public MyImage() : base()
        {
            this.Unloaded += (s, e) => OnUnloaded();
        }

        // 用于取消正在进行的下载任务
        private CancellationTokenSource _cts;

        private static async void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (MyImage)d;
            await ctrl.LoadImageAsync();
        }

        private async Task LoadImageAsync()
        {
            // 取消之前的下载任务
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            string source = Source;
            if (string.IsNullOrEmpty(source))
            {
                // 清除图片
                SetImageSource(null);
                return;
            }

            // 如果是本地文件（非 http 开头），直接显示
            if (!source.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                SetImageSource(source);
                return;
            }

            // 网络图片：先显示加载占位图
            if (!string.IsNullOrEmpty(LoadingSource))
                SetImageSource(LoadingSource);

            try
            {
                // 调用你的 DownloadFile 获取本地路径（异步，自动缓存）
                string localPath = await XeF4Core.HttpServer.DownloadFile(source, EnableCache).ConfigureAwait(false);

                if (token.IsCancellationRequested)
                    return;

                if (!string.IsNullOrEmpty(localPath) && File.Exists(localPath))
                {
                    SetImageSource(localPath);
                }
                else
                {
                    // 下载失败，尝试备用图
                    await LoadFallbackAsync(token);
                }
            }
            catch (Exception ex)
            {
                // 记录异常，尝试备用图
                Base.Log( $"下载图片失败: {source}\n{ex}");
                await LoadFallbackAsync(token);
            }
        }

        private async Task LoadFallbackAsync(CancellationToken token)
        {
            string fallback = FallbackSource;
            if (string.IsNullOrEmpty(fallback))
                return;

            // 备用图可能是本地路径，也可能是网络 URL
            if (!fallback.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                // 本地备用图直接显示
                SetImageSource(fallback);
            }
            else
            {
                try
                {
                    string localPath = await XeF4Core.HttpServer.DownloadFile(fallback, EnableCache).ConfigureAwait(false);
                    if (!token.IsCancellationRequested && !string.IsNullOrEmpty(localPath))
                        SetImageSource(localPath);
                }
                catch (Exception ex)
                {
                    Base.Log( $"备用图片也加载失败: {fallback}\n{ex}");
                }
            }
        }

        private void SetImageSource(string path)
        {
            // 确保在 UI 线程上设置 Image.Source
            Dispatcher.Invoke(() =>
            {
                if (string.IsNullOrEmpty(path))
                {
                    Source = null;
                }
                else
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(path, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze(); // 可选：提升性能
                        base.Source = bitmap;
                    }
                    catch (Exception ex)
                    {
                        Base.Log( $"设置图片源失败: {path}\n{ex}");
                        base.Source = null;
                    }
                }
            });
        }
        // 可选的：控件卸载时取消下载
        protected void OnUnloaded()
        {
            _cts?.Cancel();
        }
    }
}