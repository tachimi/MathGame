using System.Collections.Generic;
using MathGame.Enums;
using UnityEngine;

namespace MathGame.UI
{
    /// <summary>
    /// Статический класс с предустановками для кнопок операций
    /// </summary>
    public static class OperationButtonPresets
    {
        public static readonly Dictionary<string, (List<MathOperation> operations, string displayText)> Presets = 
            new Dictionary<string, (List<MathOperation>, string)>
            {
                ["Addition"] = (new List<MathOperation> { MathOperation.Addition }, "+"),
                ["Subtraction"] = (new List<MathOperation> { MathOperation.Subtraction }, "-"),
                ["Multiplication"] = (new List<MathOperation> { MathOperation.Multiplication }, "×"),
                ["Division"] = (new List<MathOperation> { MathOperation.Division }, "÷"),
                ["AddSub"] = (new List<MathOperation> { MathOperation.Addition, MathOperation.Subtraction }, "+-"),
                ["MulDiv"] = (new List<MathOperation> { MathOperation.Multiplication, MathOperation.Division }, "×÷"),
                ["AllOperations"] = (new List<MathOperation> 
                { 
                    MathOperation.Addition, 
                    MathOperation.Subtraction, 
                    MathOperation.Multiplication, 
                    MathOperation.Division 
                }, "+-×÷")
            };
        
        /// <summary>
        /// Настроить кнопку по имени пресета
        /// </summary>
        public static void ConfigureButton(OperationButton button, string presetName)
        {
            if (button == null) return;
            
            if (Presets.TryGetValue(presetName, out var preset))
            {
                button.Configure(preset.operations, preset.displayText);
            }
            else
            {
                Debug.LogWarning($"Preset '{presetName}' not found. Available presets: {string.Join(", ", Presets.Keys)}");
            }
        }
        
        /// <summary>
        /// Получить список всех доступных пресетов
        /// </summary>
        public static string[] GetAvailablePresets()
        {
            var presets = new string[Presets.Keys.Count];
            Presets.Keys.CopyTo(presets, 0);
            return presets;
        }
        
        /// <summary>
        /// Создать кастомную кнопку
        /// </summary>
        public static void CreateCustomButton(OperationButton button, MathOperation[] operations, string displayText)
        {
            if (button == null) return;
            
            var operationList = new List<MathOperation>(operations);
            button.Configure(operationList, displayText);
        }
    }
}