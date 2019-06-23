using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PopulateGrid : MonoBehaviour
{
    public GameObject prefab; 

    void Start()
    {
        Populate();
    }

    void Update()
    {

    }

    void Populate()
    {
        if (TreeLoader.TreeFiles != null)
        {
            GameObject newObj;

            int i = 0;
            foreach (TextAsset fi in TreeLoader.TreeFiles)
            {
                newObj = Instantiate(prefab, transform);
                newObj.GetComponentInChildren<Text>().text = fi.name;
                newObj.GetComponent<TreeSelectButtonHandler>().SetValue(i);
                i++;
            }
        }
    }
}