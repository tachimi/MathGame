#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MathGame.UI.Editor
{
    [CustomEditor(typeof(OperationButton))]
    public class OperationButtonEditor : UnityEditor.Editor
    {
        private int selectedPreset = 0;
        private string[] presetNames;
        
        private void OnEnable()
        {
            presetNames = OperationButtonPresets.GetAvailablePresets();
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Setup", EditorStyles.boldLabel);
            
            // Dropdown для выбора пресета
            selectedPreset = EditorGUILayout.Popup("Preset", selectedPreset, presetNames);
            
            // Кнопка применения пресета
            if (GUILayout.Button("Apply Preset"))
            {
                var button = (OperationButton)target;
                OperationButtonPresets.ConfigureButton(button, presetNames[selectedPreset]);
                EditorUtility.SetDirty(button);
            }
            
            EditorGUILayout.Space();
            
            // Информация о текущих настройках
            var operationButton = (OperationButton)target;
            var operations = operationButton.GetOperations();
            var displayText = operationButton.GetDisplayText();
            
            if (operations != null && operations.Count > 0)
            {
                EditorGUILayout.LabelField("Current Configuration:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Operations:", string.Join(", ", operations));
                EditorGUILayout.LabelField("Display Text:", displayText);
            }
        }
    }
}
#endif