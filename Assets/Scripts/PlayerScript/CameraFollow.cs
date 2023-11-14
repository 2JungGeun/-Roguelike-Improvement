using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public float smoothSpeed = 0.1f;
    public Vector3 offset = new Vector3(0.0f, 0.0f, -10.0f);
    public Vector3 minValue, maxValue;
    public void LateUpdate()
    {
        Vector3 desirePosition = target.position + offset;
        Vector3 boundPosition = new Vector3(
            Mathf.Clamp(desirePosition.x, minValue.x, maxValue.x),
            Mathf.Clamp(desirePosition.y, minValue.y, maxValue.y),
            desirePosition.z);
        Vector3 smoothedPosition = Vector3.Slerp(transform.position, boundPosition, smoothSpeed);
        transform.position = smoothedPosition;

    }
}
