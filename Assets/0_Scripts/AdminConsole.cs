using UnityEngine;
using UnityEngine.UI;

public class AdminConsole : MonoBehaviour
{
    public GameObject adminPanel;

    void Start()
    {
        adminPanel.SetActive(false);
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleAdminPanel();
        }
    }

    private void ToggleAdminPanel()
    {
        adminPanel.SetActive(!adminPanel.activeSelf);
    }
}
