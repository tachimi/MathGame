using System;
using System.Linq;
using MathGame.Enums;
using UnityEngine;

namespace MathGame.UI
{
    /// <summary>
    /// Менеджер выбора уровня сложности
    /// Управляет группой DifficultyButton и обеспечивает выбор только одной кнопки
    /// </summary>
    public class DifficultySelector : MonoBehaviour
    {
        public event Action<DifficultyLevel> OnDifficultyChanged;
        
        [Header("Difficulty Buttons")]
        [SerializeField] private DifficultyButton[] _difficultyButtons;
        
        private DifficultyButton _selectedButton;
        
        private void Awake()
        {
            SetupButtons();
        }
        
        private void SetupButtons()
        {
            if (_difficultyButtons == null) return;
            
            foreach (var button in _difficultyButtons)
            {
                if (button != null)
                {
                    button.OnDifficultySelected += SelectButton;
                }
            }
        }
        
        /// <summary>
        /// Выбрать кнопку уровня сложности
        /// </summary>
        public void SelectButton(DifficultyButton button)
        {
            if (button == _selectedButton) return;
            
            // Снимаем выделение с предыдущей кнопки
            if (_selectedButton != null)
            {
                _selectedButton.SetSelected(false);
            }
            
            // Выделяем новую кнопку
            _selectedButton = button;
            _selectedButton.SetSelected(true);
            
            // Уведомляем о изменении
            OnDifficultyChanged?.Invoke(_selectedButton.DifficultyLevel);
        }
        
        /// <summary>
        /// Выбрать уровень сложности по enum'у
        /// </summary>
        public void SelectDifficulty(DifficultyLevel difficulty)
        {
            var button = GetButtonByDifficulty(difficulty);
            if (button != null)
            {
                SelectButton(button);
            }
        }
        
        /// <summary>
        /// Найти кнопку по уровню сложности
        /// </summary>
        private DifficultyButton GetButtonByDifficulty(DifficultyLevel difficulty)
        {
            var button =_difficultyButtons?.FirstOrDefault(button => 
                button != null && button.DifficultyLevel == difficulty);
            return button;
        }
        
        private void OnDestroy()
        {
            if (_difficultyButtons != null)
            {
                foreach (var button in _difficultyButtons)
                {
                    if (button != null)
                    {
                        button.OnDifficultySelected -= SelectButton;
                    }
                }
            }
        }
    }
}