using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemManager))]
public class ItemManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Item Management", EditorStyles.boldLabel);
        
        ItemManager itemManager = (ItemManager)target;
        
        if (GUILayout.Button("Refresh Database"))
        {
            itemManager.RefreshDatabase();
        }
        
        if (GUILayout.Button("Auto-Discover All Items"))
        {
            AutoDiscoverAllItems();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Auto-Discovery will automatically find all UI_Item assets in your project. " +
                               "Make sure to set unique IDs for each item to avoid conflicts.", MessageType.Info);
    }
    
    private void AutoDiscoverAllItems()
    {
        // Find all UI_Item assets
        string[] guids = AssetDatabase.FindAssets("t:UI_Item");
        int count = 0;
        
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            UI_Item item = AssetDatabase.LoadAssetAtPath<UI_Item>(assetPath);
            
            if (item != null)
            {
                count++;
                Debug.Log($"Found UI_Item: {item.ItemName} (ID: {item.ID}) at {assetPath}");
            }
        }
        
        Debug.Log($"Auto-discovery found {count} UI_Item assets in the project.");
    }
}

// Asset creation callback to automatically refresh ItemManager when new items are created
public class ItemManagerAssetPostprocessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        bool shouldRefresh = false;
        
        // Check if any UI_Item assets were imported, deleted, or moved
        foreach (string assetPath in importedAssets)
        {
            if (assetPath.EndsWith(".asset"))
            {
                UI_Item item = AssetDatabase.LoadAssetAtPath<UI_Item>(assetPath);
                if (item != null)
                {
                    shouldRefresh = true;
                    break;
                }
            }
        }
        
        foreach (string assetPath in deletedAssets)
        {
            if (assetPath.EndsWith(".asset"))
            {
                shouldRefresh = true;
                break;
            }
        }
        
        foreach (string assetPath in movedAssets)
        {
            if (assetPath.EndsWith(".asset"))
            {
                shouldRefresh = true;
                break;
            }
        }
        
        // Refresh ItemManager if needed
        if (shouldRefresh)
        {
            // Find ItemManager in the scene
            ItemManager itemManager = null;
            ItemManager[] allItemManagers = Resources.FindObjectsOfTypeAll<ItemManager>();
            foreach (ItemManager manager in allItemManagers)
            {
                if (manager.gameObject.scene.isLoaded)
                {
                    itemManager = manager;
                    break;
                }
            }
            
            if (itemManager != null)
            {
                itemManager.RefreshDatabase();
                Debug.Log("ItemManager database refreshed due to asset changes.");
            }
        }
    }
}
