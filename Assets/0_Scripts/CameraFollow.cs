using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public float smoothSpeed = 0.125f;
    public float zoomSpeed = 5f;
    public float mouseWheelSensitivity = 5f;
    public float defaultZoom = 5f;
    public float minZoom = 2f;
    public float maxZoom = 10f;
    private Camera cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = GetComponent<Camera>();
        
        // Set the camera to the default zoom level
        if (cam != null)
        {
            cam.orthographicSize = defaultZoom;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (cam == null) return;

        float zoomDelta = 0f;

        // Check for zoom in (Equals/Plus keys)
        if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.KeypadPlus)){
            zoomDelta = -1f;
            cam.orthographicSize += zoomDelta * zoomSpeed * Time.deltaTime;
        }
        // Check for zoom out (Minus keys)
        else if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus)){
            zoomDelta = 1f;
            cam.orthographicSize += zoomDelta * zoomSpeed * Time.deltaTime;
        }
        
        // Mouse wheel zoom (separate from keyboard)
        if (Input.mouseScrollDelta.y > 0){
            cam.orthographicSize -= mouseWheelSensitivity * Time.deltaTime;
            zoomDelta = -1f; // Set zoomDelta so clamping works
        }
        else if (Input.mouseScrollDelta.y < 0){
            cam.orthographicSize += mouseWheelSensitivity * Time.deltaTime;
            zoomDelta = 1f; // Set zoomDelta so clamping works
        }

        if(zoomDelta != 0f){
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }

    void FixedUpdate()
    {
        if (target == null) return;

        Vector3 desPos = target.position + offset;
        Vector3 smoothPos = Vector3.Lerp(transform.position, desPos, smoothSpeed);
        transform.position = new Vector3(smoothPos.x, smoothPos.y, transform.position.z);
    }
}
