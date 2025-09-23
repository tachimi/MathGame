using MathGame.Models;
using MathGame.Score;
using MathGame.Configs;
using TMPro;
using UnityEngine;
using DG.Tweening;

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

        [Header("Animation Settings")]
        [SerializeField] private float _scoreAnimationDuration = 0.5f;
        [SerializeField] private Ease _scoreAnimationEase = Ease.OutBack;
        [SerializeField] private float _scoreAnimationScale = 1.2f;

        // State tracking for delayed record updates
        private bool _hasNewRecordPending = false;
        private int _pendingNewRecord = 0;

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

                // Анимация для текущего счета при новом рекорде
                if (isNewRecord)
                {
                    AnimateCurrentScore();
                }
            }

            // Отображаем лучший счет с логикой отложенного обновления
            if (_highScoreText != null)
            {
                if (isNewRecord)
                { 
                    _highScoreText.gameObject.SetActive(false);
                    // При новом рекорде показываем старое значение
                   // _highScoreText.text = bestScore.ToString();

                    // Сохраняем новый рекорд для следующего показа
                   // _hasNewRecordPending = true;
                   // _pendingNewRecord = score;
                }
                else
                {
                    _highScoreText.gameObject.SetActive(true);
                    _highScoreText.text = bestScore.ToString();
                    // Проверяем, есть ли отложенный рекорд для обновления
                   // if (_hasNewRecordPending)
                   // {
                   //     _highScoreText.text = _pendingNewRecord.ToString();
                   //     _hasNewRecordPending = false;
                   //     _pendingNewRecord = 0;
                   // }
                   // else
                   // {
                   //     _highScoreText.text = bestScore.ToString();
                   // }
                }
            }

            // Показываем фразу результата
            if (_phraseText != null)
            {
                if (isNewRecord)
                {
                    // Для нового рекорда показываем специальную фразу
                    _phraseText.gameObject.SetActive(true);

                    _phraseText.text = _phrasesConfig.GetNewRecordPhrase();
                   // var defaultText = _phrasesConfig.GetNewRecordPhrase();
                   // _phraseText.text = $"{defaultText} \n{score} <s>{bestScore}</s>";

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
            // Получаем фразу и цвет из конфига
            var (phrase, color) = _phrasesConfig.GetPhraseForScore(score);
            _phraseText.text = phrase;
            _phraseText.color = color;
        }

        /// <summary>
        /// Анимация текущего счета при достижении нового рекорда
        /// </summary>
        private void AnimateCurrentScore()
        {
            if (_currentScoreText == null) return;

            // Сброс масштаба перед анимацией
            _currentScoreText.transform.localScale = Vector3.one;

            // Создаем последовательность анимации
            var sequence = DOTween.Sequence();

            // Увеличиваем масштаб
            sequence.Append(_currentScoreText.transform.DOScale(_scoreAnimationScale, _scoreAnimationDuration * 0.6f)
                .SetEase(_scoreAnimationEase));

            // Возвращаем к исходному масштабу
            sequence.Append(_currentScoreText.transform.DOScale(1f, _scoreAnimationDuration * 0.4f)
                .SetEase(Ease.OutQuad));
            sequence.SetLoops(3);

            // Запускаем последовательность
            sequence.Play();
        }
    }
}