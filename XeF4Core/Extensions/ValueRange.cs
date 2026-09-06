using System;
using System.Collections.Generic;

namespace XeF4Core;

/// <summary>
/// 表示一个值的区间
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly struct ValueRange<T> : IEquatable<ValueRange<T>?> where T : IComparable<T>
{
    /// <summary>
    /// 下界值
    /// </summary>
    public T? Lower { get; }
    /// <summary>
    /// 上界值
    /// </summary>
    public T? Upper { get; }
    /// <summary>
    /// 下界值是否为闭区间
    /// </summary>
    public bool IsLowerSealed { get; }
    /// <summary>
    /// 上界值是否为闭区间
    /// </summary>
    public bool IsUpperSealed { get; }
    /// <summary>
    /// 创建新的实例
    /// </summary>
    /// <param name="lower"></param>
    /// <param name="upper"></param>
    /// <param name="isLowerSealed"></param>
    /// <param name="isUpperSealed"></param>
    public ValueRange(T? lower, T? upper, bool isLowerSealed, bool isUpperSealed)
    {
        Lower = lower;
        Upper = upper;
        IsLowerSealed = Lower is not null && isLowerSealed;
        IsUpperSealed = Upper is not null && isUpperSealed;
    }
    #region 构造工厂
    /// <summary> 创建区间 (<paramref name="left"/>,<paramref name="right"/>) </summary>
    public static ValueRange<T> Open(T left, T right) => new(left, right, false, false);
    /// <summary> 创建区间 [<paramref name="left"/>,<paramref name="right"/>] </summary>
    public static ValueRange<T> Closed(T left, T right) => new(left, right, true, true);
    /// <summary> 创建区间 [<paramref name="left"/>,<paramref name="right"/>) </summary>
    public static ValueRange<T> ClosedOpen(T left, T right) => new(left, right, true, false);
    /// <summary> 创建区间 (<paramref name="left"/>,<paramref name="right"/>] </summary>
    public static ValueRange<T> OpenClosed(T left, T right) => new(left, right, false, true);
    /// <summary> 创建区间 (<paramref name="value"/>,<paramref name="value"/>] </summary>
    public static ValueRange<T> Exactly(T value) => new(value, value, true, true);
    /// <summary> 创建区间 [<paramref name="lower"/>,Infinity) </summary>
    public static ValueRange<T> AtLeast(T lower) => new(lower, default, true, false);
    /// <summary> 创建区间 (<paramref name="lower"/>,Infinity) </summary>
    public static ValueRange<T> GreaterThan(T lower) => new(lower, default, false, false);
    /// <summary> 创建区间 (-Infinity,<paramref name="upper"/>] </summary>
    public static ValueRange<T> AtMost(T upper) => new(default, upper, false, true);
    /// <summary> 创建区间 (-Infinity,<paramref name="upper"/>) </summary>
    public static ValueRange<T> LessThan(T upper) => new(default, upper, false, false);
    /// <summary> 创建区间 (-Infinity,Infinity) </summary>
    public static ValueRange<T> All() => new(default, default, false, false);
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    public readonly struct ClosedRangeFactory
    {
        /// <summary>
        /// 创建闭区间 [<paramref name="lower"/>,<paramref name="upper"/>]
        /// </summary>
        public ValueRange<T> this[T lower, T upper]
        {
            get => new(lower, upper, true, true);
        }
    }
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

    /// <summary>
    /// 创建闭区间 [lower,upper]
    /// </summary>
    public static readonly ClosedRangeFactory Range = new();
    #endregion

    #region 字符串转换
    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{(IsLowerSealed ? '[' : '(')}{(Lower is null ? "-Infinity" : Lower.ToString())},{(Upper is null ? "Infinity" : Upper.ToString())}{(IsUpperSealed ? ']' : ')')}";
    }
    /// <summary>
    /// 从字符串解析<see cref="ValueRange{T}"/>实例。
    /// </summary>
    /// <param name="s">指定字符串</param>
    /// <returns>指定实例</returns>
    public static ValueRange<T> Parse(string s)
    {
        return FromString(s);
    }
    /// <summary>
    /// 从字符串解析<see cref="ValueRange{T}"/>实例。
    /// </summary>
    /// <param name="s">指定字符串</param>
    /// <returns>指定实例</returns>
    /// <exception cref="FormatException"></exception>
    public static ValueRange<T> FromString(string s) => FromString(s, null);
    /// <summary>
    /// 从字符串解析<see cref="ValueRange{T}"/>实例。
    /// </summary>
    /// <param name="s">指定字符串</param>
    /// <param name="Parser">用于转换的转换器对象</param>
    /// <returns>指定实例</returns>
    /// <exception cref="FormatException"></exception>
    public static ValueRange<T> FromString(string s, Func<string, T>? Parser)
    {
        s = s.Trim(' ', '"', '\'', '“', '”', '‘', '’');
        bool isLowerSealed = s[0] is '[';
        bool isUpperSealed = s[^1] is ']';
        if (!isLowerSealed && s[0] is not '(') throw new FormatException("格式错误：字符串必须以[或(开头");
        if (!isUpperSealed && s[^1] is not ')') throw new FormatException("格式错误：字符串必须以]或)结尾");
        int point = s.IndexOfAny(',', '，');
        if(point is -1)throw new FormatException("缺少','");
        string LowerStr = s[1..point].Trim();
        string UpperStr = s[(point + 1)..^2].Trim();
        T? Lower;
        T? Upper;
        if (LowerStr == "" || LowerStr == "-∞" || LowerStr == "-Infinity")
        {
            Lower = default;
            isLowerSealed = false;
        }
        else
            Lower = Parser is not null ? Parser(LowerStr) : (T)Convert.ChangeType(LowerStr, typeof(T));
        if (UpperStr == "" || UpperStr == "∞" || UpperStr == "+∞" || UpperStr == "Infinity" || UpperStr == "+Infinity")
        {
            Upper = default;
            isUpperSealed = false;
        }
        else
            Upper = Parser is not null ? Parser(UpperStr) : (T)Convert.ChangeType(UpperStr, typeof(T));
        return new(Lower, Upper, isLowerSealed, isUpperSealed);
    }
    #endregion
    /// <summary>
    /// 判断指定值是否位于当前区间内
    /// </summary>
    /// <param name="value">指定值</param>
    /// <returns></returns>
    public bool Contains(T value)
    {
        if (Lower is not null)
        {
            int cmp = value.CompareTo(Lower);
            if (cmp < 0) return false;
            if (cmp == 0 && !IsLowerSealed) return false;
        }
        if (Upper is not null)
        {
            int cmp = value.CompareTo(Upper);
            if (cmp > 0) return false;
            if (cmp == 0 && !IsUpperSealed) return false;
        }
        return true;
    }
    /// <summary>
    /// 区间是否是空区间
    /// </summary>
    /// <returns></returns>
    public bool IsEmpty()
    {
        if (Lower is null || Upper is null) return false;
        int Compare = Lower.CompareTo(Upper);
        if (Compare < 0) return false;
        if (Compare > 0) return true;
        if (IsLowerSealed && IsUpperSealed) return false;
        return true;
    }
    /// <summary>
    /// 两个区间的交集
    /// </summary>
    /// <param name="rangea"></param>
    /// <param name="rangeb"></param>
    /// <returns></returns>
    public static ValueRange<T>? Intersect(ValueRange<T> rangea, ValueRange<T> rangeb)
    {
        static (T? value, bool inclusive) ChooseLower(T? a, bool aInc, T? b, bool bInc)
        {
            if (a is null) return (b, bInc);
            if (b is null) return (a, aInc);
            int cmp = a.CompareTo(b);
            if (cmp > 0) return (a, aInc);
            if (cmp < 0) return (b, bInc);
            return (a, aInc && bInc);
        }
        static (T? value, bool inclusive) ChooseUpper(T? a, bool aInc, T? b, bool bInc)
        {
            if (a is null) return (b, bInc);
            if (b is null) return (a, aInc);
            int cmp = a.CompareTo(b);
            if (cmp < 0) return (a, aInc);
            if (cmp > 0) return (b, bInc);
            return (a, aInc && bInc);
        }
        var (lower, isLowerSealed) = ChooseLower(rangea.Lower, rangea.IsLowerSealed, rangeb.Lower, rangeb.IsLowerSealed);
        var (upper, isUpperSealed) = ChooseUpper(rangea.Upper, rangea.IsUpperSealed, rangeb.Upper, rangeb.IsUpperSealed);
        var result = new ValueRange<T>(lower, upper, isLowerSealed, isUpperSealed);
        return result.IsEmpty() ? null : result;
    }
    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (obj is ValueRange<T> other) return Equals(other);
        return false;
    }
    /// <inheritdoc/>
    public bool Equals(ValueRange<T>? other)
    {
        return other is not null &&
               EqualityComparer<T?>.Default.Equals(Lower, other.Value.Lower) &&
               EqualityComparer<T?>.Default.Equals(Upper, other.Value.Upper) &&
               IsLowerSealed == other.Value.IsLowerSealed &&
               IsUpperSealed == other.Value.IsUpperSealed;
    }
    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Lower, EqualityComparer<T?>.Default);
        hashCode.Add(Upper, EqualityComparer<T?>.Default);
        hashCode.Add(IsLowerSealed);
        hashCode.Add(IsUpperSealed);
        return hashCode.ToHashCode();
    }
    /// <inheritdoc/>
    public static bool operator ==(ValueRange<T>? left, ValueRange<T>? right) => EqualityComparer<ValueRange<T>?>.Default.Equals(left, right);
    /// <inheritdoc/>
    public static bool operator !=(ValueRange<T>? left, ValueRange<T>? right) => !(left == right);
}
