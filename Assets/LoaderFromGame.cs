using UnityEngine;
public class LoaderFromGame : MonoBehaviour {
    private const string GoogleSheetURL =
        "http://docs.google.com/feeds/download/spreadsheets/Export?key={0}&exportFormat=csv&gid={1}";

    // GUID таблицы
    private readonly string _googleDriveFileGuid = "1Yu8V-W25wkAqHCAPXYR6wQXLzhClOMItYeXHScXXDCY";

    // GUID листа в таблице
    private readonly string _googleDriveSheetGuid = "0";

    // Данные, полученные из гугл таблицы в CSV формате
    private string _requestedData;

    private void Start() {
        StartCoroutine(LoadSheet(GetURL()));
    }

    private System.Collections.IEnumerator LoadSheet(string URL)
    {
        var request = new WWW(URL);
        yield return request;
        _requestedData = request.text;
        Debug.Log(_requestedData);


        var data = CSVReader.Read(_requestedData);
        Debug.Log(data);
    }

    private string GetURL() {
        return string.Format(GoogleSheetURL, _googleDriveFileGuid, _googleDriveSheetGuid);
    }
}