using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace G2U {
    public abstract class AbstractFileBuilder {
        protected const string KEY = "Key";
        protected const string VALUE = "Value";
        protected const string COMMENT = "Comment";

        public static AbstractClassGenerator GetClassGenerator(SheetType sheetType, string className) {
            switch (sheetType) {
                case SheetType.Config:
                    return new BaseConfigGenerator(className, "EternalMaze");
                case SheetType.Localization:
                    return new LocalizationConfigGenerator(className, "EternalMaze");
            }
            return null;
        }

        public static AbstractFileBuilder GetJSONGenerator(SheetType sheetType) {
            switch (sheetType) {
                case SheetType.Config:
                    return new JSONConfigBuilder();
                case SheetType.Localization:
                    return new LocalizationBuilder();
            }
            return null;
        }

        public string GetEmptyFile() {
            return "";
        }


        public string GenerateFile(List<Dictionary<string, object>> data) {
            var @class = new StringBuilder();
            @class.AppendLine(GetIncludes());
            @class.AppendLine(GetFileStart());
            @class.AppendLine(GetFileData(data));
            @class.AppendLine(GetFileEnding());
            return @class.ToString();
        }


        protected virtual string GetIncludes() {
            return "";
        }

        protected virtual string GetFileData(List<Dictionary<string, object>> data) {
            return "";
        }

        protected virtual string GetFileStart() {
            return "";
        }

        protected virtual string GetFileEnding() {
            return "";
        }


        protected string GetTabulator(int count) {
            var sb = new StringBuilder(count);
            for (var i = 0; i < count; i++) {
                sb.Append("\t");
            }
            return sb.ToString();
        }


        protected string GetPropertyType(object data) {
            var str = data.ToString();
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
    }

    public class JSONConfigBuilder : AbstractFileBuilder {
        protected override string GetFileStart() {
            return "{";
        }

        protected override string GetFileData(List<Dictionary<string, object>> data) {
            var sb = new StringBuilder();
            foreach (var d in data) {
                sb.Append(GetKey(d[KEY]));
                sb.Append(GetValue(d[VALUE]));
            }
            return sb.ToString();
        }

        protected override string GetFileEnding() {
            return "}";
        }


        private string GetKey(object key) {
            return "\"" + key + "\": ";
        }

        private string GetValue(object value) {
            var objectType = GetPropertyType(value);
            var data = value.ToString();
            var val = value.ToString().Trim(' ');
            if (objectType == "string") {
                data = "\"" + value + "\"";
            }
            if (objectType == "bool") {
                data = value.ToString().ToLower() + ",";
            }
            return data + ",";
        }
    }

    public class LocalizationBuilder : AbstractFileBuilder {
        protected override string GetFileStart() {
            return "{";
        }


        protected override string GetFileData(List<Dictionary<string, object>> data) {
            var sb = new StringBuilder();
            foreach (var d in data) {
                sb.Append(GetKey(d[KEY]));
                var lKey = GetLocalizationKey();
                sb.Append(GetValue(d[lKey]));
            }
            return sb.ToString();
        }


        private string GetLocalizationKey() {
            return "ru";
        }

        protected override string GetFileEnding() {
            return "}";
        }


        private string GetKey(object key) {
            return "\"" + key + "\": ";
        }

        private string GetValue(object value) {
            var objectType = GetPropertyType(value);
            var data = value.ToString();
            if (objectType == "string") {
                data = "\"" + value + "\"";
            }
            if (objectType == "bool") {
                data = value.ToString().ToLower() + "\",";
            }
            return data + ",";
        }
    }

    public abstract class AbstractClassGenerator : AbstractFileBuilder {
        private const string KEY = "Key";
        private const string VALUE = "Value";
        private const string COMMENT = "Comment";
        private readonly string _className;

        private readonly string _namespace;
        private FileInfo _classFileInfo;

        protected AbstractClassGenerator(string nameSpace, string className) {
            _namespace = nameSpace;
            _className = className;
        }

        public string GetEmptyClass() {
            var @class = new StringBuilder();
            @class.AppendLine(GetIncludes());
            @class.AppendLine(GetClassStart());
            @class.AppendLine(GetClassEnding());
            return @class.ToString();
        }


        public virtual string GenerateClass(List<Dictionary<string, object>> data, FileInfo classLocation) {
            _classFileInfo = classLocation;
            return "";
        }

        protected string GetIncludes() {
            var sb = new StringBuilder();
            //            sb.AppendLine("using System;");
            //            sb.AppendLine("using UnityEngine;");
            //            sb.AppendLine("using System.Collections.Generic;");
            //            sb.AppendLine("using System.IO;");
            //            sb.AppendLine("using UnityEditor;");
            //            sb.AppendLine("using EternalMaze.EditorWindows;");
            return sb.ToString();
        }

        protected void Save(string @class) {
            if (_classFileInfo.Exists) {
                _classFileInfo.Delete();
            }
            if (!Directory.Exists(_classFileInfo.DirectoryName)) {
                Directory.CreateDirectory(_classFileInfo.DirectoryName);
            }
            //            LoadSaveManager.Save(_classFileInfo.FullName, @class);
        }

        protected string GetClassStart() {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("namespace {0} {{", _namespace));
            sb.AppendLine(string.Format("{0}public class {1} {{", GetTabulator(1), _className));
            return sb.ToString();
        }

        protected string GetTabulator(int count) {
            var sb = new StringBuilder(count);
            for (var i = 0; i < count; i++) {
                sb.Append("\t");
            }
            return sb.ToString();
        }

        protected string GetClassEnding() {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("{0}}}", GetTabulator(1)));
            sb.AppendLine("}");
            return sb.ToString();
        }

        protected virtual string GenerateClassData(List<Dictionary<string, object>> data) {
            var sb = new StringBuilder();
            foreach (var d in data) {
                if (d.ContainsKey(COMMENT)) {
                    var comment = (string) d[COMMENT];
                    if (comment.Length > 0) {
                        sb.Append(GetCommentData(comment, GetTabulator(2)));
                    }
                }
                sb.AppendLine(GetTabulator(2) + GetProperty(d[KEY], GetValueFromDictionary(d)));
            }
            return sb.ToString();
        }

        protected virtual string GetValueFromDictionary(Dictionary<string, object> dict) {
            return dict[VALUE].ToString();
        }

        protected string GetCommentData(string data, string tabulator) {
            var sb = new StringBuilder();
            sb.AppendLine(tabulator + "/// <summary>");
            sb.AppendLine(tabulator + "/// " + data);
            sb.AppendLine(tabulator + "/// </summary>");
            return sb.ToString();
        }

        protected string GetProperty(object name, object value) {
            var sb = new StringBuilder();
            sb.Append("public ");
            sb.Append(GetPropertyType(value) + " ");
            sb.Append(name);
            sb.Append(" { get; private set; }");
            sb.AppendLine("");
            return sb.ToString();
        }
    }

    public class BaseConfigGenerator : AbstractClassGenerator {
        public BaseConfigGenerator(string className, string nameSpace) : base(nameSpace, className) {}

        public override string GenerateClass(List<Dictionary<string, object>> data, FileInfo classLocation) {
            base.GenerateClass(data, classLocation);
            var @class = new StringBuilder();
            @class.AppendLine(GetIncludes());
            @class.AppendLine(GetClassStart());
            @class.AppendLine(GenerateClassData(data));
            @class.AppendLine(GetClassEnding());
            return @class.ToString();
        }
    }

    public class LocalizationConfigGenerator : AbstractClassGenerator {
        public LocalizationConfigGenerator(string className, string nameSpace) : base(nameSpace, className) {}

        public override string GenerateClass(List<Dictionary<string, object>> data, FileInfo classLocation) {
            base.GenerateClass(data, classLocation);
            var @class = new StringBuilder();
            @class.AppendLine(GetIncludes());
            @class.AppendLine(GetClassStart());
            @class.AppendLine(GenerateClassData(data));
            @class.AppendLine(GetClassEnding());
            return @class.ToString();
        }

        protected override string GetValueFromDictionary(Dictionary<string, object> dict) {
            return "string";
        }
    }
}