using System.Collections.Generic;
using System.Linq;
using MathGame.Configs;
using MathGame.Enums;
using MathGame.Models;
using MathGame.Settings;
using ScreenManager.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.UI
{
    public class ResultScreen : UIScreen<GameSessionResult>
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _phraseText;
        [SerializeField] private TextMeshProUGUI _finalScoreText;
        [SerializeField] private TextMeshProUGUI _accuracyText;
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private TextMeshProUGUI _operationsText;
        
        [Header("Configuration")]
        [SerializeField] private ResultPhrasesConfig _phrasesConfig;
        
        [Header("Buttons")]
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _menuButton;
        
        private GameSessionResult _sessionResult;
        
        public override void Initialize(GameSessionResult context)
        {
            _sessionResult = context;
            DisplayResults();
            SetupButtons();
        }
        
        private void DisplayResults()
        {
            // Отображаем фразу в зависимости от точности
            if (_phraseText != null && _phrasesConfig != null)
            {
                var (phrase, color) = _phrasesConfig.GetPhraseForAccuracy(_sessionResult.AccuracyPercentage);
                _phraseText.text = phrase;
                _phraseText.color = color;
            }
            else if (_phraseText != null)
            {
                // Если конфиг не настроен, используем дефолтную логику
                _phraseText.text = GetDefaultPhrase(_sessionResult.AccuracyPercentage);
                _phraseText.color = GetDefaultColor(_sessionResult.AccuracyPercentage);
            }
            
            if (_finalScoreText != null)
            {
                _finalScoreText.text = $"Правильных ответов: {_sessionResult.CorrectAnswers} из {_sessionResult.TotalQuestions}";
            }
            
            if (_accuracyText != null)
            {
                _accuracyText.text = $"{_sessionResult.AccuracyPercentage:F0}%";
            }
            
            if (_timeText != null)
            {
                var totalTime = _sessionResult.TotalTime;
                if (totalTime.TotalHours >= 1)
                {
                    // Формат: часы:минуты:секунды
                    _timeText.text = $"{(int)totalTime.TotalHours}:{totalTime.Minutes:D2}:{totalTime.Seconds:D2}";
                }
                else
                {
                    // Формат: минуты:секунды
                    _timeText.text = $"{(int)totalTime.TotalMinutes}:{totalTime.Seconds:D2}";
                }
            }
            
            if (_operationsText != null)
            {
                DisplayOperationsBreakdown();
            }
        }
        
        private void DisplayOperationsBreakdown()
        {
            // Получаем уникальные операции, которые использовались в сессии
            var usedOperations = _sessionResult.Results
                .Select(r => r.Question.Operation)
                .Distinct()
                .OrderBy(op => op);
            
            var symbols = new List<string>();
            
            foreach (var operation in usedOperations)
            {
                symbols.Add(GetOperationSymbol(operation));
            }
            
            // Объединяем символы через пробел
            _operationsText.text = string.Join("  ", symbols);
        }
        
        private string GetOperationSymbol(MathOperation operation)
        {
            return operation switch
            {
                MathOperation.Addition => "+",
                MathOperation.Subtraction => "−",
                MathOperation.Multiplication => "×",
                MathOperation.Division => "÷",
                _ => "?"
            };
        }
        
        private void SetupButtons()
        {
            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(OnRestartClicked);
            }
            
            if (_menuButton != null)
            {
                _menuButton.onClick.AddListener(OnMenuClicked);
            }
        }
        
        private void OnRestartClicked()
        {
            // Возвращаемся к игровому экрану с теми же настройками
            if (_sessionResult?.GameSettings != null)
            {
                ScreensManager.OpenScreen<GameScreen, GameSettings>(_sessionResult.GameSettings);
                CloseScreen();
            }
            else
            {
                // Если настройки не сохранены, возвращаемся в главное меню
                ScreensManager.OpenScreen<MainMenuScreen>();
                CloseScreen();
            }
        }
        
        private void OnMenuClicked()
        {
            ScreensManager.OpenScreen<MainMenuScreen>();
            CloseScreen();
        }
        
        private string GetDefaultPhrase(float accuracy)
        {
            return accuracy switch
            {
                >= 90 => "Отлично!",
                >= 70 => "Хорошо!",
                >= 50 => "Можно лучше!",
                >= 30 => "Нужна практика!",
                _ => "Попробуй снова!"
            };
        }
        
        private Color GetDefaultColor(float accuracy)
        {
            return accuracy switch
            {
                >= 90 => new Color(0.2f, 0.8f, 0.2f), // Зеленый
                >= 70 => new Color(0.4f, 0.7f, 0.4f), // Светло-зеленый
                >= 50 => new Color(1f, 0.8f, 0.2f),   // Желтый
                >= 30 => new Color(1f, 0.5f, 0.2f),   // Оранжевый
                _ => new Color(0.8f, 0.3f, 0.3f)      // Красный
            };
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (_restartButton != null)
                _restartButton.onClick.RemoveAllListeners();
                
            if (_menuButton != null)
                _menuButton.onClick.RemoveAllListeners();
        }
    }
}