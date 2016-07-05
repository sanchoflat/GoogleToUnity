using System.Collections.Generic;
using System.IO;

namespace G2U {
    public static class SaveLoadManager {
        public static G2UConfig LoadConfig() {
            G2UConfig config = null;
            if(!PathManager.ConfigFileInfo.Exists) {
                return null;
            }
            return config.DeserializeFromXML(PathManager.ConfigFileInfo.FullName);
        }

        public static void SaveConfig(G2UConfig config) {
            var path = PathManager.ConfigFileInfo.FullName;
            config.SerializeToXML(path);
        }

        public static void SaveClass(DirectoryInfo directory, Dictionary<string, string> classDict) {
            foreach(var d in classDict) {
                var p = Path.Combine(directory.FullName, PathManager.PrepareFileName(d.Key, false) + ".cs");
                SaveFile(p, @d.Value);
            }
        }

        public static void SaveData(DirectoryInfo directory, string extension, Dictionary<string, string> data) {
            foreach(var d in data) {
                var p = Path.Combine(directory.FullName, PathManager.PrepareFileName(d.Key, false) + extension);
                SaveFile(p, @d.Value);
            }
        }

        public static void SaveFile(string path, string file) {
            File.WriteAllText(path, file);
        }
    }
}