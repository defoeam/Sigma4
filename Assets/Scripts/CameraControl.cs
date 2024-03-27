using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float rotationSpeed = 5f;
    public float moveSpeed = 0.0000000000001f; 
    public float scrollSpeed = .0001f; 
    public GameObject focalPoint;

    void Update()
    {
        if(Input.GetKey(KeyCode.Mouse1))
            transform.RotateAround(focalPoint.transform.position, transform.up, Input.GetAxis("Mouse X") * rotationSpeed);

        //if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        //{
        //    transform.position += moveSpeed * new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        //}

        //if (Input.GetAxis("Mouse ScrollWheel") != 0)
        //{
        //    transform.position += scrollSpeed * new Vector3(0, -Input.GetAxis("Mouse ScrollWheel"), 0);
        //}

    }

}