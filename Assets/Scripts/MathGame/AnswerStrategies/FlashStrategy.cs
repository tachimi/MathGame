using System;
using MathGame.Interfaces;
using MathGame.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MathGame.AnswerStrategies
{
    /// <summary>
    /// Стратегия Flash - показывает правильный ответ, игрок выбирает "Запомнил" или "Не запомнил"
    /// </summary>
    public class FlashStrategy : IAnswerStrategy
    {
        private Question _currentQuestion;
        private Action<int> _onAnswerSelected;
        private GameObject _flashUI;
        
        [Header("Flash Settings")]
        [SerializeField] private float _displayTime = 3.0f; // Время показа ответа
        
        public void Initialize(Question question, Action<int> onAnswerSelected)
        {
            _currentQuestion = question;
            _onAnswerSelected = onAnswerSelected;
        }

        public GameObject CreateAnswerUI(Transform container)
        {
            if (container == null || _currentQuestion == null) return null;

            // Создаем основной контейнер
            var flashContainer = new GameObject("FlashContainer");
            flashContainer.transform.SetParent(container);
            
            var containerRect = flashContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;
            
            // Добавляем вертикальную группу
            var verticalGroup = flashContainer.AddComponent<VerticalLayoutGroup>();
            verticalGroup.spacing = 20f;
            verticalGroup.childAlignment = TextAnchor.MiddleCenter;
            verticalGroup.childControlHeight = false;
            verticalGroup.childControlWidth = true;
            verticalGroup.childForceExpandHeight = false;
            verticalGroup.childForceExpandWidth = true;
            
            // Добавляем Content Size Fitter
            var contentSizeFitter = flashContainer.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Создаем текст с правильным ответом
            CreateAnswerDisplay(flashContainer.transform);
            
            // Создаем кнопки выбора
            CreateSelectionButtons(flashContainer.transform);
            
            _flashUI = flashContainer;
            return flashContainer;
        }

        private void CreateAnswerDisplay(Transform parent)
        {
            // Контейнер для отображения ответа
            var answerContainer = new GameObject("AnswerDisplay");
            answerContainer.transform.SetParent(parent);
            
            var containerRect = answerContainer.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(0, 100);
            
            // Фон для ответа
            var backgroundImage = answerContainer.AddComponent<Image>();
            backgroundImage.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            
            // Текст с правильным ответом
            var answerTextGO = new GameObject("AnswerText");
            answerTextGO.transform.SetParent(answerContainer.transform);
            
            var answerTextRect = answerTextGO.AddComponent<RectTransform>();
            answerTextRect.anchorMin = Vector2.zero;
            answerTextRect.anchorMax = Vector2.one;
            answerTextRect.offsetMin = Vector2.zero;
            answerTextRect.offsetMax = Vector2.zero;
            
            var answerText = answerTextGO.AddComponent<TextMeshProUGUI>();
            answerText.text = $"Правильный ответ: {_currentQuestion.CorrectAnswer}";
            answerText.fontSize = 36;
            answerText.fontStyle = FontStyles.Bold;
            answerText.alignment = TextAlignmentOptions.Center;
            answerText.color = Color.black;
            
            // Добавляем Layout Element
            var layoutElement = answerContainer.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 100;
        }

        private void CreateSelectionButtons(Transform parent)
        {
            // Контейнер для кнопок
            var buttonsContainer = new GameObject("SelectionButtons");
            buttonsContainer.transform.SetParent(parent);
            
            var buttonsRect = buttonsContainer.AddComponent<RectTransform>();
            buttonsRect.sizeDelta = new Vector2(0, 80);
            
            // Горизонтальная группа для кнопок
            var horizontalGroup = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
            horizontalGroup.spacing = 30f;
            horizontalGroup.childAlignment = TextAnchor.MiddleCenter;
            horizontalGroup.childControlHeight = true;
            horizontalGroup.childControlWidth = true;
            horizontalGroup.childForceExpandHeight = true;
            horizontalGroup.childForceExpandWidth = true;
            
            // Layout Element для кнопок
            var buttonsLayoutElement = buttonsContainer.AddComponent<LayoutElement>();
            buttonsLayoutElement.preferredHeight = 80;
            
            // Кнопка "Запомнил" (считается правильным ответом)
            CreateButton(buttonsContainer.transform, "Запомнил ✓", Color.green, () => OnAnswerSelected(true));
            
            // Кнопка "Не запомнил" (считается неправильным ответом)
            CreateButton(buttonsContainer.transform, "Не запомнил ✗", Color.red, () => OnAnswerSelected(false));
        }

        private void CreateButton(Transform parent, string text, Color color, Action onClick)
        {
            var buttonGO = new GameObject($"Button_{text}");
            buttonGO.transform.SetParent(parent);
            
            var buttonRect = buttonGO.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(200, 60);
            
            // Фон кнопки
            var buttonImage = buttonGO.AddComponent<Image>();
            buttonImage.color = color;
            
            // Компонент кнопки
            var button = buttonGO.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(() => onClick?.Invoke());
            
            // Текст кнопки
            var buttonTextGO = new GameObject("Text");
            buttonTextGO.transform.SetParent(buttonGO.transform);
            
            var textRect = buttonTextGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var buttonText = buttonTextGO.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 18;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
        }

        private void OnAnswerSelected(bool remembered)
        {
            // Если запомнил - правильный ответ, если не запомнил - неправильный
            int playerAnswer = remembered ? _currentQuestion.CorrectAnswer : -1;
            _onAnswerSelected?.Invoke(playerAnswer);
        }

        public void Cleanup()
        {
            if (_flashUI != null)
            {
                UnityEngine.Object.Destroy(_flashUI);
                _flashUI = null;
            }
            
            _currentQuestion = null;
            _onAnswerSelected = null;
        }
    }
}