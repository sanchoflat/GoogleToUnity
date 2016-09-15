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

    public static bool SkipDataRow(this AbstractDataRow dataRow) {
        return dataRow.DataRowType == FileBuilder.DataRowType.Space;
    }


}