using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UI_Item))]
public class UI_ItemEditor : Editor
{
    private SerializedProperty id;
    private SerializedProperty itemName;
    private SerializedProperty itemType;
    private SerializedProperty itemCategory;
    private SerializedProperty equipmentSubcategory;
    private SerializedProperty structureSubcategory;
    private SerializedProperty toolSubcategory;
    private SerializedProperty stackable;
    private SerializedProperty placeable;
    private SerializedProperty image;
    // Hidden ground item properties - no longer serialized
    private SerializedProperty structurePrefab;

    private void OnEnable()
    {
        id = serializedObject.FindProperty("id");
        itemName = serializedObject.FindProperty("itemName");
        itemType = serializedObject.FindProperty("itemType");
        itemCategory = serializedObject.FindProperty("itemCategory");
        equipmentSubcategory = serializedObject.FindProperty("equipmentSubcategory");
        structureSubcategory = serializedObject.FindProperty("structureSubcategory");
        toolSubcategory = serializedObject.FindProperty("toolSubcategory");
        stackable = serializedObject.FindProperty("stackable");
        placeable = serializedObject.FindProperty("placeable");
        image = serializedObject.FindProperty("image");
        // Ground item properties are hidden - no longer serialized
        structurePrefab = serializedObject.FindProperty("structurePrefab");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Basic Item Properties (no header since it's the main title)
        EditorGUILayout.PropertyField(id);
        EditorGUILayout.PropertyField(itemName);
        EditorGUILayout.PropertyField(itemType);
        EditorGUILayout.PropertyField(itemCategory);
        
        // Only show equipment subcategory if item category is Equipment
        if (itemCategory.enumValueIndex == 0) // 0 = Equipment
        {
            EditorGUILayout.PropertyField(equipmentSubcategory);
        }
        // Only show structure subcategory if item category is Structure
        else if (itemCategory.enumValueIndex == 1) // 1 = Structure
        {
            EditorGUILayout.PropertyField(structureSubcategory);
        }
        // Only show tool subcategory if item category is Tool
        else if (itemCategory.enumValueIndex == 3) // 3 = Tool
        {
            EditorGUILayout.PropertyField(toolSubcategory);
        }
        
        EditorGUILayout.PropertyField(stackable);
        EditorGUILayout.PropertyField(placeable);

        EditorGUILayout.Space();
        
        // UI Item Properties
        EditorGUILayout.LabelField("UI Item Properties", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(image);
        
        // Only show structure properties if item category is Structure
        if (itemCategory.enumValueIndex == 1) // 1 = Structure
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Structure Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(structurePrefab);
        }
        
        EditorGUILayout.Space();
        
        // Ground Item Properties
        EditorGUILayout.LabelField("Ground Item Properties", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Ground items automatically use the UI Image sprite and default settings.", MessageType.Info);

        serializedObject.ApplyModifiedProperties();
    }
}
