﻿using System.Collections.Generic;

namespace G2U {
    public class G2UConfig {
        public List<GoogleSheetData> GoogleSheetData;
        public string Namespace;
        public string SkipRowPrefix;
        public bool WasInizialized;


        public static G2UConfig CreateDefaultConfig() {
            return new G2UConfig {
                Namespace = "EternalMaze.Configs",
                WasInizialized = true,
                GoogleSheetData = G2U.GoogleSheetData.CreateDefaultData(),
                SkipRowPrefix = "__"
            };
        }
    }
}