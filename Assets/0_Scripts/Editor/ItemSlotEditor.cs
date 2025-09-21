using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemSlot))]
public class ItemSlotEditor : Editor
{
    private SerializedProperty slotTypeProperty;
    private SerializedProperty equipmentSlotTypeProperty;
    
    void OnEnable()
    {
        slotTypeProperty = serializedObject.FindProperty("slotType");
        equipmentSlotTypeProperty = serializedObject.FindProperty("equipmentSlotType");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // Draw the Slot Type dropdown
        EditorGUILayout.PropertyField(slotTypeProperty, new GUIContent("Slot Type"));
        
        // Only show Equipment Slot Type if Slot Type is Equipment
        if (slotTypeProperty.enumValueIndex == (int)SlotType.Equipment)
        {
            EditorGUILayout.PropertyField(equipmentSlotTypeProperty, new GUIContent("Equipment Slot Type"));
        }
        
        // Draw the rest of the inspector
        DrawPropertiesExcluding(serializedObject, "slotType", "equipmentSlotType");
        
        serializedObject.ApplyModifiedProperties();
    }
}
