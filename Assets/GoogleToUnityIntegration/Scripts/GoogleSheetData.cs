namespace GoogleSheetIntergation {
    public enum VariableType {
        Field,
        Property
    }

    public enum AccessModifiers {
        Public,
        Private,
        Protected
    }

    public enum DataType {
        XML,
        ScriptableObject,
        Binary
    }

    public class GoogleSheetData {
        private const string GoogleDriveFormat =
            "http://docs.google.com/feeds/download/spreadsheets/Export?key={0}&exportFormat=csv&gid={1}";

        public GoogleSheetData(string googleDataName, string googleDriveFileGuid, string googleDriveSheetGuid,
            string dataExtension, string ns, DataType dataType, VariableType variableType,
            AccessModifiers setAccessModifiers, AccessModifiers fieldAccessModifiers) {
            VariableType = variableType;
            SetAccessModifiers = setAccessModifiers;
            FieldAccessModifiers = fieldAccessModifiers;
            DataExtension = dataExtension;
            Namespace = ns;
            GoogleDataName = googleDataName;
            GoogleDriveFileGuid = googleDriveFileGuid;
            GoogleDriveSheetGuid = googleDriveSheetGuid;
        }

        public GoogleSheetData() {
            GoogleDriveSheetGuid = "";
            GoogleDriveFileGuid = "";
        }

        public string GoogleDataName { get; set; }
        public string GoogleDriveFileGuid { get; set; }
        public string GoogleDriveSheetGuid { get; set; }

        public string DataExtension { get; set; }
        public string Namespace { get; set; }

        public DataType DataType { get; set; }
        public VariableType VariableType { get; set; }
        public AccessModifiers SetAccessModifiers { get; set; }
        public AccessModifiers FieldAccessModifiers { get; set; }

        public string GetURL() {
            if(string.IsNullOrEmpty(GoogleDriveFileGuid) || string.IsNullOrEmpty(GoogleDriveSheetGuid)) { return null; }
            return string.Format(GoogleDriveFormat, GoogleDriveFileGuid, GoogleDriveSheetGuid);
        }

        public static GoogleSheetData CreateDefaultData() {
            return new GoogleSheetData("", "", "", ".xml", "GoogleSheetIntergation", DataType.XML, VariableType.Field,
                AccessModifiers.Private, AccessModifiers.Public);
        }


        public GoogleSheetData Clone() {
            var newData = new GoogleSheetData() {
                VariableType = VariableType,
                DataType = DataType,
                Namespace = Namespace,
                SetAccessModifiers = SetAccessModifiers,
                FieldAccessModifiers = FieldAccessModifiers,
                DataExtension = DataExtension,
                GoogleDriveSheetGuid = "",
                GoogleDataName = "",
                GoogleDriveFileGuid = GoogleDriveFileGuid
            };
            return newData;

        }
    }
}