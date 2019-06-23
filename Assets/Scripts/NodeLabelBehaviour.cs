using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeLabelBehaviour : MonoBehaviour {

    Camera camera;

	// Use this for initialization
	void Start () {
        camera = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
        transform.LookAt(camera.transform.position);
        transform.Rotate(0, 180, 0);
    }
}
