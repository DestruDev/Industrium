using UnityEngine;
using UnityEngine.UI;

public class Hotbar : MonoBehaviour
{
    [Header("Hotbar Settings")]
    [SerializeField] private ItemSlot[] itemSlots = new ItemSlot[8];
    [SerializeField] private int selectedSlotIndex = 0;
    
    [Header("Hotbar Highlight Settings")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private float highlightAlpha = 1f;
    [SerializeField] private float normalAlpha = 0.7f;
    
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
        // No longer needed - highlighting is handled by ItemSlot components
    }
    
    private void HandleInput()
    {
        // Don't handle hotbar input if admin console is open
        if (IsAdminConsoleOpen())
        {
            return;
        }
        
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
        
        // Get item name for debug log
        string itemName = "Empty";
        if (itemSlots[slotIndex] != null && itemSlots[slotIndex].GetItemController() != null)
        {
            UI_Item itemData = itemSlots[slotIndex].GetItemController().GetItemData();
            if (itemData != null)
            {
                itemName = itemData.ItemName;
            }
        }
        
        Debug.Log($"Selected hotbar slot {slotIndex + 1} - {itemName}");
    }
    
    private void UpdateSlotHighlight(int slotIndex, bool isSelected)
    {
        if (slotIndex < 0 || slotIndex >= itemSlots.Length) return;
        
        ItemSlot slot = itemSlots[slotIndex];
        if (slot != null)
        {
            slot.SetHotbarSelected(isSelected, highlightColor, normalColor, highlightAlpha, normalAlpha);
        }
    }
    
    // Public methods for external access
    public int GetSelectedSlotIndex()
    {
        return selectedSlotIndex;
    }
    
    public ItemSlot GetSelectedSlot()
    {
        if (selectedSlotIndex >= 0 && selectedSlotIndex < itemSlots.Length)
        {
            return itemSlots[selectedSlotIndex];
        }
        return null;
    }
    
    public ItemSlot GetSlot(int index)
    {
        if (index >= 0 && index < itemSlots.Length)
        {
            return itemSlots[index];
        }
        return null;
    }
    
    public UI_Item GetSelectedItem()
    {
        ItemSlot selectedSlot = GetSelectedSlot();
        if (selectedSlot != null && selectedSlot.GetItemController() != null)
        {
            return selectedSlot.GetItemController().GetItemData();
        }
        return null;
    }
    
    /// <summary>
    /// Check if the admin console is currently open
    /// </summary>
    /// <returns>True if admin console is open, false otherwise</returns>
    private bool IsAdminConsoleOpen()
    {
        AdminConsole adminConsole = FindFirstObjectByType<AdminConsole>();
        if (adminConsole != null && adminConsole.adminPanel != null)
        {
            return adminConsole.adminPanel.activeSelf;
        }
        return false;
    }
}
