using System.IO;
using System.Xml.Serialization;

namespace Messenger.Resources.Tools.Data;

public static class XmlTools
{
    public static string ToXmlString<T>(this T input)
    {
        using var writer = new StringWriter();
        
        input.ToXml(writer);
            
        return writer.ToString();
    }
    
    public static T FromXmlString<T>(this string objectData)
    {
        using var reader = new StringReader(objectData);
        return ((T)new XmlSerializer(typeof(T)).Deserialize(reader)!)!;
    }

    private static void ToXml<T>(this T objectToSerialize, StringWriter writer)
    {
        new XmlSerializer(typeof(T)).Serialize(writer, objectToSerialize);
    }
}