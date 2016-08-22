using System.IO;
using System.Text;


namespace GoogleSheetIntergation {
    public class PathManager {
        public static FileInfo ConfigFileInfo;
        private const string _classFolderDefault = "./Assets/Scripts/Configs";
        private const string _dataFolderDefault = "./Assets/Resources/Configs";
        private const string _paramFolderDefault = "./Assets/Scripts/Game";

        public string ClassLocation { get; set; }
        public string DataLocation { get; set; }
        public string ParamLocation { get; set; }
        public string ParamClassName { get; set; }

        static PathManager() {
            ConfigFileInfo = new FileInfo("./Assets/Editor/GoogleToUnityIntegration/Config/g2uconfig.txt");
        }

        public PathManager() {
            ClassLocation = _classFolderDefault;
            DataLocation = _dataFolderDefault;
            ParamLocation = _paramFolderDefault;
        }

        public DirectoryInfo GetDataFolder() {
            return new DirectoryInfo(DataLocation);
        }

        public DirectoryInfo GetClassFolder() {
            return new DirectoryInfo(ClassLocation);
        }

        public DirectoryInfo GetParamFolder() {
            return new DirectoryInfo(ParamLocation);
        }


        public string GetDataPath(string fileName, string extension) {
            return string.Format("{0}/{1}.{2}", DataLocation, fileName, extension.Replace(".", ""));
        }

        public string GetClassPath(string fileName)
        {
            return string.Format("{0}/{1}.cs", ClassLocation, fileName);
        }

        public string GetParamPath()
        {
            return GetParamPath(ParamClassName);
        }

        public string GetParamPath(string fileName)
        {
            return string.Format("{0}/{1}.cs", ParamLocation, fileName);
        }


        public void CreateAllFolders() {
            CreateConfigFolder();
            CreateClassFolder();
            CreateDataFolder();
        }

        public void CreateConfigFolder() {
            CreateFolder(ConfigFileInfo);
        }

        public void CreateClassFolder() {
            var d = GetClassFolder();
            CreateFolder(d);
        }

        public void CreateDataFolder() {
            var d = GetDataFolder();
            CreateFolder(d);
        }

        public void CreateParameterFolder() {
            var d = GetParamFolder();
            CreateFolder(d);
        }

        private void CreateFolder(FileInfo fileInfo) {
            if(fileInfo == null) { return; }
            if(fileInfo.Directory == null) { return; }
            CreateFolder(fileInfo.Directory);
        }

        public void CreateFolder(DirectoryInfo directory) {
            if(directory == null) { return; }
            if(!directory.Exists) {
                directory.Create();
            }
        }

        public FileInfo GetParametersFileInfo(G2UConfig config = null, string name = "") {
            var directory = _paramFolderDefault;
            if(config != null) {
                directory = config.ParameterClassLocation;
            }
            return new FileInfo(directory + "/" + name + ".cs");
        }

        public static string GetResourcesPath(FileInfo path) {
            var directory = path.DirectoryName;
            var file = path.Name;
            var splittedText = directory.Split('\\');
            var sb = new StringBuilder();
            for(var i = 0; i < splittedText.Length; i++) {
                if(splittedText[i] == "Resources") {
                    for(var j = i + 1; j < splittedText.Length; j++) {
                        sb.Append(splittedText[j] + "/");
                    }
                }
            }
            sb.Append(file);
            return sb.ToString();
        }

        public static string GetProjectRelativPath(FileInfo path) {
            var directory = path.DirectoryName;
            var file = path.Name;
            var splittedText = directory.Split('\\');
            var sb = new StringBuilder();
            for(var i = 0; i < splittedText.Length; i++) {
                if(splittedText[i] == "Assets") {
                    for(var j = i; j < splittedText.Length; j++) {
                        sb.Append(splittedText[j] + "\\");
                    }
                }
            }
            sb.Append(file);
            return sb.ToString();
        }

        public static string PrepareFileName(string fileName, bool firstToUpper) {
            fileName = fileName.Replace(" ", "");
            if(firstToUpper) { fileName = fileName.UppercaseFirst(); }
            return fileName;
        }
    }
}