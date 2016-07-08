#if UNITY_EDITOR
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Assets.GoogleToUnityIntegration.Scripts;
using EternalMaze.EditorWindows;
using Microsoft.CSharp;
using UnityEditor;
using UnityEngine;


namespace G2U {
    [ExecuteInEditMode]
    public class G2UEditor : EditorWindow {
        private readonly Vector2 _minSize = new Vector2(430, 200);
        private readonly int _margin = 15;


        private static G2UConfig _g2uConfig;
        private EditorExtension _ex = new EditorExtension();
        private DataType _dataType = DataType.XML;

        [MenuItem("LoadGoogle/Load")]
        public static void Init() {
            // Get existing open window or if none, make a new one:
            var window = (G2UEditor) GetWindow(typeof(G2UEditor));
            window.Show();
            _g2uConfig = null;
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
            if(_g2uConfig == null) {
                _g2uConfig = SaveLoadManager.LoadConfig();
                if(_g2uConfig == null) { return false; }
            }
            return _g2uConfig.WasInizialized;
        }

        /// <summary>
        /// Draw settings and allow to create new config. 
        /// It saves on PathManager.ConfigFileInfo.FullName
        /// </summary>
        private void DrawSettingsMenu() {
            SettingsMenu();
            _ex.Button("Create config", () => {
                ColorManager.Reset();
                _g2uConfig.Inizialize();
                SaveLoadManager.SaveConfig(_g2uConfig);
            });
        }

        #region Settings menu

        /// <summary>
        /// Draw base setting parameters
        /// </summary>
        private void SettingsMenu() {
            if(_g2uConfig == null) { _g2uConfig = G2UConfig.CreateDefault(); }
            _ex.DrawVertical(() => {
                _g2uConfig.Namespace = _ex.TextField("Namespace", _g2uConfig.Namespace);
                _g2uConfig.SkipRowPrefix = _ex.TextField("Skip prefix", _g2uConfig.SkipRowPrefix);
                _g2uConfig.ParameterClassName = _ex.TextField("Parameter class name", _g2uConfig.ParameterClassName);
                _g2uConfig.ParameterClassLocation = _ex.TextField("Parameter class location",
                    _g2uConfig.ParameterClassLocation);
                _g2uConfig.ClassLocation = _ex.TextField("Class location", _g2uConfig.ClassLocation);
                _g2uConfig.DataLocation = _ex.TextField("Data location", _g2uConfig.DataLocation);
                _g2uConfig.CommentColumnTitle = _ex.TextField("Comment column title", _g2uConfig.CommentColumnTitle);
                _g2uConfig.DataExtension = _ex.TextField("Data extension", _g2uConfig.DataExtension);
                _g2uConfig.SetAccessModifiers = _ex.EnumPopUp("Set accessModifier", "", _g2uConfig.SetAccessModifiers, true, 145);
                if(_ex.Foldout("Extensions", "extensionsKey", true)) {
                    ShowGoogleSheetDataControl();
                    DrawGoogleSheetDataList();
                }
            });
        }

        /// <summary>
        /// Draw buttons to Create/Remove GoogleSheetData
        /// </summary>
        private void ShowGoogleSheetDataControl() {
            _ex.DrawHorizontal(() => {
                _ex.Button("+", AddGoogleSheetData);
                _ex.Button("-", RemoveGoogleSheetData);
            });
        }

        private void AddGoogleSheetData() {
            _g2uConfig.GoogleSheetData.Add(GoogleSheetData.CreateDefaultData());
        }

        private void RemoveGoogleSheetData() {
            if(_g2uConfig.GoogleSheetData.Any()) {
                _g2uConfig.GoogleSheetData.RemoveAt(_g2uConfig.GoogleSheetData.Count - 1);
            }
        }

        private void DrawGoogleSheetDataList() {
            foreach(var googleSheetData in _g2uConfig.GoogleSheetData) {
                DrawGoogleSheetData(googleSheetData);
            }
        }

        private void DrawGoogleSheetData(GoogleSheetData data) {
            _ex.DrawVertical(() => {
                data.GoogleDataName = _ex.TextField("Sheet Name", data.GoogleDataName);
                data.GoogleDriveFileGuid = _ex.TextField("GoogleDriveFileGuid", data.GoogleDriveFileGuid);
                data.GoogleDriveSheetGuid = _ex.TextField("GoogleDriveSheetGuid", data.GoogleDriveSheetGuid);
            }, bgColor: ColorManager.GetColor(), border: true);
        }

        #endregion

        #region Base Menu

        private void BaseMenu() {
            ShowMainMenu();
            // draw settings. Here you can reload or save config
            if(_ex.Foldout("Settings", "settingsKey", true)) {
                SettingsMenu();
                _ex.Button("Load config", () => { _g2uConfig = SaveLoadManager.LoadConfig(); });
                _ex.Button("Save current config", () => SaveLoadManager.SaveConfig(_g2uConfig));
            }
        }

        private void ShowMainMenu() {
            LoadSheetAndGenerateData();
            LoadSheetAndGenerateClass();
        }

        private void LoadSheetAndGenerateData() {
            _ex.DrawHorizontal(() => 
            {
                _dataType = _ex.EnumPopUp("Data type", "dataType", _dataType, true, 80, 120);
                _ex.Button("Load Google Sheets and save it", () => {
                    GoogleSheetLoaderEditor.LoadSheet(_g2uConfig.GoogleSheetData,
                        () => {
                            try {
                                GoogleDataParser.ParseSheet(GoogleSheetLoaderEditor.DataFromGoogle,
                                    _g2uConfig.GoogleSheetData);
                                GenerateData();
                            }
                            catch(Exception e) {
                                Debug.LogError(e.Message);
                            }
                        });
                }, position.width - 200 - _margin, 15);
            });

            if(_dataType == DataType.ScriptableObject) {
                _ex.Button("Generate scriptable object asset", () => {
                    var selections = Selection.objects;
                    _g2uConfig.PathManager.CreateDataFolder();
                    var directory = _g2uConfig.PathManager.GetDataFolder();
                    foreach(var selection in selections) {
                        MonoScript ms = selection as MonoScript;
                        if(ms == null) continue;

                        var t = ms.GetClass().BaseType;
                        Debug.Log(t);
                        ScriptableObject scriptableObject = CreateInstance(ms.GetClass());
//                        var d = Convert.ChangeType(scriptableObject, t);
                       
                        var path = PathManager.GetProjectRelativPath(new FileInfo(Path.Combine(directory.FullName, string.Format("{0}.asset", ms.GetClass().Name))));
                        AssetDatabase.CreateAsset(scriptableObject, path);
                        AssetDatabase.SaveAssets();
                        EditorUtility.FocusProjectWindow();
                        Selection.activeObject = scriptableObject;
                    }
                });
            }
        }


        private void GenerateData() {
            var dataFiles = new List<string>();
            var generator = AbstractFileBuilder.GetDataBuilder(_g2uConfig, _dataType);
            if(generator == null) {
                Debug.LogError("Cannot generate file. File generator is null");
                return;
            }
            _g2uConfig.PathManager.CreateFolder(GetDataDirectory());
            for(var i = 0; i < _g2uConfig.GoogleSheetData.Count; i++) {
                var data = generator.GenerateFileList(GoogleDataParser.ParsedData[i]);
                SaveLoadManager.SaveData(GetDataDirectory(), GetDataExtension(), data, _dataType);
                dataFiles.AddRange(data.Select(j => j.Key));
            }
            GenerateParameterClass(dataFiles);
            Debug.Log("Data was successful generated");
        }

        private DirectoryInfo GetDataDirectory() {
            if(_dataType == DataType.ScriptableObject)
                return _g2uConfig.PathManager.GetClassFolder();
            return _g2uConfig.PathManager.GetDataFolder();
        }
        private string GetDataExtension() {
            switch(_dataType) {
                case DataType.ScriptableObject:
                    return ".cs";
            }
            return _g2uConfig.DataExtension;
        }

        private void GenerateParameterClass(List<string> data) {
            _g2uConfig.PathManager.CreateParameterFolder();
            var file = new StringBuilder();
            file.AppendLine(string.Format("namespace {0} {{", _g2uConfig.Namespace));
            file.AppendLine(string.Format("{0}internal class {1} {{", AbstractFileBuilder.GetTabulator(1),
                _g2uConfig.ParameterClassName));
            foreach(var d in data) {
                var path = new FileInfo(Path.Combine(_g2uConfig.DataLocation, d));
                var resourcesPath = PathManager.GetResourcesPath(path);
                file.Append(string.Format("{0}public const string {1}Path = \"{2}\";\r\n",
                    AbstractFileBuilder.GetTabulator(2), d, resourcesPath));
            }
            file.Append(string.Format("{0}}}\r\n{1}}}", AbstractFileBuilder.GetTabulator(1),
                AbstractFileBuilder.GetTabulator(0)));
            SaveLoadManager.SaveFile(_g2uConfig.ParameterClassFullName, file.ToString());
        }

        private void LoadSheetAndGenerateClass() {
            _ex.Button("Load Google Sheets and generate class", () => {
                GoogleSheetLoaderEditor.LoadSheet(_g2uConfig.GoogleSheetData,
                    () => {
                        try {
                            GoogleDataParser.ParseSheet(GoogleSheetLoaderEditor.DataFromGoogle,
                                _g2uConfig.GoogleSheetData);
                            GenerateClass();
                        }
                        catch(Exception e) {
                            Debug.LogError(e.Message);
                        }
                    });
            });
        }

        private void GenerateClass() {
            var generator = AbstractFileBuilder.GetClassBuilder(_g2uConfig);
            if(generator == null) {
                Debug.LogError("Cannot generate file. File generator is null");
                return;
            }
            _g2uConfig.PathManager.CreateClassFolder();
            for(var i = 0; i < _g2uConfig.GoogleSheetData.Count; i++) {
                var @class = generator.GenerateFileList(GoogleDataParser.ParsedData[i]);
                SaveLoadManager.SaveClass(_g2uConfig.PathManager.GetClassFolder(), @class);
            }
            Debug.Log("Classes was successful generated");
        }

        #endregion
    }
}

#endif