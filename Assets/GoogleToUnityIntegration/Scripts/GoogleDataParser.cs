using System;
using System.Collections.Generic;


namespace G2U {
    public class GoogleDataParser {
        public static List<List<Dictionary<string, string>>> ParsedData;

        public static List<List<Dictionary<string, string>>> ParseSheet(List<string> dataFromGoogle,
            List<GoogleSheetData> googleData) {
            ParsedData = new List<List<Dictionary<string, string>>>();
            for(var i = 0; i < dataFromGoogle.Count; i++) {
                if(dataFromGoogle.Contains("DOCTYPE")) { continue; }
                var sheetData = ParseSheet(dataFromGoogle[i]);
                ParsedData.Add(sheetData);
            }
            return ParsedData;
        }

        public static List<Dictionary<string, string>> ParseSheet(string sheet)
        {
            try {
                return CSVReader.Read(sheet);
            }
            catch {
                throw new ArgumentException(
                    "Произошла ошибка при парсинге строки. Попробуй проверить путь или доступ к таблице: " + sheet);
            }
        }
    }
}