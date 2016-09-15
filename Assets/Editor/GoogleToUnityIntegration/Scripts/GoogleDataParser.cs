using System.Collections.Generic;
using System.Linq;


namespace GoogleSheetIntergation {
    public class GoogleDataParser {

        public static Dictionary<string, Dictionary<string, List<AbstractDataRow>>> ParseSheet(
         List<string> dataFromGoogle, List<GoogleSheetData> googleData)
        {
            if (dataFromGoogle.Count != googleData.Count)
            {
                return null;
            }
            if (dataFromGoogle == null || dataFromGoogle.Count == 0) { return null; }
            var ParsedData = new Dictionary<string, Dictionary<string, List<AbstractDataRow>>>();

            for (var i = 0; i < dataFromGoogle.Count; i++) {
                var d = ParseSheet(dataFromGoogle[i], googleData[i]);
                if(d.Count == 0) continue;
                var keys = d.Keys.ToList();
                ParsedData.Add(keys[0], d[keys[0]]);
            }
            return ParsedData;
        }

        public static Dictionary<string, Dictionary<string, List<AbstractDataRow>>> ParseSheet(
            string dataFromGoogle, GoogleSheetData googleData) {
            var ParsedData = new Dictionary<string, Dictionary<string, List<AbstractDataRow>>>();
            if(dataFromGoogle.Contains("DOCTYPE")) { return ParsedData; }
            string name;
            var sheetData = ParseSheet(dataFromGoogle, out name, googleData);
            if(ParsedData.ContainsKey(name)) { return ParsedData; }
            ParsedData.Add(name, sheetData);
            return ParsedData;
        }

        public static Dictionary<string, List<AbstractDataRow>> ParseSheet(string sheet, out string name,
            GoogleSheetData googleSheetData) {
            return CSVReader.Read(sheet, out name, googleSheetData);
        }
    }
}