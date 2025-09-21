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
    private SerializedProperty groundSprite;
    private SerializedProperty groundScale;
    private SerializedProperty hasCollider;
    private SerializedProperty isTrigger;
    private SerializedProperty pickupRadius;

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
        groundSprite = serializedObject.FindProperty("groundSprite");
        groundScale = serializedObject.FindProperty("groundScale");
        hasCollider = serializedObject.FindProperty("hasCollider");
        isTrigger = serializedObject.FindProperty("isTrigger");
        pickupRadius = serializedObject.FindProperty("pickupRadius");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Item Properties Header
        EditorGUILayout.LabelField("Item Properties", EditorStyles.boldLabel);
        
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
        
        // UI Item Properties Header
        EditorGUILayout.LabelField("UI Item Properties", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(image);
        
        EditorGUILayout.Space();
        
        // Ground Item Properties Header
        EditorGUILayout.LabelField("Ground Item Properties", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(groundSprite);
        EditorGUILayout.PropertyField(groundScale);
        EditorGUILayout.PropertyField(hasCollider);
        EditorGUILayout.PropertyField(isTrigger);
        EditorGUILayout.PropertyField(pickupRadius);

        serializedObject.ApplyModifiedProperties();
    }
}
