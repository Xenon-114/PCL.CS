using PCL.CS.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PCL.CS.Controls
{
    [ContentProperty("Buttons")]
    public class MyRadioBoxItem:MyListItem
    {
        private readonly UIElement VisualTree = null;

        private Path PathLogo { get; }

        public Geometry Logo
        {
            get => PathLogo.Data;
            set => PathLogo.Data = value;
        }

        private ScaleTransform _LogoScale = new ScaleTransform();
        public double LogoScale
        {
            get => _LogoScale.ScaleX;
            set => _LogoScale.ScaleX = _LogoScale.ScaleY = value;
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(MyRadioBoxItem));

        public string Info
        {
            get => (string)GetValue(InfoProperty);
            set => SetValue(InfoProperty, value);
        }
        public static readonly DependencyProperty InfoProperty = DependencyProperty.Register("Info", typeof(string), typeof(MyRadioBoxItem),
            new PropertyMetadata((d, e) =>
            {
                if (e.NewValue is null || e.NewValue as string == "")
                    (d as MyRadioBoxItem).LabInfo.Visibility = Visibility.Collapsed;
                else
                    (d as MyRadioBoxItem).LabInfo.Visibility = Visibility.Visible;
            }));

        private UIElement LabInfo = null;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public UIElementCollection Buttons { get; }

        protected override double DefaultAnimScale => 1;
        protected override double MouseOverScale => 1;
        protected override double RectBackCornerRadius => 0;
        protected override bool AutoCheckResult => true;
        public MyRadioBoxItem()
        {
            this.Style = (Style)Application.Current.FindResource(typeof(MyListItem));
            this.AutoCheck = true;
            this.FontSize = 14;
            this.Title = null;
            this.Info = null;

            Grid MainGrid = new Grid();
            this.VisualTree = MainGrid;
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(34, GridUnitType.Pixel) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });

            PathLogo = new Path();
            PathLogo.Stretch = Stretch.Uniform;
            PathLogo.RenderTransform = _LogoScale;
            PathLogo.RenderTransformOrigin = new Point(0.5, 0.5);
            PathLogo.Margin = new Thickness(8, 4, 6, 4);
            PathLogo.SetBinding(Path.FillProperty, new Binding
            {
                Source = this,
                Path = new PropertyPath("Foreground"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            Grid.SetColumn(PathLogo, 0);
            MainGrid.Children.Add(PathLogo);

            Grid ContentGrid = new Grid();
            ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            ContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            TextBlock LabTitle = new TextBlock();
            LabTitle.TextTrimming = TextTrimming.CharacterEllipsis;
            LabTitle.SetBinding(TextBlock.FontSizeProperty, new Binding
            {
                Source = this,
                Path = new PropertyPath("FontSize"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            LabTitle.Margin = new Thickness(2, 0, 2, 0);
            LabTitle.UseLayoutRounding = false;
            LabTitle.SetBinding(TextBlock.TextProperty, new Binding
            {
                Source = this,
                Path = new PropertyPath("Title"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            Grid.SetRow(LabTitle, 1);
            ContentGrid.Children.Add(LabTitle);

            TextBlock LabInfo = new TextBlock();
            LabInfo.TextTrimming = TextTrimming.CharacterEllipsis;
            LabInfo.Opacity = 0.6;
            LabInfo.FontSize = 12;
            LabInfo.IsHitTestVisible = false;
            LabInfo.Margin = new Thickness(2, 0, 2, 0);
            LabInfo.Visibility = Visibility.Collapsed;
            LabInfo.SetBinding(TextBlock.TextProperty, new Binding
            {
                Source = this,
                Path = new PropertyPath("Info"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            Grid.SetRow(LabInfo, 2);
            ContentGrid.Children.Add(LabInfo);
            this.LabInfo = LabInfo;

            Grid.SetColumn(ContentGrid, 1);
            MainGrid.Children.Add(ContentGrid);

            StackPanel ButtonPanel = new StackPanel();
            Buttons = ButtonPanel.Children;
            ButtonPanel.Background = Brushes.Transparent;
            ButtonPanel.Orientation = Orientation.Horizontal;
            ButtonPanel.VerticalAlignment = VerticalAlignment.Center;
            ButtonPanel.Height = 25;
            ButtonPanel.Margin = new Thickness(5, 0, 5, 0);
            ButtonPanel.SetBinding(OpacityProperty, new Binding
            {
                Source = this,
                Path = new PropertyPath("ButtonsOpacity"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            ButtonPanel.AddHandler(MouseLeftButtonDownEvent, new RoutedEventHandler(OnButtonPanelClick));
            ButtonPanel.AddHandler(MouseUpEvent, new RoutedEventHandler(OnButtonPanelClick));
            Grid.SetColumn(ButtonPanel, 2);
            MainGrid.Children.Add(ButtonPanel);

            this.Content = this.VisualTree;
        }
        private void OnButtonPanelClick(object sender, RoutedEventArgs e) => e.Handled = true;
        private double ButtonsOpacity
        {
            get => (double)GetValue(ButtonsOpacityProperty);
            set => SetValue(ButtonsOpacityProperty, value);
        }
        private static readonly DependencyProperty ButtonsOpacityProperty = DependencyProperty.Register("ButtonsOpacity", typeof(double), typeof(MyRadioBoxItem));

        //private event Action<double> ButtonsOpacityChanged;

        private Animation ButtonsOpacityAnim;
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            Animation.Stop(ButtonsOpacityAnim);
            ButtonsOpacityAnim = new DoubleAnimation(this, ButtonsOpacityProperty, this.ButtonsOpacity, 1, 120);
            Animation.Start(ButtonsOpacityAnim);
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            Animation.Stop(ButtonsOpacityAnim);
            ButtonsOpacityAnim = new DoubleAnimation(this, ButtonsOpacityProperty, this.ButtonsOpacity, 0, 180);
            Animation.Start(ButtonsOpacityAnim);
        }
    }
}
