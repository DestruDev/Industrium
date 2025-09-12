using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Scriptable Objects/UI_Item")]
public class UI_Item : ScriptableObject
{
    [Header("Item Properties")]
    [SerializeField] private int id;
    [SerializeField] private string itemName;
    [SerializeField] private Sprite image;
    
    // Public getters for accessing the properties
    public int ID => id;
    public string ItemName => itemName;
    public Sprite Image => image;
    
    // Optional: Override ToString for debugging
    public override string ToString()
    {
        return $"UI_Item: {itemName} (ID: {id})";
    }
}
