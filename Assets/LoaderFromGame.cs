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


   

}