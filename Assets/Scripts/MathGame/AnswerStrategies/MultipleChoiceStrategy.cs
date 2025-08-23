using System;
using System.Collections.Generic;
using System.Linq;
using MathGame.Interfaces;
using MathGame.Models;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MathGame.AnswerStrategies
{
    public class MultipleChoiceStrategy : IAnswerStrategy
    {
        private Question _currentQuestion;
        private Action<int> _onAnswerCallback;
        private List<int> _answerOptions;
        private GameObject _uiContainer;
        
        public void Initialize(Question question, Action<int> onAnswerCallback)
        {
            _currentQuestion = question;
            _onAnswerCallback = onAnswerCallback;
            GenerateAnswerOptions();
        }
        
        public GameObject CreateAnswerUI(Transform parent)
        {
            _uiContainer = new GameObject("MultipleChoiceContainer");
            _uiContainer.transform.SetParent(parent, false);
            
            var rectTransform = _uiContainer.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            var gridLayout = _uiContainer.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(150, 100);
            gridLayout.spacing = new Vector2(20, 20);
            gridLayout.padding = new RectOffset(20, 20, 20, 20);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 2;
            gridLayout.childAlignment = TextAnchor.MiddleCenter;
            
            CreateAnswerButtons();
            
            return _uiContainer;
        }
        
        private void GenerateAnswerOptions()
        {
            _answerOptions = new List<int> { _currentQuestion.CorrectAnswer };
            
            // Генерируем неправильные ответы
            int wrongAnswersCount = 5;
            var range = 10; // Диапазон для генерации неправильных ответов
            
            while (_answerOptions.Count < wrongAnswersCount + 1)
            {
                int wrongAnswer = _currentQuestion.CorrectAnswer + Random.Range(-range, range + 1);
                
                if (!_answerOptions.Contains(wrongAnswer) && wrongAnswer >= 0)
                {
                    _answerOptions.Add(wrongAnswer);
                }
            }
            
            // Перемешиваем ответы
            _answerOptions = _answerOptions.OrderBy(x => Random.value).ToList();
        }
        
        private void CreateAnswerButtons()
        {
            foreach (var answer in _answerOptions)
            {
                CreateAnswerButton(answer);
            }
        }
        
        private void CreateAnswerButton(int answer)
        {
            var buttonGO = new GameObject($"AnswerButton_{answer}");
            buttonGO.transform.SetParent(_uiContainer.transform, false);
            
            var image = buttonGO.AddComponent<Image>();
            image.color = new Color(0.9f, 0.9f, 0.9f);
            
            var button = buttonGO.AddComponent<Button>();
            button.targetGraphic = image;
            
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);
            
            var rectTransform = textGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            var text = textGO.AddComponent<Text>();
            text.text = answer.ToString();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 32;
            text.color = Color.black;
            text.alignment = TextAnchor.MiddleCenter;
            
            // Добавляем обработчик клика
            int capturedAnswer = answer;
            button.onClick.AddListener(() => OnAnswerSelected(capturedAnswer));
        }
        
        private void OnAnswerSelected(int answer)
        {
            _onAnswerCallback?.Invoke(answer);
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