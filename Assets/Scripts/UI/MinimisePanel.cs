using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimisePanel : MonoBehaviour {

    public GameObject target;

    public void ToggleMinimise() {
        if (target.activeInHierarchy)
        {
            target.SetActive(false);
        }
        else
        {
            target.SetActive(true);
        }
    }
}
