using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

//包含一些从高版本迁移下来的类型

namespace System;


#region Index
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

#endregion

#region HashCode


#pragma warning disable CA1066 // Implement IEquatable when overriding Object.Equals



/// <summary>
/// 对于对象哈希值的基本操作
/// </summary>
public struct HashCode
{
    private static readonly uint s_seed = GenerateGlobalSeed();

    private const uint Prime1 = 2654435761U;
    private const uint Prime2 = 2246822519U;
    private const uint Prime3 = 3266489917U;
    private const uint Prime4 = 668265263U;
    private const uint Prime5 = 374761393U;

    private uint _v1, _v2, _v3, _v4;
    private uint _queue1, _queue2, _queue3;
    private uint _length;

    private static unsafe uint GenerateGlobalSeed()
    {
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        byte[] bytes = new byte[sizeof(uint)];
        rng.GetBytes(bytes);
        return BitConverter.ToUInt32(bytes, 0);
    }
    /// <summary>
    /// 获取一个对象的哈希值
    /// </summary>
    /// <typeparam name="T1">对象的类型</typeparam>
    /// <param name="value1">对象</param>
    /// <returns>哈希值</returns>
    public static int Combine<T1>(T1 value1)
    {
        // Provide a way of diffusing bits from something with a limited
        // input hash space. For example, many enums only have a few
        // possible hashes, only using the bottom few bits of the code. Some
        // collections are built on the assumption that hashes are spread
        // over a larger space, so diffusing the bits may help the
        // collection work more efficiently.

        uint hc1 = (uint)(value1?.GetHashCode() ?? 0);

        uint hash = MixEmptyState();
        hash += 4;

        hash = QueueRound(hash, hc1);

        hash = MixFinal(hash);
        return (int)hash;
    }
    /// <summary>
    /// 组合两个对象的哈希值
    /// </summary>
    /// <typeparam name="T1">第一个对象的类型</typeparam>
    /// <typeparam name="T2">第二个对象的类型</typeparam>
    /// <param name="value1">第一个对象</param>
    /// <param name="value2">第二个对象</param>
    /// <returns>哈希值</returns>
    public static int Combine<T1, T2>(T1 value1, T2 value2)
    {
        uint hc1 = (uint)(value1?.GetHashCode() ?? 0);
        uint hc2 = (uint)(value2?.GetHashCode() ?? 0);

        uint hash = MixEmptyState();
        hash += 8;

        hash = QueueRound(hash, hc1);
        hash = QueueRound(hash, hc2);

        hash = MixFinal(hash);
        return (int)hash;
    }
    /// <summary>
    /// 组合三个对象的哈希值
    /// </summary>
    /// <typeparam name="T1">第一个对象的类型</typeparam>
    /// <typeparam name="T2">第二个对象的类型</typeparam>
    /// <typeparam name="T3">第三个对象的类型</typeparam>
    /// <param name="value1">第一个对象</param>
    /// <param name="value2">第二个对象</param>
    /// <param name="value3">第三个对象</param>
    /// <returns>哈希值</returns>
    public static int Combine<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
    {
        uint hc1 = (uint)(value1?.GetHashCode() ?? 0);
        uint hc2 = (uint)(value2?.GetHashCode() ?? 0);
        uint hc3 = (uint)(value3?.GetHashCode() ?? 0);

        uint hash = MixEmptyState();
        hash += 12;

        hash = QueueRound(hash, hc1);
        hash = QueueRound(hash, hc2);
        hash = QueueRound(hash, hc3);

        hash = MixFinal(hash);
        return (int)hash;
    }
    /// <summary>
    /// 组合四个对象的哈希值
    /// </summary>
    /// <typeparam name="T1">第一个对象的类型</typeparam>
    /// <typeparam name="T2">第二个对象的类型</typeparam>
    /// <typeparam name="T3">第三个对象的类型</typeparam>
    /// <typeparam name="T4">第四个对象的类型</typeparam>
    /// <param name="value1">第一个对象</param>
    /// <param name="value2">第二个对象</param>
    /// <param name="value3">第三个对象</param>
    /// <param name="value4">第四个对象</param>
    /// <returns></returns>
    public static int Combine<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
    {
        uint hc1 = (uint)(value1?.GetHashCode() ?? 0);
        uint hc2 = (uint)(value2?.GetHashCode() ?? 0);
        uint hc3 = (uint)(value3?.GetHashCode() ?? 0);
        uint hc4 = (uint)(value4?.GetHashCode() ?? 0);

        Initialize(out uint v1, out uint v2, out uint v3, out uint v4);

        v1 = Round(v1, hc1);
        v2 = Round(v2, hc2);
        v3 = Round(v3, hc3);
        v4 = Round(v4, hc4);

        uint hash = MixState(v1, v2, v3, v4);
        hash += 16;

        hash = MixFinal(hash);
        return (int)hash;
    }
    /// <summary>
    /// 组合五个对象的哈希值
    /// </summary>
    /// <typeparam name="T1">第一个对象的类型</typeparam>
    /// <typeparam name="T2">第二个对象的类型</typeparam>
    /// <typeparam name="T3">第三个对象的类型</typeparam>
    /// <typeparam name="T4">第四个对象的类型</typeparam>
    /// <typeparam name="T5">第五个对象的类型</typeparam>
    /// <param name="value1">第一个对象</param>
    /// <param name="value2">第二个对象</param>
    /// <param name="value3">第三个对象</param>
    /// <param name="value4">第四个对象</param>
    /// <param name="value5">第五个对象</param>
    /// <returns>哈希值</returns>
    public static int Combine<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
    {
        uint hc1 = (uint)(value1?.GetHashCode() ?? 0);
        uint hc2 = (uint)(value2?.GetHashCode() ?? 0);
        uint hc3 = (uint)(value3?.GetHashCode() ?? 0);
        uint hc4 = (uint)(value4?.GetHashCode() ?? 0);
        uint hc5 = (uint)(value5?.GetHashCode() ?? 0);

        Initialize(out uint v1, out uint v2, out uint v3, out uint v4);

        v1 = Round(v1, hc1);
        v2 = Round(v2, hc2);
        v3 = Round(v3, hc3);
        v4 = Round(v4, hc4);

        uint hash = MixState(v1, v2, v3, v4);
        hash += 20;

        hash = QueueRound(hash, hc5);

        hash = MixFinal(hash);
        return (int)hash;
    }
    /// <summary>
    /// 组合六个对象的哈希值
    /// </summary>
    /// <typeparam name="T1">第一个对象的类型</typeparam>
    /// <typeparam name="T2">第二个对象的类型</typeparam>
    /// <typeparam name="T3">第三个对象的类型</typeparam>
    /// <typeparam name="T4">第四个对象的类型</typeparam>
    /// <typeparam name="T5">第五个对象的类型</typeparam>
    /// <typeparam name="T6">第六个对象的类型</typeparam>
    /// <param name="value1">第一个对象</param>
    /// <param name="value2">第二个对象</param>
    /// <param name="value3">第三个对象</param>
    /// <param name="value4">第四个对象</param>
    /// <param name="value5">第五个对象</param>
    /// <param name="value6">第六个对象</param>
    /// <returns>哈希值</returns>
    public static int Combine<T1, T2, T3, T4, T5, T6>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
    {
        uint hc1 = (uint)(value1?.GetHashCode() ?? 0);
        uint hc2 = (uint)(value2?.GetHashCode() ?? 0);
        uint hc3 = (uint)(value3?.GetHashCode() ?? 0);
        uint hc4 = (uint)(value4?.GetHashCode() ?? 0);
        uint hc5 = (uint)(value5?.GetHashCode() ?? 0);
        uint hc6 = (uint)(value6?.GetHashCode() ?? 0);

        Initialize(out uint v1, out uint v2, out uint v3, out uint v4);

        v1 = Round(v1, hc1);
        v2 = Round(v2, hc2);
        v3 = Round(v3, hc3);
        v4 = Round(v4, hc4);

        uint hash = MixState(v1, v2, v3, v4);
        hash += 24;

        hash = QueueRound(hash, hc5);
        hash = QueueRound(hash, hc6);

        hash = MixFinal(hash);
        return (int)hash;
    }
    /// <summary>
    /// 组合七个对象的哈希值
    /// </summary>
    /// <typeparam name="T1">第一个对象的类型</typeparam>
    /// <typeparam name="T2">第二个对象的类型</typeparam>
    /// <typeparam name="T3">第三个对象的类型</typeparam>
    /// <typeparam name="T4">第四个对象的类型</typeparam>
    /// <typeparam name="T5">第五个对象的类型</typeparam>
    /// <typeparam name="T6">第六个对象的类型</typeparam>
    /// <typeparam name="T7">第七个对象的类型</typeparam>
    /// <param name="value1">第一个对象</param>
    /// <param name="value2">第二个对象</param>
    /// <param name="value3">第三个对象</param>
    /// <param name="value4">第四个对象</param>
    /// <param name="value5">第五个对象</param>
    /// <param name="value6">第六个对象</param>
    /// <param name="value7">第七个对象</param>
    /// <returns>哈希值</returns>
    public static int Combine<T1, T2, T3, T4, T5, T6, T7>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7)
    {
        uint hc1 = (uint)(value1?.GetHashCode() ?? 0);
        uint hc2 = (uint)(value2?.GetHashCode() ?? 0);
        uint hc3 = (uint)(value3?.GetHashCode() ?? 0);
        uint hc4 = (uint)(value4?.GetHashCode() ?? 0);
        uint hc5 = (uint)(value5?.GetHashCode() ?? 0);
        uint hc6 = (uint)(value6?.GetHashCode() ?? 0);
        uint hc7 = (uint)(value7?.GetHashCode() ?? 0);

        Initialize(out uint v1, out uint v2, out uint v3, out uint v4);

        v1 = Round(v1, hc1);
        v2 = Round(v2, hc2);
        v3 = Round(v3, hc3);
        v4 = Round(v4, hc4);

        uint hash = MixState(v1, v2, v3, v4);
        hash += 28;

        hash = QueueRound(hash, hc5);
        hash = QueueRound(hash, hc6);
        hash = QueueRound(hash, hc7);

        hash = MixFinal(hash);
        return (int)hash;
    }
    /// <summary>
    /// 组合八个对象的哈希值
    /// </summary>
    /// <typeparam name="T1">第一个对象的类型</typeparam>
    /// <typeparam name="T2">第二个对象的类型</typeparam>
    /// <typeparam name="T3">第三个对象的类型</typeparam>
    /// <typeparam name="T4">第四个对象的类型</typeparam>
    /// <typeparam name="T5">第五个对象的类型</typeparam>
    /// <typeparam name="T6">第六个对象的类型</typeparam>
    /// <typeparam name="T7">第七个对象的类型</typeparam>
    /// <typeparam name="T8">第八个对象的类型</typeparam>
    /// <param name="value1">第一个对象</param>
    /// <param name="value2">第二个对象</param>
    /// <param name="value3">第三个对象</param>
    /// <param name="value4">第四个对象</param>
    /// <param name="value5">第五个对象</param>
    /// <param name="value6">第六个对象</param>
    /// <param name="value7">第七个对象</param>
    /// <param name="value8">第八个对象</param>
    /// <returns>哈希值</returns>
    public static int Combine<T1, T2, T3, T4, T5, T6, T7, T8>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8)
    {
        uint hc1 = (uint)(value1?.GetHashCode() ?? 0);
        uint hc2 = (uint)(value2?.GetHashCode() ?? 0);
        uint hc3 = (uint)(value3?.GetHashCode() ?? 0);
        uint hc4 = (uint)(value4?.GetHashCode() ?? 0);
        uint hc5 = (uint)(value5?.GetHashCode() ?? 0);
        uint hc6 = (uint)(value6?.GetHashCode() ?? 0);
        uint hc7 = (uint)(value7?.GetHashCode() ?? 0);
        uint hc8 = (uint)(value8?.GetHashCode() ?? 0);

        Initialize(out uint v1, out uint v2, out uint v3, out uint v4);

        v1 = Round(v1, hc1);
        v2 = Round(v2, hc2);
        v3 = Round(v3, hc3);
        v4 = Round(v4, hc4);

        v1 = Round(v1, hc5);
        v2 = Round(v2, hc6);
        v3 = Round(v3, hc7);
        v4 = Round(v4, hc8);

        uint hash = MixState(v1, v2, v3, v4);
        hash += 32;

        hash = MixFinal(hash);
        return (int)hash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Initialize(out uint v1, out uint v2, out uint v3, out uint v4)
    {
        v1 = s_seed + Prime1 + Prime2;
        v2 = s_seed + Prime2;
        v3 = s_seed;
        v4 = s_seed - Prime1;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint RotateLeft(uint value, int offset)
    {
        offset &= 31;
        return (value << offset) | (value >> (32 - offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Round(uint hash, uint input)
    {
        return RotateLeft(hash + input * Prime2, 13) * Prime1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint QueueRound(uint hash, uint queuedValue)
    {
        return RotateLeft(hash + queuedValue * Prime3, 17) * Prime4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint MixState(uint v1, uint v2, uint v3, uint v4)
    {
        return RotateLeft(v1, 1) + RotateLeft(v2, 7) + RotateLeft(v3, 12) + RotateLeft(v4, 18);
    }

    private static uint MixEmptyState()
    {
        return s_seed + Prime5;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint MixFinal(uint hash)
    {
        hash ^= hash >> 15;
        hash *= Prime2;
        hash ^= hash >> 13;
        hash *= Prime3;
        hash ^= hash >> 16;
        return hash;
    }
    /// <summary>
    /// 向HashCode中添加一个对象
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="value">对象</param>
    public void Add<T>(T value)
    {
        Add(value?.GetHashCode() ?? 0);
    }
    /// <summary>
    /// 向HashCode中添加一个对象
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="value">对象</param>
    /// <param name="comparer">自定义哈希算法</param>
    public void Add<T>(T value, IEqualityComparer<T>? comparer)
    {
        Add(value is null ? 0 : (comparer?.GetHashCode(value) ?? value.GetHashCode()));
    }

    /// <summary>向HashCode中添加一段字节</summary>
    /// <param name="value">字节</param>
    /// <remarks>
    /// 该方法所得的结果和逐段添加字节可能不同
    /// </remarks>
    public void AddBytes(ReadOnlySpan<byte> value)
    {
        if (value.Length < (sizeof(int) * 4))
        {
            goto Small;
        }

        // Usually Add calls Initialize but if we haven't used HashCode before it won't have been called.
        if (_length == 0)
        {
            Initialize(out _v1, out _v2, out _v3, out _v4);
        }
        else
        {
            // If we have at least 16 bytes to hash, we can add them in 16-byte batches,
            // but we first have to add enough data to flush any queued values.
            switch (_length % 4)
            {
                case 1:
                    System.Diagnostics.Debug.Assert(value.Length >= sizeof(int));
                    Add(ToInt32(value));
                    value = value.Slice(sizeof(int));
                    goto case 2;
                case 2:
                    System.Diagnostics.Debug.Assert(value.Length >= sizeof(int));
                    Add(ToInt32(value));
                    value = value.Slice(sizeof(int));
                    goto case 3;
                case 3:
                    System.Diagnostics.Debug.Assert(value.Length >= sizeof(int));
                    Add(ToInt32(value));
                    value = value.Slice(sizeof(int));
                    break;
            }
        }

        // With the queue clear, we add sixteen bytes at a time until the input has fewer than sixteen bytes remaining.
        while (value.Length >= sizeof(int) * 4)
        {
            _v1 = Round(_v1, ToUInt32(value));
            _v2 = Round(_v2, ToUInt32(value.Slice(sizeof(int) * 1)));
            _v3 = Round(_v3, ToUInt32(value.Slice(sizeof(int) * 2)));
            _v4 = Round(_v4, ToUInt32(value.Slice(sizeof(int) * 3)));

            _length += 4;
            value = value.Slice(sizeof(int) * 4);
        }

    Small:
        // Add four bytes at a time until the input has fewer than four bytes remaining.
        while (value.Length >= sizeof(int))
        {
            Add(ToInt32(value));
            value = value.Slice(sizeof(int));
        }

        // Add the remaining bytes a single byte at a time.
        foreach (byte b in value)
        {
            Add((int)b);
        }
    }

    private static int ToInt32(ReadOnlySpan<byte> value) =>
        Runtime.InteropServices.MemoryMarshal.Read<int>(value);
    private static uint ToUInt32(ReadOnlySpan<byte> value) =>
        Runtime.InteropServices.MemoryMarshal.Read<uint>(value);

    private void Add(int value)
    {
        // The original xxHash works as follows:
        // 0. Initialize immediately. We can't do this in a struct (no
        //    default ctor).
        // 1. Accumulate blocks of length 16 (4 uints) into 4 accumulators.
        // 2. Accumulate remaining blocks of length 4 (1 uint) into the
        //    hash.
        // 3. Accumulate remaining blocks of length 1 into the hash.

        // There is no need for #3 as this type only accepts ints. _queue1,
        // _queue2 and _queue3 are basically a buffer so that when
        // ToHashCode is called we can execute #2 correctly.

        // We need to initialize the xxHash32 state (_v1 to _v4) lazily (see
        // #0) nd the last place that can be done if you look at the
        // original code is just before the first block of 16 bytes is mixed
        // in. The xxHash32 state is never used for streams containing fewer
        // than 16 bytes.

        // To see what's really going on here, have a look at the Combine
        // methods.

        uint val = (uint)value;

        // Storing the value of _length locally shaves of quite a few bytes
        // in the resulting machine code.
        uint previousLength = _length++;
        uint position = previousLength % 4;

        // Switch can't be inlined.

        if (position == 0)
            _queue1 = val;
        else if (position == 1)
            _queue2 = val;
        else if (position == 2)
            _queue3 = val;
        else // position == 3
        {
            if (previousLength == 3)
                Initialize(out _v1, out _v2, out _v3, out _v4);

            _v1 = Round(_v1, _queue1);
            _v2 = Round(_v2, _queue2);
            _v3 = Round(_v3, _queue3);
            _v4 = Round(_v4, val);
        }
    }
    /// <summary>
    /// 获取最终哈希结果
    /// </summary>
    /// <returns>哈希值</returns>
    public readonly int ToHashCode()
    {
        // Storing the value of _length locally shaves of quite a few bytes
        // in the resulting machine code.
        uint length = _length;

        // position refers to the *next* queue position in this method, so
        // position == 1 means that _queue1 is populated; _queue2 would have
        // been populated on the next call to Add.
        uint position = length % 4;

        // If the length is less than 4, _v1 to _v4 don't contain anything
        // yet. xxHash32 treats this differently.

        uint hash = length < 4 ? MixEmptyState() : MixState(_v1, _v2, _v3, _v4);

        // _length is incremented once per Add(Int32) and is therefore 4
        // times too small (xxHash length is in bytes, not ints).

        hash += length * 4;

        // Mix what remains in the queue

        // Switch can't be inlined right now, so use as few branches as
        // possible by manually excluding impossible scenarios (position > 1
        // is always false if position is not > 0).
        if (position > 0)
        {
            hash = QueueRound(hash, _queue1);
            if (position > 1)
            {
                hash = QueueRound(hash, _queue2);
                if (position > 2)
                    hash = QueueRound(hash, _queue3);
            }
        }

        hash = MixFinal(hash);
        return (int)hash;
    }

#pragma warning disable 0809

    /// <summary>
    /// 请使用ToHashCode来获得结果
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    [Obsolete("请使用ToHashCode来获得结果", error: true)]
    public readonly override int GetHashCode() => throw new NotSupportedException("HashCodeNotSupported:请使用ToHashCode来获得结果!");
    /// <summary>
    /// 不能比较两个HashCode
    /// </summary>
    /// <param name="obj">另一个对象</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>

    [Obsolete("不允许对比两个HashCode", error: true)]
    public readonly override bool Equals(object? obj) => throw new NotSupportedException("EqualityNotSupported:不允许对比两个HashCode!");
#pragma warning restore 0809
}

#endregion

#region Range
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
        ptr -= 2;
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

#endregion
