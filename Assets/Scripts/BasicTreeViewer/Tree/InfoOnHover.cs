using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InfoOnHover : MonoBehaviour, IPointerEnterHandler,
    IPointerExitHandler
{

    private MaintainNodeScale nodeScale;
    private static GameObject nodeInfoPanel = null;
    Text nodeInfoText;
    Node node;
    private float originalScale;
    private string infoText;
    private static GameObject player; 

    public void Start()
    {
      
    }

    public void _Init() {
        nodeScale = GetComponent<MaintainNodeScale>();

        if (nodeInfoPanel == null)
            nodeInfoPanel = GameObject.Find("NodeInfo").transform.GetChild(0).gameObject;

        if (player == null)
            player = GameObject.Find("WaveVR");

        nodeInfoText = nodeInfoPanel.GetComponentInChildren<Text>();
        node = gameObject.transform.parent.GetComponent<BasicNodeBehaviour>().getAttachedNodeInfo();

        infoText = "Node Name: " + node.getLabel() + "\n" +
            "Distance to Parent: " + node.getWeightToParent() + "\n" +
            "-Metainformation-" + node.GetMetaData();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        originalScale = nodeScale.targetScale;
        nodeScale.targetScale = 2 * originalScale;
        gameObject.transform.hasChanged = true;
        nodeInfoPanel.SetActive(true);
        nodeInfoText.text = infoText;

        nodeInfoPanel.transform.position = transform.position;
        nodeInfoPanel.transform.position = Vector3.MoveTowards(nodeInfoPanel.transform.position, player.transform.position, 1);
        nodeInfoPanel.transform.Translate(0, 0.75f, 0);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        nodeScale.targetScale = originalScale;
        gameObject.transform.hasChanged = true;
        if(nodeInfoPanel != null)
            nodeInfoPanel.SetActive(false);
    }
}