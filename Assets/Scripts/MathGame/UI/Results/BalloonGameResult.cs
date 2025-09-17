using MathGame.Models;
using MathGame.Score;
using TMPro;
using UnityEngine;

namespace MathGame.UI.Results
{
    /// <summary>
    /// Компонент для отображения результатов режима шариков
    /// </summary>
    public class BalloonGameResult : MonoBehaviour
    {
        [Header("Score Display")]
        [SerializeField] private TextMeshProUGUI _phraseText;
        [SerializeField] private TextMeshProUGUI _currentScoreText;
        [SerializeField] private TextMeshProUGUI _highScoreText;

        /// <summary>
        /// Отобразить результаты режима шариков
        /// </summary>
        /// <param name="sessionResult">Результат игровой сессии</param>
        public void DisplayResults(GameSessionResult sessionResult)
        {
            // Проверяем, есть ли расширенная информация о счете
            var balloonResult = sessionResult as BalloonGameSessionResult;
            var score = balloonResult?.CurrentScore ?? sessionResult.CorrectAnswers;

            // Получаем текущую сложность из настроек игры
            var difficulty = sessionResult.GameSettings.Difficulty;

            // Получаем лучший результат для текущей сложности
            var bestScore = BestScoreManager.GetBestScore(difficulty);

            // Проверяем и обновляем лучший результат
            var isNewRecord = BestScoreManager.TryUpdateBestScore(score, difficulty);

            // Отображаем текущий счет
            if (_currentScoreText != null)
            {
                _currentScoreText.gameObject.SetActive(true);
                _currentScoreText.text = score.ToString();
            }

            // Отображаем лучший счет
            if (_highScoreText != null)
            {
                _highScoreText.gameObject.SetActive(true);
                // Если установлен новый рекорд, показываем новое значение
                var displayBestScore = isNewRecord ? score : bestScore;
                _highScoreText.text = displayBestScore.ToString();
            }

            // Показываем уведомление о новом рекорде
            if (_phraseText != null)
            {
                if (isNewRecord)
                {
                    _phraseText.gameObject.SetActive(true);
                    _phraseText.text = "Новый рекорд!";
                }
                else
                {
                    _phraseText.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Скрыть все элементы результатов шариков
        /// </summary>
        public void Hide()
        {
            if (_phraseText != null) _phraseText.gameObject.SetActive(false);
            if (_currentScoreText != null) _currentScoreText.gameObject.SetActive(false);
            if (_highScoreText != null) _highScoreText.gameObject.SetActive(false);
        }

        /// <summary>
        /// Показать все элементы результатов шариков
        /// </summary>
        public void Show()
        {
            if (_phraseText != null) _phraseText.gameObject.SetActive(true);
            if (_currentScoreText != null) _currentScoreText.gameObject.SetActive(true);
            if (_highScoreText != null) _highScoreText.gameObject.SetActive(true);
            // _newRecordText показываем только при новом рекорде
        }
    }
}