using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Debug = UnityEngine.Debug;


namespace G2U {
    public class AbstractFileBuilder {
        protected const string COMMENT_KEY = "Comment";


        protected G2UConfig _config;
        protected GoogleSheetData _googleData;

        public AbstractFileBuilder(G2UConfig config, int counter) {
            _config = config;
            _googleData = _config.GoogleSheetData[counter];

        }

        public static AbstractFileBuilder GetClassBuilder(G2UConfig config, int counter)
        {
            return new ClassBuilder(config, counter);
        }

        public static AbstractFileBuilder GetJsonBuilder(G2UConfig config, int counter)
        {
            return new JsonBuilder(config, counter);
        }

        public virtual string GetEmptyFile() {
            return "";
        }

        public Dictionary<string, string> GenerateFiles(List<Dictionary<string, string>> data) {
            var _parsedFileData = PrepareParsedFileData(data);
            return GenerateFiles(_parsedFileData);
        }

        protected virtual Dictionary<string, string> GenerateFiles(
            Dictionary<string, List<string>> data) {
            return null;
        }


        protected virtual Dictionary<string, List<string>> PrepareParsedFileData(
            List<Dictionary<string, string>> data) {
            var keys = GetKeys(data);
            var dictionaryData = new Dictionary<string, List<string>>();
            for(var i = 0; i < keys.Count; i++) {
                dictionaryData.Add(keys[i], new List<string>());
            }
            foreach(var d in data) {
                foreach(var key in keys) {
                    dictionaryData[key].Add(d[key]);
                }
            }
            return dictionaryData;
        }

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

        protected string GetTabulator(int count) {
            var sb = new StringBuilder(count);
            for (var i = 0; i < count; i++) {
                sb.Append("\t");
            }
            return sb.ToString();
        }

        protected bool SkipColumn(string columnName, int columnNumber) {
            return columnName.Equals(COMMENT_KEY) || columnNumber == 0;
        }
        protected bool SkipRow(string rowName)
        {
            return rowName.Contains(_config.SkipRowPrefix);
        }
        #region Get Property type

        protected string GetPropertyType(string data) {
            var str = data;
            if (CheckForBool(str)) {
                return "bool";
            }
            if (CheckForFloat(str)) {
                return "float";
            }
            if (CheckForInt(str)) {
                return "int";
            }
            return "string";
        }

        private bool CheckForBool(string str) {
            return str.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                   str.Equals("false", StringComparison.OrdinalIgnoreCase);
        }

        private bool CheckForFloat(string str) {
            if (str.Contains(".") || str.Contains(",")) {
                float value;
                if (float.TryParse(str, NumberStyles.Float, new NumberFormatInfo(), out value)) {
                    return true;
                }
            }
            return false;
        }

        private bool CheckForInt(string str) {
            int value;
            if (int.TryParse(str, NumberStyles.Integer, new NumberFormatInfo(), out value)) {
                return true;
            }
            return false;
        }

        #endregion
    }


    public class JsonBuilder : AbstractFileBuilder {
        public JsonBuilder(G2UConfig config, int counter) : base(config, counter) { }

        public override string GetEmptyFile() {
            return "{}";
        }

        protected override Dictionary<string, string> GenerateFiles(Dictionary<string, List<string>> data) {
            Dictionary<string, string> output = new Dictionary<string, string>();
            var firstKey = data.Keys.ElementAt(0);
            for (int i = 1; i < data.Count; i++) {
                var fileKey = data.Keys.ElementAt(i);
                if (SkipColumn(fileKey, i)) continue;
                output.Add(fileKey, GenerateJsonFile(data[firstKey], data[fileKey]));
            }
            return output;
        }

        private string GenerateJsonFile(List<string> keyList, List<string> valueList) {
            var sb = new StringBuilder();
            var createArray = false;
            sb.Append(GetFileStart());
            for(var i = 0; i < keyList.Count; i++) {
                var key = GetKey(keyList[i]);
                var value = GetValue(valueList[i]);
                var line = new StringBuilder();
                if(SkipRow(key) || (string.IsNullOrEmpty(value) && _googleData.SkipEmptyLines)) {
                    continue;
                }
                if(i < keyList.Count - 1) {
                    var nextKey = keyList[i + 1];
                    if(string.IsNullOrEmpty(nextKey)) {
                        createArray = true;
                    }
                }

                line.Append(key + ": ");

                if(createArray) {
                    line = CreateArrayLine(line, value, keyList, valueList, ref i);
                    sb.Append(line);
                    createArray = false;
                    continue;
                }

                line.Append(value);
                if(AppendCommaAtTheEnd(i, valueList)) {
                    line.Append(",\n");
                }
                sb.Append(line);
            }
            sb.Append(GetFileEnd());
            return sb.ToString();
        }


        private bool AppendCommaAtTheEnd(int i, List<string> data) {
            return i < (data.Count - 1);
        }

        private StringBuilder CreateArrayLine(StringBuilder line, string value, List<string> keyList,
            List<string> valueList, ref int i) {
            line.Append("[");
            while(true) {
              
                line.Append(value);
                i += 1;
                if(i == valueList.Count) {
                    line.Append("]");
                    return line;
                }
                value = GetValue(valueList[i]);
                var keyFromList = keyList[i];
                if(string.IsNullOrEmpty(keyFromList)) {
                    line.Append(", ");
                }
                else {
                    line.Append("]");
                    if(AppendCommaAtTheEnd(i, valueList)) {
                        line.Append(",");
                    }
                    line.Append("\n");
                    return line;
                }
            }
        }

       

        protected override string GetFileStart() {
            return "{";
        }

        protected override string GetFileData(List<Dictionary<string, string>> data) {
            var sb = new StringBuilder();

            return sb.ToString();
        }

        protected override string GetFileEnd() {
            return "}";
        }

        private string GetKey(string key) {
            return "\"" + key + "\"";
        }

        private string GetValue(string value) {
//            value = value.TrimEnd(' ');
            var objectType = GetPropertyType(value);
            var data = value;
            var val = value.Trim(' ');
            if (objectType == "string") {
                data = "\"" + value + "\"";
            }
            if (objectType == "bool") {
                data = value.ToLower();
            }
            return data;
        }
    }

    public class ClassBuilder : AbstractFileBuilder {
        private string _className;

        public ClassBuilder(G2UConfig config, int counter)
            : base(config, counter)
        {
        }

        public override string GetEmptyFile() {
            return string.Format("namespace {0} {{\n    public class {1} {{}}\n}}", _className, _config.Namespace);
        }

        protected override string GetFileStart() {
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using UnityEngine;");
            //            sb.AppendLine("using System.Collections.Generic;");
            //            sb.AppendLine("using System.IO;");
            //            sb.AppendLine("using UnityEditor;");
            //            sb.AppendLine("using EternalMaze.EditorWindows;");

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


        protected override Dictionary<string, string> GenerateFiles(Dictionary<string, List<string>> data) {
            var output = new Dictionary<string, string>(1);
            var firstKey = data.Keys.ElementAt(0);
            var valuesKey = data.Keys.ElementAt(1);
            if (!_googleData.GenerateClassForEveryColumn) {
                _className = firstKey;
                output.Add(firstKey, GenerateClassFile(data[firstKey], data[valuesKey], GetCommentsList(data)));
            }
            else {
                for (var i = 1; i < data.Count; i++) {
                    valuesKey = data.Keys.ElementAt(i);
                    if(SkipColumn(valuesKey, i)) continue;
                    _className = valuesKey;
                    output.Add(_className, GenerateClassFile(data[firstKey], data[valuesKey], GetCommentsList(data)));
                }
            }

            return output;
        }


        private string GenerateClassFile(List<string> keys, List<string> values, List<string> comments) {
            var sb = new StringBuilder();
            sb.AppendLine(GetFileStart());
            for (var i = 0; i < keys.Count; i++) {
                if (SkipRow(keys[i])) continue;
                var comment = GetComment(comments, i);
                if (!string.IsNullOrEmpty(comment)) {
                    sb.Append(GetCommentData(comment, GetTabulator(2)));
                }


                var keyTmp = keys[i];
                var valueTmp = values[i];
                if(i < keys.Count - 1) {
                    var nextKey = keys[i + 1];
                    var nextValue = values[i + 1];
                    if ((!string.IsNullOrEmpty(nextKey) && !string.IsNullOrEmpty(nextValue)) || SkipRow(nextKey))
                    {
                        sb.AppendLine(GetTabulator(2) + GetProperty(keys[i], values[i], false));
                        continue;
                    }
                    while(true) {
                        i += 1;
                        if(i < keys.Count - 1) {
                            nextKey = keys[i + 1];
                            if(!string.IsNullOrEmpty(nextKey)) {
                                break;
                            }
                        }
                    }
                }

                sb.AppendLine(GetTabulator(2) + GetProperty(keyTmp, valueTmp, true));
            }
            sb.AppendLine(GetLoadMethod(_className));
            sb.Append(GetFileEnd());
            return sb.ToString();
        }


        private List<string> GetCommentsList(Dictionary<string, List<string>> data) {
            List<string> comments = null;
            var isCommentsAvaliable = data.Keys.Contains(COMMENT_KEY);
            if (isCommentsAvaliable) {
                comments = data[COMMENT_KEY];
            }
            return comments;
        }

        private string GetComment(List<string> comments, int count) {
            if (comments == null) return "";
            return comments[count];
        }


        private string GetCommentData(string data, string tabulator) {
            var sb = new StringBuilder();
            sb.AppendLine(tabulator + "/// <summary>");
            sb.AppendLine(tabulator + "/// " + data);
            sb.AppendLine(tabulator + "/// </summary>");
            return sb.ToString();
        }


        protected string GetProperty(string name, string value, bool array) {
            var sb = new StringBuilder();
            sb.Append("public ");
            sb.Append(GetPropertyType(value) + (array ? "[] " : " "));
            sb.Append(name);
            sb.Append(" { get; private set; }");
            sb.AppendLine("");
            return sb.ToString();
        }


        private string GetLoadMethod(string fileName) {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("{0}public {1} Load(string fileName) {{", GetTabulator(2), _className));
            sb.AppendLine(string.Format("{0}var dataType = Resources.Load(\"{1}\" + fileName) as TextAsset;", GetTabulator(3), GetResourcesPath()));
            sb.AppendLine(string.Format("{0}var d = new {1}();", GetTabulator(3), _className));
            sb.AppendLine(string.Format("{0}if(dataType == null || string.IsNullOrEmpty(dataType.text)) return null;", GetTabulator(3)));
            sb.AppendLine(string.Format("{0}d = d.LoadJSONFromString(dataType.text);", GetTabulator(3)));
            sb.AppendLine(GetTabulator(3) + "return d;");
            sb.AppendLine(GetTabulator(2) + "}");
            return sb.ToString();
        }


        private string GetResourcesPath() {
            var splittedPath = _googleData.JSONDataLocation.Split('\\');
            StringBuilder path = new StringBuilder();
            for (int i = 0; i < splittedPath.Length; i++)
            {
                if (splittedPath[i].Equals("Resources")) {
                    
                    for (int j = i; j < splittedPath.Length; j++) {
                        path.Append(splittedPath[j]);
                        path.Append("\\\\");
                    }
                    return path.ToString();
                }
            }
            return "";
        }
    }
}