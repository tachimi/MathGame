using MathGame.Models;
using MathGame.Score;
using MathGame.Configs;
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

        [Header("New Record Effects")]
        [SerializeField] private ParticleSystem[] _newRecordParticleSystems;

        [Header("Phrases Configuration")]
        [SerializeField] private BalloonPhrasesConfig _phrasesConfig;

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

            // Показываем фразу результата
            if (_phraseText != null)
            {
                if (isNewRecord)
                {
                    // Для нового рекорда показываем специальную фразу
                    _phraseText.gameObject.SetActive(true);
                    _phraseText.text = "Новый рекорд!";

                    // Запускаем эффекты частиц для нового рекорда
                    PlayNewRecordParticles();
                }
                else
                {
                    // Для обычного результата используем конфиг фраз
                    _phraseText.gameObject.SetActive(true);
                    SetPhraseFromConfig(score);
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

            // Останавливаем эффекты частиц при скрытии
            StopNewRecordParticles();
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

        /// <summary>
        /// Запустить эффекты частиц для нового рекорда
        /// </summary>
        private void PlayNewRecordParticles()
        {
            if (_newRecordParticleSystems == null || _newRecordParticleSystems.Length == 0)
                return;

            // Запускаем все системы частиц
            foreach (var particleSystem in _newRecordParticleSystems)
            {
                if (particleSystem != null)
                {
                    // Сначала останавливаем систему частиц если она уже играет
                    if (particleSystem.isPlaying)
                    {
                        particleSystem.Stop();
                    }

                    // Очищаем предыдущие частицы
                    particleSystem.Clear();

                    // Запускаем воспроизведение
                    particleSystem.Play();
                }
            }
        }

        /// <summary>
        /// Остановить эффекты частиц для нового рекорда
        /// </summary>
        private void StopNewRecordParticles()
        {
            if (_newRecordParticleSystems == null || _newRecordParticleSystems.Length == 0)
                return;

            // Останавливаем все системы частиц
            foreach (var particleSystem in _newRecordParticleSystems)
            {
                if (particleSystem != null && particleSystem.isPlaying)
                {
                    particleSystem.Stop();
                    particleSystem.Clear();
                }
            }
        }

        /// <summary>
        /// Установить фразу из конфига на основе счета
        /// </summary>
        private void SetPhraseFromConfig(int score)
        {
            if (_phrasesConfig == null)
            {
                // Если конфиг не назначен, используем дефолтную фразу
                _phraseText.text = "Отличный результат!";
                _phraseText.color = Color.white;
                return;
            }

            // Получаем фразу и цвет из конфига
            var (phrase, color) = _phrasesConfig.GetPhraseForScore(score);
            _phraseText.text = phrase;
            _phraseText.color = color;
        }
    }
}