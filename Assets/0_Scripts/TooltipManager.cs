using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;
    
    [Header("Tooltip UI")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private RectTransform tooltipRect;
    
    [Header("Settings")]
    [SerializeField] private Vector2 offset = new Vector2(10, 10);
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Start with tooltip hidden
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
    
    void Update()
    {
        // Follow mouse cursor
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            FollowMouse();
        }
    }
    
    public static void ShowTooltip(string text)
    {
        if (Instance != null)
        {
            Instance.ShowTooltipInternal(text);
        }
    }
    
    public static void HideTooltip()
    {
        if (Instance != null)
        {
            Instance.HideTooltipInternal();
        }
    }
    
    private void ShowTooltipInternal(string text)
    {
        if (tooltipPanel == null || tooltipText == null) return;
        
        tooltipText.text = text;
        tooltipPanel.SetActive(true);
        FollowMouse();
    }
    
    private void HideTooltipInternal()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
    
    private void FollowMouse()
    {
        if (tooltipRect == null) return;
        
        Vector2 mousePosition = Input.mousePosition;
        tooltipRect.position = mousePosition + offset;
        
        // Keep tooltip on screen
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            Vector3[] corners = new Vector3[4];
            tooltipRect.GetWorldCorners(corners);
            
            // Get screen bounds
            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
            
            // Adjust position if tooltip goes off screen
            if (corners[2].x > screenRect.width)
            {
                tooltipRect.position = new Vector2(mousePosition.x - tooltipRect.sizeDelta.x - offset.x, tooltipRect.position.y);
            }
            
            if (corners[2].y > screenRect.height)
            {
                tooltipRect.position = new Vector2(tooltipRect.position.x, mousePosition.y - tooltipRect.sizeDelta.y - offset.y);
            }
        }
    }
}
