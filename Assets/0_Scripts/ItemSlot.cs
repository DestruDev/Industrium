using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum SlotType
{
    Inventory,
    Hotbar,
    Equipment
}

public class ItemSlot : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Slot Settings")]
    [SerializeField] private SlotType slotType = SlotType.Inventory;
    [SerializeField] private EquipmentSubcategory equipmentSlotType = EquipmentSubcategory.Top;
    
    
    [Header("Click Settings")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private GraphicRaycaster raycaster;
    
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private UI_ItemController itemController;
    private bool isHighlighted = false;
    private Image itemBackground;
    private Color originalBackgroundColor;
    private Image borderImage; // For hotbar selection
    private Color originalBorderColor;
    private bool isHotbarSelected = false;
    
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
            
        // Find ItemBackground image
        FindItemBackground();
        // Find Border image for hotbar highlighting
        FindBorderImage();
    }
    
    #region Click Functionality
    public void OnPointerDown(PointerEventData eventData)
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
            
        // Check if this slot can accept the item
        if (!CanAcceptItem(pickedUpItemData))
        {
            Debug.LogWarning($"Cannot place {pickedUpItemData.ItemName} in this slot!");
            return;
        }
            
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
        // Only highlight if inventory is open
        if (IsInventoryOpen())
        {
            SetHighlight(true);
        }
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
        
        // For equipment slots, we might want different behavior
        // For now, return true so equipment slots work when inventory is closed
        if (slotType == SlotType.Equipment)
        {
            return true;
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
        cursorItemImage.color = new Color(1, 1, 1, 1f); // Fully opaque
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
    
    private void FindItemBackground()
    {
        // Look for ItemBackground Image component
        Transform itemBackgroundTransform = transform.Find("ItemBackground");
        if (itemBackgroundTransform != null)
        {
            itemBackground = itemBackgroundTransform.GetComponent<Image>();
        }
        
        // Fallback: search in children if not found directly
        if (itemBackground == null)
        {
            Image[] childImages = GetComponentsInChildren<Image>();
            foreach (Image img in childImages)
            {
                if (img.gameObject.name == "ItemBackground")
                {
                    itemBackground = img;
                    break;
                }
            }
        }
        
        // Store original color
        if (itemBackground != null)
        {
            originalBackgroundColor = itemBackground.color;
        }
    }
    
    private void FindBorderImage()
    {
        // Look for Border in children
        Transform borderTransform = transform.Find("Border");
        if (borderTransform != null)
        {
            borderImage = borderTransform.GetComponent<Image>();
        }
        
        // Fallback: search in all children if not found directly
        if (borderImage == null)
        {
            Image[] childImages = GetComponentsInChildren<Image>();
            foreach (Image img in childImages)
            {
                if (img.gameObject.name == "Border")
                {
                    borderImage = img;
                    break;
                }
            }
        }
        
        // Store original color
        if (borderImage != null)
        {
            originalBorderColor = borderImage.color;
        }
    }
    
    private void SetHighlight(bool highlight)
    {
        isHighlighted = highlight;
        
        // Hover highlighting (ItemBackground) - only for mouse hover
        if (itemBackground != null)
        {
            // Always check if inventory is open - if not, force highlight off
            bool shouldHighlight = highlight && IsInventoryOpen();
            
            if (shouldHighlight)
            {
                itemBackground.color = new Color(0.949f, 0.949f, 0.949f, 1f); // F2F2F2
            }
            else
            {
                // If ItemBackground has an image, use white color to show the image properly
                if (itemBackground.sprite != null)
                {
                    itemBackground.color = Color.white;
                }
                else
                {
                    itemBackground.color = originalBackgroundColor;
                }
            }
        }
        
        // Don't call UpdateHotbarHighlight() here - hover should not affect border
    }
    
    private void UpdateHotbarHighlight(Color highlightColor, Color normalColor, float highlightAlpha, float normalAlpha)
    {
        if (borderImage != null && slotType == SlotType.Hotbar)
        {
            if (isHotbarSelected)
            {
                Color newColor = highlightColor;
                newColor.a = highlightAlpha;
                borderImage.color = newColor;
            }
            else
            {
                // Return to original border color when unselected
                borderImage.color = originalBorderColor;
            }
        }
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
        if (item == null) return false;
        
        // Equipment slots have special validation
        if (slotType == SlotType.Equipment)
        {
            // Only accept equipment items
            if (item.ItemCategory != ItemCategory.Equipment) 
            {
                Debug.LogWarning($"Equipment slot {gameObject.name} rejected {item.ItemName}: Not an equipment item");
                return false;
            }
            
            // Check if the item's subcategory matches this slot's type
            bool canAccept = item.EquipmentSubcategory == equipmentSlotType;
            
            if (!canAccept)
            {
                Debug.LogWarning($"Equipment slot {gameObject.name} (Type: {equipmentSlotType}) rejected {item.ItemName} (Type: {item.EquipmentSubcategory})");
            }
            else
            {
                Debug.Log($"Equipment slot {gameObject.name} accepted {item.ItemName} (Type: {item.EquipmentSubcategory})");
            }
            
            return canAccept;
        }
        
        // For hotbar and inventory slots, accept all items
        return true;
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
    public bool IsHotbarSlot() => slotType == SlotType.Hotbar;
    public bool IsInventorySlot() => slotType == SlotType.Inventory;
    public bool IsEquipmentSlot() => slotType == SlotType.Equipment;
    public SlotType GetSlotType() => slotType;
    public EquipmentSubcategory GetEquipmentSlotType() => equipmentSlotType;
    public UI_ItemController GetItemController() => itemController;
    public bool IsPickedUp() => pickedUpSlot != null;
    public bool IsHotbarSelected() => isHotbarSelected;
    #endregion
    
    #region Hotbar Methods
    public void SetHotbarSelected(bool selected, Color highlightColor, Color normalColor, float highlightAlpha, float normalAlpha)
    {
        isHotbarSelected = selected;
        UpdateHotbarHighlight(highlightColor, normalColor, highlightAlpha, normalAlpha); // Only update hotbar highlighting, not hover
    }
    
    public static void ClearAllHoverHighlights()
    {
        ItemSlot[] allSlots = FindObjectsByType<ItemSlot>(FindObjectsSortMode.None);
        foreach (ItemSlot slot in allSlots)
        {
            // Force clear the highlight state and visual
            slot.isHighlighted = false;
            if (slot.itemBackground != null)
            {
                slot.itemBackground.color = slot.originalBackgroundColor;
            }
        }
    }
    #endregion
}
