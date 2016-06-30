using System.IO;
using System.Linq;
using System.Text;


namespace G2U {
    public static class PathManager {
        private const string _classFolderDefault = "./Assets/Data/Scripts/Configs";
        private const string _jsonDataFolderDefault = "./Assets/Resources/Configs";
        private const string _configFolderDefault = "./Assets/Plugins/G2U/Saves";
        private const string _cellTypeFolderDefaut = "./Assets/Scripts/Game/";


        public static FileInfo GetConfigPath() {
            return new FileInfo(_configFolderDefault + "/g2uconfig.txt");
        }

        public static FileInfo GetClassPath(string name = "") {
            return new FileInfo(_classFolderDefault + "/" + name + ".cs");
        }

        public static FileInfo GetJSONDataPath(string name = "") {
            return new FileInfo(_jsonDataFolderDefault + "/" + name + ".txt");
        }

        public static FileInfo GetCellTypeDataPath(G2UConfig config = null, string name = "") {
            var directory = _cellTypeFolderDefaut;
            if(config != null) {
                directory = config.ParameterClassLocation;
            }
            return new FileInfo(directory + "/" + name + ".cs");
        }

        public static void CreateCellTypeFolder(G2UConfig config = null)
        {
            if(config != null) {
                if (!Directory.Exists(config.ParameterClassLocation))
                {
                    Directory.CreateDirectory(config.ParameterClassLocation);
                }
                return;
            }
            if (!Directory.Exists(_cellTypeFolderDefaut))
            {
                Directory.CreateDirectory(_cellTypeFolderDefaut);
            }
        }


        public static void CreateG2USavesFolder() {
            if (!Directory.Exists(_configFolderDefault)) {
                Directory.CreateDirectory(_configFolderDefault);
            }
        }


        public static bool CreateClassFolder(G2UConfig config) {
            if (!Directory.Exists(_classFolderDefault)) {
                Directory.CreateDirectory(_classFolderDefault);
                return true;
            }
            return false;
        }


        public static bool CreateJSONDataFolder(G2UConfig config) {
            var path = GetJSONDataLocation(config);
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
                return true;
            }
            return false;
        }


        private static string GetJSONDataLocation(G2UConfig config) {
            string path;
            if (config.GoogleSheetData == null) {
                return _configFolderDefault;
            }
            var f = config.GoogleSheetData.First();

            if (f == null) {
                path = _configFolderDefault;
            }
            else {
                path = f.JSONDataLocation;
            }
            return path;
        }

        private static string GetClassLocation(G2UConfig config) {
            string path;
            if (config.GoogleSheetData == null) {
                return _configFolderDefault;
            }
            var f = config.GoogleSheetData.First();

            if (f == null) {
                path = _configFolderDefault;
            }
            else {
                path = f.JSONDataLocation;
            }
            return path;
        }


        public static string GetResourcesPath(FileInfo path) {
            var directory = path.DirectoryName;
            var file = path.Name;
            var splittedText = directory.Split('\\');
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < splittedText.Length; i++) {
                if(splittedText[i] == "Resources") {
                    for (int j = i + 1; j < splittedText.Length; j++) {
                        sb.Append(splittedText[j] + "\\");
                    }
                }
            }
            sb.Append(file);
            return sb.ToString();
        }

    }
}