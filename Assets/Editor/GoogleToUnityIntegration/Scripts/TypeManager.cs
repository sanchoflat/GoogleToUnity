using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;


namespace GoogleSheetIntergation {
    internal class TypeManager {
        public static Type StringType = typeof(string);
        public static Type BoolType = typeof(bool);
        public static Type IntType = typeof(int);
        public static Type LongType = typeof(long);
        public static Type FloatType = typeof(float);

        public static Type StringArrayType = typeof(string[]);
        public static Type BoolArrayType = typeof(bool[]);
        public static Type IntArrayType = typeof(int[]);
        public static Type LongArrayType = typeof(long[]);
        public static Type FloatArrayType = typeof(float[]);

        public static List<Dictionary<string, List<AbstractDataRow>>> UpdateTypes(
            List<Dictionary<string, List<AbstractDataRow>>> data) {
            var list = new List<Dictionary<string, List<AbstractDataRow>>>();
            foreach(var d in data) {
                list.Add(UpdateTypes(d));
            }
            return list;
        }

        public static Dictionary<string, List<AbstractDataRow>> UpdateTypes(
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
            var types = new List<Type>();
            var isArray = new List<bool>();
            for (int i = 0, j = 0; i < recordsCount; i++)
            {
                if (convertedData[i][0].SkipDataRow()) continue;
                isArray.Add(IsArray(convertedData[i]));
                types.Add(GetTypeFromList(convertedData[i], false));
                j++;

            }
            foreach(var d in data) {
                if(d.Value == null) { continue; }
                for(int i = 0, j = 0; i < recordsCount; i++) {
                    if(d.Value[i].SkipDataRow()) continue;
                    d.Value[i].ParameterType = types[j];
                    d.Value[i].IsArray = isArray[j];
                    j++;
                }
            }
            return data;
        }

        public static string[] PrepareArrayData(string[] data, string arraySeparator)
        {
            var list = new List<string>();
            for(int i = 0; i < data.Length; i++) {
                list.AddRange(GetArrayString(data[i], arraySeparator));
            }
            return list.ToArray();
        }

        public static Type GetPropertyType(string[] data) {
            if(data.Length == 1) {
                return GetPropertyType(data[0]);
            }
            var types = new Type[data.Length];
            for(var i = 0; i < types.Length; i++) {
                types[i] = GetPropertyType(data[i]);
            }
            return GetArrayType(types, false);
//                var arr = GetArrayString(data[0], arraySeparator);
//                if(arr.Length == 1) {
                    
//                }
//                data = arr;
            

            // значит у нас массив
//            var list = new List<string>();
//            for(var i = 0; i < data.Length; i++) {
//                var tmpArr = GetArrayString(data[i], arraySeparator);
//                list.AddRange(tmpArr);
//            }
//            data = list.ToArray();
            
        }

        private static string[] GetArrayString(string data, string arraySeparator) {
            var parsedData = data.Split(new[] {arraySeparator}, StringSplitOptions.None).Select(s => s.Trim()).ToArray();
            return parsedData;
        }

        private static Type GetTypeFromList(List<AbstractDataRow> data, bool array) {
            return GetArrayType(data.Select(row => row.ParameterType).ToArray(), array);
        }

        private static bool IsArray(List<AbstractDataRow> data) {
            return data.Any(row => row.IsArray);
        }

        #region Get Property type

        public static Type GetPropertyType(string data) {
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

        private static Type GetArrayType(Type[] arr, bool array) {
            if(arr.Any(s => s == StringType)) { return array ? StringArrayType : StringType; }
            if (arr.All(s => s == BoolType)) { return array ? BoolArrayType : BoolType; }
            if (arr.All(s => s == IntType)) { return array ? IntArrayType : IntType; }
            if (arr.All(s => s == IntType || s == LongType)) { return array ? LongArrayType : LongType; }
            if (arr.All(s => s == IntType || s == FloatType)) { return array ? FloatArrayType : FloatType; }
            if (arr.All(s => s == FloatType || s == LongType || s == IntType)) { return array ? FloatArrayType : FloatType; }
            throw new Exception("Cannot compute array type. Please check sheet data.\n" + GetErrorData(arr));
        }

        private static string GetErrorData(Type[] types) {
            var sb = new StringBuilder();
            for(var i = 0; i < types.Length; i++) {
                sb.Append(types[i] + "  ");
            }
            return sb.ToString();
        }

        #endregion
    }
}