using System;
using System.Collections.Generic;
using System.Text;

namespace System.Runtime.CompilerServices;

#pragma warning disable CS1591
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class CallerArgumentExpressionAttribute : Attribute
{
    public CallerArgumentExpressionAttribute(string parameterName)
    {
        ParameterName = parameterName;
    }

    public string ParameterName { get; }
}
#pragma warning restore
