using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Slot Settings")]
    [SerializeField] private bool isHotbarSlot = false;
    [SerializeField] private bool isInventorySlot = true;
    
    [Header("Click Settings")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private GraphicRaycaster raycaster;
    
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private UI_ItemController itemController;
    private bool isHighlighted = false;
    
    // Click pickup system
    private static ItemSlot pickedUpSlot = null;
    private static UI_Item pickedUpItemData = null;
    private static GameObject cursorItem;
    private static Image cursorItemImage;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        itemController = GetComponent<UI_ItemController>();
        
        // Get canvas and raycaster from hierarchy if not assigned
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
        if (raycaster == null)
            raycaster = canvas.GetComponent<GraphicRaycaster>();
            
        // Add CanvasGroup if it doesn't exist
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }
    
    #region Click Functionality
    public void OnPointerClick(PointerEventData eventData)
    {
        // Only work if inventory is open
        if (!IsInventoryOpen())
            return;
            
        // If we have an item picked up, try to place it
        if (pickedUpSlot != null)
        {
            PlaceItem();
        }
        // If we don't have an item picked up, try to pick up this slot's item
        else if (itemController != null && itemController.GetItemData() != null)
        {
            PickUpItem();
        }
    }
    
    private void PickUpItem()
    {
        if (itemController == null || itemController.GetItemData() == null)
            return;
            
        // Store item data before clearing
        pickedUpItemData = itemController.GetItemData();
        string itemName = pickedUpItemData.ItemName;
        
        // Set this as the picked up slot
        pickedUpSlot = this;
        
        // Create cursor item
        CreateCursorItem();
        
        // Clear the original slot completely (no semi-transparent silhouette)
        itemController.SetItemData(null);
        
        Debug.Log($"Picked up: {itemName}");
    }
    
    private void PlaceItem()
    {
        if (pickedUpSlot == null || pickedUpItemData == null)
            return;
            
        // Get the current item in this slot
        UI_Item currentItem = itemController?.GetItemData();
        
        // If this slot has an item, swap what we're holding
        if (currentItem != null)
        {
            // Store what we were holding
            UI_Item itemWeWereHolding = pickedUpItemData;
            
            // Put the current slot's item in our hand (swap what we're holding)
            pickedUpItemData = currentItem;
            
            // Put what we were holding into this slot
            itemController.SetItemData(itemWeWereHolding);
            
            // Update cursor item to show the new item we're holding
            DestroyCursorItem();
            CreateCursorItem();
            
            // Force tooltip refresh for this slot
            ForceTooltipRefresh();
            
            Debug.Log($"Swapped to holding: {pickedUpItemData.ItemName}");
        }
        else
        {
            // Move item to this empty slot and clear pickup
            itemController.SetItemData(pickedUpItemData);
            
            // Clear pickup
            pickedUpSlot = null;
            pickedUpItemData = null;
            DestroyCursorItem();
            
            // Force tooltip refresh for this slot
            ForceTooltipRefresh();
            
            Debug.Log($"Placed item in empty slot");
        }
    }
    #endregion
    
    #region Hover Functionality
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Optional: Add hover highlighting here if needed
        SetHighlight(true);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        // Remove highlight when leaving
        SetHighlight(false);
    }
    #endregion
    
    #region Helper Methods
    private bool IsInventoryOpen()
    {
        // Check if inventory UI is active
        Inventory inventory = FindFirstObjectByType<Inventory>();
        if (inventory != null)
        {
            return inventory.IsInventoryOpen();
        }
        return false;
    }
    
    private void CreateCursorItem()
    {
        if (pickedUpItemData == null) return;
        
        // Create a temporary GameObject for the cursor item
        cursorItem = new GameObject("CursorItem");
        cursorItem.transform.SetParent(canvas.transform);
        cursorItem.transform.SetAsLastSibling();
        
        // Add Image component and copy the item image
        cursorItemImage = cursorItem.AddComponent<Image>();
        cursorItemImage.sprite = pickedUpItemData.Image;
        cursorItemImage.color = new Color(1, 1, 1, 0.8f); // Semi-transparent
        cursorItemImage.raycastTarget = false; // Don't block raycasts
        
        // Get cursor settings from Inventory
        Inventory inventory = FindFirstObjectByType<Inventory>();
        float sizeX = inventory != null ? inventory.GetCursorItemSizeX() : 32f;
        float sizeY = inventory != null ? inventory.GetCursorItemSizeY() : 32f;
        float offsetX = inventory != null ? inventory.GetCursorOffsetX() : 0f;
        float offsetY = inventory != null ? inventory.GetCursorOffsetY() : 0f;
        
        // Set size using inventory values
        RectTransform cursorRect = cursorItem.GetComponent<RectTransform>();
        cursorRect.sizeDelta = new Vector2(sizeX, sizeY);
        
        // Set position with offset
        Vector2 mousePos = Input.mousePosition;
        cursorRect.position = new Vector2(mousePos.x + offsetX, mousePos.y + offsetY);
    }
    
    private void DestroyCursorItem()
    {
        if (cursorItem != null)
        {
            Destroy(cursorItem);
            cursorItem = null;
            cursorItemImage = null;
        }
    }
    
    // Update cursor item position
    void Update()
    {
        if (cursorItem != null)
        {
            // Get offset values from Inventory
            Inventory inventory = FindFirstObjectByType<Inventory>();
            float offsetX = inventory != null ? inventory.GetCursorOffsetX() : 0f;
            float offsetY = inventory != null ? inventory.GetCursorOffsetY() : 0f;
            
            Vector2 mousePos = Input.mousePosition;
            cursorItem.transform.position = new Vector2(mousePos.x + offsetX, mousePos.y + offsetY);
        }
    }
    
    // Safety cleanup in case something goes wrong
    void OnDisable()
    {
        if (pickedUpSlot == this)
        {
            pickedUpSlot = null;
            DestroyCursorItem();
        }
    }
    
    private void SetHighlight(bool highlight)
    {
        isHighlighted = highlight;
        
        // You can add visual highlighting here
        // For example, change the background color or add a border
        // This is optional - you might want to modify the ItemBackground color
    }
    
    private void ForceTooltipRefresh()
    {
        // Force a tooltip refresh by simulating mouse enter/exit
        if (itemController != null && itemController.GetItemData() != null)
        {
            // Create a fake pointer event to trigger tooltip
            PointerEventData fakeEvent = new PointerEventData(UnityEngine.EventSystems.EventSystem.current);
            fakeEvent.position = Input.mousePosition;
            
            // Trigger the tooltip
            itemController.OnPointerEnter(fakeEvent);
        }
    }
    
    public bool CanAcceptItem(UI_Item item)
    {
        // For now, accept all items
        // You can add restrictions here later (e.g., only certain item types)
        return item != null;
    }
    
    public void ReceiveItem(UI_ItemController draggedController)
    {
        if (draggedController == null) return;
        
        // Get the dragged item data
        UI_Item draggedItem = draggedController.GetItemData();
        UI_Item currentItem = itemController?.GetItemData();
        
        // If this slot has an item, swap them
        if (currentItem != null)
        {
            // Swap items
            draggedController.SetItemData(currentItem);
            itemController.SetItemData(draggedItem);
        }
        else
        {
            // Move item to this empty slot
            itemController.SetItemData(draggedItem);
            draggedController.SetItemData(null);
        }
        
        // No need to move transforms since we're only moving icons
        // The UI will update automatically through SetItemData calls
    }
    
    public bool CanDrag()
    {
        return itemController != null && itemController.GetItemData() != null;
    }
    #endregion
    
    #region Getters
    public bool IsHotbarSlot() => isHotbarSlot;
    public bool IsInventorySlot() => isInventorySlot;
    public UI_ItemController GetItemController() => itemController;
    public bool IsPickedUp() => pickedUpSlot != null;
    #endregion
}
