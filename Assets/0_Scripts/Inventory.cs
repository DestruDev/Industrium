using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Inventory UI")]
    [SerializeField] private GameObject inventoryUI;
    
    [Header("Cursor Item Settings")]
    [SerializeField] private float cursorItemSizeX = 32f;
    [SerializeField] private float cursorItemSizeY = 32f;
    [SerializeField] private float cursorOffsetX = 0f;
    [SerializeField] private float cursorOffsetY = 0f;
    
    void Start()
    {
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleInventory();
        }
    }
    
    private void ToggleInventory()
    {
        if (inventoryUI != null)
        {
            bool willBeOpen = !inventoryUI.activeSelf;

            // If the inventory is about to close, clear all hover highlights BEFORE deactivating slots
            if (!willBeOpen)
            {
                ItemSlot.ClearAllHoverHighlights();
            }

            inventoryUI.SetActive(willBeOpen);
        }
    }
    
    // Public method to check if inventory is open
    public bool IsInventoryOpen()
    {
        return inventoryUI != null && inventoryUI.activeSelf;
    }
    
    // Public getters for cursor item settings
    public float GetCursorItemSizeX() => cursorItemSizeX;
    public float GetCursorItemSizeY() => cursorItemSizeY;
    public float GetCursorOffsetX() => cursorOffsetX;
    public float GetCursorOffsetY() => cursorOffsetY;
}