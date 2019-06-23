using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using WaveVR_Log;


public class BasicTreeViewerManager : MonoBehaviour {

    public static BasicTreeViewerManager Instance{get; private set;}
    public GameObject worldErrorPanel;
    private Text worldErrorText;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        }
    }

    void Start () {
        worldErrorText = worldErrorPanel.GetComponentInChildren<Text>();
        DisableWorldError();

        TreeLoader._init();
        if (TreeLoader.CurrentTree != null)
        {
            TreeBuilder.Instance.BuildTree(TreeLoader.CurrentTree);
        }
        else //it's all gone horribly wrong - the newick parser has failed to parse the tree 
        {
            GameManager.DebugLog("Tree loading failed");
            ShowWorldError(NewickParser.ErrorMessage);
        }       
    }

	void Update () {
		
	}

    public void LoadTree(int index)
    {
        DisableWorldError();
        TreeLoader.LoadTree(index);
        if (TreeLoader.CurrentTree != null)
        {
            TreeBuilder.Instance.BuildTree(TreeLoader.CurrentTree);
        }
        else //it's all gone horribly wrong - the newick parser has failed to parse the tree 
        {
            GameManager.DebugLog("Tree loading failed");
            TreeBuilder.Instance.ResetTreeContainer();
            ShowWorldError(NewickParser.ErrorMessage);
        }
    }

    private void ShowWorldError(string errorMessage) {
        worldErrorPanel.SetActive(true);
        worldErrorText.text = errorMessage;
    }

    private void DisableWorldError() {
        worldErrorText.text = "Error...\nYou shouldn't see this text :)";
        worldErrorPanel.SetActive(false);
    }
}
