using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Inventory UI")]
    [SerializeField] private GameObject inventoryUI;
    
    void Start()
    {
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleInventory();
        }
    }
    
    private void ToggleInventory()
    {
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);
        }
    }
}