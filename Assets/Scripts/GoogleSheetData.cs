using System.Collections.Generic;
using System.IO;

namespace G2U {
    public enum SheetType {
        Config,
        Localization
    }

    public class GoogleSheetData {
        private const string GoogleDriveFormat =
            "http://docs.google.com/feeds/download/spreadsheets/Export?key={0}&exportFormat=csv&gid={1}";

        public GoogleSheetData(SheetType sheetType, string className, string jsonDataLocation, string classLocation,
            string googleDriveFileGuid, string googleDriveSheetGuid) {
            GoogleDriveSheetGuid = googleDriveSheetGuid;
            GoogleDriveFileGuid = googleDriveFileGuid;
            SheetType = sheetType;
            JSONDataLocation = jsonDataLocation;
            ClassLocation = classLocation;
            ClassName = className;
        }

        public GoogleSheetData() {
            GoogleDriveSheetGuid = "";
            GoogleDriveFileGuid = "";
            SheetType = SheetType.Config;
            ClassName = "";
            ClassLocation = "";
            JSONDataLocation = "";
        }

        public string ClassName { get; set; }
        public string ClassLocation { get; set; }
        public string JSONDataLocation { get; set; }
        public SheetType SheetType { get; set; }
        public string GoogleDriveFileGuid { get; set; }
        public string GoogleDriveSheetGuid { get; set; }

        public string GetURL() {
            if (string.IsNullOrEmpty(GoogleDriveFileGuid) || string.IsNullOrEmpty(GoogleDriveSheetGuid)) return null;
            return string.Format(GoogleDriveFormat, GoogleDriveFileGuid, GoogleDriveSheetGuid);
        }

        public FileInfo GetClassFileInfo() {
            return new FileInfo(ClassLocation + "/" + ClassName + ".cs");
        }

        public FileInfo GetJSONDataFileInfo() {
            return new FileInfo(JSONDataLocation + "/" + ClassName + ".txt");
        }

        public static List<GoogleSheetData> CreateDefaultData() {
            return new List<GoogleSheetData>(2) {
                new GoogleSheetData(SheetType.Config, "GameConfig", PathManager.GetJSONDataPath().DirectoryName,
                    PathManager.GetClassPath().DirectoryName, "1MAQ6GP3iFQ90vVMaiSxHnQBiVfUKeaklc0W2Lm6H6l0", "0"),
                new GoogleSheetData(SheetType.Localization, "Localization", PathManager.GetJSONDataPath().DirectoryName,
                    PathManager.GetClassPath().DirectoryName, "1MAQ6GP3iFQ90vVMaiSxHnQBiVfUKeaklc0W2Lm6H6l0",
                    "991512526")
            };
        }
    }
}