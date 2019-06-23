using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour {

    GameObject player;
    Vector3 offset;

	// Use this for initialization
	void Start () {
        player = GameObject.Find("head");
        offset = transform.position - player.transform.position;
	}

    private void OnEnable()
    {
    }

    // Update is called once per frame
    void Update () {
        transform.position = player.transform.position + offset;
	}
}
