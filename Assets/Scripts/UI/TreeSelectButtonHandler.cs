using UnityEngine;

public class TreeSelectButtonHandler : MonoBehaviour {

    public int Value { get; private set; }
    private bool valueSet = false;

    public void SetValue(int v)
    {
        if (!valueSet)
        {
            Value = v;
            valueSet = true;
        }
    }

    public void OnClick() {
        BasicTreeViewerManager.Instance.LoadTree(Value);
    }
}
