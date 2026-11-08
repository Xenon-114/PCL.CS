using System.Windows;

namespace PCL.CS.Controls
{
    public class MyRadioBox : MyCheckableBox, IMyRadio
    {
        protected override bool CanAutoUnSelect => false;
        event RoutedEventHandler IMyRadio.Checked
        {
            add => Checked += value;
            remove => Checked -= value;
        }
        bool IMyRadio.IsChecked { get => IsChecked; set => IsChecked = value; }
        public MyRadioGroup RadioGroup
        {
            get { return (MyRadioGroup)GetValue(RadioGroupProperty); }
            set { SetValue(RadioGroupProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RadioGroup.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RadioGroupProperty =
            DependencyProperty.Register(nameof(RadioGroup), typeof(MyRadioGroup), typeof(MyRadioBox), new PropertyMetadata((s, e) => (s as MyRadioBox)?.OnRadioGroupChanged(e)));

        private void OnRadioGroupChanged(DependencyPropertyChangedEventArgs e)
        {
            (e.OldValue as MyRadioGroup)?.RemoveChild(this);
            (e.NewValue as MyRadioGroup)?.AddChild(this);
        }
    }
}