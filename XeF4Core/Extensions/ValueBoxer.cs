using System;
using System.Collections.Generic;
using System.Text;

namespace XeF4Core
{
    /// <summary>
    /// 表示一个包装，包装任意类型为值。值不可为空。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct ValueBoxer<T>:IEquatable<ValueBoxer<T>> where T : notnull
    {
#pragma warning disable CS1591
        public ValueBoxer(T value)
        {
            if (value is null) throw new InvalidOperationException("对于包含空值的包装的访问是不允许的");
            Value = value;
        }
        public T Value
        {
            get
            {
                if (field is null) throw new InvalidOperationException("对于包含空值的包装的访问是不允许的");
                return field;
            }
        }
        public static implicit operator ValueBoxer<T>(T value)
        {
            if (value is null) throw new ArgumentNullException("value");
            return new ValueBoxer<T>(value);
        }
        public static implicit operator T(ValueBoxer<T> boxer)
        {
            return boxer.Value;
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public bool Equals(ValueBoxer<T> other)
        {
            if(Value is IEquatable<T> value)
            {
                return value.Equals(other.Value);
            }
            else
            {
                return Value.Equals(other.Value);
            }
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        public override string ToString()
        {
            return Value.ToString();
        }
#pragma warning restore CS1591
    }
}
