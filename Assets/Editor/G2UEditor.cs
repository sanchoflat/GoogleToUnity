using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EternalMaze.EditorWindows;
using UnityEditor;
using UnityEngine;

namespace G2U {
    [ExecuteInEditMode]
    public class G2UEditor : EditorWindow {
        private static G2UConfig _g2uConfig;
        private readonly EditorExtension _ex = new EditorExtension();


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
                if(CheckInitialization()) {
                    WorkMenu();
                }
                else {
                    Inizialize();
                }
            }, scroll: true);
        }

        #region Init

        /// <summary>
        ///     Шаг 1. Загрузить конфиг и проверить его
        /// </summary>
        private bool CheckInitialization() {
            if(_g2uConfig == null) {
                _g2uConfig = LoadSaveManager.LoadConfig();
                if(_g2uConfig == null) return false;
            }
            if(_g2uConfig.WasInizialized)
                CheckForFolders();
            return _g2uConfig.WasInizialized;
        }

        /// <summary>
        ///     Шаг 2. Необходимо инициализировать конфиг.
        /// </summary>
        private void Inizialize() {
            if(_g2uConfig == null)
                _g2uConfig = G2UConfig.CreateDefaultConfig();
            _g2uConfig.WasInizialized = true;
            PathManager.CreateClassFolder(_g2uConfig);
            PathManager.CreateJSONDataFolder(_g2uConfig);
            PathManager.CreateConfigFolder();
            LoadSaveManager.SaveConfig(_g2uConfig);
        }

        #endregion

        /// <summary>
        ///     Шаг 3. Конфиг инициализирован. Можно начать работу.
        /// </summary>
        private void WorkMenu() {
            ShowMainMenu();
            ShowGoogleDataItem();
        }

        /// <summary>
        ///     Показывает основное меню: кнопки загрузки
        /// </summary>
        private void ShowMainMenu() {
            LoadDataAndSaveToJson();
            LoadDataAndGenerateClass();
            LoadDataGenerateClassAndSaveToJson();
        }

        private void LoadDataAndSaveToJson() {
            _ex.Button("Скачать Google Sheets и сохранить в JSON", () => {
                GoogleSheetLoaderEditor.LoadSheet(_g2uConfig.GoogleSheetData,
                    () => {
                        try {
                            var text = GoogleDataParser.ParseSheet(GoogleSheetLoaderEditor.DataFromGoogle,
                                _g2uConfig.GoogleSheetData);
                            CheckForFolders();
                            GenerateJSON();
                        }
                        catch(Exception e) {
                            Debug.LogError(e.Message);
                        }
                    });
            });
        }

        private void LoadDataAndGenerateClass() {
            _ex.Button("Скачать Google Sheets и сгенерировать новый класс", () => {
                GoogleSheetLoaderEditor.LoadSheet(_g2uConfig.GoogleSheetData,
                    () => {
                        try {
                            var text = GoogleDataParser.ParseSheet(GoogleSheetLoaderEditor.DataFromGoogle,
                                _g2uConfig.GoogleSheetData);
                            CheckForFolders();
                            GenerateClass();
                        }
                        catch(Exception e) {
                            Debug.LogError(e.Message);
                        }
                    });
            });
        }

        private void LoadDataGenerateClassAndSaveToJson() {
//            _ex.Button("Скачать Google Sheets, сгенерировать новый класс и сохранить данные в JSON", () => {
//                GoogleSheetLoaderEditor.LoadSheet(_g2uConfig.GoogleSheetData,
//                    () => {
//                        var text = GoogleDataParser.ParseSheet(GoogleSheetLoaderEditor.DataFromGoogle,
//                            _g2uConfig.GoogleSheetData);
//                        CheckForFolders();
//                    });
//            });
        }



        #region Dopolnitelno

        private void ShowGoogleDataItem() {
            if(_ex.Foldout("Дополнительно", "qwerty", true)) {
                DrawOptionsButton();
                ShowGoogleSheetDataControl();
                DrawGoogleSheetDataList();
            }
        }


        private void DrawOptionsButton() {
            _ex.Button("Сохранить текущие настройки", SaveCurrentGoogleData);
            _ex.Button("Загрузить настройки по дефолту", LoadDefaultGoogleData);
            _ex.Button("Загрузить настройки", LoadSavedGoogleData);
        }

        private void ShowGoogleSheetDataControl() {
            _ex.DrawHorizontal(() => {
                _ex.Button("+", AddGoogleSheetData);
                _ex.Button("-", RemoveGoogleSheetData);
            });
        }

        private void DrawGoogleSheetDataList() {
            _g2uConfig.Namespace = _ex.TextField("Namespace", _g2uConfig.Namespace);
            _g2uConfig.SkipRowPrefix = _ex.TextField("Skip prefix", _g2uConfig.SkipRowPrefix);
            _g2uConfig.ParameterClassName = _ex.TextField("Parameter class name", _g2uConfig.ParameterClassName);
            _g2uConfig.ParameterClassLocation = _ex.TextField("Parameter class location", _g2uConfig.ParameterClassLocation);
            if(_ex.Foldout("Google Sheet Data", "GoogleSheetDataFoldout", true)) {
             
                foreach(var googleSheetData in _g2uConfig.GoogleSheetData) {
                    DrawGoogleSheetData(googleSheetData);
                }
            }
        }

        private void AddGoogleSheetData() {
            _g2uConfig.GoogleSheetData.Add(new GoogleSheetData());
        }

        private void RemoveGoogleSheetData() {
            if(_g2uConfig.GoogleSheetData.Any()) {
                _g2uConfig.GoogleSheetData.RemoveAt(_g2uConfig.GoogleSheetData.Count - 1);
            }
        }

        private void DrawGoogleSheetData(GoogleSheetData data) {
            _ex.DrawVertical(() => {
                data.GoogleDataName = _ex.TextField("Sheet Name", data.GoogleDataName);
                data.ClassLocation = _ex.TextField("Class Location", data.ClassLocation);
                data.JSONDataLocation = _ex.TextField("JSON Data Location", data.JSONDataLocation);
                data.GoogleDriveFileGuid = _ex.TextField("GoogleDriveFileGuid", data.GoogleDriveFileGuid);
                data.GoogleDriveSheetGuid = _ex.TextField("GoogleDriveSheetGuid", data.GoogleDriveSheetGuid);
                data.SkipEmptyLines = _ex.Toggle("SkipEmptyLines", data.SkipEmptyLines);
             
            }, bgColor: GetGoogleDataGUIColor(), border: true);
        }

        #endregion

        #region Save/Load

        private void SaveCurrentGoogleData() {
            SaveCurrentDataAsDefault(_g2uConfig);
        }

        private void SaveCurrentDataAsDefault(G2UConfig config) {
            LoadSaveManager.SaveConfig(config);
        }

        private void LoadDefaultGoogleData() {
            _g2uConfig = G2UConfig.CreateDefaultConfig();
        }

        private void LoadSavedGoogleData() {
            _g2uConfig = LoadSaveManager.LoadConfig();
        }

        #endregion

        #region Util

        private void CheckForFolders() {
            PathManager.CreateJSONDataFolder(_g2uConfig);
            PathManager.CreateClassFolder(_g2uConfig);
        }

        #endregion

        #region Generation

        private void GenerateAndSaveEmptyClass(List<GoogleSheetData> googleData) {
            foreach(var googleSheetData in _g2uConfig.GoogleSheetData) {
                var @class = GenerateEmptyClass(googleSheetData);
                LoadSaveManager.SaveClass(@class, googleSheetData.GetClassDirectory().FullName);
            }
            EditorUtility.DisplayDialog("Пустые классы были успешно сгенерированы", "", "Ok");
            Debug.Log("Пустые классы были успешно сгенерированы");
        }

        private string GenerateEmptyClass(GoogleSheetData googleData) {
//            AbstractFileBuilder cg = AbstractFileBuilder.GetClassBuilder(googleData.ClassName, _g2uConfig.Namespace);
//            return cg.GetEmptyClass();
            return null;
        }

        private void GenerateJSON() {
            List<string> jsonFiles = new List<string>();
            for(var i = 0; i < _g2uConfig.GoogleSheetData.Count; i++) {
                var generator = AbstractFileBuilder.GetJsonBuilder(_g2uConfig, i);
                if(generator == null) {
                    Debug.Log("Нельзя сгенерировать файл");
                    continue;
                }
                var @json = generator.GenerateFiles(GoogleDataParser.ParsedData[i]);
                LoadSaveManager.SaveJSON(@json, _g2uConfig.GoogleSheetData[i].GetJSONDataDirectory());

                foreach(var j in json) {
                    jsonFiles.Add(j.Key);
                }
                
            }
            GenerateParameterClassForPath(jsonFiles, _g2uConfig.GoogleSheetData[0].GetJSONDataDirectory());
            EditorUtility.DisplayDialog("", "JSON успешно сгенерирован", "Ok"); 
        }

        private void GenerateClass() {
            for(var i = 0; i < _g2uConfig.GoogleSheetData.Count; i++) {
                var generator = AbstractFileBuilder.GetClassBuilder(_g2uConfig, i);
                if(generator == null) {
                    Debug.Log("Нельзя сгенерировать файл");
                    continue;
                }
                var @class = generator.GenerateFiles(GoogleDataParser.ParsedData[i]);
                LoadSaveManager.SaveClass(@class, _g2uConfig.GoogleSheetData[i].GetClassDirectory());
            }
            EditorUtility.DisplayDialog("", "Классы успешно сгенерированы", "Ok"); 
        }


        private void GenerateParameterClassForPath(List<string> @json, FileInfo fileInfo)
        {
            PathManager.CreateCellTypeFolder(_g2uConfig);
            StringBuilder file = new StringBuilder();
            file.AppendLine(string.Format("namespace {0} {{", _g2uConfig.Namespace));
            file.AppendLine(String.Format("{0}internal class {1} {{", AbstractFileBuilder.GetTabulator(1), _g2uConfig.ParameterClassName));

            foreach (var d in @json)
            {
                var path = new FileInfo(Path.Combine(fileInfo.FullName, d));
                var resourcesPath = PathManager.GetResourcesPath(path).Replace("\\", "\\\\");

                file.Append(String.Format("{0}public const string {1}Path = \"{2}\";\n", AbstractFileBuilder.GetTabulator(2), d, resourcesPath));
            }
            file.Append(String.Format("{0}}}\n{1}}}", AbstractFileBuilder.GetTabulator(1), AbstractFileBuilder.GetTabulator(0)));
            LoadSaveManager.SaveCellType(file.ToString(), PathManager.GetCellTypeDataPath(_g2uConfig, _g2uConfig.ParameterClassName).FullName);
        }

        #endregion

        #region Color control

        private int _colorCounter;

        private Color GetGoogleDataGUIColor() {
            _colorCounter ++;
            if(_colorCounter % 2 == 0) {
                return new Color(.7f, 0, 0, 0.3f);
            }
            return new Color(0, .7f, 0, 0.3f);
        }

        #endregion

        /// <summary>
        ///     Класс загрузки и выгрузки данных
        /// </summary>
        private static class LoadSaveManager {
            public static G2UConfig LoadConfig() {
                G2UConfig config = null;
                if(!PathManager.GetConfigPath().Exists) {
                    return null;
                }
                return config.LoadJSONFromFile(PathManager.GetConfigPath().FullName);
            }

            public static void SaveConfig(G2UConfig config) {
                var path = PathManager.GetConfigPath().FullName;
                config.SaveJSONToFile(path);
            }

            public static void SaveClass(Dictionary<string, string> @class, FileInfo path) {
                foreach(var d in @class) {
                    var p = Path.Combine(path.FullName, d.Key + ".cs");
                    SaveClass(p, @d.Value);
                }
            }

            public static void SaveClass(string path, string @class) {
                File.WriteAllText(path, @class);
            }

            public static void SaveJSON(Dictionary<string, string> @json, FileInfo path) {
                foreach(var d in @json) {
                    var p = Path.Combine(path.FullName, d.Key + ".txt");
                    SaveJSON(p, @d.Value);
                }
            }

            public static void SaveCellType(string file, string path) {
                SaveJSON(path, file);
            }

            public static void SaveJSON(string path, string @json) {
                File.WriteAllText(path, @json);
            }
        }
    }
}