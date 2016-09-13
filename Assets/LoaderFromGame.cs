using JetBrains.Annotations;
using UnityEngine;

public class LoaderFromGame : MonoBehaviour {


    void Start() {
        var ass = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        Debug.Log(ass);

    }
    /*
    public GameConfig GameConfig;
    private void Start() {
       Debug.Log(GameConfig.BgMusicStartDelay);
    }*/


    private void AppendClass() {

  
        string cl = "using UnityEngine;" +
                    "public class Test : MonoBehavior { " +
                    "private void Start() { " +
                    "Debug.Log(\"test\");}" +
                    "}";
    }

}