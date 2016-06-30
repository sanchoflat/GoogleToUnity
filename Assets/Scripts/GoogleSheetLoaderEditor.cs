using System;
using System.Collections.Generic;
using UnityEngine;


namespace G2U {
    public class GoogleSheetLoaderEditor {
        public static List<string> DataFromGoogle;

        public static void LoadSheet(List<GoogleSheetData> googleData, Action onComplete = null) {
            var dataForBecome = googleData.Count;
            var dataCountWasRecieved = 0;
            DataFromGoogle = new List<string>();
            for(var i = 0; i < googleData.Count; i++) {
                var url = googleData[i].GetURL();
                if(string.IsNullOrEmpty(url)) {
                    continue;
                }
                LoadSheet(url, t => {
                    dataCountWasRecieved ++;
                    DataFromGoogle.Add(t);
                });
            }
            EditorCoroutine.Add(() => dataCountWasRecieved == dataForBecome, () => {
                Debug.Log("Данные были успешно загружены");
                if(onComplete != null) { onComplete(); }
            });
        }

        public static void LoadSheet(string sheetURL, Action<string> onComlete = null) {
            var serverCall = new WWW(sheetURL);
            EditorCoroutine.Add(() => serverCall.isDone, () => {
                if(!string.IsNullOrEmpty(serverCall.error)) {
                    Debug.LogError("WWW failed: " + serverCall.error);
                }
                if(onComlete != null) { onComlete(serverCall.text); }
            });
        }
    }
}