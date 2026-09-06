using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;

namespace XeF4Core.WPF;

public static class Extensions
{
    /// <summary>
    /// 创建ResourceReferenceExpression的创建器
    /// </summary>
    private static readonly ResourceReferenceCreator ResourceReferenceFactory;

    internal delegate object ResourceReferenceCreator(object Name);

    static Extensions()
    {
        Type resourceRefType = typeof(FrameworkElement).Assembly.GetType(
            "System.Windows.ResourceReferenceExpression"
        );
        var Ctor = resourceRefType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(object)], null);
        DynamicMethod Factory = new DynamicMethod(
            "ResourceReferenceFactory",
            typeof(object),
            [typeof(object)],
            typeof(Extensions).Module,
            true);
        var ilCodes = Factory.GetILGenerator();
        ilCodes.Emit(OpCodes.Ldarg_0);
        ilCodes.Emit(OpCodes.Newobj, Ctor);
        ilCodes.Emit(OpCodes.Ret);
        ResourceReferenceFactory = (ResourceReferenceCreator)Factory.CreateDelegate(typeof(ResourceReferenceCreator));
    }
    /// <summary>
    /// 设置资源引用。
    /// </summary>
    /// <param name="Object">你为什么要这么调用？</param>
    /// <param name="Property">指定的依赖属性</param>
    /// <param name="Name">资源名称</param>
    public static void SetResourceReference(this DependencyObject Object, DependencyProperty Property, object Name)
    {
        if (Object is FrameworkElement FElement)
            FElement.SetResourceReference(Property, Name);
        else if (Object is FrameworkContentElement FCElement)
            FCElement.SetResourceReference(Property, Name);
        else
        {
            Object.SetValue(Property, ResourceReferenceFactory.Invoke(Name));
        }
    }
    public static PropertyPath ToPropertyPath(this DependencyProperty Property)
    {
        return new PropertyPath(Property);
    }
    public static BindingExpressionBase SetBinding(this DependencyObject Object, DependencyProperty Property, BindingBase binding) =>
        BindingOperations.SetBinding(Object, Property, binding);
    
}
