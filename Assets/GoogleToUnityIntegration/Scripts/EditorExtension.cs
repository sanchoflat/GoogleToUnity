using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace EternalMaze.EditorWindows {
    public class EditorExtension {
        private readonly Dictionary<string, Vector2> _scrollVectors = new Dictionary<string, Vector2>();
        private readonly Dictionary<string, bool> _foldoutBool = new Dictionary<string, bool>();
        private readonly Dictionary<string, int> _popUP = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _intPopUP = new Dictionary<string, int>();
        private readonly Dictionary<string, string> _enumPopup = new Dictionary<string, string>();

        public bool Toggle(string label, bool value) {
            value = EditorGUILayout.Toggle(new GUIContent(label), value);
            GUI.color = Color.white;
            return value;
        }

        public int IntInput(string label, int value, Color color = default(Color)) {
            color = PrepareDefaultColor(color);
            GUI.color = color;
            value = EditorGUILayout.IntField(new GUIContent(label), value);
            GUI.color = Color.white;
            return value;
        }

        public int IntInput(string label, string tooltip, int value, Color color = default(Color)) {
            color = PrepareDefaultColor(color);
            GUI.color = color;
            value = EditorGUILayout.IntField(new GUIContent(label, tooltip), value);
            GUI.color = Color.white;
            return value;
        }

        public string TextField(string label, string value, Color color = default(Color)) {
            color = PrepareDefaultColor(color);
            GUI.color = color;
            value = EditorGUILayout.TextField(new GUIContent(label), value);
            GUI.color = Color.white;
            return value;
        }

        public string TextField(string label, string tooltip, string value, Color color = default(Color)) {
            color = PrepareDefaultColor(color);
            GUI.color = color;
            value = EditorGUILayout.TextField(new GUIContent(label, tooltip), value);
            GUI.color = Color.white;
            return value;
        }

        /// <summary>
        ///     Int Slider
        /// </summary>
        /// <param name="label">Label перед слайдером</param>
        /// <param name="value">Значение, которое будет изменяться</param>
        /// <param name="leftValue">Минимальное значение</param>
        /// <param name="rightValue">Максимальное значение</param>
        /// <param name="visualize">Если визуализировать, то рисуется слайдер. Если нет, то вернуть сохранённое значение в словаре</param>
        /// <param name="color">Цвет отрисовки элемента</param>
        /// <returns></returns>
        public int IntSlider(string label, int value, int leftValue, int rightValue, int step = 1, bool visualize = true,
            Color color = default(Color)) {
            return IntSlider(label, "", value, leftValue, rightValue, step, visualize, color);
        }

        /// <summary>
        ///     Int Slider
        /// </summary>
        /// <param name="label">Label перед слайдером</param>
        /// <param name="tooltip">Подсказка</param>
        /// <param name="value">Значение, которое будет изменяться</param>
        /// <param name="leftValue">Минимальное значение</param>
        /// <param name="rightValue">Максимальное значение</param>
        /// <param name="visualize">Если визуализировать, то рисуется слайдер. Если нет, то вернуть сохранённое значение в словаре</param>
        /// <param name="color">Цвет отрисовки элемента</param>
        /// <returns></returns>
        public int IntSlider(string label, string tooltip, int value, int leftValue, int rightValue, int step = 1,
            bool visualize = true, Color color = default(Color)) {
            color = PrepareDefaultColor(color);
            if(visualize) {
                GUI.color = color;
                value = EditorGUILayout.IntSlider(new GUIContent(label, tooltip), value, leftValue,
                    rightValue);
                if(step > 0) {
                    value = (int) Math.Round((value / (double) step)) * step;
                }
                GUI.color = Color.white;
            }
            return value;
        }

        /// <summary>
        ///     Float Slider
        /// </summary>
        /// <param name="label">Label перед слайдером</param>
        /// <param name="value">Значение, которое будет изменяться</param>
        /// <param name="leftValue">Минимальное значение</param>
        /// <param name="rightValue">Максимальное значение</param>
        /// <param name="step">Значение шага</param>
        /// <param name="visualize">Если визуализировать, то рисуется слайдер. Если нет, то вернуть сохранённое значение в словаре</param>
        /// <param name="color">Цвет отрисовки элемента</param>
        /// <param name="dValue">Дефолтное значение для инициализации</param>
        /// <returns></returns>
        public float FloatSlider(string label, float value, float leftValue, float rightValue, float step = 0.05f,
            bool visualize = true, Color color = default(Color)) {
            return FloatSlider(label, "", value, leftValue, rightValue, step, visualize, color);
        }

        /// <summary>
        ///     Float Slider
        /// </summary>
        /// <param name="label">Label перед слайдером</param>
        /// <param name="tooltip">Подсказка</param>
        /// <param name="value">Значение, которое будет изменяться</param>
        /// <param name="leftValue">Минимальное значение</param>
        /// <param name="rightValue">Максимальное значение</param>
        /// <param name="step">Значение шага слайдера</param>
        /// <param name="visualize">Если визуализировать, то рисуется слайдер. Если нет, то вернуть сохранённое значение в словаре</param>
        /// <param name="color">Цвет отрисовки элемента</param>
        /// <param name="dValue">Дефолтное значение для инициализации</param>
        /// <returns></returns>
        public float FloatSlider(string label, string tooltip, float value, float leftValue, float rightValue,
            float step = 0.05f, bool visualize = true, Color color = default(Color)) {
            color = PrepareDefaultColor(color);
            if(visualize) {
                GUI.color = color;
                value = EditorGUILayout.Slider(new GUIContent(label, tooltip), value, leftValue,
                    rightValue);
                if(step > 0) {
                    value = (float) Math.Round((value / step)) * step;
                }
                GUI.color = Color.white;
            }
            return value;
        }

        public void DrawHorizontal(Action action, Color bgColor = default(Color), bool border = false) {
            if(action == null) { return; }
            GUILayout.BeginHorizontal(GetGUIStyle(bgColor, border));
            action();
            GUILayout.EndHorizontal();
        }

        public void DrawVertical(Action action, string title = null, bool scroll = false, Color bgColor = default(Color),
            bool border = false) {
            if(action == null) { return; }
            if(scroll) {
                CheckKey(_scrollVectors, action.ToString());
                _scrollVectors[action.ToString()] = EditorGUILayout.BeginScrollView(_scrollVectors[action.ToString()]);
            }
            GUILayout.BeginVertical(GetGUIStyle(bgColor, border));
            if(title != null) {
                Label(title, true);
            }
            action();
            GUILayout.EndVertical();
            if(scroll) {
                EditorGUILayout.EndScrollView();
            }
        }

        public void DrawHorizontalWithLabel(string label, Action action, int labelWidth = 0) {
            EditorGUILayout.BeginHorizontal();
            if(labelWidth > 0) {
                EditorGUILayout.LabelField(label, GUILayout.Width(labelWidth));
            }
            else {
                EditorGUILayout.LabelField(label);
            }
            action();
            EditorGUILayout.EndHorizontal();
        }

        public void Label(string text, bool bold = false) {
            if(bold) {
                GUILayout.Label(text, EditorStyles.boldLabel);
            }
            else {
                GUILayout.Label(text);
            }
        }

        public int PopUp(string text, string[] visualizedOptions, string id, bool visualize = false) {
            CheckKey(_popUP, id);
            if(!visualize) {
                return _popUP[id];
            }
            _popUP[id] = EditorGUILayout.Popup(text, _popUP[id], visualizedOptions);
            return _popUP[id];
        }

        public T EnumPopUp<T>(string text, string id, T defaultValue, bool visualize = false,
            float textWidth = 100, float enumWidth = 100)
            where T : struct, IConvertible {
            if(id == null) {
                id = text;
            }
            CheckKey(_enumPopup, id, defaultValue.ToString());
            if(visualize) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(text, GUILayout.Width(textWidth));
                _enumPopup[id] =
                    EditorGUILayout.EnumPopup("", GetEnumFromString<T>(_enumPopup[id]), GUILayout.Width(enumWidth))
                        .ToString();
                EditorGUILayout.EndHorizontal();
            }
            return (T) Enum.Parse(typeof(T), _enumPopup[id]);
        }

        public int IntPopUp(string text, string id, int[] data, bool v) {
            CheckKey(_intPopUP, id);
            if(v) {
                return
                    _intPopUP[id] =
                        EditorGUILayout.IntPopup(text, _intPopUP[id], data.Select(i => i.ToString()).ToArray(), data);
            }
            return _intPopUP[id];
        }

        public void Button(string text, Action action, float width = -1, float height = 20) {
            if(action == null) { return; }
            if(width > 0) {
                if(GUILayout.Button(text, GUILayout.Width(width), GUILayout.Height(height))) {
                    action();
                }
            }
            else {
                if(GUILayout.Button(text, GUILayout.Height(height))) {
                    action();
                }
            }
        }

        public void Space(int count = 1) {
            for(var i = 0; i < count; i++) {
                EditorGUILayout.Space();
            }
        }

        /// <summary>
        ///     Выпадающее меню
        /// </summary>
        public bool Foldout(string text, string id = null, bool visualize = false) {
            if(id == null) {
                id = text;
            }
            CheckKey(_foldoutBool, id);
            if(visualize) {
                _foldoutBool[id] = EditorGUILayout.Foldout(_foldoutBool[id], text);
                return _foldoutBool[id];
            }
            return _foldoutBool[id];
        }

        /// <summary>
        ///     Сбросить все сохранённые данные
        /// </summary>
        public void Reset() {
            _scrollVectors.Clear();
            _popUP.Clear();
            _foldoutBool.Clear();
            _enumPopup.Clear();
        }

        /// <summary>
        ///     ПОдготавливает цвет фона. Если пришёл дефолтный цвет, то вернуть белый
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private Color PrepareDefaultColor(Color color) {
            if(color.Equals(default(Color))) {
                return new Color(1, 1, 1, 1f);
            }
            return color;
        }

        private GUIStyle GetGUIStyle(Color color, bool border = false) {
            if(border) {
                return PrepareBorderedGUIStyle(color);
            }
            return PrepareGUIStyle(color);
        }

        private GUIStyle PrepareGUIStyle(Color color) {
            var _guiStile = new GUIStyle();
            _guiStile.normal.background = MakeTex(1000, 1, color);
            return _guiStile;
        }

        private GUIStyle PrepareBorderedGUIStyle(Color color) {
            var _guiStile = EditorStyles.helpBox;
            _guiStile.normal.background = MakeTex(1000, 1, color);
            return _guiStile;
        }

        private Texture2D MakeTex(int width, int height, Color col) {
            var pix = new Color[width * height];
            for(var i = 0; i < pix.Length; i++) {
                pix[i] = col;
            }
            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void CheckKey<K, V>(Dictionary<K, V> d, K key, V value = default(V)) {
            if(!d.ContainsKey(key)) {
                d.Add(key, value);
            }
        }

        private void CheckKey(Dictionary<string, string> d, string key, string defaultValue = "") {
            if(!d.ContainsKey(key)) {
                d.Add(key, defaultValue);
            }
        }

        private Enum GetEnumFromString<T>(string v) {
            return (Enum) Enum.Parse(typeof(T), v);
        }
    }
}