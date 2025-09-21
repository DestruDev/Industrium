using UnityEngine;
using UnityEngine.UI;

public class Equipment : MonoBehaviour
{
    [Header("Equipment Slots")]
    [SerializeField] private ItemSlot topSlot;        // Top equipment (shirts, armor tops)
    [SerializeField] private ItemSlot bottomSlot;     // Bottom equipment (pants, armor bottoms)
    [SerializeField] private ItemSlot shoesSlot;      // Shoes/foot equipment
    [SerializeField] private ItemSlot helmetSlot;     // Helmet/head equipment
    [SerializeField] private ItemSlot gloveSlot;      // Gloves/hand equipment
    [SerializeField] private ItemSlot jewelrySlot;    // Jewelry (rings, necklaces, etc.)
    
    [Header("Equipment Highlight Settings")]
    [SerializeField] private Color highlightColor = new Color(0.949f, 0.949f, 0.949f, 1f); // F2F2F2
    [SerializeField] private Color normalColor = Color.white;
    
    [Header("Debug Info")]
    [SerializeField] private bool showDebugInfo = true;
    
    // Array to easily access all equipment slots
    private ItemSlot[] equipmentSlots;
    private EquipmentSubcategory[] slotTypes;
    
    void Start()
    {
        InitializeEquipmentSlots();
    }
    
    private void InitializeEquipmentSlots()
    {
        // Create arrays for easy access
        equipmentSlots = new ItemSlot[6] { topSlot, bottomSlot, shoesSlot, helmetSlot, gloveSlot, jewelrySlot };
        slotTypes = new EquipmentSubcategory[6] { 
            EquipmentSubcategory.Top, 
            EquipmentSubcategory.Bottom, 
            EquipmentSubcategory.Shoes, 
            EquipmentSubcategory.Helmet, 
            EquipmentSubcategory.Glove, 
            EquipmentSubcategory.Jewelry 
        };
        
        // Validate that all slots are assigned
        for (int i = 0; i < equipmentSlots.Length; i++)
        {
            if (equipmentSlots[i] == null)
            {
                Debug.LogError($"Equipment slot {i} ({slotTypes[i]}) is not assigned!");
            }
            else
            {
                // Configure the slot for equipment use
                ConfigureEquipmentSlot(equipmentSlots[i], slotTypes[i]);
                
                if (showDebugInfo)
                {
                    Debug.Log($"Initialized equipment slot: {slotTypes[i]} - {equipmentSlots[i].name}");
                }
            }
        }
    }
    
    private void ConfigureEquipmentSlot(ItemSlot slot, EquipmentSubcategory slotType)
    {
        if (slot == null) return;
        
        // Get the UI_ItemController component
        UI_ItemController controller = slot.GetComponent<UI_ItemController>();
        if (controller == null)
        {
            Debug.LogWarning($"Equipment slot {slot.name} doesn't have a UI_ItemController component!");
            return;
        }
        
        // Set up the slot to only accept equipment items of the correct subcategory
        // This will be handled in the CanAcceptItem method
    }
    
    /// <summary>
    /// Check if an item can be equipped in a specific slot
    /// </summary>
    /// <param name="item">The item to check</param>
    /// <param name="slotType">The equipment slot type</param>
    /// <returns>True if the item can be equipped in this slot</returns>
    public bool CanEquipItem(UI_Item item, EquipmentSubcategory slotType)
    {
        if (item == null) return false;
        
        // Only equipment items can be equipped
        if (item.ItemCategory != ItemCategory.Equipment) return false;
        
        // Check if the item's subcategory matches the slot type
        return item.EquipmentSubcategory == slotType;
    }
    
    /// <summary>
    /// Get the equipment slot for a specific subcategory
    /// </summary>
    /// <param name="subcategory">The equipment subcategory</param>
    /// <returns>The ItemSlot for this subcategory, or null if not found</returns>
    public ItemSlot GetEquipmentSlot(EquipmentSubcategory subcategory)
    {
        for (int i = 0; i < slotTypes.Length; i++)
        {
            if (slotTypes[i] == subcategory)
            {
                return equipmentSlots[i];
            }
        }
        return null;
    }
    
    /// <summary>
    /// Get the currently equipped item for a specific subcategory
    /// </summary>
    /// <param name="subcategory">The equipment subcategory</param>
    /// <returns>The equipped UI_Item, or null if nothing is equipped</returns>
    public UI_Item GetEquippedItem(EquipmentSubcategory subcategory)
    {
        ItemSlot slot = GetEquipmentSlot(subcategory);
        if (slot == null || slot.GetItemController() == null) return null;
        
        return slot.GetItemController().GetItemData();
    }
    
    /// <summary>
    /// Equip an item in the appropriate slot
    /// </summary>
    /// <param name="item">The item to equip</param>
    /// <returns>True if successfully equipped, false otherwise</returns>
    public bool EquipItem(UI_Item item)
    {
        if (item == null) return false;
        
        // Check if this is an equipment item
        if (item.ItemCategory != ItemCategory.Equipment)
        {
            if (showDebugInfo) Debug.LogWarning($"Cannot equip {item.ItemName}: Not an equipment item");
            return false;
        }
        
        // Get the appropriate slot for this equipment type
        ItemSlot targetSlot = GetEquipmentSlot(item.EquipmentSubcategory);
        if (targetSlot == null)
        {
            if (showDebugInfo) Debug.LogError($"No equipment slot found for {item.EquipmentSubcategory}");
            return false;
        }
        
        // Get the current item in the slot
        UI_Item currentItem = GetEquippedItem(item.EquipmentSubcategory);
        
        // Set the new item
        UI_ItemController controller = targetSlot.GetItemController();
        if (controller != null)
        {
            controller.SetItemData(item);
            
            if (showDebugInfo)
            {
                if (currentItem != null)
                {
                    Debug.Log($"Equipped {item.ItemName}, unequipped {currentItem.ItemName}");
                }
                else
                {
                    Debug.Log($"Equipped {item.ItemName}");
                }
            }
            
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Unequip an item from a specific slot
    /// </summary>
    /// <param name="subcategory">The equipment subcategory to unequip</param>
    /// <returns>The unequipped item, or null if nothing was equipped</returns>
    public UI_Item UnequipItem(EquipmentSubcategory subcategory)
    {
        ItemSlot slot = GetEquipmentSlot(subcategory);
        if (slot == null || slot.GetItemController() == null) return null;
        
        UI_Item currentItem = slot.GetItemController().GetItemData();
        if (currentItem != null)
        {
            slot.GetItemController().SetItemData(null);
            
            if (showDebugInfo)
            {
                Debug.Log($"Unequipped {currentItem.ItemName}");
            }
        }
        
        return currentItem;
    }
    
    /// <summary>
    /// Get all currently equipped items
    /// </summary>
    /// <returns>Array of equipped items (null entries for empty slots)</returns>
    public UI_Item[] GetAllEquippedItems()
    {
        UI_Item[] equippedItems = new UI_Item[6];
        
        for (int i = 0; i < equipmentSlots.Length; i++)
        {
            if (equipmentSlots[i] != null && equipmentSlots[i].GetItemController() != null)
            {
                equippedItems[i] = equipmentSlots[i].GetItemController().GetItemData();
            }
        }
        
        return equippedItems;
    }
    
    /// <summary>
    /// Check if a slot is empty
    /// </summary>
    /// <param name="subcategory">The equipment subcategory to check</param>
    /// <returns>True if the slot is empty</returns>
    public bool IsSlotEmpty(EquipmentSubcategory subcategory)
    {
        return GetEquippedItem(subcategory) == null;
    }
    
    /// <summary>
    /// Get the total number of equipped items
    /// </summary>
    /// <returns>Number of equipped items</returns>
    public int GetEquippedItemCount()
    {
        int count = 0;
        UI_Item[] equippedItems = GetAllEquippedItems();
        
        foreach (UI_Item item in equippedItems)
        {
            if (item != null) count++;
        }
        
        return count;
    }
    
    // Public getters for inspector access
    public ItemSlot GetTopSlot() => topSlot;
    public ItemSlot GetBottomSlot() => bottomSlot;
    public ItemSlot GetShoesSlot() => shoesSlot;
    public ItemSlot GetHelmetSlot() => helmetSlot;
    public ItemSlot GetGloveSlot() => gloveSlot;
    public ItemSlot GetJewelrySlot() => jewelrySlot;
}
