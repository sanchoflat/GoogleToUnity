using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;


namespace G2U {
    public abstract class AbstractFileBuilder {
        public enum DataType {
            JSON, 
            XML
        }

        protected G2UConfig _config;
        
        protected AbstractFileBuilder(G2UConfig config) {
            _config = config;
        }

        public static AbstractFileBuilder GetClassBuilder(G2UConfig config) {
            return new ClassBuilder(config);
        }

        public static AbstractFileBuilder GetDataBuilder(G2UConfig config, DataType dataType) {
            switch(dataType) {
                case DataType.JSON:
                    return new JsonBuilder(config);
                case DataType.XML:
                    return new XmlBuilder(config);
            }
            throw new ArgumentException("Invalid dataType: " + dataType);
        }

        public Dictionary<string, string> GenerateFiles(List<Dictionary<string, string>> data) {
            var _parsedFileData = PrepareParsedFileData(data);
            var className = data[0].ElementAt(0).Key;
            return GenerateFiles(_parsedFileData, className);
        }

        protected Dictionary<string, List<AbstractDataRow>> PrepareParsedFileData(
         List<Dictionary<string, string>> data)
        {

            var keys = GetKeys(data);
            var dictionaryData = new Dictionary<string, List<AbstractDataRow>>();
            for (var i = 1; i < keys.Count; i++)
            {
                if (SkipColumn(keys[i])) { continue; }
                dictionaryData.Add(keys[i], new List<AbstractDataRow>());
            }
            bool addArrayValue = false;

            for (var j = 1; j < keys.Count; j++)
            {
                for (var i = 0; i < data.Count; i++)
                {
                    var comment = "";
                    data[i].TryGetValue(_config.CommentColumnTitle, out comment);

                    var d = data[i];
                    var key = keys[j];
                    var rowName = data[i][keys[0]];


                    if (SkipRow(rowName)) { continue; }
                    if (SkipColumn(keys[j])) { continue; }
                    if (string.IsNullOrEmpty(data[i][keys[j]]))
                        continue;

                    var dataList = new List<string>();

                    if (i < data.Count - 1)
                    {
                        while (true)
                        {
                            dataList.Add(data[i][key]);
                            var nextData = data[i + 1];
                            if (string.IsNullOrEmpty(nextData[keys[0]]) && !string.IsNullOrEmpty(nextData[key]))
                            {
                                addArrayValue = true;
                                i += 1;
                                if (i == data.Count - 1)
                                {
                                    dataList.Add(data[i][key]);
                                    break;
                                }
                            }
                            else
                            {
                                if (addArrayValue)
                                {
                                    addArrayValue = false;
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        dataList.Add(data[i][key]);
                    }
                    var dataRow = GetRowData(rowName, dataList.ToArray(), comment);
                    dictionaryData[key].Add(dataRow);
                }
            }
            return dictionaryData;
        }

        protected abstract string GenerateFile(List<AbstractDataRow> data);
        
        protected abstract Dictionary<string, string> GenerateFiles(
            Dictionary<string, List<AbstractDataRow>> data, string className);

     

        protected abstract AbstractDataRow GetRowData(string parameterName, string[] data, string comment);

        private List<string> GetKeys(List<Dictionary<string, string>> data) {
            var _keys = new List<string>();
            foreach(var key in data[0].Keys) {
                _keys.Add(key);
            }
            return _keys;
        }

        protected virtual string GetFileStart() {
            return "";
        }

        protected virtual string GetFileData(List<Dictionary<string, string>> data) {
            return "";
        }

        protected virtual string GetFileEnd() {
            return "";
        }


        protected bool SkipColumn(string columnName) {
            return columnName.Equals(_config.CommentColumnTitle);
        }

        protected bool SkipRow(string rowName)
        {
            return rowName.Contains(_config.SkipRowPrefix);
        }

        public static string GetTabulator(int count)
        {
            var sb = new StringBuilder(count);
            for (var i = 0; i < count; i++)
            {
                sb.Append("\t");
            }
            return sb.ToString();
        }
     
     
    }

    public class JsonBuilder : AbstractFileBuilder {
        public JsonBuilder(G2UConfig config) : base(config) {}

      
        protected override AbstractDataRow GetRowData(string parameterName, string[] data, string comment) {
            return new JSONDataRow(parameterName, data, comment);
        }

        protected override Dictionary<string, string> GenerateFiles(
       Dictionary<string, List<AbstractDataRow>> data, string className)
        {
            var output = new Dictionary<string, string>();
            foreach (var keyPair in data)
            {
                output.Add(keyPair.Key, GenerateFile(keyPair.Value));
            }
            return output;
        }


        protected override string GenerateFile(List<AbstractDataRow> data) {
            var sb = new StringBuilder();
            sb.Append(GetFileStart());
            for(var i = 0; i < data.Count; i++) {
                sb.Append(data[i].GetRowString());
                if(i < data.Count - 1) {
                    sb.Append(",");
                }
            }
            sb.Append(GetFileEnd());
            return sb.ToString();
        }

       

        protected override string GetFileStart() {
            return "{";
        }

        protected override string GetFileEnd() {
            return "}";
        }
    }

    public class ClassBuilder : AbstractFileBuilder {
        private string _className;

        public ClassBuilder(G2UConfig config)
            : base(config) {}

        protected override AbstractDataRow GetRowData(string parameterName, string[] data, string comment) {
            return new ClassDataRow(parameterName, data, comment);
        }

     
        protected override Dictionary<string, string> GenerateFiles(
            Dictionary<string, List<AbstractDataRow>> data, string className)
        {
            var output = new Dictionary<string, string>();
            var firstEl = data.ElementAt(0);
            _className = className;
            output.Add(_className, GenerateFile(firstEl.Value));
            return output;
        }

        protected override string GenerateFile(List<AbstractDataRow> data) {
            var sb = new StringBuilder();
            sb.Append(GetFileStart());
            for(var i = 0; i < data.Count; i++) {
                sb.Append(data[i].GetRowString());
            }
            sb.Append(GenerateLoadingClass());
            sb.Append(GetFileEnd());
            return sb.ToString();
        }

        protected override string GetFileStart() {
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine(string.Format("namespace {0} {{", _config.Namespace));
            sb.AppendLine(string.Format("{0}public class {1} {{", GetTabulator(1), _className));
            return sb.ToString();
        }

        protected override string GetFileEnd() {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("{0}}}", GetTabulator(1)));
            sb.AppendLine("}");
            return sb.ToString();
        }

        private string GenerateLoadingClass() {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n");
            sb.AppendLine(string.Format("{0}public static {1} Load{1}(string path) {{", GetTabulator(2), _className));
            sb.AppendLine(string.Format("{0}var config = Resources.Load(path) as TextAsset;", GetTabulator(3)));
            sb.AppendLine(string.Format("{0}if(string.IsNullOrEmpty(config.text)) return null;", GetTabulator(3)));
            sb.AppendLine(string.Format("\n{0}//", GetTabulator(3)));
            sb.AppendLine(string.Format("{0}// Please past here deserealize function", GetTabulator(3)));
            sb.AppendLine(string.Format("{0}//\n", GetTabulator(3)));
            sb.AppendLine(string.Format("{0}return null;", GetTabulator(3)));
            sb.AppendLine(string.Format("{0}}}", GetTabulator(2)));
            return sb.ToString();
        }

    }

    public class XmlBuilder : AbstractFileBuilder {
        public XmlBuilder(G2UConfig config) : base(config) {}

        protected override string GenerateFile(List<AbstractDataRow> data) {
            throw new NotImplementedException();
        }

     
        protected override Dictionary<string, string> GenerateFiles(Dictionary<string, List<AbstractDataRow>> data, string className) {
            throw new NotImplementedException();
        }

        protected override AbstractDataRow GetRowData(string parameterName, string[] data, string comment) {
            throw new NotImplementedException();
        }
    }

    public abstract class AbstractDataRow {
        public string ParameterName { get; private set; }
        public string ParameterType { get; private set; }
        public string[] Data { get; private set; }
        public bool IsArray { get; private set; }
        public string Comment { get; private set; }

        public AbstractDataRow(string parameterName, string[] data, string comment) {
            ParameterName = parameterName;
            Data = data;
            if(data.Length > 0) {
                ParameterType = GetDataType(Data[0]);
            }
            else {
                ParameterType = "string";
            }
            IsArray = data.Length > 1;
            Comment = comment;
        }

        public abstract string GetRowString();

        protected string GetDataType(string data) {
            return GetPropertyType(data);
        }

       

        #region Get Property type

        protected string GetPropertyType(string data) {
            var str = data;
            if(CheckForBool(str)) {
                return "bool";
            }
            if(CheckForFloat(str)) {
                return "float";
            }
            if(CheckForInt(str)) {
                return "int";
            }
            return "string";
        }

        private bool CheckForBool(string str) {
            return str.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                   str.Equals("false", StringComparison.OrdinalIgnoreCase);
        }

        private bool CheckForFloat(string str) {
            if(str.Contains(".") || str.Contains(",")) {
                float value;
                if(float.TryParse(str, NumberStyles.Float, new NumberFormatInfo(), out value)) {
                    return true;
                }
            }
            return false;
        }

        private bool CheckForInt(string str) {
            int value;
            if(int.TryParse(str, NumberStyles.Integer, new NumberFormatInfo(), out value)) {
                return true;
            }
            return false;
        }

        #endregion
    }

    public class JSONDataRow : AbstractDataRow {
        public JSONDataRow(string parameterName, string[] data, string comment) : base(parameterName, data, comment) {}
      
        public override string GetRowString() {
            StringBuilder sb = new StringBuilder();
            sb.Append("\"");
            sb.Append(ParameterName);
            sb.Append("\": ");
            sb.Append(GetValueFormat());

            return sb.ToString();
        }

        private string GetValueFormat() {
            var sb = new StringBuilder();
            if(IsArray) {
                sb.Append("[");
            }
            for(var i = 0; i < Data.Length; i++) {
                sb.Append(GetValueFormat(Data[i], ParameterType));
                if(i != Data.Length - 1) {
                    sb.Append(", ");
                }
            }
            if(IsArray) {
                sb.Append("]");
            }
            return sb.ToString();
        }

        private string GetValueFormat(string value, string type) {
            switch(type) {
                case "bool":
                    return value.ToLower();
                case "string":
                    value = value.Replace("\n", "\\n");
                    return "\"" + value + "\"";
            }
            return value;
        }




    }
    
    public class ClassDataRow : AbstractDataRow {
        public ClassDataRow(string parameterName, string[] data, string comment) : base(parameterName, data, comment) {}

        public override string GetRowString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetCommentData(Comment, AbstractFileBuilder.GetTabulator(2)));
            sb.Append(AbstractFileBuilder.GetTabulator(2));
            sb.Append("public ");
            sb.Append(ParameterType + (IsArray ? "[]": ""));
            sb.Append(" ");
            sb.Append(ParameterName);
            sb.Append(" {get; private set;}\n");
            return sb.ToString();
        }


        private string GetCommentData(string data, string tabulator) {
            if(string.IsNullOrEmpty(data)) return "";
            var sb = new StringBuilder();
            sb.AppendLine("\n" + tabulator + "/// <summary>");
            sb.AppendLine(tabulator + "/// " + data);
            sb.AppendLine(tabulator + "/// </summary>");
            return sb.ToString();
        }


        protected string GetProperty(string name, string value, bool array)
        {
            var sb = new StringBuilder();
            sb.Append("public ");
            //            sb.Append(GetPropertyType(value) + (array ? "[] " : " "));
            sb.Append(name);
            sb.Append(" { get; private set; }");
            sb.AppendLine("");
            return sb.ToString();
        }

    }
}