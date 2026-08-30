using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

//包含一些从高版本迁移下来的类型

namespace System;


/// <summary>表示一个可以从开头或末尾访问的索引</summary>
public readonly struct Index : IEquatable<Index>
{
    /// <summary>构造一个<see cref="T:System.Index" />并以标记<paramref name="fromEnd"/>表示其是否为从尾部开始的</summary>
    /// <param name="value">索引。必须大于0。</param>
    /// <param name="fromEnd">
    ///   <see langword="true" />代表索引从尾部开始，<see langword="false" />相反。</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Index(int value, bool fromEnd = false)
    {
        if (value < 0)
        {
            Index.ThrowValueArgumentOutOfRange_NeedNonNegNumException();
        }
        if (fromEnd)
        {
            this._value = ~value;
            return;
        }
        this._value = value;
    }

    private Index(int value)
    {
        this._value = value;
    }

    /// <summary>获取一个指向第一个元素的<see cref="T:System.Index" /></summary>
    /// <returns>一个指向第一个元素的<see cref="T:System.Index" /></returns>
    public static Index Start
    {
        get
        {
            return new Index(0);
        }
    }

    /// <summary>获取一个指向最后一个元素的<see cref="T:System.Index" /></summary>
    /// <returns>一个指向最后一个元素的<see cref="T:System.Index" /></returns>
    public static Index End
    {
        get
        {
            return new Index(-1);
        }
    }

    /// <summary>获取一个指向开头指定索引的<see cref="T:System.Index" /></summary>
    /// <param name="value">从开头开始的索引</param>
    /// <returns>指定的<see cref="T:System.Index"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Index FromStart(int value)
    {
        if (value < 0)
        {
            Index.ThrowValueArgumentOutOfRange_NeedNonNegNumException();
        }
        return new Index(value);
    }

    /// <summary>获取一个指向末尾指定的反向索引的<see cref="T:System.Index" /></summary>
    /// <param name="value">从末尾开始的反向索引</param>
    /// <returns>指定的<see cref="T:System.Index"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Index FromEnd(int value)
    {
        if (value < 0)
        {
            Index.ThrowValueArgumentOutOfRange_NeedNonNegNumException();
        }
        return new Index(~value);
    }

    /// <summary>获取索引值</summary>
    /// <returns>索引值</returns>
    public int Value
    {
        get
        {
            if (this._value < 0)
            {
                return ~this._value;
            }
            return this._value;
        }
    }

    /// <summary>索引是否从末尾开始</summary>
    /// <returns>
    ///   <see langword="true" />表示从末尾开始；<see langword="false" />则相反</returns>
    public bool IsFromEnd
    {
        get
        {
            return this._value < 0;
        }
    }

    /// <summary>获取对于一个长度为<paramref name="length"/>的数组的实际索引</summary>
    /// <param name="length">数组的长度</param>
    /// <returns>实际索引</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetOffset(int length)
    {
        int num = this._value;
        if (this.IsFromEnd)
        {
            num += length + 1;
        }
        return num;
    }

    /// <inheritdoc/>
    public override bool Equals(object value)
    {
        return value is Index && this._value == ((Index)value)._value;
    }

    /// <inheritdoc/>
    public bool Equals(Index other)
    {
        return this._value == other._value;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return this._value;
    }

    /// <summary>将一个整数转换为<see cref="T:System.Index" /></summary>
    /// <param name="value">整数</param>
    /// <returns>对应的索引</returns>
    public static implicit operator Index(int value)
    {
        return Index.FromStart(value);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (this.IsFromEnd)
        {
            return this.ToStringFromEnd();
        }
        return ((uint)this.Value).ToString();
    }

    private static void ThrowValueArgumentOutOfRange_NeedNonNegNumException()
    {
        throw new ArgumentOutOfRangeException("value", "ArgumentOutOfRange：需要非负数");
    }

    private unsafe string ToStringFromEnd()
    {
        const int bufferSize = 12;
        char* buffer = stackalloc char[bufferSize];
        char* ptr = buffer + bufferSize - 1;
        var value = (uint)Value;
        *ptr = '\0';
        do
        {
            ptr--;
            *ptr = (char)('0' + value % 10);
            value /= 10;
        } while (value > 0);
        ptr--;
        *ptr = '^';
        return new string(ptr);
    }

    private readonly int _value;
}

/// <summary>
/// 提供高版本HashCode中的部分函数
/// </summary>
public struct HashCode
{
    private static readonly uint s_seed = 0x9e3779b9; // 常用黄金比例常数
    /// <summary>组合 2 个值的哈希。</summary>
    public static int Combine<T1, T2>(T1 value1, T2 value2)
    {
        uint h1 = (uint)(value1?.GetHashCode() ?? 0);
        uint h2 = (uint)(value2?.GetHashCode() ?? 0);

        uint hash = MixEmptyState() + 8U;
        hash = QueueRound(hash, h1);
        hash = QueueRound(hash, h2);
        return (int)MixFinal(hash);
    }

    /// <summary>组合 3 个值的哈希。</summary>
    public static int Combine<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
    {
        uint h1 = (uint)(value1?.GetHashCode() ?? 0);
        uint h2 = (uint)(value2?.GetHashCode() ?? 0);
        uint h3 = (uint)(value3?.GetHashCode() ?? 0);

        uint hash = MixEmptyState() + 12U;
        hash = QueueRound(hash, h1);
        hash = QueueRound(hash, h2);
        hash = QueueRound(hash, h3);
        return (int)MixFinal(hash);
    }

    /// <summary>组合 4 个值的哈希（使用四状态机，与官方完全一致）。</summary>
    public static int Combine<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
    {
        uint h1 = (uint)(value1?.GetHashCode() ?? 0);
        uint h2 = (uint)(value2?.GetHashCode() ?? 0);
        uint h3 = (uint)(value3?.GetHashCode() ?? 0);
        uint h4 = (uint)(value4?.GetHashCode() ?? 0);

        Initialize(out uint v1, out uint v2, out uint v3, out uint v4);
        v1 = Round(v1, h1);
        v2 = Round(v2, h2);
        v3 = Round(v3, h3);
        v4 = Round(v4, h4);

        uint hash = MixState(v1, v2, v3, v4) + 16U;
        return (int)MixFinal(hash);
    }

    /// <summary>
    /// 组合任意多个对象的哈希（params 版本）。
    /// 内部通过创建 HashCode 实例，循环 Add 后 ToHashCode 实现。
    /// 注意：此方法会装箱，仅用于参数数量不确定的场景。
    /// </summary>
    public static int Combine(params object?[] values)
    {
        if (values == null || values.Length == 0)
            return 0;

        // 为了保持与官方增量逻辑一致，我们使用实例方式模拟
        // 但为了简化，直接调用泛型重载（最多4个）会损失灵活性，故用 Add+ToHashCode 复制官方增量逻辑
        // 我们直接实现一个轻量循环混合（与官方 Add 逻辑等价）
        uint length = (uint)values.Length;
        uint v1, v2, v3, v4;
        Initialize(out v1, out v2, out v3, out v4);

        uint queue1 = 0, queue2 = 0, queue3 = 0;
        uint queueCount = 0;

        for (int i = 0; i < values.Length; i++)
        {
            uint h = (uint)(values[i]?.GetHashCode() ?? 0);
            switch (queueCount)
            {
                case 0: queue1 = h; queueCount = 1; break;
                case 1: queue2 = h; queueCount = 2; break;
                case 2: queue3 = h; queueCount = 3; break;
                case 3:
                    v1 = Round(v1, queue1);
                    v2 = Round(v2, queue2);
                    v3 = Round(v3, queue3);
                    v4 = Round(v4, h);
                    queueCount = 0;
                    break;
            }
        }

        uint hash = MixState(v1, v2, v3, v4) + length * 4U;

        if (queueCount > 0)
        {
            hash = QueueRound(hash, queue1);
            if (queueCount > 1)
            {
                hash = QueueRound(hash, queue2);
                if (queueCount > 2)
                    hash = QueueRound(hash, queue3);
            }
        }

        return (int)MixFinal(hash);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Initialize(out uint v1, out uint v2, out uint v3, out uint v4)
    {
        v1 = s_seed + 2654435761U + 2246822519U;
        v2 = s_seed + 2246822519U;
        v3 = s_seed;
        v4 = s_seed - 2654435761U;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Round(uint hash, uint input)
    {
        return RotateLeft(hash + input * 2246822519U, 13) * 2654435761U;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint QueueRound(uint hash, uint queuedValue)
    {
        return RotateLeft(hash + queuedValue * 3266489917U, 17) * 668265263U;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint MixFinal(uint hash)
    {
        hash ^= hash >> 15;
        hash *= 2246822519U;
        hash ^= hash >> 13;
        hash *= 3266489917U;
        hash ^= hash >> 16;
        return hash;
    }
    private static uint MixState(uint v1, uint v2, uint v3, uint v4)
            => RotateLeft(v1, 1) + RotateLeft(v2, 7) + RotateLeft(v3, 12) + RotateLeft(v4, 18);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint MixEmptyState()
        => s_seed + 374761393U;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint RotateLeft(uint value, int offset)
    {
        offset &= 31;
        return (value << offset) | (value >> (32 - offset));
    }
}


/// <summary>表示一个具有起始索引和结束索引的范围。</summary>
/// <remarks>
/// 对应 C# 的 .. 范围语法，例如 0..^1。
/// </remarks>
public readonly struct Range : IEquatable<Range>
{
    /// <summary>获取范围的包含起始索引。</summary>
    /// <returns>范围的起始索引。</returns>
    public Index Start { get; }

    /// <summary>获取范围的独占结束索引。</summary>
    /// <returns>范围的结束索引。</returns>
    public Index End { get; }

    /// <summary>使用指定的起始和结束索引实例化一个新的<see cref="Range" />。</summary>
    /// <param name="start">范围的包含起始索引。</param>
    /// <param name="end">范围的独占结束索引。</param>
    public Range(Index start, Index end)
    {
        this.Start = start;
        this.End = end;
    }

    /// <inheritdoc/>
    public override bool Equals(object value)
    {
        if (value is Range)
        {
            Range range = (Range)value;
            if (range.Start.Equals(this.Start))
            {
                return range.End.Equals(this.End);
            }
        }
        return false;
    }

    /// <summary>指示当前实例是否等于另一个<see cref="Range" />对象。</summary>
    /// <param name="other">要比较的<see cref="Range" />对象。</param>
    /// <returns>如果当前实例等于<paramref name="other" />，则为<see langword="true" />；否则为<see langword="false" />。</returns>
    public bool Equals(Range other)
    {
        return other.Start.Equals(this.Start) && other.End.Equals(this.End);
    }


    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine<int, int>(this.Start.GetHashCode(), this.End.GetHashCode());
    }

    /// <inheritdoc/>
    public unsafe override string ToString()
    {
        const int BufferLenth = 25;
        char* buffer = stackalloc char[BufferLenth];
        char* ptr = buffer + BufferLenth - 1;
        *ptr = '\0';
        uint value = (uint)this.End.Value;
        do
        {
            ptr--;
            *ptr = (char)('0' + value % 10);
            value /= 10;
        } while (value > 0);
        if (this.End.IsFromEnd)
        {
            ptr--;
            *ptr = '^';
        }
        ptr-=2;
        ptr[0] = ptr[1] = '.';
        value = (uint)this.Start.Value;
        do
        {
            ptr--;
            *ptr = (char)('0' + value % 10);
            value /= 10;
        } while (value > 0);
        if (this.Start.IsFromEnd)
        {
            ptr--;
            *ptr = '^';
        }
        return new string(ptr);
    }

    /// <summary>创建一个从指定起始索引到集合末尾的<see cref="Range" />实例。</summary>
    /// <param name="start">范围的起始索引。</param>
    /// <returns>从<paramref name="start" />到集合末尾的范围。</returns>
    public static Range StartAt(Index start)
    {
        return new Range(start, Index.End);
    }

    /// <summary>创建一个从集合开头到指定结束索引的<see cref="Range" />实例。</summary>
    /// <param name="end">范围的结束索引。</param>
    /// <returns>从集合开头到<paramref name="end" />的范围。</returns>
    public static Range EndAt(Index end)
    {
        return new Range(Index.Start, end);
    }

    /// <summary>获取一个从集合开头到集合末尾的<see cref="Range" />实例。</summary>
    /// <returns>代表整个集合的范围。</returns>
    public static Range All
    {
        get
        {
            return new Range(Index.Start, Index.End);
        }
    }

    /// <summary>使用集合长度计算范围的实际起始偏移量和元素个数。</summary>
    /// <param name="length">集合的长度（必须为正整数）。</param>
    /// <returns>包含起始偏移量和长度的元组。</returns>
    /// <exception cref="ArgumentOutOfRangeException">当<paramref name="length" />超出当前范围边界时抛出。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTuple<int, int> GetOffsetAndLength(int length)
    {
        int offset = this.Start.GetOffset(length);
        int offset2 = this.End.GetOffset(length);
        if (offset2 > length || offset > offset2)
        {
            Range.ThrowArgumentOutOfRangeException();
        }
        return new ValueTuple<int, int>(offset, offset2 - offset);
    }

    private static void ThrowArgumentOutOfRangeException()
    {
        throw new ArgumentOutOfRangeException("length");
    }
}


