using System;
using System.Runtime.Serialization;

namespace XeF4Core.MinecraftCore;

/// <summary>
/// Minecraft抛出的异常
/// </summary>
public abstract class MinecraftException : Exception
{
    /// <summary>
    /// Minecraft抛出的异常
    /// </summary>
    public MinecraftException() : base() { }
    /// <summary>
    /// Minecraft抛出的异常
    /// </summary>
    public MinecraftException(string message) : base(message) { }
    /// <summary>
    /// Minecraft抛出的异常
    /// </summary>
    public MinecraftException(string message, Exception innerException) : base(message, innerException) { }
    /// <summary>
    /// Minecraft抛出的异常
    /// </summary>
    public MinecraftException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
/// <summary>
/// 当Minecraft找不到Java时抛出
/// </summary>
public class MinecraftJavaNotFoundException : MinecraftException
{
    /// <summary>
    /// 当Minecraft找不到Java时抛出
    /// </summary>
    public MinecraftJavaNotFoundException() : base() { }
    /// <summary>
    /// 当Minecraft找不到Java时抛出
    /// </summary>
    public MinecraftJavaNotFoundException(string message) : base(message) { }
    /// <summary>
    /// 当Minecraft找不到Java时抛出
    /// </summary>
    public MinecraftJavaNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}
/// <summary>
/// Minecraft启动时抛出的异常
/// </summary>
public class MinecraftStartingException:MinecraftException
{
    /// <summary>
    /// Minecraft启动时抛出的异常
    /// </summary>
    public MinecraftStartingException() : base() { }
    /// <summary>
    /// Minecraft启动时抛出的异常
    /// </summary>
    public MinecraftStartingException(string message) : base(message) { }
    /// <summary>
    /// Minecraft启动时抛出的异常
    /// </summary>
    public MinecraftStartingException(string message, Exception innerException) : base(message, innerException) { }
}
