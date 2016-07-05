using System.IO;


namespace G2U {
    public enum SheetType {
        Config,
        Localization
    }

    public class GoogleSheetData {
        private const string GoogleDriveFormat =
            "http://docs.google.com/feeds/download/spreadsheets/Export?key={0}&exportFormat=csv&gid={1}";

        public GoogleSheetData(string googleDriveFileGuid, string googleDriveSheetGuid, string name) {
            GoogleDriveSheetGuid = googleDriveSheetGuid;
            GoogleDriveFileGuid = googleDriveFileGuid;
            GoogleDataName = name;
        }

        public GoogleSheetData() {
            GoogleDriveSheetGuid = "";
            GoogleDriveFileGuid = "";
        }

        public string GoogleDataName { get; set; }
       
        public string GoogleDriveFileGuid { get; set; }
        public string GoogleDriveSheetGuid { get; set; }

        public string GetURL() {
            if(string.IsNullOrEmpty(GoogleDriveFileGuid) || string.IsNullOrEmpty(GoogleDriveSheetGuid)) { return null; }
            return string.Format(GoogleDriveFormat, GoogleDriveFileGuid, GoogleDriveSheetGuid);
        }


        public static GoogleSheetData CreateDefaultData() {
            return new GoogleSheetData("", "", "");
        }
    }
}