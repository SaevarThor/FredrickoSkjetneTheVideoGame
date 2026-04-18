using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void LateUpdate()
    {
        // Rotate to always face the camera
        transform.LookAt(transform.position + _cam.transform.forward);
    }
}