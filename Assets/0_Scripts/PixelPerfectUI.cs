using UnityEngine;
using UnityEngine.UI;

public class PixelPerfectUI : MonoBehaviour
{
    [Header("Pixel Perfect UI Settings")]
    [SerializeField] private bool enablePixelPerfectUI = true;
    [SerializeField] private int uiPixelsPerUnit = 32;
    
    private Canvas canvas;
    private CanvasScaler canvasScaler;
    
    void Start()
    {
        canvas = GetComponent<Canvas>();
        canvasScaler = GetComponent<CanvasScaler>();
        
        if (enablePixelPerfectUI)
        {
            ApplyPixelPerfectUISettings();
        }
    }
    
    private void ApplyPixelPerfectUISettings()
    {
        if (canvasScaler == null) return;
        
        // Set canvas scaler to pixel perfect mode
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f; // Balance between width and height
        
        // Set reference resolution based on your game's resolution
        canvasScaler.referenceResolution = new Vector2(960, 600);
        
        // Ensure pixel perfect rendering
        canvas.pixelPerfect = true;
    }
    
    void LateUpdate()
    {
        if (enablePixelPerfectUI)
        {
            SnapUIToPixelPerfect();
        }
    }
    
    private void SnapUIToPixelPerfect()
    {
        // Snap all UI elements to pixel boundaries
        RectTransform[] uiElements = GetComponentsInChildren<RectTransform>();
        
        foreach (RectTransform rect in uiElements)
        {
            // Skip the canvas itself
            if (rect == canvas.transform as RectTransform) continue;
            
            // Snap position to pixel boundaries
            Vector3 pos = rect.anchoredPosition;
            float pixelSize = 1f / uiPixelsPerUnit;
            
            float snappedX = Mathf.Round(pos.x / pixelSize) * pixelSize;
            float snappedY = Mathf.Round(pos.y / pixelSize) * pixelSize;
            
            rect.anchoredPosition = new Vector2(snappedX, snappedY);
            
            // Also snap size to pixel boundaries for crisp rendering
            Vector2 size = rect.sizeDelta;
            float snappedWidth = Mathf.Round(size.x / pixelSize) * pixelSize;
            float snappedHeight = Mathf.Round(size.y / pixelSize) * pixelSize;
            
            rect.sizeDelta = new Vector2(snappedWidth, snappedHeight);
        }
    }
    
    // Public method to toggle pixel perfect UI
    public void SetPixelPerfectUIEnabled(bool enabled)
    {
        enablePixelPerfectUI = enabled;
        
        if (enabled)
        {
            ApplyPixelPerfectUISettings();
        }
        else if (canvas != null)
        {
            canvas.pixelPerfect = false;
        }
    }
    
    // Public method to update UI pixel perfect settings
    public void UpdateUIPixelPerfectSettings(int newUIPixelsPerUnit)
    {
        uiPixelsPerUnit = newUIPixelsPerUnit;
        
        if (enablePixelPerfectUI)
        {
            ApplyPixelPerfectUISettings();
        }
    }
}
