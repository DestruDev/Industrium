using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AdminConsole : MonoBehaviour
{
    public GameObject adminPanel;
    public TMP_InputField textInput;
    public Button spawnButton;
    private Inventory inventory;
    void Start()
    {
        adminPanel.SetActive(false);
        inventory = FindFirstObjectByType<Inventory>();
        
        // Setup spawn button if it exists
        if (spawnButton != null)
        {
            spawnButton.onClick.AddListener(OnSpawnButtonClicked);
        }
        
        // Setup input field to only allow numbers
        if (textInput != null)
        {
            textInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            textInput.onValidateInput += ValidateNumberInput;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Only allow admin console toggle if inventory is not open
            if (inventory == null || !inventory.IsInventoryOpen())
            {
                ToggleAdminPanel();
            }
        }
        
        // Allow Enter key to spawn item
        if (Input.GetKeyDown(KeyCode.Return) && adminPanel.activeSelf)
        {
            OnSpawnButtonClicked();
        }
    }

    private void ToggleAdminPanel()
    {
        adminPanel.SetActive(!adminPanel.activeSelf);
        
        // Focus on input field when opening
        if (adminPanel.activeSelf && textInput != null)
        {
            textInput.Select();
            textInput.ActivateInputField();
        }
    }
    
    private char ValidateNumberInput(string text, int charIndex, char addedChar)
    {
        // Only allow digits (0-9)
        if (char.IsDigit(addedChar))
        {
            return addedChar;
        }
        
        // Reject all other characters
        return '\0';
    }
    
    private void OnSpawnButtonClicked()
    {
        if (textInput == null || inventory == null)
        {
            Debug.LogWarning("AdminConsole: Missing textInput or inventory reference!");
            return;
        }
        
        string inputText = textInput.text.Trim();
        
        if (string.IsNullOrEmpty(inputText))
        {
            Debug.LogWarning("AdminConsole: Please enter an item ID!");
            return;
        }
        
        // Try to parse the input as an integer
        if (int.TryParse(inputText, out int itemID))
        {
            // Debug: Show available item IDs
            if (ItemManager.Instance != null)
            {
                int[] availableIDs = ItemManager.Instance.GetAllItemIDs();
                Debug.Log($"Available item IDs: [{string.Join(", ", availableIDs)}]");
            }
            
            bool success = inventory.SpawnItemByID(itemID);
            
            if (success)
            {
                Debug.Log($"AdminConsole: Successfully spawned item with ID {itemID}");
                textInput.text = ""; // Clear the input field
            }
            else
            {
                Debug.LogWarning($"AdminConsole: Failed to spawn item with ID {itemID}");
            }
        }
        else
        {
            Debug.LogWarning("AdminConsole: Please enter a valid number for the item ID!");
        }
    }
}
