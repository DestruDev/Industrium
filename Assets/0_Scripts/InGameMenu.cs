using UnityEngine;
using UnityEngine.UI;

public class InGameMenu : MonoBehaviour
{
    [Header("Menu References")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        // Ensure menu panel starts closed
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
    }

    private void Start()
    {
        // Set up exit button
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
        }
    }

    private void Update()
    {
        // Toggle menu panel with ESC key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menuPanel != null)
            {
                menuPanel.SetActive(!menuPanel.activeSelf);
            }
        }
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    /// <summary>
    /// Check if the menu panel is currently open
    /// </summary>
    /// <returns>True if menu panel is open, false otherwise</returns>
    public bool IsMenuOpen()
    {
        return menuPanel != null && menuPanel.activeSelf;
    }
}
