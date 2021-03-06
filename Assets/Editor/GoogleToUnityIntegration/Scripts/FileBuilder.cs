﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.CSharp;
using UnityEditor;
using UnityEngine;


namespace GoogleSheetIntergation {
    public class FileBuilder {
        public enum DataRowType {
            Space,
            Class
        }

        public static string GetTabulator(int c) {
            var sb = new StringBuilder(c);
            for(var i = 0; i < c; i++) {
                sb.Append("\t");
            }
            return sb.ToString();
        }

        public static void GenerateClassFile(Dictionary<string, Dictionary<string, List<AbstractDataRow>>> data, GoogleSheetData googleData)
        {
            if (data == null) { return; }
            foreach (var d in data)
            {
                GenerateClassFile(d.Key, d.Value, googleData);
            }
        }

        private static void GenerateClassFile(string className, Dictionary<string, List<AbstractDataRow>> data,
            GoogleSheetData googleData) {
            var classBuilder = GetClassBuilder(googleData);
            googleData.CreateClassFolder();
            var dataRow = data.ElementAt(0).Value;
            var @class = classBuilder.GenerateClass(dataRow, className);
            var path = googleData.GetClassPath(className);
            File.WriteAllText(path, @class);

            AssetDatabase.ImportAsset(path.Replace("./", ""), ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
            Debug.Log(string.Format("Class <b>{0}</b> was successful generated", className));
        }

        public static bool GenerateData(Dictionary<string, Dictionary<string, List<AbstractDataRow>>> data,
            GoogleSheetData googleSheetData) {
            bool output = false;
            switch(googleSheetData.DataType) {
                case DataType.ScriptableObject:
                    output = GenerateSOPrefab(data, googleSheetData);
                    break;
                case DataType.XML:
                    output = GenerateTextPrefab(data, googleSheetData);
                    break;
            }
            return output;
        }

        private static bool GenerateSOPrefab(Dictionary<string, Dictionary<string, List<AbstractDataRow>>> data,
            GoogleSheetData googleSheetData) {
                var currentAssemblyName = "Assembly-CSharp";
                var soName = data.ElementAt(0).Key;
                var t = Type.GetType(string.Format("{0}.{1}, {2}", googleSheetData.Namespace, soName, currentAssemblyName));
                if (t == null)
                {
                    Debug.LogWarning(string.Format("Can't gennerate SO, because can't find <b>{0}</b> in assembly", soName));
                    return false;
                }
                if (t.BaseType != typeof(ScriptableObject)) { return false; }
                var keys = data.Keys.ToArray();
            var values = data.Values.ToArray();
            googleSheetData.CreateDataFolder();
            for (var i = 0; i < keys.Length; i++)
            {
                foreach (var val in values[i])
                {
                    if (string.IsNullOrEmpty(val.Key))
                        continue;
                    var so = ScriptableObject.CreateInstance(soName);
                    var path = string.Format("{0}/{1}.asset", googleSheetData.DataLocation.Replace("./", ""),
                        val.Key);

                    AssetDatabase.CreateAsset(so, path);
                    InitFields(so, val.Value);

                    path = AssetDatabase.GetAssetPath(so);
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    AssetDatabase.Refresh();
                    EditorUtility.SetDirty(so);
                    Debug.Log(string.Format("SO asset <b>{0}</b> was successful generated", val.Key));
                }
            }
            return true;
        }

        private static bool GenerateTextPrefab(Dictionary<string, Dictionary<string, List<AbstractDataRow>>> data,
            GoogleSheetData googleData) {
            
            googleData.CreateClassFolder();
            var className = data.Keys.ElementAt(0);

            var classBuilder = GetClassBuilder(googleData);
            var fullClassName = googleData.Namespace + "." + className;

            var dataRow = data.ElementAt(0).Value.ElementAt(0).Value;
            var @class = classBuilder.GenerateClass(dataRow, className);
            
            
            var assembly = CompileSource(@class);
            var instance = assembly.CreateInstance(fullClassName);

            foreach(var value in data.Values) {
                SerializeData(instance, value, googleData);
            }
            return true;
        }

        private static ClassBuilder GetClassBuilder(GoogleSheetData googleData) {
            switch(googleData.DataType) {
                case DataType.ScriptableObject:
                    return new ScriptableObjectBuilder(googleData);
                case DataType.XML:
                    return new ClassBuilder(googleData);
            }
            throw new InvalidDataException();
        }

        private static Assembly CompileSource(string source) {
            var provider = new CSharpCodeProvider();
            var param = new CompilerParameters();

            // Add specific assembly references
            param.ReferencedAssemblies.Add("./Library/UnityAssemblies/UnityEngine.dll");
            param.ReferencedAssemblies.Add("System.dll");

            // Generate a dll in memory
            param.GenerateExecutable = false;
            param.GenerateInMemory = true;

            // Compile the source
            var result = provider.CompileAssemblyFromSource(param, source);
            if(result.Errors.Count > 0) {
                var msg = new StringBuilder();
                foreach(CompilerError error in result.Errors) {
                    msg.AppendFormat("Error ({0}): {1}\r\n",
                        error.ErrorNumber, error.ErrorText);
                }
                throw new Exception(msg.ToString());
            }
            return result.CompiledAssembly;
        }

        private static void SerializeData(object instance, Dictionary<string, List<AbstractDataRow>> dataRow,
            GoogleSheetData googleData) {
            if(instance == null) {
                Debug.Log("Instance is null");
                return;
            }
            var serializer = GetSerializer(googleData.DataType);
            foreach(var concreteData in dataRow) {
                var concreteDataName = concreteData.Key;
                instance = InitValues(instance, concreteData.Value, googleData.VariableType);
                var serializedClass = serializer.Serialize(instance);
                var path = googleData.GetDataPath(concreteDataName);
                File.WriteAllText(path,
                    serializedClass);
                G2UConfig.Instance.ConstantsClassBuilder.UpdateDataLocation(path);

                AssetDatabase.ImportAsset(path.Replace("./", ""), ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();
            }
        }

        private static AbstractSerializer GetSerializer(DataType dataType) {
            switch(dataType) {
                case DataType.ScriptableObject:
                    return new ScriptableObjectSerializer();
                case DataType.XML:
                    return new XMLSerializer();
            }
            throw new InvalidDataException();
        }

        private static object InitValues(object @object, List<AbstractDataRow> dataList, VariableType varType) {
            switch(varType) {
                case VariableType.Field:
                    return InitFields(@object, dataList);
                case VariableType.Property:
                    return InitProperty(@object, dataList);
            }
            return null;
        }

        private static object InitFields(object @object, List<AbstractDataRow> dataList) {
            var type = @object.GetType();
            foreach(var row in dataList) {
                if(row.SkipDataRow()) { continue; }
                var field = type.GetField(row.ParameterName);
                if(field != null) {
                    var genericListType = typeof(AbstractDataRow.Test<>);
                    var specificListType = genericListType.MakeGenericType(row.ParameterType);
                    var Test = Activator.CreateInstance(specificListType);
                    var methodName = "GetData";
                    if(row.IsArray) {
                        methodName = "GetDataArray";
                    }
                    var m = Test.GetType().GetMethod(methodName);
                    var output = m.Invoke(Test, new object[] {row.Data});
                    try {
                        field.SetValue(@object, output);
                    }
                    catch {
                        Debug.LogErrorFormat("Can't set <b>{0}</b> value to <b>{1}</b>",output,  @object);
                    }
                }
            }
            return @object;
        }

        private static object InitProperty(object @object, List<AbstractDataRow> dataList) {
            var type = @object.GetType();
            foreach(var row in dataList) {
                if(row.SkipDataRow()) { continue; }
                var field = type.GetProperty(row.ParameterName);
                if(field != null) {
                    var genericListType = typeof(AbstractDataRow.Test<>);
                    var specificListType = genericListType.MakeGenericType(row.ParameterType);
                    var Test = Activator.CreateInstance(specificListType);
                    var methodName = "GetData";
                    if(row.IsArray) {
                        methodName = "GetDataArray";
                    }
                    var m = Test.GetType().GetMethod(methodName);
                    var output = m.Invoke(Test, new object[] {row.Data});
                    field.SetValue(@object, output, null);
                }
            }
            return @object;
        }

        public static string GetTabulator(byte count) {
            var sb = new StringBuilder(count);
            for(var i = 0; i < count; i++) {
                sb.Append("\t");
            }
            return sb.ToString();
        }
    }

    public class ClassBuilder {
        protected GoogleSheetData _googleData;
        protected string _className;
        protected readonly VariableType _varibleType;

        public ClassBuilder(GoogleSheetData googleData) {
            _googleData = googleData;
        }

        public string GenerateClass(List<AbstractDataRow> data, string className) {
            _className = PathManager.PrepareFileName(className, true);
            data = data.Where(row => !string.IsNullOrEmpty(row.ParameterName)).ToList();
            var sb = new StringBuilder();
            sb.Append(GetFileStart());
            sb.Append(GetEnumDeclaration(data));
            sb.Append(GetFileData(data));
            sb.Append(GenerateLoadingMethods(data));
            sb.Append(GetFileEnd());
            return sb.ToString();
        }

        protected StringBuilder GetFileData(List<AbstractDataRow> data) {
            var sb = new StringBuilder();
            for(var i = 0; i < data.Count; i++) {
                sb.Append(data[i].GetRowString());
            }
            return sb;
        }

        protected virtual StringBuilder GetFileStart() {
            var sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using System;");
            sb.AppendLine(string.Format("namespace {0} {{", _googleData.Namespace));
            sb.AppendLine(string.Format("{0}[Serializable]public class {1} {{",
                FileBuilder.GetTabulator(1),
                _className));
            return sb;
        }

        protected virtual StringBuilder GetEnumDeclaration(List<AbstractDataRow> data) {
            StringBuilder sb = new StringBuilder();

            if (!(_googleData.GenerateGetMethod && _googleData.GetMethodType == ClassDataType.Enum)) {
                return sb;
            }
            sb.AppendLine(FileBuilder.GetTabulator(2) + "public enum " + _googleData.EnumName + " {");
            for(int i = 0; i < data.Count; i++) {
                sb.AppendLine(FileBuilder.GetTabulator(3) + data[i].ParameterName + ",");
            }
            sb.AppendLine(FileBuilder.GetTabulator(2) + "}");
            return sb;

        }

        private StringBuilder GenerateLoadingMethods(List<AbstractDataRow> data) {
            var sb = new StringBuilder();
            if(!_googleData.GenerateGetMethod) return sb;

            sb.AppendLine(GenerateLoadingClass("GetString",
                row => !row.IsArray && row.ParameterType == typeof(string) && !row.SkipDataRow(), typeof(string), data));

            sb.AppendLine(GenerateLoadingClass("GetStringArray",
                row => row.IsArray && row.ParameterType == typeof(string) && !row.SkipDataRow(), typeof(string[]), data));

            sb.AppendLine(GenerateLoadingClass("GetInt",
                row => !row.IsArray && row.ParameterType == typeof(int) && !row.SkipDataRow(), typeof(int), data));

            sb.AppendLine(GenerateLoadingClass("GetIntArray",
                row => row.IsArray && row.ParameterType == typeof(int) && !row.SkipDataRow(), typeof(int[]), data));

            sb.AppendLine(GenerateLoadingClass("GetFloat",
                row => !row.IsArray && row.ParameterType == typeof(float) && !row.SkipDataRow(), typeof(float), data));

            sb.AppendLine(GenerateLoadingClass("GetFloatArray",
                row => row.IsArray && row.ParameterType == typeof(float) && !row.SkipDataRow(), typeof(float[]), data));

            sb.AppendLine(GenerateLoadingClass("GetLong",
                row => !row.IsArray && row.ParameterType == typeof(long) && !row.SkipDataRow(), typeof(long), data));

            sb.AppendLine(GenerateLoadingClass("GetLongArray",
                row => row.IsArray && row.ParameterType == typeof(long) && !row.SkipDataRow(), typeof(long[]), data));

            sb.AppendLine(GenerateLoadingClass("GetBool",
                row => !row.IsArray && row.ParameterType == typeof(bool) && !row.SkipDataRow(), typeof(bool), data));

            sb.AppendLine(GenerateLoadingClass("GetBoolArray",
                row => row.IsArray && row.ParameterType == typeof(bool) && !row.SkipDataRow(), typeof(bool[]), data));

            return sb;
        }

        private string GenerateLoadingClass(string methodName, Func<AbstractDataRow, bool> isValid, Type returnType,
            List<AbstractDataRow> data) {
            if(!data.Any(isValid)) { return ""; }
            var sb = new StringBuilder();
            sb.Append("\r\n");

            sb.AppendLine(string.Format("{0}public {1} {2}({3} key) {{",
                FileBuilder.GetTabulator(2), returnType, methodName, _googleData.GetMethodType == ClassDataType.String ? "string" : _googleData.EnumName));

            if(_googleData.GetMethodType == ClassDataType.String) {
                sb.AppendLine(FileBuilder.GetTabulator(3) + "key = key.ToLower();");
                sb.AppendLine(FileBuilder.GetTabulator(3) + "switch(key){");
                for(var i = 0; i < data.Count; i++) {
                    if(!isValid(data[i])) { continue; }
                    sb.AppendLine(string.Format("{0}case \"{1}\":", FileBuilder.GetTabulator(4),
                        data[i].ParameterName.ToLower()));
                    sb.AppendLine(string.Format("{0}return {1};", FileBuilder.GetTabulator(5), data[i].ParameterName));
                }
            }
            else {
                sb.AppendLine(FileBuilder.GetTabulator(3) + "switch(key){");
                for (var i = 0; i < data.Count; i++)
                {
                    if (!isValid(data[i])) { continue; }
                    sb.AppendLine(string.Format("{0}case {1}:", FileBuilder.GetTabulator(4),
                        _googleData.EnumName + "." + data[i].ParameterName));
                    sb.AppendLine(string.Format("{0}return {1};", FileBuilder.GetTabulator(5), data[i].ParameterName));
                }
            }
            sb.AppendLine(FileBuilder.GetTabulator(4) + "default:");
            sb.AppendLine(string.Format("{0}Debug.LogWarning(\"Can't find key <b>\" + {1} + \"</b>\");",
                FileBuilder.GetTabulator(5), "key"));
            sb.AppendLine(string.Format("{0}return {1};", FileBuilder.GetTabulator(5), GetDefault(returnType)));
            sb.AppendLine(FileBuilder.GetTabulator(3) + "}");
            sb.AppendLine(FileBuilder.GetTabulator(2) + "}");
            return sb.ToString();
        }

        protected virtual StringBuilder GetFileEnd() {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("{0}}}", FileBuilder.GetTabulator(1)));
            sb.AppendLine("}");
            return sb;
        }

        public static string GetDefault(Type type) {
            if(type == typeof(bool))
                return "false";
            if(type.IsValueType) {
                return Activator.CreateInstance(type).ToString();
            }
            if(type == typeof(string[]) || type == typeof(int[]) || type == typeof(float[]) || type == typeof(long[])) {
                return "null";
            }
            return "string.Empty";
        }

//        private string GenerateLoadingClass() {
//            var sb = new StringBuilder();
//            sb.Append("\r\n");
//            sb.AppendLine(string.Format("{0}public static {1} Load{1}(string path) {{",
//                FileBuilder.GetTabulator(2), _className));
//            sb.AppendLine(string.Format("{0}var configAsset = Resources.Load(path) as TextAsset;",
//                FileBuilder.GetTabulator(3)));
//            sb.AppendLine(string.Format("{0}var configText = configAsset.text;",
//                FileBuilder.GetTabulator(3)));
//            sb.AppendLine(string.Format("{0}if(string.IsNullOrEmpty(configText)) return null;",
//                FileBuilder.GetTabulator(3)));
//            sb.AppendLine(string.Format("{0}// You can change deserialize function below",
//                FileBuilder.GetTabulator(3)));
//            sb.AppendLine(string.Format("{0}var config = configText.DeserializeFromXMLString<{1}>();",
//                FileBuilder.GetTabulator(3),
//                _className));
//            sb.AppendLine(string.Format("{0}return config;", FileBuilder.GetTabulator(3)));
//            sb.AppendLine(string.Format("{0}}}", FileBuilder.GetTabulator(2)));
//            return sb.ToString();
//        }
    }

    public class ScriptableObjectBuilder : ClassBuilder {
        public ScriptableObjectBuilder(GoogleSheetData googleData) : base(googleData) {}

        protected override StringBuilder GetFileStart() {
            var sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine(string.Format("namespace {0} {{", _googleData.Namespace));
            sb.AppendLine(string.Format("{0}public class {1} : ScriptableObject {{",
                FileBuilder.GetTabulator(1), _className));
            return sb;
        }
    }

    public abstract class AbstractDataRow {
        public abstract FileBuilder.DataRowType DataRowType { get; }
        protected readonly DataType _dataType;
        public string ParameterName { get; set; }
        public Type ParameterType { get; set; }
        public string[] Data { get; set; }
        public bool IsArray { get; set; }
        public string Comment { get; set; }

        protected AbstractDataRow(string parameterName, string[] data, string comment, string arraySeparator,
            DataType dataType) {
            if(parameterName == null) { return; }
            ParameterName = PathManager.PrepareFileName(parameterName, true);
            Comment = comment;
            _dataType = dataType;
            if(data.Length == 0) {
                ParameterType = typeof(string);
                return;
            }
            data = TypeManager.PrepareArrayData(data, arraySeparator);
            ParameterType = TypeManager.GetPropertyType(data[0]);
            IsArray = data.Length > 1;
            Data = data;
        }

        public abstract string GetRowString();

        public override string ToString() {
            return string.Format("Name: {0}, Type: {1}, IsArray: {2}", ParameterName, ParameterType, IsArray);
        }

        public class Test<T> {
            public T data;

            public T[] GetDataArray(string[] data) {
                var _data = new T[data.Length];
                for(var i = 0; i < data.Length; i++) {
                    _data[i] = (T) Convert.ChangeType(data[i], typeof(T));
                }
                return _data;
            }

            public T GetData(string[] data) {
                var _data = default(T);
                _data = (T) Convert.ChangeType(data[0], typeof(T));
                return _data;
            }
        }
    }

    public class ClassDataRow : AbstractDataRow {
        public override FileBuilder.DataRowType DataRowType {
            get { return FileBuilder.DataRowType.Class; }
        }

        private readonly AccessModifiers _fieldAccessModifier;
        private readonly AccessModifiers _setAccessModifier;
        private readonly VariableType _variableType;

        public ClassDataRow(string parameterName, string[] data, string comment, string arraySeparator,
            AccessModifiers fieldAccessModifier, AccessModifiers setAccessModifier,
            VariableType varType, DataType dataType)
            : base(parameterName, data, comment, arraySeparator, dataType) {
            _fieldAccessModifier = fieldAccessModifier;
            _setAccessModifier = setAccessModifier;
            _variableType = varType;
        }

        public override string GetRowString() {
            var sb = new StringBuilder();
            sb.Append(GetCommentData(Comment, FileBuilder.GetTabulator(2)));
            sb.Append(GetTooltip(Comment));
            sb.Append(FileBuilder.GetTabulator(2));
            sb.Append(string.Format("{0} {1}{2}", GetFieldAccessModifier(), ParameterType, (IsArray ? "[]" : "")));
            sb.Append(string.Format(" {0}", ParameterName));
            if(_variableType == VariableType.Property) {
                sb.Append(string.Format(" {{ get; {0}set; }}\r\n", GetSetAccessModifier()));
            }
            if(_variableType == VariableType.Field) {
                sb.Append(";\r\n");
            }
            return sb.ToString();
        }

        private string GetTooltip(string comment) {
            if(string.IsNullOrEmpty(comment) || _dataType != DataType.ScriptableObject) { return ""; }
            return string.Format("{0}[Tooltip(\"{1}\")]\r\n", FileBuilder.GetTabulator(2), comment);
        }

        private string GetSetAccessModifier() {
            if(_setAccessModifier == AccessModifiers.Public) {
                return "";
            }
            if(_variableType == VariableType.Property) {
                if((_setAccessModifier == AccessModifiers.Private || _setAccessModifier == AccessModifiers.Protected) &&
                   _fieldAccessModifier == AccessModifiers.Private) {
                    return "";
                }
                if(_fieldAccessModifier == AccessModifiers.Protected && _setAccessModifier == AccessModifiers.Protected) {
                    return "";
                }
            }
            var modifier = _setAccessModifier.ToString().ToLower() + " ";
            return modifier;
        }

        private string GetFieldAccessModifier() {
            return _fieldAccessModifier.ToString().ToLower();
        }

        private string GetCommentData(string data, string tabulator) {
            if(string.IsNullOrEmpty(data)) { return "\r\n"; }
            var sb = new StringBuilder();
            sb.AppendLine("\r\n" + tabulator + "/// <summary>");
            sb.AppendLine(tabulator + "/// " + data);
            sb.AppendLine(tabulator + "/// </summary>");
            return sb.ToString();
        }
    }

    public class SpaceDataRow : AbstractDataRow {
        public override FileBuilder.DataRowType DataRowType {
            get { return FileBuilder.DataRowType.Space; }
        }

        private readonly string _header;
        private const int SpaceSize = 10;

        public SpaceDataRow(string name) : base(null, null, null, null, DataType.ScriptableObject) {
            _header = name.Replace(G2UConfig.Instance.SkipRowPrefix, "");
        }

        public override string GetRowString() {
            if(string.IsNullOrEmpty(_header)) {
                return string.Format("\r\n{0}[Space({1})]", FileBuilder.GetTabulator(2), SpaceSize);
            }
            return string.Format("\r\n{0}[Space({1}, order = 1), Header(\"{2}\", order = 2)]",
                FileBuilder.GetTabulator(2), SpaceSize, _header);
        }
    }

    public abstract class AbstractSerializer {
        public abstract string Serialize(object t);
    }

    public class XMLSerializer : AbstractSerializer {
        public override string Serialize(object t) {
            return (SerializeObject(t));
        }

        private string SerializeObject(object pObject) {
            var memoryStream = new MemoryStream();
            var xs = new XmlSerializer(pObject.GetType());
            var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            xs.Serialize(xmlTextWriter, pObject);
            memoryStream = (MemoryStream) xmlTextWriter.BaseStream;
            var XmlizedString = UTF8ByteArrayToString(memoryStream.ToArray());
            return XmlizedString;
        }

        private string UTF8ByteArrayToString(byte[] characters) {
            var encoding = new UTF8Encoding();
            var constructedString = encoding.GetString(characters);
            return (constructedString);
        }
    }

    public class BinarySerializer : AbstractSerializer {
        public override string Serialize(object t) {
            using(var stream = new MemoryStream()) {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, t);
                stream.Flush();
                stream.Position = 0;
                return Convert.ToBase64String(stream.ToArray());
            }
        }
    }

    public class ScriptableObjectSerializer : AbstractSerializer {
        public override string Serialize(object t) {
            var so = ScriptableObject.CreateInstance(t.GetType());
            AssetDatabase.CreateAsset(so, "Assets/GameConfig.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return "";
        }
    }
}