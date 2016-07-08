using System.Collections.Generic;
using System.Text.RegularExpressions;
using G2U;

public class CSVReader {
    private static readonly string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    private static readonly string LINE_SPLIT_RE = @"\r\n|\n\r";
    private static readonly char[] TRIM_CHARS = {'\"'};

    public static List<Dictionary<string, string>> Read(string file) {
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

    private static bool AllowRow(string[] data) {
        for(var i = 0; i < data.Length; i++) {
            if(!string.IsNullOrEmpty(data[i])) {
                return false;
            }
        }
        return true;
    }
}