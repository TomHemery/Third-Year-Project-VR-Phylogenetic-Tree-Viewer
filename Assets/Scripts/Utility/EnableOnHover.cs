using UnityEngine;
using UnityEngine.EventSystems;

public class EnableOnHover : MonoBehaviour,

    IPointerEnterHandler,
    IPointerExitHandler

{
    private MeshRenderer mesh;

    public void Start()
    {
        mesh = gameObject.GetComponent<MeshRenderer>();
        mesh.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mesh.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mesh.enabled = false;
    }
}
