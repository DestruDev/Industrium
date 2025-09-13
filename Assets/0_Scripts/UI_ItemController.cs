using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UI_ItemController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Item Data")]
    [SerializeField] private UI_Item itemData;
    
    [Header("UI Components")]
    [SerializeField] private Image itemImage;
    /*
    [Header("Optional Components")]
    [SerializeField] private Button itemButton;
    */
    void Start()
    {
        UpdateUI();
        //SetupButton();
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
            }
            else
            {
                // If itemData exists but has no image, disable the image component
                itemImage.enabled = false;
                itemImage.sprite = null;
            }
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
        ItemSlot[] allSlots = FindObjectsOfType<ItemSlot>();
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
}
