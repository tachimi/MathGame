#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MathGame.UI.Editor
{
    [CustomEditor(typeof(QuestionCountButton))]
    public class QuestionCountButtonEditor : UnityEditor.Editor
    {
        private int selectedQuestionCount = 10;
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Setup", EditorStyles.boldLabel);
            
            // Поле для ввода количества вопросов
            selectedQuestionCount = EditorGUILayout.IntField("Question Count", selectedQuestionCount);
            selectedQuestionCount = Mathf.Max(1, selectedQuestionCount); // Минимум 1
            
            // Dropdown с пресетами
            EditorGUILayout.LabelField("Available Presets:");
            var availableCounts = QuestionCountButtonPresets.GetAvailableQuestionCounts();
            
            EditorGUILayout.BeginHorizontal();
            foreach (var count in availableCounts)
            {
                if (GUILayout.Button($"{count}"))
                {
                    selectedQuestionCount = count;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            // Кнопка применения пресета
            if (GUILayout.Button("Apply Configuration"))
            {
                var button = (QuestionCountButton)target;
                QuestionCountButtonPresets.ConfigureButton(button, selectedQuestionCount);
                EditorUtility.SetDirty(button);
            }
            
            EditorGUILayout.Space();
            
            // Информация о текущих настройках
            var questionCountButton = (QuestionCountButton)target;
            var questionCount = questionCountButton.QuestionCount;
            var displayText = questionCountButton.DisplayText;
            var description = questionCountButton.Description;
            
            EditorGUILayout.LabelField("Current Configuration:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Question Count:", questionCount.ToString());
            EditorGUILayout.LabelField("Display Text:", displayText);
            EditorGUILayout.LabelField("Description:", description);
            
            // Дополнительная информация
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Additional Info:", EditorStyles.boldLabel);
            
            var estimatedTime = questionCountButton.GetEstimatedTimeMinutes();
            EditorGUILayout.LabelField("Estimated Time:", $"{estimatedTime} min");
            
            var isRecommended = questionCountButton.IsRecommendedForBeginners();
            EditorGUILayout.LabelField("Recommended for Beginners:", isRecommended.ToString());
            
            var difficulty = QuestionCountButtonPresets.GetDifficultyCategory(questionCount);
            EditorGUILayout.LabelField("Difficulty Category:", difficulty);
            
            // Показываем цвет сложности
            var difficultyColor = QuestionCountButtonPresets.GetDifficultyColor(questionCount);
            EditorGUILayout.LabelField("Difficulty Color:");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ColorField(difficultyColor);
            EditorGUILayout.LabelField(difficulty);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Selection State:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Is Selected:", questionCountButton.IsSelected.ToString());
            
            // Кнопки быстрой настройки
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Presets:", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Quick (5)"))
            {
                var button = (QuestionCountButton)target;
                QuestionCountButtonPresets.ConfigureButton(button, 5);
                EditorUtility.SetDirty(button);
            }
            
            if (GUILayout.Button("Standard (10)"))
            {
                var button = (QuestionCountButton)target;
                QuestionCountButtonPresets.ConfigureButton(button, 10);
                EditorUtility.SetDirty(button);
            }
            
            if (GUILayout.Button("Marathon (30)"))
            {
                var button = (QuestionCountButton)target;
                QuestionCountButtonPresets.ConfigureButton(button, 30);
                EditorUtility.SetDirty(button);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif