using PCL.CS.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PCL.CS.Controls
{
    public class HintControl:Control
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(HintControl));
        
        


        public double LeftMargin
        {
            get { return (double)GetValue(LeftMarginProperty); }
            set { SetValue(LeftMarginProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftMargin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftMarginProperty =
            DependencyProperty.Register(nameof(LeftMargin), typeof(double), typeof(HintControl), new PropertyMetadata((d, e) => (d as HintControl).OnMarginChange()));

        public double TrueHeight
        {
            get { return (double)GetValue(TrueHeightProperty); }
            set { SetValue(TrueHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActuralHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TrueHeightProperty =
            DependencyProperty.Register(nameof(TrueHeight), typeof(double), typeof(HintControl), new PropertyMetadata((d, e) => (d as HintControl).OnMarginChange()));

        private void OnMarginChange()
        {
            this.Margin = new Thickness(LeftMargin, TrueHeight - 26, 0, 0);
        }

        public HintColorState ColorType
        {
            get { return (HintColorState)GetValue(ColorTypeProperty); }
            set { SetValue(ColorTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColorType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorTypeProperty =
            DependencyProperty.Register(nameof(ColorType), typeof(HintColorState), typeof(HintControl));


    }
}
