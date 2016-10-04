#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.GoogleToUnityIntegration.Scripts;
using EternalMaze.EditorWindows;
using UnityEditor;
using UnityEngine;


namespace GoogleSheetIntergation {
    [ExecuteInEditMode]
    public class GoogleSheetLoader : EditorWindow {
        private readonly Vector2 _minSize = new Vector2(430, 200);
        private readonly EditorExtension _ex = new EditorExtension();

        [MenuItem("Tools/Google Sheet Integration")]
        public static void Init() {
            // Get existing open window or if none, make a new one:
            var window = (GoogleSheetLoader) GetWindow(typeof(GoogleSheetLoader));
            window.Show();
        }

        private void OnGUI() {
            InitWindow();
            MenuDrawer();
        }

        private void InitWindow() {
            minSize = _minSize;
        }

        private void MenuDrawer() {
            _ex.DrawVertical(() => {
                if(!CheckInitialization()) {
                    DrawSettingsMenu();
                }
                else {
                    BaseMenu();
                }
            }, scroll: true);
        }

        /// <summary>
        ///     Check if plugin was inizialized
        /// </summary>
        private bool CheckInitialization() {
            if(!G2UConfig.Instance.WasInizialized) {
                if(G2UConfig.Instance.LoadConfig()) {
                    return G2UConfig.Instance.WasInizialized;
                }
                return false;
            }
            return G2UConfig.Instance.WasInizialized;
        }

        /// <summary>
        ///     Draw settings and allow to create new config.
        ///     It saves on PathManager.ConfigFileInfo.FullName
        /// </summary>
        private void DrawSettingsMenu() {
            _ex.Button("Create config", () => {
                ColorManager.Reset();
                G2UConfig.Instance.Inizialize();
                G2UConfig.Instance.SaveConfig();
            });
        }

        #region Settings menu

        /// <summary>
        ///     Draw base setting parameters
        /// </summary>
        private void GoogleSheetDataMenu() {
            _ex.DrawVertical(() => {
                if(_ex.Foldout("Google sheet data", visualize: true)) {
                    DrawGoogleSheetDataList();
                    ShowGoogleSheetDataControl();
                }
            });
        }

        private void ShowGoogleSheetDataControl() {
            _ex.DrawHorizontal(() => {
                _ex.Button("+", AddGoogleSheetData);
                _ex.Button("-", RemoveGoogleSheetData);
            });
        }

        private void AddGoogleSheetData() {
            if(G2UConfig.Instance.GoogleSheetData.Any()) {
                G2UConfig.Instance.GoogleSheetData.Add(G2UConfig.Instance.GoogleSheetData.Last().Clone());
            }
            else {
                G2UConfig.Instance.GoogleSheetData.Add(GoogleSheetData.CreateDefaultData());
            }
        }

        private void RemoveGoogleSheetData() {
            if(G2UConfig.Instance.GoogleSheetData.Any()) {
                G2UConfig.Instance.GoogleSheetData.RemoveAt(G2UConfig.Instance.GoogleSheetData.Count - 1);
            }
        }

        private void DrawGoogleSheetDataList() {
            for(var i = 0; i < G2UConfig.Instance.GoogleSheetData.Count; i++) {
                DrawGoogleSheetData(G2UConfig.Instance.GoogleSheetData[i], i);
            }
        }

        private void DrawGoogleSheetData(GoogleSheetData data, int counter) {
            _ex.DrawVertical(() => {
                data.GoogleDataName = _ex.TextField("Sheet Name", data.GoogleDataName);
                if(_ex.Foldout("Parameters", data.GoogleDataName + "paramExpander", true)) {
                    data.GoogleSheetFileGuid = _ex.TextField("Google Sheet File Guid", data.GoogleSheetFileGuid);
                    data.GoogleSheetGuid = _ex.TextField("Google Sheet Guid", data.GoogleSheetGuid);
                    data.DataExtension = _ex.TextField("Data extension", data.DataExtension);
                    data.DataType = _ex.EnumPopUp("Data type", "Data type" + data.GoogleSheetGuid, data.DataType,
                        textWidth: 145);
                    data.Namespace = _ex.TextField("Namespace", data.Namespace);
                    if(data.DataLocation == null || data.ClassLocation == null) { data.CreateDefaultPath(); }
                    data.ClassLocation = _ex.TextField("Class location", data.ClassLocation);
                    data.DataLocation = _ex.TextField("Data location", data.DataLocation);
                    CheckForLocationEnding(data);
                    AccessModifiers(data);
                }
                _ex.Button("Generate class file", () => { GenerateClassFile(data); });
                _ex.Button(string.Format("Generate {0} file", data.DataType), () => { GenerateDataFile(data);});
                _ex.Button("Remove", () => { G2UConfig.Instance.GoogleSheetData.RemoveAt(counter); });
            }, bgColor: ColorManager.GetColor(), border: true);
        }

        private void CheckForLocationEnding(GoogleSheetData data) {
            var cLoc = data.ClassLocation;
            if(cLoc.EndsWith("/")) {
                data.ClassLocation = cLoc.Substring(0, cLoc.Length - 1);
            }
            var dLoc = data.DataLocation;
            if(dLoc.EndsWith("/")) {
                data.DataLocation = dLoc.Substring(0, dLoc.Length - 1);
            }
        }

        private void AccessModifiers(GoogleSheetData data) {
            if(data.DataType == DataType.ScriptableObject) {
                _ex.SetEnumPopUpValue("VariableType" + data.GoogleSheetGuid, VariableType.Field);
                _ex.SetEnumPopUpValue("Fieldaccessmodifier" + data.GoogleSheetGuid,
                    GoogleSheetIntergation.AccessModifiers.Public);
                data.DataExtension = ".asset";
                GUI.enabled = false;
            }
            else {
                if(data.DataExtension == ".asset") {
                    data.DataExtension = ".xml";
                }
            }
            data.VariableType = _ex.EnumPopUp("Variable Type", "VariableType" + data.GoogleSheetGuid,
                data.VariableType, textWidth: 145);
            data.FieldAccessModifiers = _ex.EnumPopUp("Field access modifier",
                "Fieldaccessmodifier" + data.GoogleSheetGuid, data.FieldAccessModifiers, textWidth: 145);
            if(data.VariableType == VariableType.Property) {
                data.SetAccessModifiers = _ex.EnumPopUp("Set access modifier",
                    "Set access modifier" + data.GoogleSheetGuid, data.SetAccessModifiers, textWidth: 145);
            }
            GUI.enabled = true;
        }

        #endregion

        #region Base Menu

        private void BaseMenu() {
            ShowMainMenu();
            GoogleSheetDataMenu();
            if(_ex.Foldout("Settings", "settingsKey", true)) {
                G2UConfig.Instance.SkipRowPrefix = _ex.TextField("Skip prefix", G2UConfig.Instance.SkipRowPrefix);
//                G2UConfig.Instance.ConstantClassName = _ex.TextField("Constant class name",
//                    G2UConfig.Instance.ConstantClassName);
//                G2UConfig.Instance.ConstantClassLocation = _ex.TextField("Constant class location",
//                    G2UConfig.Instance.ConstantClassLocation);
//                var parLoc = G2UConfig.Instance.ConstantClassLocation;
//                if(parLoc.EndsWith("/")) {
//                    G2UConfig.Instance.ConstantClassLocation = parLoc.Substring(0, parLoc.Length - 1);
//                }
                G2UConfig.Instance.CommentColumnTitle = _ex.TextField("Comment column title",
                    G2UConfig.Instance.CommentColumnTitle);
                G2UConfig.Instance.ArraySeparator = _ex.TextField("Array separator", G2UConfig.Instance.ArraySeparator);
                _ex.Button("Load config", () => { G2UConfig.Instance.LoadConfig(); });
                _ex.Button("Save current config", () => G2UConfig.Instance.SaveConfig());
            }
        }

        private void ShowMainMenu() {
            GenerateData();
        }

        private void GenerateData() {
            _ex.DrawVertical(() => {
                _ex.Button("Generate class files", GenerateClassFile);
                _ex.Button("Generate data files", GenerateDataFile);
//                _ex.Button("Generate class with constants", G2UConfig.Instance.ConstantsClassBuilder.GenerateConstantClass);
            });
        }

        #region Generate class file

        private void GenerateClassFile() {
            GenerateClassFile(G2UConfig.Instance.GoogleSheetData);
        }

        private void GenerateClassFile(List<GoogleSheetData> data) {
            foreach(var googleSheetData in data) {
                GenerateClassFile(googleSheetData);
            }
        }

        private void GenerateClassFile(GoogleSheetData data) {
            LoadDataFromGoogle(data, (parsedData, sheetData) => {
                for(var i = 0; i < parsedData.Count; i++) {
                    FileBuilder.GenerateClassFile(parsedData, data);
                }
            });
        }

        #endregion

        #region Generate data file

        private void GenerateDataFile() {
            GenerateDataFile(G2UConfig.Instance.GoogleSheetData);
        }

        private void GenerateDataFile(List<GoogleSheetData> data) {
            foreach(var googleSheetData in data) {
                GenerateDataFile(googleSheetData);
            }
        }

        private void GenerateDataFile(GoogleSheetData data) {
            LoadDataFromGoogle(data, (parsedData, sheetData) => {
                for(var i = 0; i < parsedData.Count; i++) {
                    FileBuilder.GenerateData(parsedData, sheetData);
                }
            });
        }

        #endregion

        #region Load data from google

        private void LoadDataFromGoogle(Action<Dictionary<string, Dictionary<string, List<AbstractDataRow>>>,
            GoogleSheetData> onComplete) {
            LoadDataFromGoogle(G2UConfig.Instance.GoogleSheetData, onComplete);
        }

        private void LoadDataFromGoogle(List<GoogleSheetData> googleSheetData,
            Action<Dictionary<string, Dictionary<string, List<AbstractDataRow>>>, GoogleSheetData> onComplete) {
            if(googleSheetData == null || !googleSheetData.Any()) { return; }
            foreach(var sheetData in googleSheetData) {
                LoadDataFromGoogle(sheetData, onComplete);
            }
        }

        private void LoadDataFromGoogle(GoogleSheetData googleSheetData,
            Action<Dictionary<string, Dictionary<string, List<AbstractDataRow>>>, GoogleSheetData> onComplete) {
            if(googleSheetData == null) { return; }
            GoogleSheetLoaderEditor.LoadSheet(googleSheetData,
                (recievedData) => {
                    if(string.IsNullOrEmpty(recievedData)) return;
                    var data = GoogleDataParser.ParseSheet(recievedData, googleSheetData);
                    if(data == null || !data.Any()) return;
                    if(onComplete != null) {
                        onComplete.Invoke(data, googleSheetData);
                    }
                });
        }

        #endregion

        #endregion
    }
}


#endif
