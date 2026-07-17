using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PCL.CS.Controls
{
    public class MyMsg : DependencyObject
    {
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(MyMsg), new PropertyMetadata(null));



        public object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(nameof(Content), typeof(object), typeof(MyMsg), new PropertyMetadata(null));



        public object ExtraContent
        {
            get { return (object)GetValue(ExtraContentProperty); }
            set { SetValue(ExtraContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExtraContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExtraContentProperty =
            DependencyProperty.Register(nameof(ExtraContent), typeof(object), typeof(MyMsg), new PropertyMetadata(null));

        public enum ColorState
        {
            Normal,
            Red
        }


        public ColorState ColorType
        {
            get;
            set;
        }



        public string Btn1Text
        {
            get { return (string)GetValue(Btn1TextProperty); }
            set { SetValue(Btn1TextProperty, value); }
        }

        public static readonly DependencyProperty Btn1TextProperty =
            DependencyProperty.Register(nameof(Btn1Text), typeof(string), typeof(MyMsg),new PropertyMetadata("关闭"));



        public string Btn2Text
        {
            get { return (string)GetValue(Btn2TextProperty); }
            set { SetValue(Btn2TextProperty, value); }
        }

        public static readonly DependencyProperty Btn2TextProperty =
            DependencyProperty.Register(nameof(Btn2Text), typeof(string), typeof(MyMsg));


        public string Btn3Text
        {
            get { return (string)GetValue(Btn3TextProperty); }
            set { SetValue(Btn3TextProperty, value); }
        }

        public static readonly DependencyProperty Btn3TextProperty =
            DependencyProperty.Register(nameof(Btn3Text), typeof(string), typeof(MyMsg));

        /// <summary>
        /// 表示要启用多少个按钮。<see cref="BtnCount"/>应属于[1,3]。
        /// </summary>
        public int BtnCount { get; set; }



        public bool Btn2Enable
        {
            get { return (bool)GetValue(Btn2EnableProperty); }
            set { SetValue(Btn2EnableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Btn2Enable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Btn2EnableProperty =
            DependencyProperty.Register(nameof(Btn2Enable), typeof(bool), typeof(MyMsg), new PropertyMetadata(true));



        public bool Btn3Enable
        {
            get { return (bool)GetValue(Btn3EnableProperty); }
            set { SetValue(Btn3EnableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Btn3Enable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Btn3EnableProperty =
            DependencyProperty.Register(nameof(Btn3Enable), typeof(bool), typeof(MyMsg), new PropertyMetadata(true));

        public event EventHandler Btn2Action;

        public event EventHandler Btn3Action;

        public void TryRunFunc2()
        {
            if (Btn2Action is null)
                ResultTask.SetResult((bool?) (BtnCount > 2 ? false : true));
            else
                Btn2Action.Invoke(this, EventArgs.Empty);
        }
        public void TryRunFunc3()
        {
            if (Btn3Action is null)
                ResultTask.SetResult((bool?)true);
        }

        public bool FirstBtnHighlight { get; set; }

        public void SetBinding(DependencyProperty Property, BindingBase Binding) =>
            BindingOperations.SetBinding(this, Property, Binding);

        /// <summary>
        /// 弹窗返回的结果。通常情况下是bool?。也可能是由内部逻辑设置的。
        /// </summary>
        public TaskCompletionSource<object> ResultTask { get; } = new TaskCompletionSource<object>();
    }
}
