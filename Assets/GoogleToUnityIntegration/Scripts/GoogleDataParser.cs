using System;
using System.Collections.Generic;


namespace GoogleSheetIntergation {
    public class GoogleDataParser {
        public static Dictionary<string, Dictionary<string, List<AbstractDataRow>>> ParsedData;

        public static Dictionary<string, Dictionary<string, List<AbstractDataRow>>> ParseSheet(
            List<string> dataFromGoogle, List<GoogleSheetData> googleData) {
            if(dataFromGoogle == null || dataFromGoogle.Count == 0) return null;
            ParsedData = new Dictionary<string, Dictionary<string, List<AbstractDataRow>>>();
            for(var i = 0; i < dataFromGoogle.Count; i++) {
                if(dataFromGoogle.Contains("DOCTYPE")) { continue; }
                string name;
                var sheetData = ParseSheet(dataFromGoogle[i], out name, googleData[i]);
                ParsedData.Add(name, sheetData);
            }
            return ParsedData;
        }

        public static Dictionary<string, List<AbstractDataRow>> ParseSheet(string sheet, out string name,
            GoogleSheetData googleSheetData) {
            try {
                return CSVReader.Read(sheet, out name, googleSheetData);
            }
            catch {
                throw new ArgumentException(
                    "Произошла ошибка при парсинге строки. Попробуй проверить путь или доступ к таблице: " + sheet);
            }
        }
    }
}