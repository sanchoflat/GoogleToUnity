#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assets.GoogleToUnityIntegration.Scripts;
using EternalMaze.EditorWindows;
using UnityEditor;
using UnityEngine;


namespace G2U {
    [ExecuteInEditMode]
    public class G2UEditor : EditorWindow {
        private static G2UConfig _g2uConfig;
        private readonly EditorExtension _ex = new EditorExtension();
        private AbstractFileBuilder.DataType _dataType = AbstractFileBuilder.DataType.XML;

        [MenuItem("LoadGoogle/Load")]
        public static void Init() {
            // Get existing open window or if none, make a new one:
            var window = (G2UEditor) GetWindow(typeof(G2UEditor));
            window.Show();
            _g2uConfig = null;
        }

        private void OnGUI() {
            MenuDrawer();
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
        /// Dra settings and allow to create new config. 
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
            _g2uConfig.PathManager.CreateDataFolder();
            _g2uConfig.PathManager.CreateClassFolder();
            _ex.DrawHorizontal(() => {
                _dataType = _ex.EnumPopUp("Data type", "dataType", _dataType, true);
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
                });
            });
        }

        private void LoadSheetAndGenerateClass() {
            _g2uConfig.PathManager.CreateClassFolder();
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

        #region Generation

        private void GenerateData() {
            var dataFiles = new List<string>();
            var generator = AbstractFileBuilder.GetDataBuilder(_g2uConfig, _dataType);
            if(generator == null) {
                EditorUtility.DisplayDialog("Error", "Cannot generate file. File generator is null", "Ok");
                return;
            }
            for(var i = 0; i < _g2uConfig.GoogleSheetData.Count; i++) {
                var data = generator.GenerateFiles(GoogleDataParser.ParsedData[i]);
                SaveLoadManager.SaveData(_g2uConfig.PathManager.GetDataDirectory(), _g2uConfig.DataExtension, data);
                dataFiles.AddRange(data.Select(j => j.Key));
            }
            GenerateParameterClass(dataFiles);
            EditorUtility.DisplayDialog("", "Data was successful generated", "Ok");
        }

        private void GenerateClass() {
            var generator = AbstractFileBuilder.GetClassBuilder(_g2uConfig);
            if(generator == null) {
                EditorUtility.DisplayDialog("Error", "Cannot generate file. File generator is null", "Ok");
                return;
            }
            for(var i = 0; i < _g2uConfig.GoogleSheetData.Count; i++) {
                var @class = generator.GenerateFiles(GoogleDataParser.ParsedData[i]);
                SaveLoadManager.SaveClass(_g2uConfig.PathManager.GetClassDirectory(), @class);
            }
            EditorUtility.DisplayDialog("", "Classes was successful generated", "Ok");
        }

        private void GenerateParameterClass(List<string> data) {
            var file = new StringBuilder();
            file.AppendLine(string.Format("namespace {0} {{", _g2uConfig.Namespace));
            file.AppendLine(string.Format("{0}internal class {1} {{", AbstractFileBuilder.GetTabulator(1),
                _g2uConfig.ParameterClassName));
            foreach(var d in data) {
                var path = new FileInfo(Path.Combine(_g2uConfig.DataLocation, d));
                var resourcesPath = PathManager.GetResourcesPath(path).Replace("\\", "\\\\");
                file.Append(string.Format("{0}public const string {1}Path = \"{2}\";\n",
                    AbstractFileBuilder.GetTabulator(2), PathManager.PrepareFileName(d, true),
                    PathManager.PrepareFileName(resourcesPath, false)));
            }
            file.Append(string.Format("{0}}}\n{1}}}", AbstractFileBuilder.GetTabulator(1),
                AbstractFileBuilder.GetTabulator(0)));
            SaveLoadManager.SaveFile(_g2uConfig.ParameterClassFullName, file.ToString());
        }

        #endregion

        #endregion

    }
}

#endif