﻿using System.Collections.Generic;


namespace G2U {
    public class G2UConfig {
        public List<GoogleSheetData> GoogleSheetData { get; set; }
        public bool WasInizialized { get; set; }
        public string Namespace { get; set; }
        public string SkipRowPrefix { get; set; }
        public string CommentColumnTitle { get; set; }
        public string DataExtension { get; set; }

        public string ParameterClassName {
            get { return PathManager.ParamClassName; }
            set { PathManager.ParamClassName = value; }
        }

        public string ParameterClassLocation {
            get { return PathManager.ParamLocation; }
            set { PathManager.ParamLocation = value; }
        }

        public string ClassLocation {
            get { return PathManager.ClassFolderCurrent; }
            set { PathManager.ClassFolderCurrent = value; }
        }

        public string DataLocation {
            get { return PathManager.DataLocation; }
            set { PathManager.DataLocation = value; }
        }

        public string ParameterClassFullName {
            get { return ParameterClassLocation + "/" + ParameterClassName + ".cs"; }
        }

        public PathManager PathManager;

        public G2UConfig() {
            PathManager = new PathManager();
        }

        public static G2UConfig CreateDefault() {
            return new G2UConfig {
                WasInizialized = false,
                Namespace = "G2U",
                SkipRowPrefix = "__",
                CommentColumnTitle = "Comment",
                ParameterClassName = "Param",
                ParameterClassLocation = "./Assets/Scripts",
                GoogleSheetData = new List<GoogleSheetData> {
                    G2U.GoogleSheetData.CreateDefaultData()
                },
                DataExtension = ".txt"
            };
        }

        public void Inizialize() {
            PathManager.CreateAllFolders();
            WasInizialized = true;
        }
    }
}