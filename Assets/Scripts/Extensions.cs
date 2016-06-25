// #region License
// Filename: Extensions.cs
// Last Change Date: 25.08.2015 9:47
// Author: Александр Еронин
// Project: EternalMaze
// Solution: UnityVS.svc_EternalMaze
// #endregion

#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

using Newtonsoft.Json;
using UnityEngine;
using Random = System.Random;

#endregion

public static class Extensions {

    #region json extension

    public static void SaveJSONToFile<T>(this T @object, string path) {
        File.WriteAllText(path, GetJSON(@object));
    }
   
    public static void SaveJSONToPlayerPrefs<T>(this T @object, string key)
    {
        PlayerPrefs.SetString(key, GetJSON(@object));
    }

    public static string GetJSON<T>(this T @object)
    {
        var json = JsonConvert.SerializeObject(@object);
        return json;
    }
    public static T LoadJSONFromString<T>(this T _object, string json)
    {
        if (string.IsNullOrEmpty(json))
            return default(T);
        var l = JsonConvert.DeserializeObject<T>(json);
        return l;
    }
    public static T LoadJSONFromFile<T>(this T _object, string path)
    {
        var file = File.ReadAllText(path);
        if(string.IsNullOrEmpty(file))
            return default(T);
        var l = JsonConvert.DeserializeObject<T>(file);
        return l;
    }
    public static T LoadJSONFromPlayerPrefs<T>(this object _object, string key) {
        var file = PlayerPrefs.GetString(key); 
        if (string.IsNullOrEmpty(file))
            return default(T);
        var l = JsonConvert.DeserializeObject<T>(file);
        return l;
    }

    public static T LoadJSONFromFile<T>(this string path) {
        var file = File.ReadAllText(path);
        var l = JsonConvert.DeserializeObject<T>(file);
        return l;
    }
 
    #endregion


}