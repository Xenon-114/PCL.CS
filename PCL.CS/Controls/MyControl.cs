using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PCL.CS.Controls
{
    /// <summary>
    /// 自定义按钮接口
    /// </summary>
    public interface IMyButton
    {
        event RoutedEventHandler Click;
    }
    /// <summary>
    /// 自定义选择项接口
    /// </summary>
    public interface IMyRadio
    {
        bool IsChecked { get; set; }
        event RoutedEventHandler Checked;
        MyRadioGroup RadioGroup { get; set; }
    }
    /// <summary>
    /// 自定义单选框接口
    /// </summary>
    public interface IMyRadioStack
    {
        IMyRadio SelectItem { get; set; }
        event RoutedEventHandler SelectIndexChanged;
    }

    public class MyRadioGroup
    {
        private List<IMyRadio> RadioChildren = new List<IMyRadio>();
        public IMyRadio SelectedItem
        {
            get => _SelectedItem;
            set
            {
                if (_SelectedItem == value) return;
                if (!RadioChildren.Contains(value)) value = null;
                _SelectedItem = value;
                foreach (var i in RadioChildren)
                {
                    if (i != value)
                    {
                        if (i.IsChecked != false) i.IsChecked = false;
                    }
                    else
                    {
                        if (i.IsChecked == false) i.IsChecked = true;
                    }
                }
                SelectedItemChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        private IMyRadio _SelectedItem = null;
        public bool IsChecked(IMyRadio radio) => radio == SelectedItem;
        public void AddChild(IMyRadio Child)
        {
            if (Child is null) return;
            Child.Checked += OnChildChecked;
            RadioChildren.Add(Child);
            if(SelectedItem is null && Child.IsChecked)
            {
                SelectedItem = Child;
            }
            else if(SelectedItem != null)
            {
                Child.IsChecked = false;
            }
        }
        private void OnChildChecked(object sender, RoutedEventArgs e)
        {
            if (e.Source as IMyRadio != null)
                this.SelectedItem = e.Source as IMyRadio;
        }
        public void RemoveChild(IMyRadio Child)
        {
            if (Child is null) return;
            RadioChildren.Remove(Child);
            Child.Checked -= OnChildChecked;
            if (Child == SelectedItem) SelectedItem = null;
        }
        public event EventHandler SelectedItemChanged;
    }
}
