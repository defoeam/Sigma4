using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float rotationSpeed = 5f;
    public float scrollSpeed = 5f; 
    public float upAngleMax = 70f;
    public float downAngleMax = 0f;
    public float scrollMin = 5.7f;
    public float scrollMax = 20f;
    private Vector2 rotation;
    private float newOffset;
    public GameObject target;


    private void Start()
    {
        rotation.y = transform.localRotation.eulerAngles.z;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            rotation.x += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            rotation.y += Input.GetAxis("Mouse Y") * rotationSpeed * -1 * Time.deltaTime;
            rotation.y = Mathf.Clamp(rotation.y, downAngleMax, upAngleMax);
            transform.localRotation = Quaternion.Euler(0, rotation.x, rotation.y);
        }

        if (Input.mouseScrollDelta.y != 0f)
        {
            newOffset = target.transform.localPosition.x - Input.mouseScrollDelta.y * scrollSpeed * Time.deltaTime;
            newOffset = Mathf.Clamp(newOffset, scrollMin, scrollMax);
            target.transform.localPosition = new Vector3(newOffset, target.transform.localPosition.y, target.transform.localPosition.z);
        }

    }

}