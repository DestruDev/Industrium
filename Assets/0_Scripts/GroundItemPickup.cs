using UnityEngine;

/// <summary>
/// Script for ground items that can be picked up by the player
/// </summary>
public class GroundItemPickup : MonoBehaviour
{
    private UI_Item itemData;
    private float pickupRadius = 1f;
    private Transform player;
    
    void Start()
    {
        // Find the player
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        
        // Set pickup radius from item data if available
        if (itemData != null)
        {
            pickupRadius = itemData.PickupRadius;
        }
    }
    
    void Update()
    {
        // Check if player is within pickup radius
        if (player != null && Vector3.Distance(transform.position, player.position) <= pickupRadius)
        {
            // Try to pick up the item
            TryPickupItem();
        }
    }
    
    /// <summary>
    /// Set the item data for this ground item
    /// </summary>
    /// <param name="item">The UI_Item data</param>
    public void SetItemData(UI_Item item)
    {
        itemData = item;
        if (itemData != null)
        {
            pickupRadius = itemData.PickupRadius;
        }
    }
    
    /// <summary>
    /// Try to pick up this item and add it to the player's inventory
    /// </summary>
    private void TryPickupItem()
    {
        if (itemData == null) return;
        
        // Find the inventory
        Inventory inventory = FindFirstObjectByType<Inventory>();
        if (inventory == null)
        {
            Debug.LogWarning("GroundItemPickup: No inventory found!");
            return;
        }
        
        // Try to add the item to inventory
        bool success = inventory.SpawnItemByID(itemData.ID);
        
        if (success)
        {
            Debug.Log($"Picked up ground item: {itemData.ItemName}");
            Destroy(gameObject); // Remove the ground item
        }
        else
        {
            Debug.LogWarning($"Failed to pick up {itemData.ItemName} - inventory might be full");
        }
    }
    
    /// <summary>
    /// Draw pickup radius in the editor for debugging
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}

