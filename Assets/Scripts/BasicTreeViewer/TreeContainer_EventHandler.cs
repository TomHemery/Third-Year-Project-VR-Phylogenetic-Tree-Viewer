using UnityEngine;
using UnityEngine.EventSystems;

public class TreeContainer_EventHandler : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public bool treeTargeted { get; private set; }
    private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

    void Start() {
        treeTargeted = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        treeTargeted = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        treeTargeted = false;
    }
}