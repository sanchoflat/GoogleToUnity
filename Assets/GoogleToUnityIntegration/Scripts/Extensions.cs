// #region License
// Filename: Extensions.cs
// Last Change Date: 25.08.2015 9:47
// Author: Александр Еронин
// Project: EternalMaze
// Solution: UnityVS.svc_EternalMaze
// #endregion

#region

using System.IO;
using System.Xml;
using System.Xml.Serialization;
using GoogleSheetIntergation;

#endregion


//namespace System.Runtime.CompilerServices {
//    public class ExtensionAttribute : Attribute {}
//}


public static class Extensions {
    public static string UppercaseFirst(this string s) {
        if(string.IsNullOrEmpty(s)) {
            return string.Empty;
        }
        return char.ToUpper(s[0]) + s.Substring(1);
    }

    public static T DeserializeXMLFromPath<T>(this T @object, string path) {
        var serializer = new XmlSerializer(typeof(T));
        var reader = new StreamReader(path);
        @object = (T) serializer.Deserialize(reader);
        reader.Close();
        return @object;
    }

    public static T DeserializeFromXMLString<T>(this string data) {
        var serializer = new XmlSerializer(typeof(T));
        using(TextReader reader = new StringReader(data)) {
            var result = (T) serializer.Deserialize(reader);
            return result;
        }
    }

    public static void SerializeToXML<T>(this T @object, string path) {
        var ser = new XmlSerializer(typeof(T));
        using(var sww = new StringWriter()) {
            using(var writer = XmlWriter.Create(sww)) {
                ser.Serialize(writer, @object);
                var xml = sww.ToString();
                File.WriteAllText(path, xml);
            }
        }
    }

    public static bool SkipDataRow(this AbstractDataRow dataRow) {
        return dataRow.DataRowType == FileBuilder.DataRowType.Space;
    }
}