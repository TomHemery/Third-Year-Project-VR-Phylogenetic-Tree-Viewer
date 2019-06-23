using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Line : MonoBehaviour
{
    private GameObject labelPrefab;

    private GameObject myLabel;

    public GameObject gameObject1;      
    public GameObject gameObject2;          

    private LineRenderer line;

    private void Awake()
    {
        labelPrefab = Resources.Load<GameObject>("Prefabs/Label");
        myLabel = Instantiate(labelPrefab, gameObject.transform.position, Quaternion.identity);
        myLabel.transform.SetParent(gameObject.transform);
    }

    void Start()
    {
        line = this.gameObject.AddComponent<LineRenderer>();
        line.startWidth = 0.02f;
        line.endWidth = 0.02f;
        line.positionCount = 2;
    }

    public void setLabel(string value) {
        myLabel.transform.Find("Text").GetComponent<Text>().text = value;
    }

    void Update()
    {
        if (gameObject1 != null && gameObject2 != null)
        {
            line.SetPosition(0, gameObject1.transform.position);
            line.SetPosition(1, gameObject2.transform.position);
            myLabel.transform.position = (gameObject1.transform.position + gameObject2.transform.position) / 2;
        }
    }
}