using System;
using MathGame.Enums;
using UnityEngine;

namespace MathGame.UI
{
    /// <summary>
    /// Менеджер группы кнопок режимов ответа, обеспечивает выбор только одной кнопки
    /// </summary>
    public class AnswerModeSelector : MonoBehaviour
    {
        [Header("Answer Mode Buttons")]
        [SerializeField] private AnswerModeButton[] _answerModeButtons;
        
        public event Action<AnswerMode> OnAnswerModeChanged;
        
        private AnswerModeButton _currentSelected;
        
        private void Awake()
        {
            SetupButtons();
        }
        
        private void SetupButtons()
        {
            if (_answerModeButtons == null) return;
            
            foreach (var button in _answerModeButtons)
            {
                if (button != null)
                {
                    button.OnAnswerModeSelected += OnAnswerModeButtonClicked;
                }
            }
        }
        
        private void OnAnswerModeButtonClicked(AnswerModeButton clickedButton)
        {
            SelectButton(clickedButton);
        }
        
        /// <summary>
        /// Выбрать кнопку режима ответа
        /// </summary>
        public void SelectButton(AnswerModeButton button)
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
            OnAnswerModeChanged?.Invoke(_currentSelected.AnswerMode);
        }
        
        /// <summary>
        /// Выбрать кнопку по режиму ответа
        /// </summary>
        public void SelectAnswerMode(AnswerMode answerMode)
        {
            if (_answerModeButtons is not { Length: > 0 }) return;
            
            foreach (var button in _answerModeButtons)
            {
                if (button != null && button.AnswerMode == answerMode)
                {
                    SelectButton(button);
                    return;
                }
            }

            if (_answerModeButtons[0] != null)
            {
                SelectButton(_answerModeButtons[0]);
            }
        }
        
        /// <summary>
        /// Получить текущий выбранный режим ответа
        /// </summary>
        public AnswerMode GetSelectedAnswerMode()
        {
            return _currentSelected?.AnswerMode ?? AnswerMode.MultipleChoice; // По умолчанию множественный выбор
        }
        
        /// <summary>
        /// Получить текущую выбранную кнопку
        /// </summary>
        public AnswerModeButton GetSelectedButton()
        {
            return _currentSelected;
        }
        
        /// <summary>
        /// Проверить, есть ли выбранная кнопка
        /// </summary>
        public bool HasSelection()
        {
            return _currentSelected != null;
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
        
        /// <summary>
        /// Получить информацию о всех доступных режимах ответа
        /// </summary>
        public AnswerMode[] GetAvailableAnswerModes()
        {
            if (_answerModeButtons == null) return new AnswerMode[0];
            
            var modes = new AnswerMode[_answerModeButtons.Length];
            for (int i = 0; i < _answerModeButtons.Length; i++)
            {
                modes[i] = _answerModeButtons[i]?.AnswerMode ?? AnswerMode.MultipleChoice;
            }
            
            return modes;
        }
        
        private void OnDestroy()
        {
            // Очистка обработчиков событий
            if (_answerModeButtons != null)
            {
                foreach (var button in _answerModeButtons)
                {
                    if (button != null)
                    {
                        button.OnAnswerModeSelected -= OnAnswerModeButtonClicked;
                    }
                }
            }
        }
    }
}