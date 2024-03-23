using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float rotationSpeed = 5f;
    public GameObject focalPoint;

    void Update()
    {
        if(Input.GetKey(KeyCode.Mouse1))
            transform.RotateAround(focalPoint.transform.position, transform.up, Input.GetAxis("Mouse X") * rotationSpeed);
        
    }

}