﻿using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;


namespace GoogleSheetIntergation {
    public class G2UConfig {

        public static G2UConfig Instance {
            get {
                if(_instance == null) { _instance = CreateDefault(); }
                return _instance;
            }
        }

        private static G2UConfig _instance;

        public List<GoogleSheetData> GoogleSheetData { get; set; }
        public bool WasInizialized { get; set; }
        public string SkipRowPrefix { get; set; }
        public string CommentColumnTitle { get; set; }
        public string ArraySeparator { get; set; }
        public string TypeColumnName { get; set; }
        public bool UseTypeColumn { get; set; }
       
        public ConstantsClassBuilder ConstantsClassBuilder = new ConstantsClassBuilder();

        public string ConstantClassName {
            get { return PathManager.ConstantClassName; }
            set { PathManager.ConstantClassName = value; }
        }

        public string ConstantClassLocation {
            get { return PathManager.ConstantClassLocation; }
            set { PathManager.ConstantClassLocation = value; }
        }

        public string ParameterClassFullName {
            get { return ConstantClassLocation + "/" + ConstantClassName + ".cs"; }
        }

        public PathManager PathManager { get; private set; }

        public G2UConfig() {
            PathManager = new PathManager();
        }

        public bool LoadConfig() {
            var config = SaveLoadManager.LoadConfig();
            if(config == null) { return false; }
            WasInizialized = config.WasInizialized;
            GoogleSheetData = config.GoogleSheetData;
            SkipRowPrefix = config.SkipRowPrefix;
            CommentColumnTitle = config.CommentColumnTitle;
            ArraySeparator = config.ArraySeparator;
            PathManager.ConstantClassName = config.ConstantClassName;
            PathManager.ConstantClassLocation = config.ConstantClassLocation;
            TypeColumnName = config.TypeColumnName;
            UseTypeColumn = config.UseTypeColumn;
            return true;
        }

        public void SaveConfig() {
            SaveLoadManager.SaveConfig(this);
        }

        private static G2UConfig CreateDefault() {
            return new G2UConfig {
                WasInizialized = false,
                SkipRowPrefix = "__",
                CommentColumnTitle = "Comment",
                ConstantClassName = "Constants",
                ConstantClassLocation = "./Assets/Scripts",
                TypeColumnName = "Type",
                GoogleSheetData = new List<GoogleSheetData> {
                    GoogleSheetIntergation.GoogleSheetData.CreateDefaultData()
                },
                ArraySeparator = "|"
            };
        }

        public void Inizialize() {
            WasInizialized = true;
        }

        public static G2UConfig DeserializeXMLFromPath(string path) {
            var serializer = new XmlSerializer(typeof(G2UConfig));
            var reader = new StreamReader(path);
            var config = (G2UConfig) serializer.Deserialize(reader);
            reader.Close();
            return config;
        }

        public void SerializeToXML(string path) {
            var ser = new XmlSerializer(typeof(G2UConfig));
            using(var sww = new StringWriter()) {
                using(var writer = XmlWriter.Create(sww)) {
                    ser.Serialize(writer, this);
                    var xml = sww.ToString();
                    File.WriteAllText(path, xml);
                }
            }
        }
    }
}