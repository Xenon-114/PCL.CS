using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace PCL.CS.Controls
{
    public class MyRadioStack : StackPanel, IMyRadioStack
    {
        // 依赖属性
        


        public IMyRadio SelectItem
        {
            get => _Group.SelectedItem;
            set => _Group.SelectedItem = value;
        }


        // 路由事件
        public static readonly RoutedEvent SelectIndexChangedEvent =
            EventManager.RegisterRoutedEvent(nameof(SelectIndexChanged), RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(MyRadioStack));

        public event RoutedEventHandler SelectIndexChanged
        {
            add => AddHandler(SelectIndexChangedEvent, value);
            remove => RemoveHandler(SelectIndexChangedEvent, value);
        }

        protected virtual void OnSelectIndexChanged()
        {
            RaiseEvent(new RoutedEventArgs(SelectIndexChangedEvent, this));
        }

        // 公开的子项列表（只读）
        private MyRadioGroup _Group = new MyRadioGroup();

        public MyRadioStack()
        {
            _Group.SelectedItemChanged += (s, e) =>
                OnSelectIndexChanged();
        }

        // 当视觉子元素变化时，重建列表并重新订阅事件
        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            _Group.AddChild(visualAdded as IMyRadio);
            _Group.RemoveChild(visualRemoved as IMyRadio);
        }

    }
}