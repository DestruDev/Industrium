using UnityEngine;

public class PixelPerfectCamera : MonoBehaviour
{
    [Header("Pixel Perfect Settings")]
    [SerializeField] private int pixelsPerUnit = 32;
    [SerializeField] private int referenceResolution = 960;
    [SerializeField] private bool enablePixelPerfect = true;
    
    private Camera cam;
    private float originalOrthographicSize;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            originalOrthographicSize = cam.orthographicSize;
            ApplyPixelPerfectSettings();
        }
    }
    
    void LateUpdate()
    {
        if (enablePixelPerfect && cam != null)
        {
            SnapCameraToPixelPerfect();
        }
    }
    
    private void ApplyPixelPerfectSettings()
    {
        if (cam == null) return;
        
        // Calculate the correct orthographic size for pixel perfect rendering
        float pixelPerfectSize = (referenceResolution / (float)pixelsPerUnit) * 0.5f;
        cam.orthographicSize = pixelPerfectSize;
        
        // Ensure camera position is snapped to pixel boundaries
        SnapCameraToPixelPerfect();
    }
    
    private void SnapCameraToPixelPerfect()
    {
        if (cam == null) return;
        
        // Get the current camera position
        Vector3 cameraPos = transform.position;
        
        // Calculate pixel size in world units
        float pixelSize = 1f / pixelsPerUnit;
        
        // Snap camera position to pixel boundaries
        float snappedX = Mathf.Round(cameraPos.x / pixelSize) * pixelSize;
        float snappedY = Mathf.Round(cameraPos.y / pixelSize) * pixelSize;
        
        // Apply snapped position
        transform.position = new Vector3(snappedX, snappedY, cameraPos.z);
        
        // Also snap the orthographic size to ensure clean rendering
        float snappedSize = Mathf.Round(cam.orthographicSize * pixelsPerUnit) / pixelsPerUnit;
        cam.orthographicSize = snappedSize;
    }
    
    // Public method to update pixel perfect settings at runtime
    public void UpdatePixelPerfectSettings(int newPixelsPerUnit, int newReferenceResolution)
    {
        pixelsPerUnit = newPixelsPerUnit;
        referenceResolution = newReferenceResolution;
        
        if (cam != null)
        {
            ApplyPixelPerfectSettings();
        }
    }
    
    // Public method to toggle pixel perfect on/off
    public void SetPixelPerfectEnabled(bool enabled)
    {
        enablePixelPerfect = enabled;
        
        if (!enabled && cam != null)
        {
            // Restore original orthographic size when disabling
            cam.orthographicSize = originalOrthographicSize;
        }
        else if (enabled && cam != null)
        {
            // Reapply pixel perfect settings when enabling
            ApplyPixelPerfectSettings();
        }
    }
    
    // Getters for inspector
    public int GetPixelsPerUnit() => pixelsPerUnit;
    public int GetReferenceResolution() => referenceResolution;
    public bool IsPixelPerfectEnabled() => enablePixelPerfect;
}
