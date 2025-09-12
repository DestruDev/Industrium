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
            // Handle empty slot
            if (itemImage != null)
            {
                itemImage.sprite = null;
                itemImage.color = new Color(1, 1, 1, 0); // Make transparent
            }
            return;
        }
        
        // Set item image
        if (itemImage != null)
        {
            if (itemData.Image != null)
            {
                itemImage.sprite = itemData.Image;
                itemImage.color = new Color(1, 1, 1, 1); // Make visible
            }
            else
            {
                itemImage.sprite = null;
                itemImage.color = new Color(1, 1, 1, 0); // Make transparent
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
        // Only show tooltip if we have an item
        if (itemData != null)
        {
            TooltipManager.ShowTooltip(itemData.ItemName);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.HideTooltip();
    }
    
    // Getters for accessing item data
    public UI_Item GetItemData() => itemData;
    public int GetItemID() => itemData?.ID ?? -1;
    public string GetItemName() => itemData?.ItemName ?? "Unknown";
}
