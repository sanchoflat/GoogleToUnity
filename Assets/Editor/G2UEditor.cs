using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var window = (G2UEditor) GetWindow(typeof (G2UEditor));
            window.Show();
            _g2uConfig = null;
        }

        private void OnGUI() {
            _ex.DrawVertical(() => {
                if (CheckInitialization()) {
                    // показать меню работы
                    WorkMenu();
                }
                else {
                    Inizialize();
                }
            }, scroll: true);
        }

        #region Шаг 1

        /// <summary>
        ///     Шаг 1. Загрузить конфиг и проверить его
        /// </summary>
        private bool CheckInitialization() {
            if (_g2uConfig == null) {
                _g2uConfig = LoadSaveManager.LoadConfig();
                if (_g2uConfig == null) return false;
            }
            if (_g2uConfig.WasInizialized)
                CheckForFolders();
            return _g2uConfig.WasInizialized;
        }

        #endregion

        #region Util

        private void CheckForFolders() {
            PathManager.CreateJSONDataFolder(_g2uConfig);
            PathManager.CreateClassFolder(_g2uConfig);
        }

        #endregion

        private void ShowGoogleDataItem() {
            if (_ex.Foldout("Дополнительно", "qwerty", true)) {
                DrawOptionsButton();
                ShowGoogleSheetDataControl();
                DrawGoogleSheetDataList();
            }
        }

        private void ShowGoogleSheetDataControl() {
            _ex.DrawHorizontal(() => {
                _ex.Button("+", AddGoogleSheetData);
                _ex.Button("-", RemoveGoogleSheetData);
            });
        }

        private void AddGoogleSheetData() {
            _g2uConfig.GoogleSheetData.Add(new GoogleSheetData());
        }

        private void RemoveGoogleSheetData() {
            if (_g2uConfig.GoogleSheetData.Any()) {
                _g2uConfig.GoogleSheetData.RemoveAt(_g2uConfig.GoogleSheetData.Count - 1);
            }
        }


        private void DrawGoogleSheetDataList() {
            if (_ex.Foldout("Google Sheet Data", "GoogleSheetDataFoldout", true)) {
                foreach (var googleSheetData in _g2uConfig.GoogleSheetData) {
                    DrawGoogleSheetData(googleSheetData);
                }
            }
        }

        private void DrawGoogleSheetData(GoogleSheetData data) {
            _ex.DrawVertical(() => {
                data.ClassName = _ex.TextField("Class ClassName", data.ClassName);
                data.SheetType = _ex.EnumPopUp("Sheet type",
                    "SheetTypePopUP" + data.GoogleDriveFileGuid + data.GoogleDriveSheetGuid, data.SheetType, true);
                data.ClassLocation = _ex.TextField("Class Location", data.ClassLocation);
                data.JSONDataLocation = _ex.TextField("JSON Data Location", data.JSONDataLocation);
                data.GoogleDriveFileGuid = _ex.TextField("GoogleDriveFileGuid", data.GoogleDriveFileGuid);
                data.GoogleDriveSheetGuid = _ex.TextField("GoogleDriveSheetGuid", data.GoogleDriveSheetGuid);
            }, bgColor: GetGoogleDataGUIColor(), border: true);
        }


        private void DrawOptionsButton() {
            _ex.Button("Сохранить текущие настройки", SaveCurrentGoogleData);
            _ex.Button("Загрузить настройки по дефолту", LoadDefaultGoogleData);
            _ex.Button("Загрузить настройки", LoadSavedGoogleData);
        }

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


        /// <summary>
        ///     Класс загрузки и выгрузки данных
        /// </summary>
        private static class LoadSaveManager {
            public static G2UConfig LoadConfig() {
                G2UConfig config = null;
                if (!PathManager.GetConfigPath().Exists)
                    return null;
                return config.LoadJSONFromFile(PathManager.GetConfigPath().FullName);
            }

            public static void SaveConfig(G2UConfig config) {
                var path = PathManager.GetConfigPath().FullName;
                config.SaveJSONToFile(path);
            }

            public static void SaveClass(string @class, string path) {
                File.WriteAllText(path, @class);
            }

            public static void SaveJSON(string @json, string path) {
                File.WriteAllText(path, @json);
            }
        }

        #region Шаг 2

        /// <summary>
        ///     Шаг 2. Необходимо инициализировать конфиг.
        /// </summary>
        private void Inizialize() {
            if (_g2uConfig == null)
                _g2uConfig = new G2UConfig();
            _g2uConfig.Namespace = _ex.TextField("Namespace", _g2uConfig.Namespace);
            _ex.Button("Инициализировать", InizializeConfig);
        }


        private void InizializeConfig() {
            _g2uConfig.GoogleSheetData = GoogleSheetData.CreateDefaultData();
            _g2uConfig.WasInizialized = true;

            PathManager.CreateClassFolder(_g2uConfig);
            PathManager.CreateJSONDataFolder(_g2uConfig);
            PathManager.CreateConfigFolder();
            GenerateAndSaveEmptyClass(_g2uConfig.GoogleSheetData);
            LoadSaveManager.SaveConfig(_g2uConfig);
        }


        /// <summary>
        ///     Генерирует список пустых классов и сохраняет их
        /// </summary>
        private void GenerateAndSaveEmptyClass(List<GoogleSheetData> googleData) {
            foreach (var googleSheetData in _g2uConfig.GoogleSheetData) {
                var @class = GenerateEmptyClass(googleSheetData);
                LoadSaveManager.SaveClass(@class, googleSheetData.GetClassFileInfo().FullName);
            }
            Debug.Log("Пустые классы были успешно сгенерированы");
        }

        private string GenerateEmptyClass(GoogleSheetData googleData) {
            AbstractClassGenerator cg = new BaseConfigGenerator(googleData.ClassName, _g2uConfig.Namespace);
            return cg.GetEmptyClass();
        }

        #endregion

        #region Шаг 3

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
                        var text = GoogleDataParser.ParseSheet(GoogleSheetLoaderEditor.DataFromGoogle,
                            _g2uConfig.GoogleSheetData);
                        CheckForFolders();
                        GenerateJSON();
                    });
            });
        }

        private void LoadDataAndGenerateClass() {
            _ex.Button("Скачать Google Sheets и сгенерировать новый класс", () => {
                GoogleSheetLoaderEditor.LoadSheet(_g2uConfig.GoogleSheetData,
                    () => {
                        var text = GoogleDataParser.ParseSheet(GoogleSheetLoaderEditor.DataFromGoogle,
                            _g2uConfig.GoogleSheetData);
                        CheckForFolders();
                        GenerateClass();
                    });
            });
        }

        private void LoadDataGenerateClassAndSaveToJson() {
            _ex.Button("Скачать Google Sheets, сгенерировать новый класс и сохранить данные в JSON", () => {
                GoogleSheetLoaderEditor.LoadSheet(_g2uConfig.GoogleSheetData,
                    () => {
                        var text = GoogleDataParser.ParseSheet(GoogleSheetLoaderEditor.DataFromGoogle,
                            _g2uConfig.GoogleSheetData);
                        CheckForFolders();
                    });
            });
        }


        private void GenerateJSON() {
            for (var i = 0; i < _g2uConfig.GoogleSheetData.Count; i++) {
                var generator = AbstractFileBuilder.GetJSONGenerator(
                    GoogleDataParser.ParsedData.ElementAt(i).Key);
                if (generator == null) {
                    Debug.Log("Нельзя сгенерировать файл для: " + GoogleDataParser.ParsedData.ElementAt(i).Key);
                    continue;
                }
                var @json = generator.GenerateFile(GoogleDataParser.ParsedData.Values.ElementAt(i));
                LoadSaveManager.SaveJSON(@json, _g2uConfig.GoogleSheetData[i].GetJSONDataFileInfo().FullName);
            }
            Debug.Log("JSON успешно сгенерирован");
        }

        private void GenerateClass() {
            for (var i = 0; i < _g2uConfig.GoogleSheetData.Count; i++) {
                var generator = AbstractFileBuilder.GetClassGenerator(
                    GoogleDataParser.ParsedData.ElementAt(i).Key, _g2uConfig.GoogleSheetData[i].ClassName);
                if (generator == null) {
                    Debug.Log("Нельзя сгенерировать файл для: " + GoogleDataParser.ParsedData.ElementAt(i).Key);
                    continue;
                }
                var @class = generator.GenerateClass(GoogleDataParser.ParsedData.Values.ElementAt(i),
                    _g2uConfig.GoogleSheetData[i].GetClassFileInfo());
                LoadSaveManager.SaveClass(@class, _g2uConfig.GoogleSheetData[i].GetClassFileInfo().FullName);
            }
            Debug.Log("Классы успешно сгенерированы");
        }

        #endregion

        #region Get Color

        private int counter;

        private Color GetGoogleDataGUIColor() {
            counter ++;
            if (counter%2 == 0) {
                return new Color(.7f, 0, 0, 0.3f);
            }
            return new Color(0, .7f, 0, 0.3f);
        }

        #endregion
    }
}