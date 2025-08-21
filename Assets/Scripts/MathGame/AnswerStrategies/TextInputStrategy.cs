using System;
using MathGame.Interfaces;
using MathGame.Models;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.AnswerStrategies
{
    public class TextInputStrategy : IAnswerStrategy
    {
        private Question _currentQuestion;
        private Action<int> _onAnswerCallback;
        private GameObject _uiContainer;
        private InputField _inputField;
        
        public void Initialize(Question question, Action<int> onAnswerCallback)
        {
            _currentQuestion = question;
            _onAnswerCallback = onAnswerCallback;
        }
        
        public GameObject CreateAnswerUI(Transform parent)
        {
            _uiContainer = new GameObject("TextInputContainer");
            _uiContainer.transform.SetParent(parent, false);
            
            var rectTransform = _uiContainer.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.3f, 0.4f);
            rectTransform.anchorMax = new Vector2(0.7f, 0.6f);
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            var verticalLayout = _uiContainer.AddComponent<VerticalLayoutGroup>();
            verticalLayout.spacing = 20;
            verticalLayout.childAlignment = TextAnchor.MiddleCenter;
            verticalLayout.childControlHeight = false;
            verticalLayout.childControlWidth = false;
            verticalLayout.childForceExpandHeight = false;
            verticalLayout.childForceExpandWidth = false;
            
            CreateInputField();
            CreateSubmitButton();
            
            return _uiContainer;
        }
        
        private void CreateInputField()
        {
            var inputGO = new GameObject("InputField");
            inputGO.transform.SetParent(_uiContainer.transform, false);
            
            var inputRect = inputGO.AddComponent<RectTransform>();
            inputRect.sizeDelta = new Vector2(300, 60);
            
            var image = inputGO.AddComponent<Image>();
            image.color = Color.white;
            
            _inputField = inputGO.AddComponent<InputField>();
            _inputField.targetGraphic = image;
            
            // Создаем текст для ввода
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(inputGO.transform, false);
            
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = new Vector2(-20, -10);
            textRect.anchoredPosition = Vector2.zero;
            
            var text = textGO.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 28;
            text.color = Color.black;
            text.alignment = TextAnchor.MiddleCenter;
            text.supportRichText = false;
            
            _inputField.textComponent = text;
            _inputField.contentType = InputField.ContentType.IntegerNumber;
            
            // Создаем placeholder
            var placeholderGO = new GameObject("Placeholder");
            placeholderGO.transform.SetParent(inputGO.transform, false);
            
            var placeholderRect = placeholderGO.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.sizeDelta = new Vector2(-20, -10);
            placeholderRect.anchoredPosition = Vector2.zero;
            
            var placeholderText = placeholderGO.AddComponent<Text>();
            placeholderText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            placeholderText.fontSize = 24;
            placeholderText.fontStyle = FontStyle.Italic;
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            placeholderText.text = "Введите ответ";
            placeholderText.alignment = TextAnchor.MiddleCenter;
            
            _inputField.placeholder = placeholderText;
            
            // Обработчик Enter
            _inputField.onEndEdit.AddListener(OnEndEdit);
        }
        
        private void CreateSubmitButton()
        {
            var buttonGO = new GameObject("SubmitButton");
            buttonGO.transform.SetParent(_uiContainer.transform, false);
            
            var buttonRect = buttonGO.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(200, 50);
            
            var image = buttonGO.AddComponent<Image>();
            image.color = new Color(0.3f, 0.7f, 0.3f);
            
            var button = buttonGO.AddComponent<Button>();
            button.targetGraphic = image;
            
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);
            
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            var text = textGO.AddComponent<Text>();
            text.text = "Ответить";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            button.onClick.AddListener(SubmitAnswer);
        }
        
        private void OnEndEdit(string value)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                SubmitAnswer();
            }
        }
        
        private void SubmitAnswer()
        {
            if (string.IsNullOrEmpty(_inputField.text))
                return;
            
            if (int.TryParse(_inputField.text, out int answer))
            {
                _onAnswerCallback?.Invoke(answer);
            }
        }
        
        public void Cleanup()
        {
            if (_uiContainer != null)
            {
                GameObject.Destroy(_uiContainer);
            }
        }
    }
}