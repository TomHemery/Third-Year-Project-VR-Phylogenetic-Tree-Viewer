using UnityEngine;

public class Movement : MonoBehaviour {

    public bool doMovement = false;
    public Vector3 movementVector;

    void Update()
    {
        if (doMovement)
        {
            transform.Translate(movementVector * Time.deltaTime, Space.World);
        }
    }
}