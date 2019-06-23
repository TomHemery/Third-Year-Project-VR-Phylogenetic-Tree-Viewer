using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FrameRateDisplay : MonoBehaviour {

    public Text frameRateText;
    private float avgFrameRate;
    private long numFrames = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        numFrames++;
        avgFrameRate += ((1.0f / Time.deltaTime) - avgFrameRate) / numFrames;
        frameRateText.GetComponent<Text>().text = "Avg FPS: " + avgFrameRate;
    }
}
