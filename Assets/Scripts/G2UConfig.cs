using System.Collections.Generic;

namespace G2U {
    public class G2UConfig {
        public List<GoogleSheetData> GoogleSheetData;
        public string Namespace;
        public string SkipRowPrefix;
        public string ParameterClassName;
        public string ParameterClassLocation;


        public static G2UConfig CreateDefaultConfig() {
            return new G2UConfig {
                Namespace = "EternalMaze.Configs",
                GoogleSheetData = G2U.GoogleSheetData.CreateDefaultData(),
                SkipRowPrefix = "__",
                ParameterClassName = "CellType",
                ParameterClassLocation = PathManager.GetCellTypeDataPath().DirectoryName

            };
        }
    }
}