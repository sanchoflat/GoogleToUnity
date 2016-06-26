﻿using System.Collections.Generic;
using System.IO;

namespace G2U {
    public enum SheetType {
        Config,
        Localization
    }

    public class GoogleSheetData {
        private const string GoogleDriveFormat =
            "http://docs.google.com/feeds/download/spreadsheets/Export?key={0}&exportFormat=csv&gid={1}";

        public GoogleSheetData(string jsonDataLocation, string classLocation,
            string googleDriveFileGuid, string googleDriveSheetGuid, bool skipEmptyLines, bool generateClassForEveryColumn)
        {
            GoogleDriveSheetGuid = googleDriveSheetGuid;
            GoogleDriveFileGuid = googleDriveFileGuid;
            JSONDataLocation = jsonDataLocation;
            ClassLocation = classLocation;
            SkipEmptyLines = skipEmptyLines;
            GenerateClassForEveryColumn = generateClassForEveryColumn;
        }

        public GoogleSheetData() {
            GoogleDriveSheetGuid = "";
            GoogleDriveFileGuid = "";
            ClassLocation = "";
            JSONDataLocation = "";
            SkipEmptyLines = false;
            GenerateClassForEveryColumn = false;
        }

    
        public bool GenerateClassForEveryColumn { get; set; }
        public bool SkipEmptyLines { get; set; }
        public string ClassLocation { get; set; }
        public string JSONDataLocation { get; set; }
        public string GoogleDriveFileGuid { get; set; }
        public string GoogleDriveSheetGuid { get; set; }

        public string GetURL() {
            if (string.IsNullOrEmpty(GoogleDriveFileGuid) || string.IsNullOrEmpty(GoogleDriveSheetGuid)) return null;
            return string.Format(GoogleDriveFormat, GoogleDriveFileGuid, GoogleDriveSheetGuid);
        }

        public FileInfo GetClassDirectory() {
            return new FileInfo(ClassLocation + "\\");
        }

        public FileInfo GetJSONDataDirectory() {
            return new FileInfo(JSONDataLocation + "\\");
        }

        public static List<GoogleSheetData> CreateDefaultData() {
            return new List<GoogleSheetData>(2) {
                new GoogleSheetData(PathManager.GetJSONDataPath().DirectoryName,
                    PathManager.GetClassPath().DirectoryName, "1MAQ6GP3iFQ90vVMaiSxHnQBiVfUKeaklc0W2Lm6H6l0", "0", true, true),
                new GoogleSheetData(PathManager.GetJSONDataPath().DirectoryName,
                    PathManager.GetClassPath().DirectoryName, "1MAQ6GP3iFQ90vVMaiSxHnQBiVfUKeaklc0W2Lm6H6l0",
                    "991512526", false, false)
            };
        }
    }
}