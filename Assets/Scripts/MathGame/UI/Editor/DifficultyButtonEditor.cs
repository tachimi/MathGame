#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using MathGame.Enums;

namespace MathGame.UI.Editor
{
    [CustomEditor(typeof(DifficultyButton))]
    public class DifficultyButtonEditor : UnityEditor.Editor
    {
        private DifficultyLevel selectedDifficulty = DifficultyLevel.Easy;
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Setup", EditorStyles.boldLabel);
            
            // Dropdown для выбора уровня сложности
            selectedDifficulty = (DifficultyLevel)EditorGUILayout.EnumPopup("Difficulty Level", selectedDifficulty);
            
            // Кнопка применения пресета
            if (GUILayout.Button("Apply Preset"))
            {
                var button = (DifficultyButton)target;
                DifficultyButtonPresets.ConfigureButton(button, selectedDifficulty);
                EditorUtility.SetDirty(button);
            }
            
            EditorGUILayout.Space();
            
            // Информация о текущих настройках
            var difficultyButton = (DifficultyButton)target;
            var difficulty = difficultyButton.DifficultyLevel;
            var displayText = difficultyButton.DisplayText;
            var description = difficultyButton.Description;
            
            EditorGUILayout.LabelField("Current Configuration:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Difficulty:", difficulty.ToString());
            EditorGUILayout.LabelField("Display Text:", displayText);
            EditorGUILayout.LabelField("Description:", description);
            
            // Информация о диапазоне чисел
            var range = DifficultyButtonPresets.GetNumberRange(difficulty);
            EditorGUILayout.LabelField("Number Range:", $"{range.min}-{range.max}");
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Selection State:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Is Selected:", difficultyButton.IsSelected.ToString());
        }
    }
}
#endif