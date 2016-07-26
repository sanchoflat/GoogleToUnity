using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace GoogleSheetIntergation {
    public static class SaveLoadManager {

        public static G2UConfig LoadConfig() {
            if(!File.Exists(PathManager.ConfigFileInfo.FullName)) {
                return null;
            }
            G2UConfig config = null;
            return config.DeserializeXMLFromPath(PathManager.ConfigFileInfo.FullName);
        }

        public static void SaveConfig(G2UConfig config) {
            var path = PathManager.ConfigFileInfo.FullName;
            config.SerializeToXML(path);
        }

        public static void SaveClass(DirectoryInfo directory, Dictionary<string, string> classDict) {
            foreach(var d in classDict) {
                var p = Path.Combine(directory.FullName, d.Key + ".cs");
                SaveFile(p, @d.Value);
            }
        }

        public static void SaveData(DirectoryInfo directory, string extension, Dictionary<string, string> data,
            DataType dataType) {
            foreach(var d in data) {
                var p = Path.Combine(directory.FullName, d.Key + extension);
                SaveFile(p, @d.Value);
            }
        }

        public static void SaveFile(string path, string file) {
            File.WriteAllText(path, file);
            AssetDatabase.Refresh();
        }
    }
}