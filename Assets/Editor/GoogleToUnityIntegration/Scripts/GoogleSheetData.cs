using System;
using System.IO;


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
        ScriptableObject
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

        public string ClassLocation { get; set; }

        public string DataLocation { get; set; }


        public void CreateDataFolder()
        {
            G2UConfig.Instance.PathManager.CreateFolder(new DirectoryInfo(DataLocation));
        }

        public void CreateClassFolder()
        {
            G2UConfig.Instance.PathManager.CreateFolder(new DirectoryInfo(ClassLocation));
        }

        public string GetDataPath(string name) {
            return String.Format("{0}/{1}.{2}", DataLocation, name, DataExtension);
        }

        public string GetClassPath(string name)
        {
            return String.Format("{0}/{1}.{2}", ClassLocation, name, "cs");
        }

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
                GoogleDriveFileGuid = GoogleDriveFileGuid,
                DataLocation = "./Assets/Resources/Configs",
                ClassLocation = "./Assets/Scripts/Configs"
            };
            return newData;

        }

        public void CreateDefaultPath() {
            DataLocation = "./Assets/Resources/Configs";
            ClassLocation = "./Assets/Scripts/Configs";
        }
    }
}