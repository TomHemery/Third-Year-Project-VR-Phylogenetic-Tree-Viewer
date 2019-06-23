using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugKeyPress : MonoBehaviour {

    private InputEventManager inputEventManager;

	// Use this for initialization
	void Start () {
        inputEventManager = gameObject.GetComponent<InputEventManager>();	
	}
	
	// Update is called once per frame
    // this script is ugly but it's only for debugging 
	void Update () {
        if (Input.GetKeyUp(KeyCode.T))
        {
            inputEventManager.OnSelectTreeView((int)InputEventManager.TreeState.TerrainTree);
        }
        else if (Input.GetKeyUp(KeyCode.C))
        {
            inputEventManager.OnSelectTreeView((int)InputEventManager.TreeState.CircularTree);
        }
        else if (Input.GetKeyUp(KeyCode.B))
        {
            inputEventManager.OnSelectTreeView((int)InputEventManager.TreeState.BasicTree);
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            BasicTreeViewerManager.Instance.LoadTree(TreeLoader.CurrentTreeIndex + 1);
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            BasicTreeViewerManager.Instance.LoadTree(TreeLoader.CurrentTreeIndex - 1);
        }
	}
}
