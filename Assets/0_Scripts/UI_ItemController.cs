using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_ItemController : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private UI_Item itemData;
    
    [Header("UI Components")]
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
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
            Debug.LogWarning("No item data assigned to UI_ItemController on " + gameObject.name);
            return;
        }
        
        // Set item image
        if (itemImage != null && itemData.Image != null)
        {
            itemImage.sprite = itemData.Image;
        }
        
        // Set item name text
        if (itemNameText != null)
        {
            itemNameText.text = itemData.ItemName;
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
    
    // Getters for accessing item data
    public UI_Item GetItemData() => itemData;
    public int GetItemID() => itemData?.ID ?? -1;
    public string GetItemName() => itemData?.ItemName ?? "Unknown";
}
