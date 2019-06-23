using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaintainNodeScale : MonoBehaviour {

    GameObject treeContainer;

    public float targetScale = 1f;

    private void Awake()
    {
        treeContainer = GameObject.Find("TreeContainer");
    }

    // Use this for initialization
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.hasChanged)
        {
            transform.localScale = new Vector3(
                targetScale / treeContainer.transform.localScale.x,
                targetScale / treeContainer.transform.localScale.y,
                targetScale / treeContainer.transform.localScale.z
            );
            transform.hasChanged = false;
        }
    }
}
