using PCL.CS.Modules;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using XeF4Core;

namespace PCL.CS.Controls
{
    public class MyImage : Image
    {
        // 依赖属性：Source（网络或本地路径）
        public new static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(MyImageSource), typeof(MyImage),
                new PropertyMetadata(null, OnSourceChanged));

        public new MyImageSource Source
        {
            get => (MyImageSource)GetValue(SourceProperty);
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
            DependencyProperty.Register("LoadingSource", typeof(MyImageSource), typeof(MyImage),
                new PropertyMetadata(App.Current.Resources["Image.Icons.NoIcon.png"]));

        public MyImageSource LoadingSource
        {
            get => (MyImageSource)GetValue(LoadingSourceProperty);
            set => SetValue(LoadingSourceProperty, value);
        }

        // 备用图片（主图下载失败时尝试）
        public static readonly DependencyProperty FallbackSourceProperty =
            DependencyProperty.Register("FallbackSource", typeof(MyImageSource), typeof(MyImage),
                new PropertyMetadata(null));

        public MyImageSource FallbackSource
        {
            get => (MyImageSource)GetValue(FallbackSourceProperty);
            set => SetValue(FallbackSourceProperty, value);
        }

        public MyImage() : base()
        {
        }


        private static async void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (MyImage)d;
            await ctrl.LoadImageAsync();
        }


        private async Task LoadImageAsync()
        {
            try
            {
                var Task = Source.GetImageSourceAsync();
                SetImageSource( await LoadingSource.GetImageSourceAsync());
                SetImageSource(await Task);
            }
            catch(Exception ex) 
            {
                Base.Log(ex);
                await LoadFallbackAsync();
            }
        }

        private async Task LoadFallbackAsync()
        {
            SetImageSource(await FallbackSource.GetImageSourceAsync());
        }

        private void SetImageSource(ImageSource image)
        {
            // 确保在 UI 线程上设置 Image.Source
            Dispatcher.Invoke(() =>
            {
                base.Source = image;
            });
        }
    }
}