using PCL.CS.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace PCL.CS.Controls
{
    public class ColorMixer : DependencyObject
    {
        public Color ColorA
        {
            get => (Color)GetValue(ColorAProperty);
            set => SetValue(ColorAProperty, value);
        }
        public Color ColorB
        {
            get => (Color)GetValue(ColorBProperty);
            set => SetValue(ColorBProperty, value);
        }
        public double ColorARatio
        {
            get => (double)GetValue(ColorARatioProperty);
            set => SetValue(ColorARatioProperty, value);
        }
        public Color ColorResult
        {
            get => (Color)GetValue(ColorResultProperty);
            private set => SetValue(ColorResultPropertyKey, value);
        }

        public static readonly DependencyProperty ColorAProperty;
        public static readonly DependencyProperty ColorBProperty;
        public static readonly DependencyProperty ColorARatioProperty;
        private static readonly DependencyPropertyKey ColorResultPropertyKey;
        public static readonly DependencyProperty ColorResultProperty;
        static ColorMixer()
        {
            ColorAProperty = DependencyProperty.Register("ColorA", typeof(Color), typeof(ColorMixer), new PropertyMetadata((d, e) => { (d as ColorMixer).OnPropertyChanged(); }));
            ColorBProperty = DependencyProperty.Register("ColorB", typeof(Color), typeof(ColorMixer), new PropertyMetadata((d, e) => { (d as ColorMixer).OnPropertyChanged(); }));
            ColorARatioProperty = DependencyProperty.Register("ColorARatio", typeof(double), typeof(ColorMixer), new PropertyMetadata((d, e) => { (d as ColorMixer).OnPropertyChanged(); }));
            ColorResultPropertyKey = DependencyProperty.RegisterReadOnly("ColorResult", typeof(Color), typeof(ColorMixer), new PropertyMetadata(new Color()));
            ColorResultProperty = ColorResultPropertyKey.DependencyProperty;
        }

        private void OnPropertyChanged()
        {
            Color ColorA = this.ColorA;
            Color ColorB = this.ColorB;
            double ratio = this.ColorARatio;
            ratio = MathHelper.Clamp(ratio, 0, 1);
            ratio = 1 - ratio;
            Color Result = Color.FromArgb(
                (byte)((ColorB.A - ColorA.A) * ratio + ColorA.A),
                (byte)((ColorB.R - ColorA.R) * ratio + ColorA.R),
                (byte)((ColorB.G - ColorA.G) * ratio + ColorA.G),
                (byte)((ColorB.B - ColorA.B) * ratio + ColorA.B)
                );
            ColorResult = Result;
        }

        public void SetBinding(DependencyProperty Property, BindingBase Binding) =>
            BindingOperations.SetBinding(this, Property, Binding);
    }

}
