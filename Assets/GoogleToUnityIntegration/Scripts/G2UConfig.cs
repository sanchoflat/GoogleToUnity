using System.Collections.Generic;

namespace GoogleSheetIntergation {


    public class G2UConfig {

        public static G2UConfig Instance {
            get {
                if (_instance == null) _instance = CreateDefault();
                return _instance;
            }
        }

        private static G2UConfig _instance;

        public List<GoogleSheetData> GoogleSheetData { get; set; }
        public bool WasInizialized { get; set; }
        public string SkipRowPrefix { get; set; }
        public string CommentColumnTitle { get; set; }
        public string ArraySeparator { get; set; }


        public string ParameterClassName {
            get { return PathManager.ParamClassName; }
            set { PathManager.ParamClassName = value; }
        }

        public string ParameterClassLocation {
            get { return PathManager.ParamLocation; }
            set { PathManager.ParamLocation = value; }
        }

        public string ClassLocation {
            get { return PathManager.ClassLocation; }
            set { PathManager.ClassLocation = value; }
        }

        public string DataLocation {
            get { return PathManager.DataLocation; }
            set { PathManager.DataLocation = value; }
        }

        public string ParameterClassFullName {
            get { return ParameterClassLocation + "/" + ParameterClassName + ".cs"; }
        }

        public PathManager PathManager { get; private set; }

        public G2UConfig() {
            PathManager = new PathManager();
        }

        public bool LoadConfig() {
            var config = SaveLoadManager.LoadConfig();
            if(config == null) return false;
            WasInizialized = config.WasInizialized;
            GoogleSheetData = config.GoogleSheetData;
            SkipRowPrefix = config.SkipRowPrefix;
            CommentColumnTitle = config.CommentColumnTitle;
            ArraySeparator = config.ArraySeparator;
            return true;
        }
        public void SaveConfig()
        {
             SaveLoadManager.SaveConfig(this);
        }

        private static G2UConfig CreateDefault() {
         
            return new G2UConfig {
                WasInizialized = false,
                SkipRowPrefix = "__",
                CommentColumnTitle = "Comment",
                ParameterClassName = "Param",
                ParameterClassLocation = "./Assets/Scripts",
                GoogleSheetData = new List<GoogleSheetData> {
                    GoogleSheetIntergation.GoogleSheetData.CreateDefaultData()
                },
                ArraySeparator = "|",
            };
        }
        
        public void Inizialize() {
            WasInizialized = true;
        }
    }
}