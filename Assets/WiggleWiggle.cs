using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WiggleWiggle : MonoBehaviour
{
   public float wiggleSpeed = 1f;
    public float wiggleMagnitude = 0.5f;
    public float fallSpeed = 0.1f;
    public float rotationSpeed = 30f;
    public float rotationMagnitude = 5f;

    private Vector3 startPos;
    private Quaternion startRot;

    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
    }

    void FixedUpdate()
    {
        float wiggleOffset = Mathf.Sin(Time.time * wiggleSpeed) * wiggleMagnitude;
        transform.position = new Vector3(startPos.x + wiggleOffset, startPos.y - Time.time * fallSpeed, startPos.z);

        float rotationOffset = Mathf.Sin(Time.time * rotationSpeed) * rotationMagnitude;
        transform.rotation = startRot * Quaternion.Euler(0f, 0f, rotationOffset);
    }
}
