using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ItemType
{
    UI,
    Ground
}

public enum ItemCategory
{
    Equipment,
    Structure,
    Consumable,
    Tool,
    Material
}

    public enum EquipmentSubcategory
    {
        Top,
        Bottom,
        Shoes,
        Helmet,
        Glove,
        Jewelry
    }

    public enum StructureSubcategory
    {
        Size1x1,
        Size1x2,
        Size2x1,
        Size2x2,
        Size2x3,
        Size3x1,
        Size3x2,
        Size3x3
    }

    public enum ToolSubcategory
    {
        Weapon,
        Tool
    }

    public enum MaterialSubcategory
    {
        Metal,
        Wood,
        Stone,
        Fabric,
        Gem,
        Chemical,
        Organic,
        Other
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
    [SerializeField] private StructureSubcategory structureSubcategory = StructureSubcategory.Size1x1;
    [SerializeField] private ToolSubcategory toolSubcategory = ToolSubcategory.Weapon;
    [SerializeField] private MaterialSubcategory materialSubcategory = MaterialSubcategory.Other;
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
    
    [Header("Structure Properties")]
    [SerializeField] private GameObject structurePrefab;
    
    // Public getters for accessing the properties
    public int ID => id;
    public string ItemName => itemName;
    public ItemType ItemType => itemType;
    public ItemCategory ItemCategory => itemCategory;
    public EquipmentSubcategory EquipmentSubcategory => equipmentSubcategory;
    public StructureSubcategory StructureSubcategory => structureSubcategory;
    public ToolSubcategory ToolSubcategory => toolSubcategory;
    public MaterialSubcategory MaterialSubcategory => materialSubcategory;
    public bool Stackable => stackable;
    public bool Placeable => placeable;
    public Sprite Image => image;
    public Sprite GroundSprite => groundSprite;
    public Vector3 GroundScale => groundScale;
    public bool HasCollider => hasCollider;
    public bool IsTrigger => isTrigger;
    public float PickupRadius => pickupRadius;
    public GameObject StructurePrefab => structurePrefab;

    
    // Validation method to ensure proper settings based on item category
    private void OnValidate()
    {
        if (itemCategory == ItemCategory.Structure)
        {
            stackable = false;
            placeable = true;
        }
        else if (itemCategory == ItemCategory.Equipment || itemCategory == ItemCategory.Consumable || itemCategory == ItemCategory.Tool)
        {
            placeable = false;
        }
    }
    
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
    
    public bool IsTool()
    {
        return itemCategory == ItemCategory.Tool;
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

    // Structure subcategory helper methods
    public bool IsSize1x1()
    {
        return itemCategory == ItemCategory.Structure && structureSubcategory == StructureSubcategory.Size1x1;
    }

    public bool IsSize1x2()
    {
        return itemCategory == ItemCategory.Structure && structureSubcategory == StructureSubcategory.Size1x2;
    }

    public bool IsSize2x1()
    {
        return itemCategory == ItemCategory.Structure && structureSubcategory == StructureSubcategory.Size2x1;
    }

    public bool IsSize2x2()
    {
        return itemCategory == ItemCategory.Structure && structureSubcategory == StructureSubcategory.Size2x2;
    }

    public bool IsSize2x3()
    {
        return itemCategory == ItemCategory.Structure && structureSubcategory == StructureSubcategory.Size2x3;
    }

    public bool IsSize3x1()
    {
        return itemCategory == ItemCategory.Structure && structureSubcategory == StructureSubcategory.Size3x1;
    }

    public bool IsSize3x2()
    {
        return itemCategory == ItemCategory.Structure && structureSubcategory == StructureSubcategory.Size3x2;
    }

    public bool IsSize3x3()
    {
        return itemCategory == ItemCategory.Structure && structureSubcategory == StructureSubcategory.Size3x3;
    }

    // Tool subcategory helper methods
    public bool IsWeapon()
    {
        return itemCategory == ItemCategory.Tool && toolSubcategory == ToolSubcategory.Weapon;
    }

    public bool IsToolType()
    {
        return itemCategory == ItemCategory.Tool && toolSubcategory == ToolSubcategory.Tool;
    }
    
    // Material subcategory helper methods
    public bool IsMetal()
    {
        return itemCategory == ItemCategory.Material && materialSubcategory == MaterialSubcategory.Metal;
    }
    
    public bool IsWood()
    {
        return itemCategory == ItemCategory.Material && materialSubcategory == MaterialSubcategory.Wood;
    }
    
    public bool IsStone()
    {
        return itemCategory == ItemCategory.Material && materialSubcategory == MaterialSubcategory.Stone;
    }
    
    public bool IsFabric()
    {
        return itemCategory == ItemCategory.Material && materialSubcategory == MaterialSubcategory.Fabric;
    }
    
    public bool IsGem()
    {
        return itemCategory == ItemCategory.Material && materialSubcategory == MaterialSubcategory.Gem;
    }
    
    public bool IsChemical()
    {
        return itemCategory == ItemCategory.Material && materialSubcategory == MaterialSubcategory.Chemical;
    }
    
    public bool IsOrganic()
    {
        return itemCategory == ItemCategory.Material && materialSubcategory == MaterialSubcategory.Organic;
    }
    
    public bool IsOtherMaterial()
    {
        return itemCategory == ItemCategory.Material && materialSubcategory == MaterialSubcategory.Other;
    }
    
    // Optional: Override ToString for debugging
    public override string ToString()
    {
        string subcategoryInfo = "";
        if (itemCategory == ItemCategory.Equipment)
        {
            subcategoryInfo = $", Subcategory: {equipmentSubcategory}";
        }
        else if (itemCategory == ItemCategory.Structure)
        {
            subcategoryInfo = $", Subcategory: {structureSubcategory}";
        }
        else if (itemCategory == ItemCategory.Tool)
        {
            subcategoryInfo = $", Subcategory: {toolSubcategory}";
        }
        else if (itemCategory == ItemCategory.Material)
        {
            subcategoryInfo = $", Subcategory: {materialSubcategory}";
        }
        
        return $"UI_Item: {itemName} (ID: {id}, Type: {itemType}, Category: {itemCategory}{subcategoryInfo})";
    }
}
