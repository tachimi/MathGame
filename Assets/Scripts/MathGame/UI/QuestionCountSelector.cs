using System;
using UnityEngine;

namespace MathGame.UI
{
    /// <summary>
    /// Менеджер группы кнопок количества вопросов, обеспечивает выбор только одной кнопки
    /// </summary>
    public class QuestionCountSelector : MonoBehaviour
    {
        [Header("Question Count Buttons")]
        [SerializeField] private QuestionCountButton[] _questionCountButtons;
        
        public event Action<int> OnQuestionCountChanged;
        
        private QuestionCountButton _currentSelected;
        
        private void Awake()
        {
            SetupButtons();
        }
        
        private void SetupButtons()
        {
            if (_questionCountButtons == null) return;
            
            foreach (var button in _questionCountButtons)
            {
                if (button != null)
                {
                    button.OnQuestionCountSelected += OnQuestionCountButtonClicked;
                }
            }
        }
        
        private void OnQuestionCountButtonClicked(QuestionCountButton clickedButton)
        {
            SelectButton(clickedButton);
        }
        
        /// <summary>
        /// Выбрать кнопку количества вопросов
        /// </summary>
        private void SelectButton(QuestionCountButton button)
        {
            if (button == null) return;
            
            // Снять выбор с предыдущей кнопки
            if (_currentSelected != null)
            {
                _currentSelected.SetSelected(false);
            }
            
            // Установить выбор на новую кнопку
            _currentSelected = button;
            _currentSelected.SetSelected(true);
            
            // Уведомить о изменении
            OnQuestionCountChanged?.Invoke(_currentSelected.QuestionCount);
        }
        
        /// <summary>
        /// Выбрать кнопку по количеству вопросов
        /// </summary>
        public void SelectQuestionCount(int questionCount)
        {
            foreach (var button in _questionCountButtons)
            {
                if (button != null && button.QuestionCount == questionCount)
                {
                    SelectButton(button);
                    return;
                }
            }
        }
        
        /// <summary>
        /// Очистить выбор (снять выбор со всех кнопок)
        /// </summary>
        public void ClearSelection()
        {
            if (_currentSelected != null)
            {
                _currentSelected.SetSelected(false);
                _currentSelected = null;
            }
        }
        
        private void OnDestroy()
        {
            // Очистка обработчиков событий
            if (_questionCountButtons != null)
            {
                foreach (var button in _questionCountButtons)
                {
                    if (button != null)
                    {
                        button.OnQuestionCountSelected -= OnQuestionCountButtonClicked;
                    }
                }
            }
        }
    }
}