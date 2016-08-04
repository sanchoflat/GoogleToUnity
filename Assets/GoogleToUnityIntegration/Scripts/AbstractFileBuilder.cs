using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using G2U;
using Microsoft.CSharp;
using UnityEngine;


namespace GoogleSheetIntergation {
    public enum DataType {
        JSON,
        XML,
        ScriptableObject
    }

    public abstract class AbstractFileBuilder {
        protected G2UConfig _config;

        protected AbstractFileBuilder(G2UConfig config) {
            _config = config;
        }

        public static AbstractFileBuilder GetClassBuilder(G2UConfig config, VariableType variableType) {
            return new ClassBuilder(config, variableType);
        }

        public static AbstractFileBuilder GetDataBuilder(G2UConfig config, DataType dataType) {
            switch(dataType) {
                case DataType.JSON:
                    return new JsonBuilder(config);
                case DataType.XML:
                    return new XmlBuilder(config);
                case DataType.ScriptableObject:
                    return new ScriptableObjectBuilder(config, VariableType.Field); 
            }
            throw new ArgumentException("Invalid dataType: " + dataType);
        }


        public Dictionary<string, string> GenerateFileList(List<Dictionary<string, string>> data) {
            
            // тут сгенерировали список <имя, список параметров>
            var _parsedFileData = PrepareParsedFileData(data);
            _parsedFileData = TypeManager.UpdateParsedFileData(_parsedFileData);

            bool @continue = false;
            var ca = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly asm in ca)
            {
                if(asm.GetType("G2U.GameConfig") != null) {
                    var instance = asm.CreateInstance("G2U.GameConfig");
                    instance = InitValues(instance, _parsedFileData["gameConfig"]);
                    Serialize(instance);
                    @continue = true;
                }
            };
            if(@continue) return null;


            // далее собираем класс (текстовое представление)
            var classBuilder = new ClassBuilder(_config, _config.VariableType);
            var @class = classBuilder.GenerateFileList(_parsedFileData);

            // далее необходимо сделать сборки из полученных классов
            var assembly = GetAssemblies(@class);
            
            assembly = InitValues(assembly, _parsedFileData);


            foreach (var value in assembly.Values)
            {
               
            }

            // после сборки, необходимо создать инстансы сборок
            // после заполняем их значениями из "списка параметров"
            // после необходимо сгенерировать файл с данными (сериализация в нужный формат)
            return GenerateFileList(_parsedFileData);
        }


        private void Serialize(object o) {
            XmlSerializer serial = new XmlSerializer(o.GetType());
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            try
            {
                using (FileStream fs = new FileStream("./test.xml", FileMode.Create, FileAccess.Write))
                {
                    using (XmlTextWriter tw = new XmlTextWriter(fs, Encoding.UTF8))
                    {
                        tw.Formatting = Formatting.Indented;
                        serial.Serialize(tw, o, ns);
                    }

                }
            }
            catch { }
        }

        protected Dictionary<string, List<AbstractDataRow>> PrepareParsedFileData(List<Dictionary<string, string>> data) {
            var keys = GetKeys(data);
            var dictionaryData = new Dictionary<string, List<AbstractDataRow>> {{keys[0], null}};
            for(var column = 1; column < keys.Count; column++) {
                var columnName = keys[column];
                if(SkipColumn(columnName)) { continue; }
                dictionaryData.Add(columnName, new List<AbstractDataRow>());
                for(var row = 0; row < data.Count; row++) {
                    var currentData = data[row];
                    var rowName = data[row][keys[0]];
                    if(SkipRow(rowName, currentData[columnName])) { continue; }
                    var dataList = new List<string>();
                    dataList.Add(currentData[columnName]);
                    if(row == data.Count - 1) { continue; }
                    var nextData = data[row + 1];

                    // if it's array parameter - try to get it
                    var oldRow = row;
                    while(true) {
                        if(IsArrayParameter(nextData, keys, columnName)) {
                            dataList.Add(nextData[columnName]);
                            row++;
                            if(row == data.Count - 1) { break; }
                            nextData = data[row + 1];
                        }
                        else {
                            break;
                        }
                    }
                    var dataRow = GetRowData(rowName, dataList.ToArray(), GetComment(data[oldRow]));
                    dictionaryData[columnName].Add(dataRow);
                }
            }
            return dictionaryData;
        }

        private string GetComment(Dictionary<string, string> row) {
            string comment;
            row.TryGetValue(_config.CommentColumnTitle, out comment);
            return comment;
        }

        private bool IsArrayParameter(Dictionary<string, string> nextData, List<string> keyList, string currentKey) {
            return string.IsNullOrEmpty(nextData[keyList[0]]) && !string.IsNullOrEmpty(nextData[currentKey]);
        }     

        private Dictionary<string, object> GetAssemblies(Dictionary<string, string> input) {
            var assemblies = new Dictionary<string, object>();
            foreach(var keyPair in input) {

                var assembly = CompileSource(keyPair.Value);
                var instance = assembly.CreateInstance("G2U.GameConfig");
                assemblies.Add(keyPair.Key, instance);
            }
            return assemblies;
        }

        private Assembly CompileSource(string source) {
            var provider = new CSharpCodeProvider();
            var param = new CompilerParameters();

            // Add ALL of the assembly references
            foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                param.ReferencedAssemblies.Add(assembly.Location);
            }

            // Add specific assembly references
            //param.ReferencedAssemblies.Add("System.dll");
            //param.ReferencedAssemblies.Add("CSharp.dll");
            param.ReferencedAssemblies.Add("./Library/UnityAssemblies/UnityEngine.dll");

            // Generate a dll in memory
            param.GenerateExecutable = false;
            param.GenerateInMemory = true;

            // Compile the source
            var result = provider.CompileAssemblyFromSource(param, source);
            if(result.Errors.Count > 0) {
                var msg = new StringBuilder();
                foreach(CompilerError error in result.Errors) {
                    msg.AppendFormat("Error ({0}): {1}\n",
                        error.ErrorNumber, error.ErrorText);
                }
                throw new Exception(msg.ToString());
            }
            return result.CompiledAssembly;
        }

        private Dictionary<string, object> InitValues(Dictionary<string, object> objects,
            Dictionary<string, List<AbstractDataRow>> dataList) {
            var keys = dataList.Keys.ToList();
            for(int i = 1; i < keys.Count; i++) {
                objects[keys[0]] = InitValues(objects[keys[0]], dataList[keys[i]]);
            }
        
            return objects;
        }

        private object InitValues(object @object, List<AbstractDataRow> dataList) {
            switch(_config.VariableType) {
                case VariableType.Field:
                    return InitFields(@object, dataList);
                case VariableType.Property:
                    return InitProperty(@object, dataList);
            }
            return null;
        }

        private object InitFields(object @object, List<AbstractDataRow> dataList) {
            var type = @object.GetType();
            foreach(var row in dataList) {
                type.GetField(row.ParameterName).SetValue(@object, row.Data.Data);
            }
            return @object;
        }
        private object InitProperty(object @object, List<AbstractDataRow> dataList)
        {
            var type = @object.GetType();
            foreach (var row in dataList)
            {
                type.GetProperty(row.ParameterName).SetValue(@object, row.Data.Data, null);
            }
            return @object;
        }
        protected abstract AbstractDataRow GetRowData(string parameterName, string[] data, string comment);

        protected abstract Dictionary<string, string> GenerateFileList(
            Dictionary<string, List<AbstractDataRow>> data);

        protected string GenerateFile(List<AbstractDataRow> data) {
            var sb = new StringBuilder();
            sb.Append(GetFileStart());
            sb.Append(GetFileData(data));
            sb.Append(GetFileEnd());
            return sb.ToString();
        }

        protected virtual StringBuilder GetFileStart() {
            return null;
        }

        protected virtual StringBuilder GetFileData(List<AbstractDataRow> data) {
            return null;
        }

        protected virtual StringBuilder GetFileEnd() {
            return null;
        }

        protected bool SkipColumn(string columnName) {
            return columnName.Equals(_config.CommentColumnTitle);
        }

        protected bool SkipRow(string rowName, string currentData) {
            if(rowName.Contains(_config.SkipRowPrefix)) { return true; }
            return string.IsNullOrEmpty(rowName) && string.IsNullOrEmpty(currentData);
        }

        private List<string> GetKeys(List<Dictionary<string, string>> data) {
            return data[0].Keys.ToList();
        }

        public static string GetTabulator(int count) {
            var sb = new StringBuilder(count);
            for(var i = 0; i < count; i++) {
                sb.Append("\t");
            }
            return sb.ToString();
        }
    }

    // ----------------------------------------------
    // Here you can and your own ...Builder.
    // You should create AbstractFileBuilder subclass and AbstractDataRow subclass
    // ----------------------------------------------

    public class JsonBuilder : AbstractFileBuilder {
        public JsonBuilder(G2UConfig config) : base(config) {}

        protected override AbstractDataRow GetRowData(string parameterName, string[] data, string comment) {
            return new JSONDataRow(parameterName, data, comment, _config.ArraySeparator);
        }

        protected override Dictionary<string, string> GenerateFileList(
            Dictionary<string, List<AbstractDataRow>> data) {
            var output = new Dictionary<string, string>();
            foreach(var keyPair in data) {
                if(keyPair.Value == null) { continue; }
                output.Add(keyPair.Key, GenerateFile(keyPair.Value));
            }
            return output;
        }

        protected override StringBuilder GetFileData(List<AbstractDataRow> data) {
            var sb = new StringBuilder();
            for(var i = 0; i < data.Count; i++) {
                sb.Append(data[i].GetRowString());
                if(i < data.Count - 1) {
                    sb.Append(",");
                }
            }
            return sb;
        }

        protected override StringBuilder GetFileStart() {
            return new StringBuilder("{");
        }

        protected override StringBuilder GetFileEnd() {
            return new StringBuilder("}");
        }
    }

    public class XmlBuilder : AbstractFileBuilder {
        public XmlBuilder(G2UConfig config) : base(config) {}

        private string _dataName;
        private string _configName;

        protected override Dictionary<string, string> GenerateFileList(Dictionary<string, List<AbstractDataRow>> data) {
            var output = new Dictionary<string, string>();
            _configName = data.ElementAt(0).Key;
            foreach(var keyPair in data) {
                if(keyPair.Value == null) { continue; }
                _dataName = keyPair.Key.Replace(" ", "");
                output.Add(_dataName, GenerateFile(keyPair.Value));
            }
            return output;
        }

        protected override StringBuilder GetFileData(List<AbstractDataRow> data) {
            var sb = new StringBuilder();
            for(var i = 0; i < data.Count; i++) {
                sb.Append(data[i].GetRowString());
            }
            return sb;
        }

        protected override StringBuilder GetFileStart() {
            return new StringBuilder(string.Format("<{0}>", _configName));
        }

        protected override StringBuilder GetFileEnd() {
            return new StringBuilder(string.Format("</{0}>", _configName));
        }

        protected override AbstractDataRow GetRowData(string parameterName, string[] data, string comment) {
            return new XMLDataRow(parameterName, data, comment, _config.ArraySeparator);
        }
    }

    public class ClassBuilder : AbstractFileBuilder {
        protected string _className;
        protected readonly VariableType _varibleType;

        public ClassBuilder(G2UConfig config, VariableType varibleType)
            : base(config) {
            _varibleType = varibleType;
        }

        protected override AbstractDataRow GetRowData(string parameterName, string[] data, string comment) {
            return new ClassDataRow(parameterName, data, comment, _config.ArraySeparator, _config.FieldAccessModifiers,
                _config.SetAccessModifiers,
                _varibleType);
        }

        protected override Dictionary<string, string> GenerateFileList(
            Dictionary<string, List<AbstractDataRow>> data) {
            var output = new Dictionary<string, string>();
            var firstEl = data.ElementAt(1);
            _className = PathManager.PrepareFileName(data.Keys.ElementAt(0), true);
            output.Add(_className, GenerateFile(firstEl.Value));
            return output;
        }

        protected override StringBuilder GetFileData(List<AbstractDataRow> data) {
            var sb = new StringBuilder();
            for(var i = 0; i < data.Count; i++) {
                sb.Append(data[i].GetRowString());
            }
            sb.Append(GenerateLoadingClass());
            return sb;
        }

        protected override StringBuilder GetFileStart() {
            var sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine(string.Format("namespace {0} {{", _config.Namespace));
            sb.AppendLine(string.Format("{0}public class {1} {{", GetTabulator(1), _className));
            return sb;
        }

        protected override StringBuilder GetFileEnd() {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("{0}}}", GetTabulator(1)));
            sb.AppendLine("}");
            return sb;
        }

        private string GenerateLoadingClass() {
            var sb = new StringBuilder();
            sb.Append("\r\n");
            sb.AppendLine(string.Format("{0}public static {1} Load{1}(string path) {{", GetTabulator(2), _className));
            sb.AppendLine(string.Format("{0}var configAsset = Resources.Load(path) as TextAsset;", GetTabulator(3)));
            sb.AppendLine(string.Format("{0}var configText = configAsset.text;", GetTabulator(3)));
            sb.AppendLine(string.Format("{0}if(string.IsNullOrEmpty(configText)) return null;", GetTabulator(3)));
            sb.AppendLine(string.Format("{0}// You can change deserialize function below", GetTabulator(3)));
            sb.AppendLine(string.Format("{0}var config = configText.DeserializeFromXMLString<{1}>();", GetTabulator(3),
                _className));
            sb.AppendLine(string.Format("{0}return config;", GetTabulator(3)));
            sb.AppendLine(string.Format("{0}}}", GetTabulator(2)));
            return sb.ToString();
        }
    }

    public class ScriptableObjectBuilder : ClassBuilder
    {
        public ScriptableObjectBuilder(G2UConfig config, VariableType varibleType) : base(config, varibleType) {}

        protected override StringBuilder GetFileStart()
        {
            var sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine(string.Format("namespace {0} {{", _config.Namespace));
            sb.AppendLine(string.Format("{0}public class {1} : ScriptableObject{{", GetTabulator(1), _className));
            return sb;
        }

    }
  
    public abstract class AbstractDataRow {
        public string ParameterName { get; set; }
        public Type ParameterType { get; set; }
        public D Data { get; set; }
        public bool IsArray { get; set; }
        public string Comment { get; set; }

        protected AbstractDataRow(string parameterName, string[] data, string comment, string arraySeparator) {
            ParameterName = PathManager.PrepareFileName(parameterName, true);
            Comment = comment;
            if(data.Length == 0) {
                ParameterType = typeof(string);
                return;
            }
            ParameterType = TypeManager.GetPropertyType(ref data, arraySeparator);
            Data = new D(data, ParameterType);
          
        }

        public abstract string GetRowString();

        public override string ToString() {
            return string.Format("Name: {0}, Type: {1}, IsArray: {2}", ParameterName, ParameterType, IsArray);
        }


        public struct D {
            private readonly object[] data;
            private bool isArray ;
            public object Data {
                get {
                    if (isArray)
                        return data;
                    return data[0];
                }
            }

           
            public D(string[] data, Type t) : this() {
                isArray = data.Length > 1;
                this.data = new object[data.Length];
                if (!isArray)
                {
                    this.data[0] = Convert.ChangeType(data[0], t);
                }
                else {
                    for(int i = 0; i < data.Length; i++) {
                        this.data[i] = Convert.ChangeType(data[i], t);
                    }
                }
            }
        }
    }

    public class JSONDataRow : AbstractDataRow {
        public JSONDataRow(string parameterName, string[] data, string comment, string arraySeparator)
            : base(parameterName, data, comment, arraySeparator) {}

        public override string GetRowString() {
            var sb = new StringBuilder();
            sb.Append("\"");
            sb.Append(ParameterName);
            sb.Append("\": ");
            sb.Append(GetValueFormat());
            return sb.ToString();
        }

        private string GetValueFormat() {
            var sb = new StringBuilder();
//            if(IsArray) {
//                sb.Append("[");
//            }
//            for(var i = 0; i < Data.Length; i++) {
//                sb.Append(GetValueFormat(Data[i], ParameterType));
//                if(i != Data.Length - 1) {
//                    sb.Append(", ");
//                }
//            }
//            if(IsArray) {
//                sb.Append("]");
//            }
            return sb.ToString();
        }

        private string GetValueFormat(string value, Type type) {
            if(type == TypeManager.BoolType) {
                return value.ToLower();
            }
            if(type == TypeManager.StringType) {
                value = value.Replace("\n", "\\n");
                return "\"" + value + "\"";
            }
            return value;
        }
    }

    public class XMLDataRow : AbstractDataRow {
        public XMLDataRow(string parameterName, string[] data, string comment, string arraySeparator)
            : base(parameterName, data, comment, arraySeparator) {}

        public override string GetRowString() {
            var sb = new StringBuilder();
            sb.Append(GetOpenTag(ParameterName));
            sb.Append(GetValueFormat());
            sb.Append(GetCloseTag(ParameterName));
            return sb.ToString();
        }

        private string GetValueFormat() {
            var sb = new StringBuilder();
//            var addTypeTag = IsArray;
//            for(var i = 0; i < Data.Length; i++) {
//                if(addTypeTag) {
//                    sb.Append(GetOpenTag(ParameterType.ToString()));
//                }
//                sb.Append(GetType(i));
//                if(addTypeTag) {
//                    sb.Append(GetCloseTag(ParameterType.ToString()));
//                }
//            }
            return sb.ToString();
        }
//
//        private string GetType(int counter) {
//            return ParameterType == TypeManager.BoolType ? Data[counter].ToLower() : Data[counter];
//        }

        private string GetOpenTag(string par) {
            return "<" + par + ">";
        }

        private string GetCloseTag(string par) {
            return "</" + par + ">";
        }
    }

    public class ClassDataRow : AbstractDataRow {
        private readonly AccessModifiers _fieldAccessModifier;
        private readonly AccessModifiers _setAccessModifier;
        private readonly VariableType _variableType;

        public ClassDataRow(string parameterName, string[] data, string comment, string arraySeparator,
            AccessModifiers fieldAccessModifier, AccessModifiers setAccessModifier,
            VariableType varType)
            : base(parameterName, data, comment, arraySeparator) {
            _fieldAccessModifier = fieldAccessModifier;
            _setAccessModifier = setAccessModifier;
            _variableType = varType;
        }

        public override string GetRowString() {
            var sb = new StringBuilder();
            sb.Append(GetCommentData(Comment, AbstractFileBuilder.GetTabulator(2)));
            sb.Append(AbstractFileBuilder.GetTabulator(2));

         
            sb.Append(string.Format("{0} {1}{2}", GetFieldAccessModifier(), ParameterType, (IsArray? "[]" : "")));
            sb.Append(string.Format(" {0}", ParameterName));
            if(_variableType == VariableType.Property) {
                sb.Append(string.Format(" {{ get; {0}set; }}\r\n", GetSetAccessModifier()));
            }
            if(_variableType == VariableType.Field) {
                sb.Append(";\r\n");
            }
            return sb.ToString();
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

}