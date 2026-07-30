using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace XeF4Core.WPF
{
    public static class Extensions
    {
        /// <summary>
        /// 创建ResourceReferenceExpression的创建器
        /// </summary>
        private static readonly ConstructorInfo ResourceReferenceCreator;
        static Extensions()
        {
            Type resourceRefType = typeof(FrameworkElement).Assembly.GetType(
                "System.Windows.ResourceReferenceExpression"
            );
            ResourceReferenceCreator = resourceRefType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(object), typeof(bool)], null);
        }
        /// <summary>
        /// 设置资源引用。
        /// </summary>
        /// <param name="Object">你为什么要这么调用？</param>
        /// <param name="Property">指定的依赖属性</param>
        /// <param name="Name">资源名称</param>
        public static void SetResourceReference(this DependencyObject Object,DependencyProperty Property,object Name)
        {
            //使用反射调用那个SB ResourceReferenceExpression 的构造函数
            object instance = ResourceReferenceCreator.Invoke([Name]);
            Object.SetValue(Property, instance);
        }
        public static PropertyPath ToPropertyPath(this DependencyProperty Property)
        {
            return new PropertyPath(Property.Name);
        }
    }
}
