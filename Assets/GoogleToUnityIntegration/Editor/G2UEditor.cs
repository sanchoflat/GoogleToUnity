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


namespace GoogleSheetIntergation {
    [ExecuteInEditMode]
    public class GoogleSheetLoader : EditorWindow {
        private readonly Vector2 _minSize = new Vector2(430, 200);
        private readonly int _margin = 15;

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
            SettingsMenu();
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
        private void SettingsMenu() {
            _ex.DrawVertical(() => {
                G2UConfig.Instance.SkipRowPrefix = _ex.TextField("Skip prefix", G2UConfig.Instance.SkipRowPrefix);
                G2UConfig.Instance.ParameterClassName = _ex.TextField("Parameter class name",
                    G2UConfig.Instance.ParameterClassName);
                G2UConfig.Instance.ParameterClassLocation = _ex.TextField("Parameter class location",
                    G2UConfig.Instance.ParameterClassLocation);
                G2UConfig.Instance.ClassLocation = _ex.TextField("Class location", G2UConfig.Instance.ClassLocation);
                G2UConfig.Instance.DataLocation = _ex.TextField("Data location", G2UConfig.Instance.DataLocation);
                G2UConfig.Instance.CommentColumnTitle = _ex.TextField("Comment column title",
                    G2UConfig.Instance.CommentColumnTitle);
                G2UConfig.Instance.ArraySeparator = _ex.TextField("Array separator", G2UConfig.Instance.ArraySeparator);
                if(_ex.Foldout("Google sheet data", visualize: true)) {
                    ShowGoogleSheetDataControl();
                    DrawGoogleSheetDataList();
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
            G2UConfig.Instance.GoogleSheetData.Add(GoogleSheetData.CreateDefaultData());
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
                data.GoogleDriveFileGuid = _ex.TextField("GoogleDriveFileGuid", data.GoogleDriveFileGuid);
                data.GoogleDriveSheetGuid = _ex.TextField("GoogleDriveSheetGuid", data.GoogleDriveSheetGuid);
                data.DataExtension = _ex.TextField("Data extension", data.DataExtension);
                data.DataType = _ex.EnumPopUp("Data type", "Data type" + data.GoogleDriveSheetGuid, data.DataType, textWidth:145);
                data.Namespace = _ex.TextField("Namespace", data.Namespace);
                AccessModifiers(data);
                _ex.Button("Remove", () => { G2UConfig.Instance.GoogleSheetData.RemoveAt(counter); });
            }, bgColor: ColorManager.GetColor(), border: true);
        }

        private void AccessModifiers(GoogleSheetData data)
        {
            if(data.DataType == DataType.ScriptableObject) {
                _ex.SetEnumPopUpValue("VariableType" + data.GoogleDriveSheetGuid, VariableType.Field);
                _ex.SetEnumPopUpValue("Fieldaccessmodifier" + data.GoogleDriveSheetGuid, GoogleSheetIntergation.AccessModifiers.Public);
                GUI.enabled = false;
            }
            data.VariableType = _ex.EnumPopUp("Variable Type", "VariableType" + data.GoogleDriveSheetGuid, data.VariableType, textWidth: 145);
            data.FieldAccessModifiers = _ex.EnumPopUp("Field access modifier", "Fieldaccessmodifier" + data.GoogleDriveSheetGuid, data.FieldAccessModifiers, textWidth: 145);
            if (data.VariableType == VariableType.Property)
            {
                data.SetAccessModifiers = _ex.EnumPopUp("Set access modifier", "Set access modifier" + data.GoogleDriveSheetGuid, data.SetAccessModifiers, textWidth: 145);
            }
            GUI.enabled = true;
        }

        #endregion

        #region Base Menu

        private void BaseMenu() {
            ShowMainMenu();
            if(_ex.Foldout("Settings", "settingsKey", true)) {
                SettingsMenu();
                _ex.Button("Load config", () => { G2UConfig.Instance.LoadConfig(); });
                _ex.Button("Save current config", () => G2UConfig.Instance.SaveConfig());
            }
        }

        private void ShowMainMenu() {
            GenerateData();
        }

        private void GenerateData() {
            _ex.DrawHorizontal(() => {
                _ex.Button("Generate data", () => { LoadDataFromGoogle(GenerateBaseData); });

                if(G2UConfig.Instance.GoogleSheetData.Any(data => data.DataType == DataType.ScriptableObject)) {
                    _ex.Button("Generate SO prefab", GenerateSOPrefab);
                }
            });
        }

       
        private void GenerateBaseData(Dictionary<string, Dictionary<string, List<AbstractDataRow>>> inputData) {
            
            FileBuilder.Generate(inputData);
            //GenerateParameterClass();
            Debug.Log("Classes was successful generated");
        }

 
        private void GenerateSOPrefab() {
            if(GoogleDataParser.ParsedData == null) {
                LoadDataFromGoogleAndParseIt(GenerateSOPrefab);
                return;
            }
            for (int i = 0; i < GoogleDataParser.ParsedData.Count; i++) {
                var keyValuePair = GoogleDataParser.ParsedData.ElementAt(i);
                if(G2UConfig.Instance.GoogleSheetData[i].DataType == DataType.ScriptableObject) {
                    FileBuilder.GenerateSOPrefab(keyValuePair.Key, keyValuePair.Value);
                }
            }
            Debug.Log("SO asset was successful generated");
        }

        private void GenerateParameterClass() {
            var files = Directory.GetFiles(G2UConfig.Instance.PathManager.GetDataFolder().FullName);
            GenerateParameterClass(null);
        }

        private void GenerateParameterClass(List<string> data) {
            G2UConfig.Instance.PathManager.CreateParameterFolder();
            var file = new StringBuilder();
            file.AppendLine(string.Format("{0}internal class {1} {{", ClassGenerator.ClassGenerator.GetTabulator(1),
                G2UConfig.Instance.ParameterClassName));
            foreach(var d in data) {
                var path = new FileInfo(Path.Combine(G2UConfig.Instance.DataLocation, d));
                var resourcesPath = PathManager.GetResourcesPath(path);
                file.Append(string.Format("{0}public const string {1}Path = \"{2}\";\r\n",
                    ClassGenerator.ClassGenerator.GetTabulator(2), d.UppercaseFirst(), resourcesPath));
            }
            file.Append(string.Format("{0}}}\r\n{1}}}", ClassGenerator.ClassGenerator.GetTabulator(1),
                ClassGenerator.ClassGenerator.GetTabulator(0)));
            SaveLoadManager.SaveFile(G2UConfig.Instance.ParameterClassFullName, file.ToString());
        }

        private void LoadDataFromGoogleAndParseIt(Action onComplete)
        {
            GoogleSheetLoaderEditor.LoadSheet(G2UConfig.Instance.GoogleSheetData,
                () =>
                {
                     GoogleDataParser.ParseSheet(GoogleSheetLoaderEditor.DataFromGoogle,
                        G2UConfig.Instance.GoogleSheetData); if (onComplete != null)
                     {
                         onComplete.Invoke();
                     }
                });
        }

        private void LoadDataFromGoogle(Action<Dictionary<string, Dictionary<string, List<AbstractDataRow>>>> onComplete) {
            GoogleSheetLoaderEditor.LoadSheet(G2UConfig.Instance.GoogleSheetData,
                () => {
                    var data = GoogleDataParser.ParseSheet(GoogleSheetLoaderEditor.DataFromGoogle,
                        G2UConfig.Instance.GoogleSheetData);
                    if(onComplete != null) {
                        onComplete.Invoke(data);
                    }
                });
        }

        #endregion
    }
}


#endif

//  public static Assembly Compile(string source) {
//            var provider = new CSharpCodeProvider();
//            var param = new CompilerParameters();
//
//            // Add ALL of the assembly references
//            foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
//                param.ReferencedAssemblies.Add(assembly.Location);
//            }
//
//            // Add specific assembly references
//            //param.ReferencedAssemblies.Add("System.dll");
//            //param.ReferencedAssemblies.Add("CSharp.dll");
//            //param.ReferencedAssemblies.Add("UnityEngines.dll");
//
//            // Generate a dll in memory
//            param.GenerateExecutable = false;
//            param.GenerateInMemory = true;
//
//            // Compile the source
//            var result = provider.CompileAssemblyFromSource(param, source);
//            if(result.Errors.Count > 0) {
//                var msg = new StringBuilder();
//                foreach(CompilerError error in result.Errors) {
//                    msg.AppendFormat("Error ({0}): {1}\n",
//                        error.ErrorNumber, error.ErrorText);
//                }
//                throw new Exception(msg.ToString());
//            }
//
//            // Return the assembly
//            return result.CompiledAssembly;
//        }