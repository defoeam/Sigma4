using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    public GameObject target;
    public float smoothPositionSpeed = 0.1f;
    public float smoothRotationSpeed = 0.1f;
    void Start()
    {
        transform.position = target.transform.position;
        transform.rotation = target.transform.rotation;
    }

    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, target.transform.position, smoothPositionSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, target.transform.rotation, smoothRotationSpeed * Time.deltaTime);
    }
}
