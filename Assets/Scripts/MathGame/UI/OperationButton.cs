using System;
using System.Collections.Generic;
using MathGame.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.UI
{
    /// <summary>
    /// Кнопка-держатель данных для арифметических операций
    /// </summary>
    public class OperationButton : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _buttonText;
        
        [Header("Operation Data")]
        [SerializeField] private List<MathOperation> _operations = new List<MathOperation>();
        [SerializeField] private string _displayText;
        
        public event Action<List<MathOperation>> OnOperationSelected;
        
        private void Awake()
        {
            SetupButton();
        }
        
        private void SetupButton()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(OnButtonClicked);
            }
            
            UpdateDisplayText();
        }
        
        private void OnButtonClicked()
        {
            OnOperationSelected?.Invoke(new List<MathOperation>(_operations));
        }
        
        /// <summary>
        /// Настройка кнопки с операциями и текстом
        /// </summary>
        public void Configure(List<MathOperation> operations, string displayText)
        {
            _operations = operations ?? new List<MathOperation>();
            _displayText = displayText;
            UpdateDisplayText();
        }
        
        /// <summary>
        /// Настройка кнопки с одной операцией
        /// </summary>
        public void Configure(MathOperation operation, string displayText)
        {
            Configure(new List<MathOperation> { operation }, displayText);
        }
        
        private void UpdateDisplayText()
        {
            if (_buttonText != null && !string.IsNullOrEmpty(_displayText))
            {
                _buttonText.text = _displayText;
            }
        }
        
        /// <summary>
        /// Получить список операций для этой кнопки
        /// </summary>
        public List<MathOperation> GetOperations()
        {
            return new List<MathOperation>(_operations);
        }
        
        /// <summary>
        /// Получить отображаемый текст
        /// </summary>
        public string GetDisplayText()
        {
            return _displayText;
        }
        
        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveAllListeners();
        }
        
        // Методы для настройки в Inspector'е
        [ContextMenu("Configure Addition")]
        private void ConfigureAddition()
        {
            Configure(MathOperation.Addition, "+");
        }
        
        [ContextMenu("Configure Subtraction")]
        private void ConfigureSubtraction()
        {
            Configure(MathOperation.Subtraction, "-");
        }
        
        [ContextMenu("Configure Multiplication")]
        private void ConfigureMultiplication()
        {
            Configure(MathOperation.Multiplication, "×");
        }
        
        [ContextMenu("Configure Division")]
        private void ConfigureDivision()
        {
            Configure(MathOperation.Division, "÷");
        }
        
        [ContextMenu("Configure Addition + Subtraction")]
        private void ConfigureAddSub()
        {
            Configure(new List<MathOperation> 
            { 
                MathOperation.Addition, 
                MathOperation.Subtraction 
            }, "+-");
        }
        
        [ContextMenu("Configure Multiplication + Division")]
        private void ConfigureMulDiv()
        {
            Configure(new List<MathOperation> 
            { 
                MathOperation.Multiplication, 
                MathOperation.Division 
            }, "×÷");
        }
        
        [ContextMenu("Configure All Operations")]
        private void ConfigureAll()
        {
            Configure(new List<MathOperation> 
            { 
                MathOperation.Addition, 
                MathOperation.Subtraction,
                MathOperation.Multiplication, 
                MathOperation.Division 
            }, "+-×÷");
        }
    }
}