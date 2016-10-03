using System.IO;
using System.Text;


namespace GoogleSheetIntergation {
    public class PathManager {
        public static FileInfo ConfigFileInfo;
        private const string _constantFolderDefault = "./Assets/Scripts/Game";

        public string ConstantClassLocation { get; set; }
        public string ConstantClassName { get; set; }

        static PathManager() {
            ConfigFileInfo = new FileInfo("./Assets/Editor/GoogleToUnityIntegration/Config/g2uconfig.txt");
        }

        public PathManager() {
            ConstantClassLocation = _constantFolderDefault;
        }


        public DirectoryInfo GetConstantClassFolder() {
            return new DirectoryInfo(ConstantClassLocation);
        }

        public string GetParamPath()
        {
            return GetParamPath(ConstantClassName);
        }

        public string GetParamPath(string fileName)
        {
            return string.Format("{0}/{1}.cs", ConstantClassLocation, fileName);
        }

        public void CreateAllFolders() {
            CreateConfigFolder();
        }

        public void CreateConfigFolder() {
            CreateFolder(ConfigFileInfo);
        }

      
        public void CreateConstantClassFolder() {
            var d = GetConstantClassFolder();
            CreateFolder(d);
        }

        private void CreateFolder(string folderName)
        {
            CreateFolder(new DirectoryInfo(folderName));
            
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
            var directory = _constantFolderDefault;
            if(config != null) {
                directory = config.ConstantClassLocation;
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