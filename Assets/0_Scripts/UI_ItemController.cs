using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UI_ItemController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Item Data")]
    [SerializeField] private UI_Item itemData;
    [SerializeField] private int quantity = 1;
    
    [Header("UI Components")]
    [SerializeField] private Image itemImage;
    private TextMeshProUGUI quantityText;
    
    /*
    [Header("Optional Components")]
    [SerializeField] private Button itemButton;
    */
    void Start()
    {
        FindQuantityText();
        UpdateUI();
        //SetupButton();
    }
    
    // Find the QuantityText child object
    private void FindQuantityText()
    {
        // Check if this is an equipment slot - equipment items are not stackable
        ItemSlot parentSlot = GetComponentInParent<ItemSlot>();
        if (parentSlot != null && parentSlot.GetSlotType() == SlotType.Equipment)
        {
            // Equipment slots don't need quantity text, so we can skip this
            return;
        }
        
        Transform quantityTextTransform = transform.Find("QuantityText");
        if (quantityTextTransform != null)
        {
            quantityText = quantityTextTransform.GetComponent<TextMeshProUGUI>();
            if (quantityText == null)
            {
                Debug.LogWarning($"QuantityText child object found but it doesn't have a TextMeshProUGUI component on {gameObject.name}");
            }
        }
        else
        {
            Debug.LogWarning($"No child object named 'QuantityText' found for {gameObject.name}");
        }
    }
    
    // Update the UI components with item data
    public void UpdateUI()
    {
        if (itemData == null)
        {
            // Handle empty slot: disable the image component
            if (itemImage != null)
            {
                itemImage.enabled = false;
                itemImage.sprite = null; // Clear sprite just in case
            }
            
            // TEMPORARY: Don't hide quantity text for debugging - show "0" for empty slots
            // Hide quantity text for empty slots
            // if (quantityText != null)
            // {
            //     quantityText.enabled = false;
            // }
            
            // TEMPORARY: Still update quantity text for empty slots to show "0"
            UpdateQuantityText();
            return;
        }
        
        // Set item image: ensure image is enabled and set sprite/color
        if (itemImage != null)
        {
            itemImage.enabled = true; // Ensure image is enabled
            if (itemData.Image != null)
            {
                itemImage.sprite = itemData.Image;
                itemImage.color = new Color(1, 1, 1, 1); // Make visible and fully opaque
                itemImage.type = Image.Type.Simple; // Ensure Simple type
            }
            else
            {
                // If itemData exists but has no image, disable the image component
                itemImage.enabled = false;
                itemImage.sprite = null;
            }
        }
        
        // Update quantity text
        UpdateQuantityText();
    }
    
    // Update quantity text display and positioning
    private void UpdateQuantityText()
    {
        // Check if this is an equipment slot - equipment items are not stackable
        ItemSlot parentSlot = GetComponentInParent<ItemSlot>();
        if (parentSlot != null && parentSlot.GetSlotType() == SlotType.Equipment)
        {
            // Equipment slots don't need quantity text
            return;
        }
        
        if (quantityText == null) return;
        
        // Hide quantity text for empty slots, non-stackable items, or single items
        if (itemData == null || (itemData != null && !itemData.Stackable) || quantity < 2)
        {
            quantityText.enabled = false;
        }
        else
        {
            quantityText.enabled = true;
            quantityText.text = quantity.ToString();
            
            // Set font size based on slot type
            SetQuantityTextFontSize();
            
            // Position the quantity text based on slot type
            PositionQuantityText();
        }
        
        /* ORIGINAL CODE - COMMENTED OUT FOR DEBUGGING
        // Hide quantity text for empty slots and non-stackable items
        if (itemData == null || (itemData != null && !itemData.Stackable))
        {
            quantityText.enabled = false;
        }
        // Only show quantity text for stackable items with quantity > 1
        else if (itemData != null && itemData.Stackable && quantity > 1)
        {
            quantityText.enabled = true;
            quantityText.text = quantity.ToString();
            
            // Set font size based on slot type
            SetQuantityTextFontSize();
            
            // Position the quantity text based on slot type
            PositionQuantityText();
        }
        else
        {
            quantityText.enabled = false;
        }
        */
    }
    
    // Set font size and color based on slot type
    private void SetQuantityTextFontSize()
    {
        if (quantityText == null) return;
        
        // Get the parent ItemSlot to determine slot type
        ItemSlot parentSlot = GetComponentInParent<ItemSlot>();
        if (parentSlot == null) return;
        
        // Get the Inventory script to access font size and color values
        Inventory inventory = FindFirstObjectByType<Inventory>();
        if (inventory == null) return;
        
        // Set font size, color, and style based on slot type
        if (parentSlot.IsHotbarSlot())
        {
            quantityText.fontSize = inventory.GetHotbarFontSize();
            quantityText.color = inventory.GetHotbarTextColor();
        }
        else if (parentSlot.IsInventorySlot())
        {
            quantityText.fontSize = inventory.GetInventoryFontSize();
            quantityText.color = inventory.GetInventoryTextColor();
        }
        
        // Use centralized font style, material, and font asset settings
        quantityText.fontStyle = inventory.GetQuantityFontStyle();
        
        // Apply font asset if available
        TMP_FontAsset quantityFontAsset = inventory.GetQuantityFontAsset();
        if (quantityFontAsset != null)
        {
            quantityText.font = quantityFontAsset;
        }
        
        // Apply material preset if available
        Material quantityMaterial = inventory.GetQuantityTextMaterial();
        if (quantityMaterial != null)
        {
            quantityText.fontMaterial = quantityMaterial;
        }
    }
    
    // Position quantity text based on slot type
    private void PositionQuantityText()
    {
        if (quantityText == null) return;
        
        // Get the parent ItemSlot to determine slot type
        ItemSlot parentSlot = GetComponentInParent<ItemSlot>();
        if (parentSlot == null) return;
        
        // Get the Inventory script to access global offset values
        Inventory inventory = FindFirstObjectByType<Inventory>();
        if (inventory == null) return;
        
        RectTransform quantityRect = quantityText.GetComponent<RectTransform>();
        if (quantityRect == null) return;
        
        // Apply offset based on slot type using global values
        if (parentSlot.IsHotbarSlot())
        {
            quantityRect.anchoredPosition = new Vector2(inventory.GetHotbarOffsetX(), inventory.GetHotbarOffsetY());
        }
        else if (parentSlot.IsInventorySlot())
        {
            quantityRect.anchoredPosition = new Vector2(inventory.GetInventoryOffsetX(), inventory.GetInventoryOffsetY());
        }
    }
    
    /*
    // Set up button functionality
    private void SetupButton()
    {
        if (itemButton != null)
        {
            itemButton.onClick.AddListener(OnItemClicked);
        }
    }
    */
    
    // Called when the item is clicked
    private void OnItemClicked()
    {
        if (itemData != null)
        {
            Debug.Log($"Item clicked: {itemData.ItemName} (ID: {itemData.ID})");
            // Add your item click logic here
        }
    }
    
    // Public method to set item data programmatically
    public void SetItemData(UI_Item newItemData)
    {
        itemData = newItemData;
        UpdateUI();
    }
    
    // Public method to set item data with quantity
    public void SetItemData(UI_Item newItemData, int newQuantity)
    {
        itemData = newItemData;
        quantity = newQuantity;
        UpdateUI();
    }
    
    // Public method to set quantity
    public void SetQuantity(int newQuantity)
    {
        quantity = newQuantity;
        UpdateQuantityText();
    }
    
    // Hover tooltip functionality
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Only show tooltip if we have an item and no item is currently picked up
        if (itemData != null && !IsItemPickedUp())
        {
            TooltipManager.ShowTooltip(itemData.ItemName);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.HideTooltip();
    }
    
    private bool IsItemPickedUp()
    {
        // Check if any ItemSlot has an item picked up
        ItemSlot[] allSlots = FindObjectsByType<ItemSlot>(FindObjectsSortMode.None);
        foreach (ItemSlot slot in allSlots)
        {
            if (slot.IsPickedUp())
            {
                return true;
            }
        }
        return false;
    }
    
    // Getters for accessing item data
    public UI_Item GetItemData() => itemData;
    public int GetItemID() => itemData?.ID ?? -1;
    public string GetItemName() => itemData?.ItemName ?? "Unknown";
    public int GetQuantity() => quantity;
    
}
