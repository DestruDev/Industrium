using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ItemManager : MonoBehaviour
{
    [Header("Item Database")]
    [SerializeField] private UI_Item[] itemDatabase;
    
    [Header("Auto-Discovery Settings")]
    [SerializeField] private bool useAutoDiscovery = true;
    [SerializeField] private string[] searchPaths = { "Assets/1_Prefabs/Items(ScriptableObjects)" };
    
    [Header("Ground Item Settings")]
    [SerializeField] private Transform groundItemContainer;
    
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
        
        // Use auto-discovery if enabled
        if (useAutoDiscovery)
        {
            LoadItemsFromAssets();
        }
        else
        {
            LoadItemsFromArray();
        }
        
        Debug.Log($"ItemManager initialized with {itemLookup.Count} items");
    }
    
    private void LoadItemsFromAssets()
    {
#if UNITY_EDITOR
        // Find all UI_Item assets in the specified paths
        string[] guids = AssetDatabase.FindAssets("t:UI_Item", searchPaths);
        
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            UI_Item item = AssetDatabase.LoadAssetAtPath<UI_Item>(assetPath);
            
            if (item != null)
            {
                if (itemLookup.ContainsKey(item.ID))
                {
                    Debug.LogWarning($"Duplicate item ID {item.ID} found: {item.ItemName} at {assetPath}. Skipping duplicate.");
                    continue;
                }
                
                itemLookup[item.ID] = item;
                Debug.Log($"Auto-loaded item: {item.ItemName} (ID: {item.ID}) from {assetPath}");
            }
        }
#else
        // Fallback to manual array in builds
        LoadItemsFromArray();
#endif
    }
    
    private void LoadItemsFromArray()
    {
        foreach (UI_Item item in itemDatabase)
        {
            if (item != null)
            {
                if (itemLookup.ContainsKey(item.ID))
                {
                    Debug.LogWarning($"Duplicate item ID {item.ID} found: {item.ItemName}. Skipping duplicate.");
                    continue;
                }
                
                itemLookup[item.ID] = item;
                Debug.Log($"Loaded item: {item.ItemName} (ID: {item.ID})");
            }
        }
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
    /// Refresh the item database (useful for testing or when new items are added)
    /// </summary>
    public void RefreshDatabase()
    {
        itemLookup.Clear();
        InitializeItemDatabase();
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
    
    /// <summary>
    /// Spawn a ground item in the world
    /// </summary>
    /// <param name="itemID">The ID of the item to spawn</param>
    /// <param name="spawnPosition">Position to spawn the item (optional, defaults to player position)</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool SpawnGroundItem(int itemID, Vector3? spawnPosition = null)
    {
        UI_Item itemToSpawn = GetItemByID(itemID);
        if (itemToSpawn == null)
        {
            Debug.LogWarning($"Item with ID {itemID} not found in database.");
            return false;
        }
        
        // Find the player if no position specified
        Vector3 finalSpawnPosition;
        if (spawnPosition.HasValue)
        {
            finalSpawnPosition = spawnPosition.Value;
        }
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("Player not found! Make sure the player has a 'Player' tag.");
                return false;
            }
            finalSpawnPosition = player.transform.position + Vector3.right * 2f;
        }
        
        // Create a new GameObject for the ground item
        GameObject groundItem = new GameObject($"Ground_{itemToSpawn.ItemName}_{itemID}");
        
        // Add SpriteRenderer component
        SpriteRenderer spriteRenderer = groundItem.AddComponent<SpriteRenderer>();
        // Always use the UI image for ground items
        spriteRenderer.sprite = itemToSpawn.Image;
        spriteRenderer.sortingOrder = 1; // Make sure it's visible above ground
        
        // Set position and scale
        groundItem.transform.position = finalSpawnPosition;
        groundItem.transform.localScale = itemToSpawn.GroundScale;
        
        // Parent to the ground item container if specified
        if (groundItemContainer != null)
        {
            groundItem.transform.SetParent(groundItemContainer);
        }
        
        // Add collider if specified
        if (itemToSpawn.HasCollider)
        {
            BoxCollider2D collider = groundItem.AddComponent<BoxCollider2D>();
            collider.isTrigger = itemToSpawn.IsTrigger;
        }
        
        // Add pickup script
        GroundItemPickup pickup = groundItem.AddComponent<GroundItemPickup>();
        pickup.SetItemData(itemToSpawn);
        
        Debug.Log($"Successfully spawned ground item: {itemToSpawn.ItemName} at position {finalSpawnPosition}");
        return true;
    }
}
