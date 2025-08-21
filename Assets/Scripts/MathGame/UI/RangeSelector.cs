using System;
using System.Collections.Generic;
using System.Linq;
using MathGame.Models;
using UnityEngine;

namespace MathGame.UI
{
    /// <summary>
    /// Селектор для выбора диапазонов чисел с поддержкой множественного выбора
    /// Поддерживает логику выбора диапазона между двумя кнопками
    /// </summary>
    public class RangeSelector : MonoBehaviour
    {
        public event Action<List<NumberRange>> OnSelectionChanged;
        
        [Header("Selection Settings")]
        [SerializeField] private bool _allowMultipleSelection = true;
        [SerializeField] private bool _selectRangeBetween = true;
        
        private List<NumberRangeButton> _buttons = new();
        private List<NumberRangeButton> _selectedButtons = new();
        private NumberRangeButton _firstSelectedButton;
        
        /// <summary>
        /// Получить все выбранные диапазоны
        /// </summary>
        public List<NumberRange> SelectedRanges
        {
            get
            {
                return _selectedButtons
                    .Where(button => button != null && button.Range != null)
                    .Select(button => button.Range)
                    .ToList();
            }
        }
        
        /// <summary>
        /// Инициализировать селектор с кнопками
        /// </summary>
        public void Initialize(List<NumberRangeButton> buttons)
        {
            ClearSelection();
            UnsubscribeFromButtons();
            
            _buttons = new List<NumberRangeButton>(buttons);
            SubscribeToButtons();
        }

        /// <summary>
        /// Добавить кнопку в селектор
        /// </summary>
        public void AddButton(NumberRangeButton button)
        {
            if (button == null || _buttons.Contains(button)) return;
            
            _buttons.Add(button);
            button.OnRangeSelected += OnButtonSelected;
        }

        /// <summary>
        /// Удалить кнопку из селектора
        /// </summary>
        public void RemoveButton(NumberRangeButton button)
        {
            if (button == null) return;
            
            _buttons.Remove(button);
            _selectedButtons.Remove(button);
            button.OnRangeSelected -= OnButtonSelected;
            
            if (_firstSelectedButton == button)
                _firstSelectedButton = null;
        }

        /// <summary>
        /// Очистить весь выбор
        /// </summary>
        public void ClearSelection()
        {
            foreach (var button in _selectedButtons)
            {
                if (button != null)
                    button.SetSelected(false);
            }
            
            _selectedButtons.Clear();
            _firstSelectedButton = null;
            
            OnSelectionChanged?.Invoke(SelectedRanges);
        }

        /// <summary>
        /// Выбрать диапазон по умолчанию (первый подходящий)
        /// </summary>
        public void SelectDefault()
        {
            if (_buttons.Count == 0) return;
            
            // Ищем подходящий для новичков диапазон
            var defaultButton = _buttons.FirstOrDefault(btn => btn.IsBeginnerFriendly());
            if (defaultButton == null)
                defaultButton = _buttons[0];
                
            SelectSingleButton(defaultButton);
        }

        private void SubscribeToButtons()
        {
            foreach (var button in _buttons)
            {
                if (button != null)
                    button.OnRangeSelected += OnButtonSelected;
            }
        }

        private void UnsubscribeFromButtons()
        {
            foreach (var button in _buttons)
            {
                if (button != null)
                    button.OnRangeSelected -= OnButtonSelected;
            }
        }

        private void OnButtonSelected(NumberRangeButton selectedButton)
        {
            if (!_allowMultipleSelection)
            {
                SelectSingleButton(selectedButton);
                return;
            }

            HandleMultipleSelection(selectedButton);
        }

        private void SelectSingleButton(NumberRangeButton button)
        {
            // Сбрасываем предыдущий выбор
            foreach (var btn in _selectedButtons)
            {
                if (btn != null)
                    btn.SetSelected(false);
            }
            
            _selectedButtons.Clear();
            
            // Выбираем новую кнопку
            if (button != null)
            {
                button.SetSelected(true);
                _selectedButtons.Add(button);
                _firstSelectedButton = button;
            }
            
            OnSelectionChanged?.Invoke(SelectedRanges);
        }

        private void HandleMultipleSelection(NumberRangeButton selectedButton)
        {
            if (_firstSelectedButton == null)
            {
                // Первый выбор
                SelectSingleButton(selectedButton);
                return;
            }

            // Проверяем, выбрана ли уже эта кнопка
            if (_selectedButtons.Contains(selectedButton))
            {
                // Убираем выбор с этой кнопки и всех последующих
                RemoveSelectionFromAndAfter(selectedButton);
                return;
            }

            if (_selectRangeBetween)
            {
                SelectRangeBetween(_firstSelectedButton, selectedButton);
            }
            else
            {
                ToggleButtonSelection(selectedButton);
            }
        }

        private void SelectRangeBetween(NumberRangeButton startButton, NumberRangeButton endButton)
        {
            if (startButton == null || endButton == null) return;

            // Находим индексы кнопок
            int startIndex = _buttons.IndexOf(startButton);
            int endIndex = _buttons.IndexOf(endButton);
            
            if (startIndex == -1 || endIndex == -1) return;

            // Находим текущие границы выбранного диапазона
            var selectedIndices = _selectedButtons
                .Select(btn => _buttons.IndexOf(btn))
                .Where(idx => idx != -1)
                .OrderBy(idx => idx)
                .ToList();

            int currentMinIndex = selectedIndices.Count > 0 ? selectedIndices.First() : startIndex;
            int currentMaxIndex = selectedIndices.Count > 0 ? selectedIndices.Last() : startIndex;
            
            // Определяем новые границы диапазона
            int newMinIndex = Math.Min(currentMinIndex, endIndex);
            int newMaxIndex = Math.Max(currentMaxIndex, endIndex);

            // Сбрасываем предыдущий выбор
            foreach (var btn in _selectedButtons)
            {
                if (btn != null)
                    btn.SetSelected(false);
            }
            _selectedButtons.Clear();

            // Выбираем все кнопки в расширенном диапазоне
            for (int i = newMinIndex; i <= newMaxIndex; i++)
            {
                var button = _buttons[i];
                if (button != null)
                {
                    button.SetSelected(true);
                    _selectedButtons.Add(button);
                }
            }

            OnSelectionChanged?.Invoke(SelectedRanges);
        }

        private void ToggleButtonSelection(NumberRangeButton button)
        {
            if (_selectedButtons.Contains(button))
            {
                // Убираем из выбора
                button.SetSelected(false);
                _selectedButtons.Remove(button);
                
                // Если это была первая выбранная кнопка, выбираем новую первую
                if (_firstSelectedButton == button)
                {
                    _firstSelectedButton = _selectedButtons.FirstOrDefault();
                }
            }
            else
            {
                // Добавляем в выбор
                button.SetSelected(true);
                _selectedButtons.Add(button);
            }

            OnSelectionChanged?.Invoke(SelectedRanges);
        }

        /// <summary>
        /// Настроить режим выбора
        /// </summary>
        public void SetSelectionMode(bool allowMultiple, bool selectRangeBetween = true)
        {
            _allowMultipleSelection = allowMultiple;
            _selectRangeBetween = selectRangeBetween;
            
            // Если отключили множественный выбор, оставляем только первую кнопку
            if (!allowMultiple && _selectedButtons.Count > 1)
            {
                var firstButton = _selectedButtons[0];
                SelectSingleButton(firstButton);
            }
        }

        /// <summary>
        /// Убирает выбор с указанной кнопки и всех кнопок после неё (включительно)
        /// </summary>
        private void RemoveSelectionFromAndAfter(NumberRangeButton fromButton)
        {
            if (fromButton == null) return;

            int fromIndex = _buttons.IndexOf(fromButton);
            if (fromIndex == -1) return;

            // Убираем выбор со всех кнопок начиная с fromIndex
            var buttonsToRemove = new List<NumberRangeButton>();
            
            for (int i = fromIndex; i < _buttons.Count; i++)
            {
                var button = _buttons[i];
                if (button != null && _selectedButtons.Contains(button))
                {
                    button.SetSelected(false);
                    buttonsToRemove.Add(button);
                }
            }

            // Удаляем из списка выбранных
            foreach (var button in buttonsToRemove)
            {
                _selectedButtons.Remove(button);
            }

            // Обновляем первую выбранную кнопку
            if (_selectedButtons.Count > 0)
            {
                // Находим кнопку с наименьшим индексом среди оставшихся выбранных
                _firstSelectedButton = _selectedButtons
                    .OrderBy(btn => _buttons.IndexOf(btn))
                    .FirstOrDefault();
            }
            else
            {
                _firstSelectedButton = null;
            }

            OnSelectionChanged?.Invoke(SelectedRanges);
        }

        private void OnDestroy()
        {
            UnsubscribeFromButtons();
        }
    }
}