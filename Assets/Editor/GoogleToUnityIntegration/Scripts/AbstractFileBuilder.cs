using System;
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

        public static bool GenerateSOPrefab(string soName, Dictionary<string, List<AbstractDataRow>> data,
            string @namespace) {
            var currentAssemblyName = "Assembly-CSharp";
            var t = Type.GetType(string.Format("{0}.{1}, {2}", @namespace, soName, currentAssemblyName));
            if(t == null) {
                Debug.LogWarning(string.Format("Can't gennerate SO, because can't find <b>{0}</b> in assembly", soName));
                return false;
            }
            if(t.BaseType != typeof(ScriptableObject)) { return false; }
            var da = G2UConfig.Instance.GoogleSheetData;
            var i = 0;
            foreach(var d in data) {
                var so = ScriptableObject.CreateInstance(soName);
                AssetDatabase.CreateAsset(so,
                    string.Format("{0}/{1}.asset", da[i++].DataLocation.Replace("./", ""),
                        d.Key));
                InitFields(so, d.Value);
                Debug.Log(string.Format("SO asset <b>{0}</b> was successful generated", d.Key));
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return true;
        }

        public static void Generate(Dictionary<string, Dictionary<string, List<AbstractDataRow>>> data) {
            if(data == null) { return; }
            var googleData = G2UConfig.Instance.GoogleSheetData;
            var counter = 0;
            foreach(var d in data) {
                SaveData(d.Key, d.Value, googleData[counter++]);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void SaveData(string className, Dictionary<string, List<AbstractDataRow>> data,
            GoogleSheetData googleData) {
            var classBuilder = GetClassBuilder(googleData);
            var fullClassName = googleData.Namespace + "." + className;
            googleData.CreateDataFolder();
            googleData.CreateClassFolder();
            var dataRow = data.ElementAt(0).Value;
            var @class = classBuilder.GenerateClass(dataRow, className);
            File.WriteAllText(googleData.GetClassPath(className), @class);
            if(googleData.DataType == DataType.ScriptableObject) { return; }
            var assembly = CompileSource(@class);
            var instance = assembly.CreateInstance(fullClassName);
            SaveConcreteData(instance, data, googleData);
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

        private static void SaveConcreteData(object instance, Dictionary<string, List<AbstractDataRow>> dataRow,
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
                File.WriteAllText(googleData.GetDataPath(concreteDataName),
                    serializedClass);
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
                    field.SetValue(@object, output);
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
            var sb = new StringBuilder();
            sb.Append(GetFileStart());
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

        protected virtual StringBuilder GetFileEnd() {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("{0}}}", FileBuilder.GetTabulator(1)));
            sb.AppendLine("}");
            return sb;
        }

        private string GenerateLoadingMethods(List<AbstractDataRow> data) {
            var sb = new StringBuilder();
            sb.AppendLine(GenerateLoadingClass("GetStringByName",
                row => !row.IsArray && row.ParameterType == typeof(string) && !row.SkipDataRow(), typeof(string), data));
            sb.AppendLine(GenerateLoadingClass("GetIntByName",
                row => !row.IsArray && row.ParameterType == typeof(int) && !row.SkipDataRow(), typeof(int), data));
            sb.AppendLine(GenerateLoadingClass("GetFloatByName",
                row => !row.IsArray && row.ParameterType == typeof(float) && !row.SkipDataRow(), typeof(float), data));
            sb.AppendLine(GenerateLoadingClass("GetLongByName",
                row => !row.IsArray && row.ParameterType == typeof(long) && !row.SkipDataRow(), typeof(long), data));
            return sb.ToString();
        }

        private string GenerateLoadingClass(string methodName, Func<AbstractDataRow, bool> isValid, Type returnType,
            List<AbstractDataRow> data) {
            if(!data.Any(isValid)) { return ""; }
            var sb = new StringBuilder();
            sb.Append("\r\n");
            sb.AppendLine(string.Format("{0}public {1} {2}(string key) {{",
                FileBuilder.GetTabulator(2), returnType, methodName));
            sb.AppendLine(FileBuilder.GetTabulator(3) + "key = key.ToLower();");
            sb.AppendLine(FileBuilder.GetTabulator(3) + "switch(key){");
            for(var i = 0; i < data.Count; i++) {
                if(!isValid(data[i])) { continue; }
                sb.AppendLine(string.Format("{0}case \"{1}\":", FileBuilder.GetTabulator(4),
                    data[i].ParameterName.ToLower()));
                sb.AppendLine(string.Format("{0}return {1};", FileBuilder.GetTabulator(5), data[i].ParameterName));
            }
            sb.AppendLine(FileBuilder.GetTabulator(4) + "default:");
            sb.AppendLine(string.Format("{0}Debug.Log(\"Can't find key <b>\" + {1} + \"</b>\");",
                FileBuilder.GetTabulator(5), "key"));
            sb.AppendLine(string.Format("{0}return {1};", FileBuilder.GetTabulator(5), GetDefault(returnType)));
            sb.AppendLine(FileBuilder.GetTabulator(3) + "}");
            sb.AppendLine(FileBuilder.GetTabulator(2) + "}");
            return sb.ToString();
        }

        public static string GetDefault(Type type) {
            if(type.IsValueType) {
                return Activator.CreateInstance(type).ToString();
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