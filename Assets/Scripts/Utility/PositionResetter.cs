using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionResetter : MonoBehaviour {

    Vector3 startPos;

    private void Awake()
    {
        startPos = transform.position;
    }

    public void ResetPosition() {
        transform.position = startPos;
    }
}
