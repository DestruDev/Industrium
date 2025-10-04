using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class Inventory : MonoBehaviour
{
    [Header("Inventory UI")]
    [SerializeField] private GameObject inventoryUI;
    private AdminConsole adminConsole;
    
    [Header("Cursor Item Settings")]
    [SerializeField] private float cursorItemSizeX = 32f;
    [SerializeField] private float cursorItemSizeY = 32f;
    [SerializeField] private float cursorOffsetX = 0f;
    [SerializeField] private float cursorOffsetY = 0f;
    [SerializeField] private float cursorQuantityOffsetX = -2f;
    [SerializeField] private float cursorQuantityOffsetY = 2f;
    
    [Header("Quantity Text Offset Settings")]
    [SerializeField] private float hotbarOffsetX = 0f;
    [SerializeField] private float hotbarOffsetY = 0f;
    [SerializeField] private float inventoryOffsetX = 0f;
    [SerializeField] private float inventoryOffsetY = 0f;
    
    [Header("Quantity Text Font Size Settings")]
    [SerializeField] private float hotbarFontSize = 12f;
    [SerializeField] private float inventoryFontSize = 12f;
    
    [Header("Quantity Text Color Settings")]
    [SerializeField] private Color hotbarTextColor = Color.white;
    [SerializeField] private Color inventoryTextColor = Color.white;
    
    [Header("Quantity Text Style Settings")]
    [SerializeField] private FontStyles quantityFontStyle = FontStyles.Bold;
    [SerializeField] private Material quantityTextMaterial;
    [SerializeField] private TMP_FontAsset quantityFontAsset;
    
    [Header("Slot References")]
    [SerializeField] private ItemSlot[] hotbarSlots = new ItemSlot[8];
    [SerializeField] private ItemSlot[] inventorySlots;
    
    [Header("Debug Info")]
    [SerializeField] private bool showDebugInfo = true;
    
    void Start()
    {
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
        }
        adminConsole = FindFirstObjectByType<AdminConsole>();
        
        // Initialize slot references
        InitializeSlotReferences();
    }
    
    private void InitializeSlotReferences()
    {
        // If hotbar slots are not assigned, try to find them
        if (hotbarSlots == null || hotbarSlots.Length == 0 || hotbarSlots[0] == null)
        {
            if (showDebugInfo) Debug.Log("Hotbar slots not assigned, attempting to find them...");
            
            // Try to find hotbar slots by looking for ItemSlots that are marked as hotbar slots
            ItemSlot[] allSlots = FindObjectsByType<ItemSlot>(FindObjectsSortMode.None);
            List<ItemSlot> foundHotbarSlots = new List<ItemSlot>();
            
            foreach (ItemSlot slot in allSlots)
            {
                if (slot.IsHotbarSlot())
                {
                    foundHotbarSlots.Add(slot);
                }
            }
            
            if (foundHotbarSlots.Count > 0)
            {
                hotbarSlots = foundHotbarSlots.ToArray();
                if (showDebugInfo) Debug.Log($"Found {hotbarSlots.Length} hotbar slots automatically");
            }
        }
        
        // If inventory slots are not assigned, try to find them
        if (inventorySlots == null || inventorySlots.Length == 0 || (inventorySlots.Length > 0 && inventorySlots[0] == null))
        {
            if (showDebugInfo) Debug.Log("Inventory slots not assigned, attempting to find them...");
            
            // Try to find inventory slots by looking for ItemSlots that are marked as inventory slots
            ItemSlot[] allSlots = FindObjectsByType<ItemSlot>(FindObjectsSortMode.None);
            List<ItemSlot> foundInventorySlots = new List<ItemSlot>();
            
            foreach (ItemSlot slot in allSlots)
            {
                if (slot.IsInventorySlot())
                {
                    foundInventorySlots.Add(slot);
                }
            }
            
            if (foundInventorySlots.Count > 0)
            {
                inventorySlots = foundInventorySlots.ToArray();
                if (showDebugInfo) Debug.Log($"Found {inventorySlots.Length} inventory slots automatically");
            }
        }
        
        if (showDebugInfo) Debug.Log($"Slot initialization complete - Hotbar: {hotbarSlots?.Length ?? 0}, Inventory: {inventorySlots?.Length ?? 0}");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Only allow inventory toggle if admin console is not open
            if (adminConsole == null || !IsAdminConsoleOpen())
            {
                ToggleInventory();
            }
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
    
    // Private method to check if admin console is open
    private bool IsAdminConsoleOpen()
    {
        return adminConsole != null && adminConsole.adminPanel != null && adminConsole.adminPanel.activeSelf;
    }
    
    // Public getters for cursor item settings
    public float GetCursorItemSizeX() => cursorItemSizeX;
    public float GetCursorItemSizeY() => cursorItemSizeY;
    public float GetCursorOffsetX() => cursorOffsetX;
    public float GetCursorOffsetY() => cursorOffsetY;
    public float GetCursorQuantityOffsetX() => cursorQuantityOffsetX;
    public float GetCursorQuantityOffsetY() => cursorQuantityOffsetY;
    
    /// <summary>
    /// Spawn an item into the inventory by ID, prioritizing hotbar slots first
    /// If the item is stackable and already exists, increment the quantity instead
    /// </summary>
    /// <param name="itemID">The ID of the item to spawn</param>
    /// <returns>True if the item was successfully spawned, false otherwise</returns>
    public bool SpawnItemByID(int itemID)
    {
        if (showDebugInfo) Debug.Log($"Attempting to spawn item with ID: {itemID}");
        
        // Get the item from ItemManager
        if (ItemManager.Instance == null)
        {
            Debug.LogError("ItemManager not found! Make sure ItemManager is in the scene.");
            return false;
        }
        
        UI_Item itemToSpawn = ItemManager.Instance.GetItemByID(itemID);
        if (itemToSpawn == null)
        {
            Debug.LogWarning($"Item with ID {itemID} not found in database.");
            return false;
        }
        
        if (showDebugInfo) Debug.Log($"Found item: {itemToSpawn.ItemName}");
        
        // If item is stackable, try to add to existing stack first
        if (itemToSpawn.Stackable)
        {
            if (showDebugInfo) Debug.Log($"Item {itemToSpawn.ItemName} is stackable, looking for existing stack...");
            ItemSlot existingSlot = FindExistingStackSlot(itemToSpawn);
            if (existingSlot != null)
            {
                if (showDebugInfo) Debug.Log($"Found existing stack, attempting to add to it...");
                return AddToExistingStack(existingSlot, itemToSpawn);
            }
            else
            {
                if (showDebugInfo) Debug.Log($"No existing stack found, will create new slot");
            }
        }
        else
        {
            if (showDebugInfo) Debug.Log($"Item {itemToSpawn.ItemName} is not stackable, will create new slot");
        }
        
        // Find an available slot for new item
        ItemSlot availableSlot = FindAvailableSlot();
        if (availableSlot == null)
        {
            Debug.LogWarning("No available slots in inventory!");
            return false;
        }
        
        if (showDebugInfo) Debug.Log($"Found available slot: {availableSlot.name}");
        
        // Spawn the item into the slot
        return SpawnItemIntoSlot(availableSlot, itemToSpawn);
    }
    
    /// <summary>
    /// Find the first available slot, prioritizing hotbar slots (1-8) then inventory slots
    /// </summary>
    /// <returns>The first available ItemSlot, or null if none are available</returns>
    private ItemSlot FindAvailableSlot()
    {
        // First, try to use the Hotbar component if it exists
        Hotbar hotbar = FindFirstObjectByType<Hotbar>();
        if (hotbar != null)
        {
            if (showDebugInfo) Debug.Log("Found Hotbar component, using it for slot detection");
            
            // Check hotbar slots first (slots 0-7, which correspond to 1-8)
            for (int i = 0; i < 8; i++)
            {
                ItemSlot slot = hotbar.GetSlot(i);
                if (slot != null && IsSlotEmpty(slot))
                {
                    if (showDebugInfo) Debug.Log($"Selected hotbar slot {i + 1}: {slot.name}");
                    return slot;
                }
            }
            if (showDebugInfo) Debug.Log("All hotbar slots are full, checking inventory slots...");
        }
        
        // Use cached hotbar slots if available
        if (hotbarSlots != null && hotbarSlots.Length > 0)
        {
            if (showDebugInfo) Debug.Log("Using cached hotbar slots");
            for (int i = 0; i < hotbarSlots.Length; i++)
            {
                if (hotbarSlots[i] != null && IsSlotEmpty(hotbarSlots[i]))
                {
                    if (showDebugInfo) Debug.Log($"Selected cached hotbar slot {i + 1}: {hotbarSlots[i].name}");
                    return hotbarSlots[i];
                }
            }
            if (showDebugInfo) Debug.Log("All cached hotbar slots are full, checking inventory slots...");
        }
        
        // Use cached inventory slots if available
        if (inventorySlots != null && inventorySlots.Length > 0)
        {
            if (showDebugInfo) Debug.Log("Using cached inventory slots");
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i] != null && IsSlotEmpty(inventorySlots[i]))
                {
                    if (showDebugInfo) Debug.Log($"Selected cached inventory slot {i + 1}: {inventorySlots[i].name}");
                    return inventorySlots[i];
                }
            }
        }
        
        // Fallback: Get all ItemSlots in the scene (including inactive ones)
        ItemSlot[] allSlots = FindObjectsByType<ItemSlot>(FindObjectsSortMode.None);
        
        Debug.Log($"Found {allSlots.Length} total ItemSlots in scene (including inactive)");
        
        // First, try to find an empty hotbar slot (slots 1-8)
        List<ItemSlot> foundHotbarSlots = new List<ItemSlot>();
        List<ItemSlot> foundInventorySlots = new List<ItemSlot>();
        
        foreach (ItemSlot slot in allSlots)
        {
            if (slot.IsHotbarSlot())
            {
                foundHotbarSlots.Add(slot);
                Debug.Log($"Found hotbar slot: {slot.name} (Active: {slot.gameObject.activeInHierarchy})");
            }
            else if (slot.IsInventorySlot())
            {
                foundInventorySlots.Add(slot);
                Debug.Log($"Found inventory slot: {slot.name} (Active: {slot.gameObject.activeInHierarchy})");
            }
            else
            {
                Debug.LogWarning($"Found slot that is neither hotbar nor inventory: {slot.name}");
            }
        }
        
        Debug.Log($"Total hotbar slots: {foundHotbarSlots.Count}, Total inventory slots: {foundInventorySlots.Count}");
        
        // Sort hotbar slots by name to get proper order (assuming they're named like "Slot1", "Slot2", etc.)
        foundHotbarSlots.Sort((a, b) => a.name.CompareTo(b.name));
        
        // Check hotbar slots first (prioritize slots 1-8)
        foreach (ItemSlot slot in foundHotbarSlots)
        {
            bool isEmpty = IsSlotEmpty(slot);
            Debug.Log($"Hotbar slot {slot.name}: Empty = {isEmpty}");
            if (isEmpty)
            {
                Debug.Log($"Selected hotbar slot: {slot.name}");
                return slot;
            }
        }
        
        // If no hotbar slots available, check inventory slots
        foreach (ItemSlot slot in foundInventorySlots)
        {
            bool isEmpty = IsSlotEmpty(slot);
            Debug.Log($"Inventory slot {slot.name}: Empty = {isEmpty}");
            if (isEmpty)
            {
                Debug.Log($"Selected inventory slot: {slot.name}");
                return slot;
            }
        }
        
        Debug.LogWarning("No available slots found!");
        return null; // No available slots
    }
    
    /// <summary>
    /// Check if a slot is empty (has no item)
    /// </summary>
    /// <param name="slot">The slot to check</param>
    /// <returns>True if the slot is empty, false otherwise</returns>
    private bool IsSlotEmpty(ItemSlot slot)
    {
        if (slot == null) return false;
        
        UI_ItemController controller = slot.GetItemController();
        if (controller == null) return false;
        
        return controller.GetItemData() == null;
    }
    
    /// <summary>
    /// Find an existing slot with the same stackable item that has room for more
    /// </summary>
    /// <param name="item">The item to look for</param>
    /// <returns>ItemSlot with existing stack, or null if none found</returns>
    private ItemSlot FindExistingStackSlot(UI_Item item)
    {
        const int MAX_STACK_SIZE = 99;
        
        if (showDebugInfo) Debug.Log($"Looking for existing stack of {item.ItemName} (ID: {item.ID})");
        
        // Get all slots (hotbar first, then inventory)
        ItemSlot[] allSlots = GetAllSlots();
        
        if (showDebugInfo) Debug.Log($"Checking {allSlots.Length} slots for existing stack");
        
        foreach (ItemSlot slot in allSlots)
        {
            if (slot == null) 
            {
                if (showDebugInfo) Debug.Log("Skipping null slot");
                continue;
            }
            
            UI_ItemController controller = slot.GetItemController();
            if (controller == null) 
            {
                if (showDebugInfo) Debug.Log($"Slot {slot.name} has no controller");
                continue;
            }
            
            UI_Item existingItem = controller.GetItemData();
            if (existingItem == null) 
            {
                if (showDebugInfo) Debug.Log($"Slot {slot.name} is empty");
                continue;
            }
            
            if (showDebugInfo) Debug.Log($"Slot {slot.name} contains {existingItem.ItemName} (ID: {existingItem.ID})");
            
            // Check if it's the same item and has room for more
            if (existingItem.ID == item.ID && existingItem.Stackable)
            {
                int currentQuantity = controller.GetQuantity();
                if (showDebugInfo) Debug.Log($"Found matching item {existingItem.ItemName} with quantity {currentQuantity}");
                
                if (currentQuantity < MAX_STACK_SIZE)
                {
                    if (showDebugInfo) Debug.Log($"Found existing stack of {existingItem.ItemName} with {currentQuantity} items (room for {MAX_STACK_SIZE - currentQuantity} more)");
                    return slot;
                }
                else
                {
                    if (showDebugInfo) Debug.Log($"Stack is full at {currentQuantity} items");
                }
            }
        }
        
        if (showDebugInfo) Debug.Log("No existing stack found");
        return null;
    }
    
    /// <summary>
    /// Add an item to an existing stack
    /// </summary>
    /// <param name="slot">The slot with the existing stack</param>
    /// <param name="item">The item to add</param>
    /// <returns>True if successful, false otherwise</returns>
    private bool AddToExistingStack(ItemSlot slot, UI_Item item)
    {
        const int MAX_STACK_SIZE = 99;
        
        if (showDebugInfo) Debug.Log($"Attempting to add {item.ItemName} to existing stack in slot {slot.name}");
        
        UI_ItemController controller = slot.GetItemController();
        if (controller == null) 
        {
            if (showDebugInfo) Debug.LogError("Controller is null!");
            return false;
        }
        
        int currentQuantity = controller.GetQuantity();
        int newQuantity = currentQuantity + 1;
        
        if (showDebugInfo) Debug.Log($"Current quantity: {currentQuantity}, New quantity: {newQuantity}");
        
        // Check if we can add the item (respect max stack size)
        if (newQuantity <= MAX_STACK_SIZE)
        {
            controller.SetQuantity(newQuantity);
            if (showDebugInfo) Debug.Log($"Added to existing stack: {item.ItemName} now has {newQuantity} items");
            return true;
        }
        else
        {
            if (showDebugInfo) Debug.Log($"Cannot add to stack: {item.ItemName} is at max capacity ({MAX_STACK_SIZE})");
            return false;
        }
    }
    
    /// <summary>
    /// Get all slots in order (hotbar first, then inventory)
    /// </summary>
    /// <returns>Array of all ItemSlots</returns>
    private ItemSlot[] GetAllSlots()
    {
        List<ItemSlot> allSlots = new List<ItemSlot>();
        
        // Add hotbar slots first
        if (hotbarSlots != null)
        {
            foreach (ItemSlot slot in hotbarSlots)
            {
                if (slot != null) allSlots.Add(slot);
            }
        }
        
        // Add inventory slots
        if (inventorySlots != null)
        {
            foreach (ItemSlot slot in inventorySlots)
            {
                if (slot != null) allSlots.Add(slot);
            }
        }
        
        return allSlots.ToArray();
    }
    
    /// <summary>
    /// Spawn an item into a specific slot
    /// </summary>
    /// <param name="slot">The slot to spawn the item into</param>
    /// <param name="item">The item to spawn</param>
    /// <returns>True if successful, false otherwise</returns>
    private bool SpawnItemIntoSlot(ItemSlot slot, UI_Item item)
    {
        if (slot == null || item == null)
        {
            Debug.LogError("Cannot spawn item: slot or item is null");
            return false;
        }
        
        UI_ItemController controller = slot.GetItemController();
        if (controller == null)
        {
            Debug.LogError("Cannot spawn item: slot has no UI_ItemController");
            return false;
        }
        
        // Set the item data with quantity 1
        controller.SetItemData(item, 1);
        
        Debug.Log($"Successfully spawned {item.ItemName} (ID: {item.ID}) into slot: {slot.name}");
        return true;
    }
    
    // Quantity Text Offset Getters
    public float GetHotbarOffsetX() => hotbarOffsetX;
    public float GetHotbarOffsetY() => hotbarOffsetY;
    public float GetInventoryOffsetX() => inventoryOffsetX;
    public float GetInventoryOffsetY() => inventoryOffsetY;
    
    // Quantity Text Font Size Getters
    public float GetHotbarFontSize() => hotbarFontSize;
    public float GetInventoryFontSize() => inventoryFontSize;
    
    // Quantity Text Color Getters
    public Color GetHotbarTextColor() => hotbarTextColor;
    public Color GetInventoryTextColor() => inventoryTextColor;
    
    // Quantity Text Style Getters
    public FontStyles GetQuantityFontStyle() => quantityFontStyle;
    public Material GetQuantityTextMaterial() => quantityTextMaterial;
    public TMP_FontAsset GetQuantityFontAsset() => quantityFontAsset;
    
    // Quantity Text Offset Setters
    public void SetHotbarOffset(float x, float y)
    {
        hotbarOffsetX = x;
        hotbarOffsetY = y;
        UpdateAllQuantityTextPositions();
    }
    
    public void SetInventoryOffset(float x, float y)
    {
        inventoryOffsetX = x;
        inventoryOffsetY = y;
        UpdateAllQuantityTextPositions();
    }
    
    // Quantity Text Color Setters
    public void SetHotbarTextColor(Color color)
    {
        hotbarTextColor = color;
        UpdateAllQuantityTextPositions();
    }
    
    public void SetInventoryTextColor(Color color)
    {
        inventoryTextColor = color;
        UpdateAllQuantityTextPositions();
    }
    
    // Update all quantity text positions in the scene
    private void UpdateAllQuantityTextPositions()
    {
        // Find all UI_ItemController components and update their quantity text positions
        UI_ItemController[] allControllers = FindObjectsByType<UI_ItemController>(FindObjectsSortMode.None);
        foreach (UI_ItemController controller in allControllers)
        {
            if (controller != null)
            {
                // Force update the quantity text positioning
                controller.UpdateUI();
            }
        }
    }
}