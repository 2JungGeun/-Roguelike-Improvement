using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public float smoothSpeed = 100.0f;
    public Vector3 offset = new Vector3(0.0f, 0.0f, -10.0f);
    public Vector3 minValue, maxValue;
    //private Vector3 targetPosition; // 대상의 현재 위치
    public void LateUpdate()
    {
        Vector3 desirePosition = target.position + offset;
        Vector3 boundPosition = new Vector3(
            Mathf.Clamp(desirePosition.x, minValue.x, maxValue.x),
            Mathf.Clamp(desirePosition.y, minValue.y, maxValue.y),
            desirePosition.z);
        
        transform.position = Vector3.Lerp(this.transform.position, boundPosition, 1000.0f * Time.deltaTime);
    }
}
