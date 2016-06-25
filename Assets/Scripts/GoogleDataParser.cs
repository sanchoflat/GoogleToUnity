using System;
using System.Collections.Generic;
using G2U;

namespace G2U {
    public class GoogleDataParser {
        public static Dictionary<SheetType, List<Dictionary<string, object>>> ParsedData;

        public static Dictionary<SheetType, List<Dictionary<string, object>>> ParseSheet(List<string> dataFromGoogle, List<GoogleSheetData> googleData)
        {
            ParsedData = new Dictionary<SheetType, List<Dictionary<string, object>>>();
            for (var i = 0; i < dataFromGoogle.Count; i++) {
                if (dataFromGoogle.Contains("DOCTYPE")) continue;

                var sheetData = ParseSheet(dataFromGoogle[i]);
                ParsedData.Add(googleData[i].SheetType, sheetData);
            }
            return ParsedData;
        }

        public static List<Dictionary<string, object>> ParseSheet(string sheet)
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
