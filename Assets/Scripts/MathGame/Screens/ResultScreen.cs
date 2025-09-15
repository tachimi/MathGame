using System.Collections.Generic;
using System.Linq;
using MathGame.Configs;
using MathGame.Enums;
using MathGame.Models;
using MathGame.Screens;
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
        [SerializeField] private TextMeshProUGUI _accuracyText;
        [SerializeField] private TextMeshProUGUI _timeText;

        [Header("Score Display (for Balloon Mode)")]
        [SerializeField] private TextMeshProUGUI _currentScoreText;
        [SerializeField] private TextMeshProUGUI _highScoreText;
        [SerializeField] private TextMeshProUGUI _newRecordText;

        [Header("Configuration")]
        [SerializeField] private ResultPhrasesConfig _phrasesConfig;

        [Header("Buttons")]
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _homeButton;

        private GameSessionResult _sessionResult;

        public override void Initialize(GameSessionResult context)
        {
            _sessionResult = context;
            DisplayResults();
            SetupButtons();
        }

        private void DisplayResults()
        {
            if (_sessionResult.GameSettings.GameType == GameType.Balloons)
            {
                DisplayBalloonModeResults();
            }
            else
            {
                DisplayCardsModeResults();
            }
        }

        private void DisplayCardsModeResults()
        {
            var (phrase, color) = _phrasesConfig.GetPhraseForAccuracy(_sessionResult.AccuracyPercentage);
            _phraseText.text = phrase;
            _phraseText.color = color;

            _accuracyText.gameObject.SetActive(true);
            _accuracyText.text = $"{_sessionResult.AccuracyPercentage:F0}%";

            var totalTime = _sessionResult.TotalTime;
            _timeText.text = totalTime.TotalHours >= 1
                ? $"{(int)totalTime.TotalHours}:{totalTime.Minutes:D2}:{totalTime.Seconds:D2}"
                : $"{(int)totalTime.TotalMinutes}:{totalTime.Seconds:D2}";

            // Скрываем элементы режима шариков
            if (_currentScoreText != null) _currentScoreText.gameObject.SetActive(false);
            if (_highScoreText != null) _highScoreText.gameObject.SetActive(false);
            if (_newRecordText != null) _newRecordText.gameObject.SetActive(false);
        }

        private void DisplayBalloonModeResults()
        {
            // Проверяем, есть ли расширенная информация о счете
            var balloonResult = _sessionResult as BalloonGameSessionResult;
            var score = balloonResult?.CurrentScore ?? _sessionResult.CorrectAnswers;

            // Отображаем основную информацию о счете
            if (_accuracyText != null)
                _accuracyText.text = $"{score}";

            // Отображаем текущий и лучший счет если доступна расширенная информация
            if (balloonResult != null)
            {
                if (_currentScoreText != null)
                    _currentScoreText.text = $"Счет: {balloonResult.CurrentScore}";

                if (_highScoreText != null)
                    _highScoreText.text = $"Рекорд: {balloonResult.HighScore}";

                // Показываем уведомление о новом рекорде
                if (_newRecordText != null)
                {
                    if (balloonResult.IsNewHighScore)
                    {
                        _newRecordText.gameObject.SetActive(true);
                        _newRecordText.text = "🎉 НОВЫЙ РЕКОРД! 🎉";
                        _newRecordText.color = Color.yellow;
                    }
                    else
                    {
                        _newRecordText.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                // Скрываем элементы расширенной информации если их нет
                if (_currentScoreText != null) _currentScoreText.gameObject.SetActive(false);
                if (_highScoreText != null) _highScoreText.gameObject.SetActive(false);
                if (_newRecordText != null) _newRecordText.gameObject.SetActive(false);
            }

            // Логика выбора фразы на основе счета
            string phrase;
            Color color;

            if (score >= 50)
            {
                phrase = "Невероятно! Ты настоящий мастер!";
                color = Color.green;
            }
            else if (score >= 30)
            {
                phrase = "Отличный результат!";
                color = Color.green;
            }
            else if (score >= 20)
            {
                phrase = "Хорошая работа!";
                color = new Color(0.5f, 0.8f, 0.2f); // Светло-зеленый
            }
            else if (score >= 10)
            {
                phrase = "Неплохо, продолжай тренироваться!";
                color = Color.yellow;
            }
            else if (score >= 5)
            {
                phrase = "Ты можешь лучше!";
                color = new Color(1f, 0.6f, 0f); // Оранжевый
            }
            else
            {
                phrase = "Попробуй еще раз!";
                color = Color.red;
            }

            _phraseText.text = phrase;
            _phraseText.color = color;
        }

        private void SetupButtons()
        {
            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (_homeButton != null)
            {
                _homeButton.onClick.AddListener(OnMenuClicked);
            }
        }

        private void OnRestartClicked()
        {
            // Возвращаемся к игровому экрану с теми же настройками
            if (_sessionResult?.GameSettings.GameType == GameType.Balloons)
            {
                ScreensManager.OpenScreen<BalloonGameScreen, GameSettings>(_sessionResult.GameSettings);
                CloseScreen();
            }
            else
            {
                ScreensManager.OpenScreen<CardsGameScreen, GameSettings>(_sessionResult.GameSettings);
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
                >= 50 => new Color(1f, 0.8f, 0.2f), // Желтый
                >= 30 => new Color(1f, 0.5f, 0.2f), // Оранжевый
                _ => new Color(0.8f, 0.3f, 0.3f) // Красный
            };
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_restartButton != null)
                _restartButton.onClick.RemoveAllListeners();

            if (_homeButton != null)
                _homeButton.onClick.RemoveAllListeners();
        }
    }
}