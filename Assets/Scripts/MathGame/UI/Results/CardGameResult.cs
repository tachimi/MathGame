using MathGame.Configs;
using MathGame.Models;
using TMPro;
using UnityEngine;

namespace MathGame.UI.Results
{
    /// <summary>
    /// Компонент для отображения результатов режима карточек
    /// </summary>
    public class CardGameResult : MonoBehaviour
    {
        [Header("Score Display")]
        [SerializeField] private TextMeshProUGUI _phraseText;
        [SerializeField] private TextMeshProUGUI _accuracyText;
        [SerializeField] private TextMeshProUGUI _timeText;

        [Header("Configuration")]
        [SerializeField] private CardPhrasesConfig _phrasesConfig;

        /// <summary>
        /// Отобразить результаты режима карточек
        /// </summary>
        /// <param name="sessionResult">Результат игровой сессии</param>
        public void DisplayResults(GameSessionResult sessionResult)
        {
            // Отображаем фразу и цвет на основе точности
            var (phrase, color) = GetPhraseForAccuracy(sessionResult.AccuracyPercentage);
            if (_phraseText != null)
            {
                _phraseText.text = phrase;
                _phraseText.color = color;
            }

            // Отображаем процент точности
            if (_accuracyText != null)
            {
                _accuracyText.gameObject.SetActive(true);
                _accuracyText.text = $"{sessionResult.AccuracyPercentage:F0}%";
            }

            // Отображаем время
            if (_timeText != null)
            {
                var totalTime = sessionResult.TotalTime;
                _timeText.text = totalTime.TotalHours >= 1
                    ? $"{(int)totalTime.TotalHours}:{totalTime.Minutes:D2}:{totalTime.Seconds:D2}"
                    : $"{(int)totalTime.TotalMinutes}:{totalTime.Seconds:D2}";
            }
        }

        /// <summary>
        /// Получить фразу и цвет для заданной точности
        /// </summary>
        private (string phrase, Color color) GetPhraseForAccuracy(float accuracy)
        {
            // Если есть конфиг с фразами, используем его
            if (_phrasesConfig != null)
            {
                return _phrasesConfig.GetPhraseForAccuracy(accuracy);
            }

            // Иначе используем значения по умолчанию
            return GetDefaultPhraseAndColor(accuracy);
        }

        /// <summary>
        /// Получить фразу и цвет по умолчанию
        /// </summary>
        private (string phrase, Color color) GetDefaultPhraseAndColor(float accuracy)
        {
            string phrase = accuracy switch
            {
                >= 90 => "Отлично!",
                >= 70 => "Хорошо!",
                >= 50 => "Можно лучше!",
                >= 30 => "Нужна практика!",
                _ => "Попробуй снова!"
            };

            Color color = accuracy switch
            {
                >= 90 => new Color(0.2f, 0.8f, 0.2f), // Зеленый
                >= 70 => new Color(0.4f, 0.7f, 0.4f), // Светло-зеленый
                >= 50 => new Color(1f, 0.8f, 0.2f), // Желтый
                >= 30 => new Color(1f, 0.5f, 0.2f), // Оранжевый
                _ => new Color(0.8f, 0.3f, 0.3f) // Красный
            };

            return (phrase, color);
        }

        /// <summary>
        /// Скрыть все элементы результатов карточек
        /// </summary>
        public void Hide()
        {
            if (_phraseText != null) _phraseText.gameObject.SetActive(false);
            if (_accuracyText != null) _accuracyText.gameObject.SetActive(false);
            if (_timeText != null) _timeText.gameObject.SetActive(false);
        }

        /// <summary>
        /// Показать все элементы результатов карточек
        /// </summary>
        public void Show()
        {
            if (_phraseText != null) _phraseText.gameObject.SetActive(true);
            if (_accuracyText != null) _accuracyText.gameObject.SetActive(true);
            if (_timeText != null) _timeText.gameObject.SetActive(true);
        }

        /// <summary>
        /// Установить конфиг фраз (можно вызвать из ResultScreen если нужно)
        /// </summary>
        public void SetPhrasesConfig(CardPhrasesConfig config)
        {
            _phrasesConfig = config;
        }
    }
}