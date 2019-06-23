using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfrontOfPlayer : MonoBehaviour {

    GameObject player;
    Camera camera;
    Vector3 offset;
    float yPos;
    float yOffset;
    float offsetMag;

    // Use this for initialization
    void Start()
    {
        player = GameObject.Find("head");
        offset = transform.position - player.transform.position;
        offsetMag = offset.magnitude;
        yPos = transform.position.y;
        yOffset = player.transform.position.y - transform.position.y;
        camera = Camera.main;
    }

    private void OnEnable()
    {
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = camera.transform.position + camera.transform.forward * offsetMag;
        transform.position = new Vector3(transform.position.x, player.transform.position.y - yOffset, transform.position.z);
    }
}
