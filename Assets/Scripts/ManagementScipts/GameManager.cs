using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WaveVR_Log;

public class GameManager : MonoBehaviour {

    public static GameManager Instance = null;
    public static readonly string LOG_TAG = "PHYLO_VIEWER";

    public void Awake()
    {
        if(Instance == null) { Instance = this; }
    }

    // Use this for initialization
    void Start () {
        DebugLog("Game Manager Started");
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public static void DebugLog(string message) {
        Debug.Log(message);
        Log.d(LOG_TAG, message);
    }
}