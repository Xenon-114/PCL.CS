using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace XeF4Core;

/// <summary>
/// 对于Json管理的简单扩展
/// </summary>
public static class JsonExtensions
{

    /// <summary>
    /// 反序列化Json
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static T? DeserializeJson<T>(Stream stream)
    {
        using var sr = new StreamReader(stream);
        using var jsonTextReader = new JsonTextReader(sr);

        var serializer = new JsonSerializer();
        return serializer.Deserialize<T>(jsonTextReader);
    }
}
