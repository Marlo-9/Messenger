using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using Messenger.Resources.Tools.Enums;

namespace Messenger.Resources.Tools.Data;

public static class JsonTools
{
    public static JsonSerializerOptions Options = new JsonSerializerOptions
    {
        AllowTrailingCommas = false,
        IgnoreReadOnlyProperties = false,
        IgnoreReadOnlyFields = false,
        WriteIndented = false
    };
    
    public static string ToJsonString<T>(this T input) where T : new()
    {
        using var writer = new StringWriter();
        
        writer.WriteLine(JsonSerializer.Serialize(input));
            
        return writer.ToString();
    }
    
    public static T FromJsonString<T>(this string objectData)
    {
        return objectData.GetMessageType() != NetworkMessageType.Null
            ? JsonSerializer.Deserialize<T>(objectData.TrimMessageType())!
            : JsonSerializer.Deserialize<T>(objectData)!;
    }
    
    public static string SetMessageType(this string message, NetworkMessageType type = NetworkMessageType.Null)
    {
        return "{" + type + "}" + message;
    }

    public static NetworkMessageType GetMessageType(this string? message)
    {
        return message != null && Enum.TryParse(message.Substring(1, message.IndexOf('}') - 1),
            out NetworkMessageType type)
            ? type
            : NetworkMessageType.Null;
    }

    public static string TrimMessageType(this string message)
    {
        if (message.GetMessageType() == NetworkMessageType.Null)
            return message;

        return message.Substring(message.IndexOf('}') + 1);
    }

    private static void ToJson<T>(this T objectToSerialize, StringWriter writer) where T : new()
    {
        writer.WriteLine(JsonSerializer.Serialize(objectToSerialize, Options));
    }
}