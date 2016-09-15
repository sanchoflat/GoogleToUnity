using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace GoogleSheetIntergation {
    public class GoogleSheetLoaderEditor {

        public static void LoadSheet(List<GoogleSheetData> googleData, Action<string> onComplete = null) {
            if(!googleData.Any()) {
                Debug.LogError("Google sheet list is empty");
            }
            for(var i = 0; i < googleData.Count; i++) {
                LoadSheet(googleData[i], onComplete); 
            }
        }

        public static void LoadSheet(GoogleSheetData googleData, Action<string> onComplete = null) {
            var dataCountWasRecieved = 0;
            var url = googleData.GetURL();
            if(string.IsNullOrEmpty(url)) {
                Debug.LogWarning(string.Format("URL for {0} is empty", googleData.GoogleDataName));
            }
            var recievedData = "";
            LoadSheet(url, t => {
                dataCountWasRecieved++;
                recievedData = t;
            });
            EditorCoroutine.Add(() => dataCountWasRecieved == 1,
                () => { if (onComplete != null) { onComplete(recievedData); } });
        }

        public static void LoadSheet(string sheetURL, Action<string> onComlete = null) {
            var serverCall = new WWW(sheetURL);
            EditorCoroutine.Add(() => serverCall.isDone, () => {
                if(!string.IsNullOrEmpty(serverCall.error)) {
                    Debug.LogError("WWW failed. Try again");
                }
                if(onComlete != null) {
                    onComlete(serverCall.text);
                }
            });
        }
    }
}