using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;


namespace G2U {
    internal class TypeManager {
        public const string StringType = "string";
        public const string BoolType = "bool";
        public const string IntType = "int";
        public const string LongType = "long";
        public const string FloatType = "float";

        public static Dictionary<string, List<AbstractDataRow>> UpdateParsedFileData(
            Dictionary<string, List<AbstractDataRow>> data) {
            var convertedData = new List<List<AbstractDataRow>>();
            var recordsCount = data.ElementAt(1).Value.Count;
            var configCount = data.Count;
            for(var i = 0; i < recordsCount; i++) {
                convertedData.Add(new List<AbstractDataRow>(configCount - 1));
                for(var j = 0; j < configCount - 1; j++) {
                    convertedData[i].Add(null);
                }
            }
            for(var i = 0; i < recordsCount; i++) {
                for(var j = 1; j < configCount; j++) {
                    var row = data.ElementAt(j);
                    var value = row.Value[i];
                    convertedData[i][j - 1] = value;
                }
            }
            var types = new List<string>();
            var isArray = new List<bool>();
            for(var i = 0; i < recordsCount; i++) {
                types.Add(GetTypeFromList(convertedData[i]));
                isArray.Add(IsArray(convertedData[i]));
            }
            foreach(var d in data) {
                if(d.Value == null) { continue; }
                for(var i = 0; i < recordsCount; i++) {
                    d.Value[i].ParameterType = types[i];
                    d.Value[i].IsArray = isArray[i];
                }
            }
            return data;
        }

        public static string GetPropertyType(string[] data) {
            if(data.Length == 1) {
                return GetPropertyType(data[0]);
            }
            var types = new string[data.Length];
            for(var i = 0; i < types.Length; i++) {
                types[i] = GetPropertyType(data[i]);
            }
            return GetArrayType(types);
        }

        private static string GetTypeFromList(List<AbstractDataRow> data) {
            return GetArrayType(data.Select(row => row.ParameterType).ToArray());
        }

        private static bool IsArray(List<AbstractDataRow> data) {
            return data.Any(row => row.IsArray);
        }

        #region Get Property type

        private static string GetPropertyType(string data) {
            var str = data;
            if(CheckForBool(str)) {
                return BoolType;
            }
            if(CheckForFloat(str)) {
                return FloatType;
            }
            if(CheckForInt(str)) {
                return IntType;
            }
            if(CheckForLong(str)) {
                return LongType;
            }
            return StringType;
        }

        private static bool CheckForBool(string str) {
            return str.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                   str.Equals("false", StringComparison.OrdinalIgnoreCase);
        }

        private static bool CheckForFloat(string str) {
            if(str.Contains(".") || str.Contains(",")) {
                float value;
                if(float.TryParse(str, NumberStyles.Float, new NumberFormatInfo(), out value)) {
                    return true;
                }
            }
            return false;
        }

        private static bool CheckForInt(string str) {
            int value;
            if(int.TryParse(str, NumberStyles.Integer, new NumberFormatInfo(), out value)) {
                return true;
            }
            return false;
        }

        private static bool CheckForLong(string str) {
            long value;
            if(long.TryParse(str, NumberStyles.Integer, new NumberFormatInfo(), out value)) {
                return true;
            }
            return false;
        }

        private static string GetArrayType(string[] arr) {
            if(arr.Any(s => s == StringType)) { return StringType; }
            if(arr.All(s => s == BoolType)) { return BoolType; }
            if(arr.All(s => s == IntType)) { return IntType; }
            if(arr.All(s => s == IntType || s == LongType)) { return LongType; }
            if(arr.All(s => s == IntType || s == FloatType)) { return FloatType; }
            if(arr.All(s => s == FloatType || s == LongType || s == IntType)) { return FloatType; }
            throw new Exception("Cannot compute array type. Please check sheet data.\n" + GetErrorData(arr));
        }

        private static string GetErrorData(string[] types) {
            var sb = new StringBuilder();
            for(var i = 0; i < types.Length; i++) {
                sb.Append(types[i] + "  ");
            }
            return sb.ToString();
        }

        #endregion
    }
}