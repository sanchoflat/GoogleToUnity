using System.IO;
using System.Linq;

namespace G2U {
    public static class PathManager {
        private const string _classFolderDefault = "./Assets/Data/Scripts/Configs";
        private const string _jsonDataFolderDefault = "./Assets/Resources/Configs";
        private const string _configFolderDefault = "./Assets/Plugins/G2U/Saves";


        public static FileInfo GetConfigPath() {
            return new FileInfo(_configFolderDefault + "/g2uconfig.txt");
        }

        public static FileInfo GetClassPath(string name = "") {
            return new FileInfo(_classFolderDefault + "/" + name + ".cs");
        }

        public static FileInfo GetJSONDataPath(string name = "") {
            return new FileInfo(_jsonDataFolderDefault + "/" + name + ".txt");
        }


        public static void CreateConfigFolder() {
            if (!Directory.Exists(_configFolderDefault)) {
                Directory.CreateDirectory(_configFolderDefault);
            }
        }


        public static bool CreateClassFolder(G2UConfig config) {
            var path = GetClassLocation(config);
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
    }
}