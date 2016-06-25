using System.Collections.Generic;
using UnityEngine;

namespace G2U {
    public class G2UConfig {
        public List<GoogleSheetData> GoogleSheetData;
        public string Namespace;
        public bool WasInizialized;


        public static G2UConfig CreateDefaultConfig() {
            return new G2UConfig {
                Namespace = "",
                WasInizialized = true,
                GoogleSheetData = G2U.GoogleSheetData.CreateDefaultData(),
            };
        }
    }
}