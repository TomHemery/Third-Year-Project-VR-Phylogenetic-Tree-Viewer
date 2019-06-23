using System.Collections;
using UnityEngine;

public static class TreeForceApplicator {

    public static readonly int numPasses = 40;
    private static readonly float baseAttractionDelta = 0.01f;
    private static readonly float baseRepulsionDelta = 0.1f; 
    private static readonly float minDistanceSquared = 0.25f;

    public static void Pass(GameObject[] leafNodes) {
        GameObject node;
        GameObject other;
        for(int i = 0; i < leafNodes.Length; i++) {
            node = leafNodes[i];
            for(int j = i + 1; j < leafNodes.Length; j++) {
                other = leafNodes[j];

                float distanceSquared = (node.transform.position - other.transform.position).sqrMagnitude;

                if (AreSiblings(node, other) && distanceSquared > minDistanceSquared)
                {
                    node.transform.position = Vector3.MoveTowards(node.transform.position, 
                        other.transform.position, baseAttractionDelta * distanceSquared);
                    other.transform.position = Vector3.MoveTowards(other.transform.position, 
                        node.transform.position, baseAttractionDelta * distanceSquared);
                }
                else
                {
                    node.transform.position = Vector3.MoveTowards(node.transform.position,
                        other.transform.position, -baseRepulsionDelta / distanceSquared);
                    other.transform.position = Vector3.MoveTowards(other.transform.position,
                        node.transform.position, -baseRepulsionDelta / distanceSquared);
                }
            }
        }
    }

    public static bool AreSiblings(GameObject node, GameObject other) {
        foreach (Transform sibling in node.transform.parent)
        {
            if(sibling.gameObject == other)
                return true;
        }
        return false;
    }
	
}
