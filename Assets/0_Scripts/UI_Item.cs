using UnityEngine;

public enum ItemType
{
    UI,
    Ground
}

public enum ItemCategory
{
    Equipment,
    Structure,
    Consumable
}

public enum EquipmentSubcategory
{
    Top,
    Bottom,
    Jewelry,
    Helmet,
    Shoes,
    Glove
}

[CreateAssetMenu(fileName = "New Item", menuName = "Scriptable Objects/UI_Item")]
public class UI_Item : ScriptableObject
{
    [Header("Item Properties")]
    [SerializeField] private int id;
    [SerializeField] private string itemName;
    [SerializeField] private ItemType itemType = ItemType.UI;
    [SerializeField] private ItemCategory itemCategory = ItemCategory.Equipment;
    [SerializeField] private EquipmentSubcategory equipmentSubcategory = EquipmentSubcategory.Top;
    [SerializeField] private bool stackable;
    [SerializeField] private bool placeable;

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
    public ItemCategory ItemCategory => itemCategory;
    public EquipmentSubcategory EquipmentSubcategory => equipmentSubcategory;
    public bool Stackable => stackable;
    public bool Placeable => placeable;
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
    
    // Helper methods for working with item categories
    public bool IsEquipment()
    {
        return itemCategory == ItemCategory.Equipment;
    }
    
    public bool IsStructure()
    {
        return itemCategory == ItemCategory.Structure;
    }
    
    public bool IsConsumable()
    {
        return itemCategory == ItemCategory.Consumable;
    }
    
    // Helper methods for working with equipment subcategories
    public bool IsTop()
    {
        return itemCategory == ItemCategory.Equipment && equipmentSubcategory == EquipmentSubcategory.Top;
    }
    
    public bool IsBottom()
    {
        return itemCategory == ItemCategory.Equipment && equipmentSubcategory == EquipmentSubcategory.Bottom;
    }
    
    public bool IsJewelry()
    {
        return itemCategory == ItemCategory.Equipment && equipmentSubcategory == EquipmentSubcategory.Jewelry;
    }
    
    public bool IsHelmet()
    {
        return itemCategory == ItemCategory.Equipment && equipmentSubcategory == EquipmentSubcategory.Helmet;
    }
    
    public bool IsShoes()
    {
        return itemCategory == ItemCategory.Equipment && equipmentSubcategory == EquipmentSubcategory.Shoes;
    }
    
    public bool IsGlove()
    {
        return itemCategory == ItemCategory.Equipment && equipmentSubcategory == EquipmentSubcategory.Glove;
    }
    
    // Optional: Override ToString for debugging
    public override string ToString()
    {
        string subcategoryInfo = itemCategory == ItemCategory.Equipment ? $", Subcategory: {equipmentSubcategory}" : "";
        return $"UI_Item: {itemName} (ID: {id}, Type: {itemType}, Category: {itemCategory}{subcategoryInfo})";
    }
}
