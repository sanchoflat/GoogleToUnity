using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace G2U {
    public class GoogleSheetLoaderEditor {
        public static List<string> DataFromGoogle;

        public static void LoadSheet(List<GoogleSheetData> googleData, Action onComplete = null) {
            var dataForBecome = googleData.Count;
            var dataCountWasRecieved = 0;
            DataFromGoogle = new List<string>();
            if(!googleData.Any()) {
                Debug.LogError("Google sheet list is empty");
            }
            for(var i = 0; i < googleData.Count; i++) {
                var url = googleData[i].GetURL();
                if(string.IsNullOrEmpty(url)) {
                    Debug.LogWarning("URL is empty");
                    continue;
                }
                LoadSheet(url, t => {
                    dataCountWasRecieved ++;
                    DataFromGoogle.Add(t);
                });
            }
            EditorCoroutine.Add(() => dataCountWasRecieved == dataForBecome,
                () => { if(onComplete != null) { onComplete(); } });
        }

        public static void LoadSheet(string sheetURL, Action<string> onComlete = null) {
            var serverCall = new WWW(sheetURL);
            EditorCoroutine.Add(() => serverCall.isDone, () => {
                if(!string.IsNullOrEmpty(serverCall.error)) {
                    Debug.LogError("WWW failed. Try again");
                }
                if(onComlete != null) { onComlete(serverCall.text); }
            });
        }
    }
}