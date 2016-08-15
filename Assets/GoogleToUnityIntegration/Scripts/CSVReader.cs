using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GoogleSheetIntergation;

public class CSVReader {
    private static readonly string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    private static readonly string LINE_SPLIT_RE = @"\r\n|\n\r";
    private static readonly char[] TRIM_CHARS = {'\"'};

    public static Dictionary<string, List<AbstractDataRow>> Read(string file, out string name,
        GoogleSheetData googleSheetData) {
        var firstData = ReadFirst(file, googleSheetData);
        var parsedFileData = TypeManager.UpdateTypes(PrepareParsedFileData(firstData, googleSheetData));
        var newParsedFile = ConvertList(parsedFileData);
        name = parsedFileData.ElementAt(0).Key;
        return newParsedFile;
    }

    private static List<Dictionary<string, string>> ReadFirst(string file, GoogleSheetData googleSheetData) {
        var list = new List<Dictionary<string, string>>();
        var lines = Regex.Split(file, LINE_SPLIT_RE);
        if(lines.Length <= 1) { return list; }
        var header = Regex.Split(lines[0], SPLIT_RE);
        for(var i = 0; i < header.Length; i++) {
            header[i] = PathManager.PrepareFileName(header[i], false);
        }
        for(var i = 1; i < lines.Length; i++) {
            var values = Regex.Split(lines[i], SPLIT_RE);
            if(values.Length == 0 || AllowRow(values)) { continue; }
            var entry = new Dictionary<string, string>();
            for(var j = 0; j < header.Length && j < values.Length; j++) {
                var value = values[j];
                if(j == 0) {
                    value = PathManager.PrepareFileName(value, true);
                }
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                entry[header[j]] = value;
            }
            list.Add(entry);
        }
        return list;
    }

    protected static List<Dictionary<string, List<AbstractDataRow>>>
        PrepareParsedFileData(List<List<Dictionary<string, string>>> data, GoogleSheetData googleSheetData) {
        var list = new List<Dictionary<string, List<AbstractDataRow>>>();
        foreach(var d in data) {
            list.Add(PrepareParsedFileData(d, googleSheetData));
        }
        return list;
    }

    protected static Dictionary<string, List<AbstractDataRow>> PrepareParsedFileData(
        List<Dictionary<string, string>> data, GoogleSheetData googleData) {
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
                Dictionary<string, string> nextData = null;
                if(row < data.Count - 1) {
                    nextData = data[row + 1];
                }

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
                var dataRow = GetRowData(rowName, dataList.ToArray(), GetComment(data[oldRow]), googleData);
                dictionaryData[columnName].Add(dataRow);
            }
        }
        return dictionaryData;
    }

    private static AbstractDataRow GetRowData(string parameterName, string[] data, string comment,
        GoogleSheetData googleData) {
        return new ClassDataRow(parameterName, data, comment, G2UConfig.Instance.ArraySeparator,
            googleData.FieldAccessModifiers,
            googleData.SetAccessModifiers, googleData.VariableType);
    }

    private static List<string> GetKeys(List<Dictionary<string, string>> data) {
        return data[0].Keys.ToList();
    }

    protected static bool SkipColumn(string columnName) {
        return columnName.Equals(G2UConfig.Instance.CommentColumnTitle);
    }

    protected static bool SkipRow(string rowName, string currentData) {
        if(rowName.Contains(G2UConfig.Instance.SkipRowPrefix)) { return true; }
        return string.IsNullOrEmpty(rowName) && string.IsNullOrEmpty(currentData);
    }

    private static bool IsArrayParameter(Dictionary<string, string> nextData, List<string> keyList, string currentKey) {
        if(nextData == null) { return false; }
        return string.IsNullOrEmpty(nextData[keyList[0]]) && !string.IsNullOrEmpty(nextData[currentKey]);
    }

    private static string GetComment(Dictionary<string, string> row) {
        string comment;
        row.TryGetValue(G2UConfig.Instance.CommentColumnTitle, out comment);
        return comment;
    }

    private static Dictionary<string, List<AbstractDataRow>> ConvertList(Dictionary<string, List<AbstractDataRow>> data) {
        var file = new Dictionary<string, List<AbstractDataRow>>();
        foreach(var concreteData in data) {
            if(concreteData.Value == null) { continue; }
            file.Add(concreteData.Key, concreteData.Value);
        }
        return file;
    }

    private static bool AllowRow(string[] data) {
        for(var i = 0; i < data.Length; i++) {
            if(!string.IsNullOrEmpty(data[i])) {
                return false;
            }
        }
        return true;
    }
}