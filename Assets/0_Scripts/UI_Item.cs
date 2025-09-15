using UnityEngine;

public enum ItemType
{
    UI,
    Ground
}

[CreateAssetMenu(fileName = "New Item", menuName = "Scriptable Objects/UI_Item")]
public class UI_Item : ScriptableObject
{
    [Header("Item Properties")]
    [SerializeField] private int id;
    [SerializeField] private string itemName;
    [SerializeField] private ItemType itemType = ItemType.UI;
    
    [Header("UI Item Properties")]
    [SerializeField] private Sprite image;
    
    [Header("Ground Item Properties")]
    [SerializeField] private Sprite groundSprite;
    [SerializeField] private Vector3 groundScale = Vector3.one;
    [SerializeField] private bool hasCollider = true;
    [SerializeField] private bool isTrigger = false;
    [SerializeField] private float pickupRadius = 1f;
    
    // Public getters for accessing the properties
    public int ID => id;
    public string ItemName => itemName;
    public ItemType ItemType => itemType;
    public Sprite Image => image;
    public Sprite GroundSprite => groundSprite;
    public Vector3 GroundScale => groundScale;
    public bool HasCollider => hasCollider;
    public bool IsTrigger => isTrigger;
    public float PickupRadius => pickupRadius;
    
    // Helper methods for working with different item types
    public Sprite GetDisplaySprite()
    {
        return itemType == ItemType.UI ? image : groundSprite;
    }
    
    public bool IsUIItem()
    {
        return itemType == ItemType.UI;
    }
    
    public bool IsGroundItem()
    {
        return itemType == ItemType.Ground;
    }
    
    // Optional: Override ToString for debugging
    public override string ToString()
    {
        return $"UI_Item: {itemName} (ID: {id}, Type: {itemType})";
    }
}
