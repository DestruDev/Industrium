using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ItemManager : MonoBehaviour
{
    [Header("Item Database")]
    [SerializeField] private UI_Item[] itemDatabase;
    
    private Dictionary<int, UI_Item> itemLookup;
    
    public static ItemManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            InitializeItemDatabase();
        }
        else if (Instance != this)
        {
            Debug.LogWarning("Multiple ItemManager instances found! Destroying duplicate.");
            Destroy(this);
        }
    }
    
    private void InitializeItemDatabase()
    {
        // Create lookup dictionary for fast ID-based access
        itemLookup = new Dictionary<int, UI_Item>();
        
        foreach (UI_Item item in itemDatabase)
        {
            if (item != null)
            {
                itemLookup[item.ID] = item;
                Debug.Log($"Loaded item: {item.ItemName} (ID: {item.ID})");
            }
        }
        
        Debug.Log($"ItemManager initialized with {itemLookup.Count} items");
    }
    
    /// <summary>
    /// Get a UI_Item by its ID
    /// </summary>
    /// <param name="id">The ID of the item to retrieve</param>
    /// <returns>The UI_Item with the specified ID, or null if not found</returns>
    public UI_Item GetItemByID(int id)
    {
        if (itemLookup.TryGetValue(id, out UI_Item item))
        {
            return item;
        }
        
        Debug.LogWarning($"Item with ID {id} not found in database");
        return null;
    }
    
    /// <summary>
    /// Get all available item IDs
    /// </summary>
    /// <returns>Array of all item IDs in the database</returns>
    public int[] GetAllItemIDs()
    {
        return itemLookup.Keys.ToArray();
    }
    
    /// <summary>
    /// Check if an item with the specified ID exists
    /// </summary>
    /// <param name="id">The ID to check</param>
    /// <returns>True if the item exists, false otherwise</returns>
    public bool ItemExists(int id)
    {
        return itemLookup.ContainsKey(id);
    }
    
    /// <summary>
    /// Get the total number of items in the database
    /// </summary>
    /// <returns>Number of items in the database</returns>
    public int GetItemCount()
    {
        return itemLookup.Count;
    }
}
