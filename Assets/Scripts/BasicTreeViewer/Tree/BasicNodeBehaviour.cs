using UnityEngine;
using UnityEngine.UI;

public class BasicNodeBehaviour : MonoBehaviour {

    Node nodeInfo;
    GameObject myLabel;
    GameObject myRenderer;
    private bool isRoot = false;

    void Awake()
    {
        myRenderer = gameObject.transform.Find("Renderer").gameObject;
    }

    // Use this for initialization
    void Start () {
        	
	}
	
	// Update is called once per frame
	void Update () {

    }

    public void attachNodeInfo(Node n) {
        nodeInfo = n;
        isRoot = (nodeInfo.getParent() == null);
        Transform textTransform = gameObject.transform.Find("Renderer/NodeLabel/Text");
        if (textTransform != null)
        {
            myLabel = gameObject.transform.Find("Renderer/NodeLabel/Text").gameObject;
            myLabel.GetComponent<Text>().text = nodeInfo.getLabel();
        }

        if (isRoot) // I AM ROOT!
        {
            Transform sphereTransform = myRenderer.transform.Find("Sphere");
            if (sphereTransform != null)
            {
                sphereTransform.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
                myRenderer.GetComponent<MaintainNodeScale>().targetScale = 2;
                myRenderer.transform.hasChanged = true;
            }
        }

    }

    public Node getAttachedNodeInfo() {
        return nodeInfo;
    }
}
