using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TreeConverter3D {

    public static void Convert(GameObject root) {
        GameManager.DebugLog("Converting tree with root: " + root + " tree to 3D");
        BasicRecursiveConstantRotate(root);
    }

    private static void BasicRecursiveConstantRotate(GameObject currentNode, float degreesPerChild = 90f) {
        foreach (Transform childTransform in currentNode.transform) {
            GameObject child = childTransform.gameObject;
            //loop through each child gameobject looking for nodes, we also have a renderer as a child which we have to ignore
            if (child.GetComponent<BasicNodeBehaviour>() != null) { //check that we definitely are dealing with nodes
                Vector3 rotationAxis = child.transform.position - currentNode.transform.position;
                child.transform.RotateAround(
                        child.transform.position,
                        rotationAxis,
                        degreesPerChild
                    );
                BasicRecursiveConstantRotate(child);
            }
        }
    }	
}