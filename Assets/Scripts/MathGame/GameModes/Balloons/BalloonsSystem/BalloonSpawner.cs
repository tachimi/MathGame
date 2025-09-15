using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MathGame.Configs;
using MathGame.Models;
using MathGame.Utils;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace MathGame.GameModes.Balloons.BalloonsSystem
{
    /// <summary>
    /// Отвечает за спавн шариков с рандомными позициями и физикой
    /// </summary>
    public class BalloonSpawner
    {
        public event Action<BalloonAnswer> OnBalloonCreated;

        private readonly BalloonModeConfig _config;
        private readonly Transform _spawnParent;
        private readonly RectTransform _balloonContainer;
        private CancellationTokenSource _spawnCancellation;

        public BalloonSpawner(BalloonModeConfig config, RectTransform spawnParent)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _spawnParent = spawnParent ?? throw new ArgumentNullException(nameof(spawnParent));
            _balloonContainer = spawnParent;
        }

        #region Public Methods

        /// <summary>
        /// Спавн шариков поэтапно с задержкой
        /// </summary>
        public void SpawnAllBalloons(Question question, BalloonDifficultySettings difficultySettings)
        {
            // Отменяем предыдущий спавн если он был
            StopSpawning();

            // Создаем новый токен отмены
            _spawnCancellation = new CancellationTokenSource();

            var balloonData = GenerateBalloonData(question);
            SpawnBalloonsSequentiallyAsync(balloonData, difficultySettings, _spawnCancellation.Token).Forget();
        }

        /// <summary>
        /// Поэтапный спавн шариков с задержкой
        /// </summary>
        private async UniTaskVoid SpawnBalloonsSequentiallyAsync(List<BalloonData> balloonData, BalloonDifficultySettings difficultySettings, CancellationToken cancellationToken)
        {
            try
            {
                for (int i = 0; i < balloonData.Count; i++)
                {
                    // Проверяем отмену перед каждым спавном
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var data = balloonData[i];
                    var balloon = CreateBalloon(data, i, difficultySettings);
                    OnBalloonCreated?.Invoke(balloon);

                    // Задержка между спавном шариков из конфига с проверкой отмены
                    await UniTask.Delay(_config.SpawnIntervalMs, cancellationToken: cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        /// <summary>
        /// Остановить текущий спавн шариков
        /// </summary>
        public void StopSpawning()
        {
            if (_spawnCancellation != null)
            {
                _spawnCancellation.Cancel();
                _spawnCancellation.Dispose();
                _spawnCancellation = null;
            }
        }

        
        /// <summary>
        /// Очистить все созданные шарики
        /// </summary>
        public void ClearAllBalloons()
        {
            if (_spawnParent == null) return;
            
            // Находим все дочерние объекты и уничтожаем их
            var childCount = _spawnParent.childCount;
            var clearedCount = 0;
            
            for (int i = childCount - 1; i >= 0; i--)
            {
                var child = _spawnParent.GetChild(i);
                if (child != null && child.GetComponent<BalloonAnswer>() != null)
                {
                    // Сначала отключаем взаимодействие
                    var balloonAnswer = child.GetComponent<BalloonAnswer>();
                    balloonAnswer?.DisableInteraction();
                    balloonAnswer?.StopMovement();
                    
                    // Немедленно уничтожаем объект
                    Object.DestroyImmediate(child.gameObject);
                    clearedCount++;
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Генерировать данные для всех шариков раунда
        /// </summary>
        private List<BalloonData> GenerateBalloonData(Question question)
        {
            var balloonData = new List<BalloonData>();
            var totalBalloons = _config.BalloonsCount;

            for (var i = 0; i < totalBalloons; i++)
            {
                var isCorrect = i == 0;
                var answer = isCorrect
                    ? question.CorrectAnswer
                    : GenerateWrongAnswer(question.CorrectAnswer, balloonData);

                balloonData.Add(new BalloonData
                {
                    Answer = answer,
                    IsCorrect = isCorrect
                });
            }

            // Перемешиваем шарики для случайного порядка правильного ответа
            for (int i = balloonData.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                (balloonData[i], balloonData[randomIndex]) = (balloonData[randomIndex], balloonData[i]);
            }

            return balloonData;
        }

        /// <summary>
        /// Сгенерировать неправильный ответ
        /// </summary>
        private int GenerateWrongAnswer(int correctAnswer, List<BalloonData> existingBalloons)
        {
            var existingAnswers = existingBalloons.Select(b => b.Answer).ToHashSet();
            existingAnswers.Add(correctAnswer);

            int wrongAnswer;
            int attempts = 0;

            do
            {
                wrongAnswer = correctAnswer + Random.Range(-10, 11);
                attempts++;
            } while ((existingAnswers.Contains(wrongAnswer) || wrongAnswer < 0) && attempts < _config.MaxWrongAnswerAttempts);

            return Math.Max(0, wrongAnswer);
        }

        /// <summary>
        /// Создать шарик с рандомной позицией и начальной скоростью
        /// </summary>
        private BalloonAnswer CreateBalloon(BalloonData data, int balloonIndex, BalloonDifficultySettings difficultySettings)
        {
            if (_config.BalloonPrefab == null)
            {
                return null;
            }

            var spawnPosition = GetRandomSpawnPosition();

            var balloonObject = Object.Instantiate(_config.BalloonPrefab, _spawnParent);
            balloonObject.name = $"Balloon_{data.Answer}_{(data.IsCorrect ? "CORRECT" : "WRONG")}";
            var balloon = balloonObject.GetComponent<BalloonAnswer>();
            balloon.RectTransform.anchoredPosition = spawnPosition;

            var balloonColor = _config.GetBalloonColor(data.IsCorrect, balloonIndex);
            var randomGravity = Random.Range(difficultySettings.MinGravity, difficultySettings.MaxGravity);
            var randomDrag = Random.Range(difficultySettings.Drag * 0.8f, difficultySettings.Drag * 1.2f);

            balloon.Initialize(data.Answer, data.IsCorrect, _config, balloonColor, difficultySettings, randomGravity, randomDrag);

            // Добавляем случайный начальный импульс
            var initialForce = GetRandomInitialForce();
            balloon.AddInitialForce(initialForce);

            return balloon;
        }

        /// <summary>
        /// Получить рандомную позицию спавна ВНИЗУ экрана
        /// </summary>
        private Vector3 GetRandomSpawnPosition()
        {
            var containerRect = _balloonContainer.rect;

            // Рандомная позиция по всей ширине контейнера
            var randomX = Random.Range(
                containerRect.xMin + _config.SafeBoundsPadding,
                containerRect.xMax - _config.SafeBoundsPadding);

            // Спавн внизу экрана с небольшим разбросом
            var spawnY = containerRect.yMin + _config.SafeBoundsPadding;
            var randomY = spawnY + Random.Range(0, _config.SpawnVerticalRandomness);

            return new Vector3(randomX, randomY, 0);
        }

        /// <summary>
        /// Получить случайную начальную силу для шарика ВВЕРХ
        /// </summary>
        private Vector2 GetRandomInitialForce()
        {
            var forceX = Random.Range(-_config.InitialForceRange * 0.3f, _config.InitialForceRange * 0.3f);
            var forceY = Random.Range(_config.InitialForceRange * 0.8f, _config.InitialForceRange); // Сильно вверх

            return new Vector2(forceX, forceY);
        }

        #endregion
    }

    /// <summary>
    /// Данные для создания шарика
    /// </summary>
    public class BalloonData
    {
        public int Answer { get; set; }
        public bool IsCorrect { get; set; }
    }
}