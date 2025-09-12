using UnityEngine;
using UnityEngine.UI;

public class Hotbar : MonoBehaviour
{
    [Header("Hotbar Settings")]
    [SerializeField] private UI_ItemController[] itemSlots = new UI_ItemController[8];
    [SerializeField] private int selectedSlotIndex = 0;
    
    [Header("Visual Highlighting")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private float highlightAlpha = 1f;
    [SerializeField] private float normalAlpha = 0.7f;
    
    // Store original colors for each slot
    private Image[] slotBackgrounds;
    private Color[] originalColors;
    
    void Start()
    {
        InitializeSlots();
        SelectSlot(selectedSlotIndex);
    }
    
    void Update()
    {
        HandleInput();
    }
    
    private void InitializeSlots()
    {
        // Get background images for visual highlighting
        slotBackgrounds = new Image[itemSlots.Length];
        originalColors = new Color[itemSlots.Length];
        
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] != null)
            {
                // Look for the ItemBackground Image component specifically
                Transform itemBackgroundTransform = itemSlots[i].transform.Find("ItemBackground");
                Image bgImage = null;
                
                if (itemBackgroundTransform != null)
                {
                    bgImage = itemBackgroundTransform.GetComponent<Image>();
                }
                
                // Fallback: search in children if not found directly
                if (bgImage == null)
                {
                    Image[] childImages = itemSlots[i].GetComponentsInChildren<Image>();
                    foreach (Image img in childImages)
                    {
                        if (img.gameObject.name == "ItemBackground")
                        {
                            bgImage = img;
                            break;
                        }
                    }
                }
                
                slotBackgrounds[i] = bgImage;
                if (bgImage != null)
                {
                    originalColors[i] = bgImage.color;
                }
                else
                {
                    Debug.LogWarning($"ItemBackground Image component not found for slot {i}");
                }
            }
        }
    }
    
    private void HandleInput()
    {
        // Check for number key inputs (1-8)
        for (int i = 0; i < 8; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
                break;
            }
        }
        
        // Optional: Also support numpad keys
        for (int i = 0; i < 8; i++)
        {
            if (Input.GetKeyDown(KeyCode.Keypad1 + i))
            {
                SelectSlot(i);
                break;
            }
        }
    }
    
    public void SelectSlot(int slotIndex)
    {
        // Validate slot index
        if (slotIndex < 0 || slotIndex >= itemSlots.Length)
        {
            Debug.LogWarning($"Invalid slot index: {slotIndex}");
            return;
        }
        
        // Update selected slot
        int previousSlot = selectedSlotIndex;
        selectedSlotIndex = slotIndex;
        
        // Update visual highlighting
        UpdateSlotHighlight(previousSlot, false);
        UpdateSlotHighlight(selectedSlotIndex, true);
        
        Debug.Log($"Selected hotbar slot {slotIndex + 1}");
    }
    
    private void UpdateSlotHighlight(int slotIndex, bool isSelected)
    {
        if (slotIndex < 0 || slotIndex >= slotBackgrounds.Length) return;
        
        Image bgImage = slotBackgrounds[slotIndex];
        if (bgImage == null) return;
        
        if (isSelected)
        {
            // Highlight the selected slot
            Color newColor = highlightColor;
            newColor.a = highlightAlpha;
            bgImage.color = newColor;
        }
        else
        {
            // Return to normal color
            Color newColor = normalColor;
            newColor.a = normalAlpha;
            bgImage.color = newColor;
        }
    }
    
    // Public methods for external access
    public int GetSelectedSlotIndex()
    {
        return selectedSlotIndex;
    }
    
    public UI_ItemController GetSelectedSlot()
    {
        if (selectedSlotIndex >= 0 && selectedSlotIndex < itemSlots.Length)
        {
            return itemSlots[selectedSlotIndex];
        }
        return null;
    }
    
    public UI_ItemController GetSlot(int index)
    {
        if (index >= 0 && index < itemSlots.Length)
        {
            return itemSlots[index];
        }
        return null;
    }
    
    public UI_Item GetSelectedItem()
    {
        UI_ItemController selectedSlot = GetSelectedSlot();
        return selectedSlot?.GetItemData();
    }
}
