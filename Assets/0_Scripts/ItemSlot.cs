using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Slot Settings")]
    [SerializeField] private bool isHotbarSlot = false;
    [SerializeField] private bool isInventorySlot = true;
    
    [Header("Drag Settings")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private GraphicRaycaster raycaster;
    
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private UI_ItemController itemController;
    private bool isHighlighted = false;
    
    // Drag icon stuff
    private GameObject dragIcon;
    private Image dragIconImage;
    
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
    
    #region Drag Functionality
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Only drag if we have an item
        if (itemController == null || itemController.GetItemData() == null)
            return;
            
        // Create drag icon
        CreateDragIcon();
        
        // Make original slot semi-transparent but keep it in place
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        // Only drag if we have an item and drag icon exists
        if (itemController == null || itemController.GetItemData() == null || dragIcon == null)
            return;
            
        // Move drag icon with mouse
        Vector2 mousePosition = Input.mousePosition;
        dragIcon.transform.position = mousePosition;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        // Always destroy the drag icon first
        DestroyDragIcon();
        
        // Always restore transparency and raycasting
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        
        // Only process if we have an item
        if (itemController == null || itemController.GetItemData() == null)
            return;
            
        // Check what we're dropping on
        ItemSlot targetSlot = GetDropTarget(eventData);
        
        if (targetSlot != null && targetSlot.CanAcceptItem(itemController.GetItemData()))
        {
            // Valid drop - move item to new slot
            targetSlot.ReceiveItem(itemController);
        }
    }
    #endregion
    
    #region Drop Functionality
    public void OnDrop(PointerEventData eventData)
    {
        ItemSlot draggedSlot = eventData.pointerDrag?.GetComponent<ItemSlot>();
        
        if (draggedSlot != null && draggedSlot.CanDrag())
        {
            ReceiveItem(draggedSlot.itemController);
        }
        
        // Remove highlight
        SetHighlight(false);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Highlight when dragging over
        if (eventData.pointerDrag != null)
        {
            ItemSlot draggedSlot = eventData.pointerDrag.GetComponent<ItemSlot>();
            if (draggedSlot != null && draggedSlot.CanDrag())
            {
                SetHighlight(true);
            }
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        // Remove highlight when leaving
        SetHighlight(false);
    }
    #endregion
    
    #region Helper Methods
    private ItemSlot GetDropTarget(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        raycaster.Raycast(eventData, results);
        
        foreach (var result in results)
        {
            ItemSlot slot = result.gameObject.GetComponent<ItemSlot>();
            if (slot != null && slot != this)
                return slot;
        }
        
        return null;
    }
    
    private void CreateDragIcon()
    {
        if (itemController == null || itemController.GetItemData() == null) return;
        
        // Create a temporary GameObject for the drag icon
        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(canvas.transform);
        dragIcon.transform.SetAsLastSibling();
        
        // Add Image component and copy the item image
        dragIconImage = dragIcon.AddComponent<Image>();
        dragIconImage.sprite = itemController.GetItemData().Image;
        dragIconImage.color = new Color(1, 1, 1, 0.8f); // Semi-transparent
        dragIconImage.raycastTarget = false; // Don't block raycasts
        
        // Set size and position
        RectTransform dragRect = dragIcon.GetComponent<RectTransform>();
        dragRect.sizeDelta = rectTransform.sizeDelta;
        dragRect.position = Input.mousePosition;
    }
    
    private void DestroyDragIcon()
    {
        if (dragIcon != null)
        {
            Destroy(dragIcon);
            dragIcon = null;
            dragIconImage = null;
        }
    }
    
    // Safety cleanup in case something goes wrong
    void OnDisable()
    {
        DestroyDragIcon();
    }
    
    private void SetHighlight(bool highlight)
    {
        isHighlighted = highlight;
        
        // You can add visual highlighting here
        // For example, change the background color or add a border
        // This is optional - you might want to modify the ItemBackground color
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
    #endregion
}
