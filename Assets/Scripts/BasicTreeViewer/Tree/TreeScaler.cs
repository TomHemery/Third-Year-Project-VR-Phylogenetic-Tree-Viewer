using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using wvr;
using WaveVR_Log;

public class TreeScaler : MonoBehaviour {

    public WVR_DeviceType device = WVR_DeviceType.WVR_DeviceType_Controller_Right;
    private float minScale = 0.1f;
    private TreeContainer_EventHandler treeContainerEventHandler;
    private InputEventManager inputEventManager;
    private float yaw, pitch = 0;

    // Use this for initialization
    void Start () {
        treeContainerEventHandler = GameObject.Find("TreeHandler").GetComponent<TreeContainer_EventHandler>();
        inputEventManager = GameObject.Find("_Scene").GetComponent<InputEventManager>();
        yaw = transform.rotation.x;
        pitch = transform.rotation.y;
	}

    // Update is called once per frame
    void Update()
    {
        if (treeContainerEventHandler.treeTargeted)
        {
            if (WaveVR_Controller.Input(device).GetTouch(WVR_InputId.WVR_InputId_Alias1_Touchpad))
            {
                switch (inputEventManager.Current_Controller_State)
                {
                    case InputEventManager.ControllerState.Rotate:
                        {
                            RotateByAxis();
                            break;
                        }
                    case InputEventManager.ControllerState.Scale:
                        {
                            ScaleByAxis();
                            break;
                        }
                }
            }
        }
    }

    public void ResetTransform() {
        transform.localScale = new Vector3(1, 1, 1);
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
    }

    private void RotateByAxis() {
        var axis = WaveVR_Controller.Input(device).GetAxis(WVR_InputId.WVR_InputId_Alias1_Touchpad);
        float xangle = 360 * -1 * axis.x, yangle = 360 * axis.y;
        if ((xangle * xangle) > (yangle * yangle))
        {
            pitch += xangle * Time.deltaTime;
        }
        else
        {
            yaw += yangle * Time.deltaTime;
        }
        transform.rotation = Quaternion.Euler(yaw, pitch, 0);
    }

    private void TestRotate() {
        yaw += 30f * Time.deltaTime;
        transform.rotation = Quaternion.Euler(yaw, pitch, 0);
    }

    private void ScaleByAxis() {
        var axis = WaveVR_Controller.Input(device).GetAxis(WVR_InputId.WVR_InputId_Alias1_Touchpad);
        float scale = axis.y * 0.1f;

        transform.localScale = transform.localScale + new Vector3(scale, scale, scale);

        if (transform.localScale.x < minScale)
            transform.localScale = new Vector3(minScale, transform.localScale.y, transform.localScale.z);
        if (transform.localScale.y < minScale)
            transform.localScale = new Vector3(transform.localScale.x, minScale, transform.localScale.z);
        if (transform.localScale.z < minScale)
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, minScale);
    }

}
