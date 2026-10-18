using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace XeF4Core;

/// <summary>
/// 对于Json管理的简单扩展
/// </summary>
public static class JsonExtensions
{
    /// <summary>
    /// 默认属性
    /// </summary>
    public static JsonSerializerOptions MyJsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        MaxDepth = 64,
        ReadCommentHandling = JsonCommentHandling.Skip,
        IgnoreReadOnlyProperties = true,
        IgnoreReadOnlyFields = true,
        IncludeFields = false,
        ReferenceHandler = ReferenceHandler.Preserve,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };
    /// <summary>
    /// 定义枚举类型的转换逻辑
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    public class EnumConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
    {
        private static readonly ConcurrentDictionary<string, TEnum> _nameToValue = new(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<TEnum, string> _valueToName = new();
        static EnumConverter()
        {
            var type = typeof(TEnum);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);//这里的Field就是枚举值

            foreach(var field in fields)
            {
                var value = (TEnum)field.GetValue(null)!;

                string name;
                var jsonProp = field.GetCustomAttribute<JsonPropertyNameAttribute>();
                if (jsonProp != null && !string.IsNullOrWhiteSpace(jsonProp.Name))
                {
                    name = jsonProp.Name;
                }
                else
                {
                    name = field.Name;
                }
                //存进去
                _nameToValue.TryAdd(name, value);
                //只保留先遇到的
                _valueToName.TryAdd(value, name);
            }
        }
        /// <inheritdoc/>
        public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType is JsonTokenType.String)
            {
                string? enumString = reader.GetString();
                if (enumString != null && _nameToValue.TryGetValue(enumString, out var result))
                    return result;
                else
                    throw new JsonException($"无法将 \"{enumString}\" 转换为枚举 {typeof(TEnum).Name}");
            }
            else if (reader.TokenType is JsonTokenType.Number && reader.TryGetInt32(out int intValue))
                return (TEnum)Enum.ToObject(typeof(TEnum), intValue);
            else throw new JsonException($"无法识别枚举类型 \"{reader.TokenType}\"");
        }
        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        {
            if (_valueToName.TryGetValue(value, out var result))
                writer.WriteStringValue(result);
            else
                writer.WriteNumberValue(Convert.ToInt64(value));
        }
    }
}