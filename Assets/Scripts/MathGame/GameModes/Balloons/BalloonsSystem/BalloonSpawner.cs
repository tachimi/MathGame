using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MathGame.Configs;
using MathGame.Models;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace MathGame.GameModes.Balloons.BalloonsSystem
{
    /// <summary>
    /// Отвечает за спавн шариков в определенном количестве и позициях
    /// </summary>
    public class BalloonSpawner
    {
        #region Events

        public event Action<BalloonAnswer> OnBalloonCreated;

        #endregion

        #region Private Fields

        private readonly BalloonModeConfig _config;
        private readonly Transform _spawnParent;
        private readonly RectTransform _balloonContainer;

        private bool _isSpawning;

        // Константы
        private const string BALLOON_PREFAB_PATH = "GameModes/BalloonAnswer";

        #endregion

        #region Constructor

        public BalloonSpawner(BalloonModeConfig config, RectTransform spawnParent)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _spawnParent = spawnParent ?? throw new ArgumentNullException(nameof(spawnParent));
            _balloonContainer = spawnParent;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Спавн всех шариков для раунда одновременно
        /// </summary>
        public async UniTask SpawnAllBalloons(Question question, CancellationToken cancellationToken)
        {
            if (_isSpawning)
            {
                Debug.LogWarning("BalloonSpawner: Попытка начать спавн во время активного спавна");
                return;
            }

            _isSpawning = true;

            try
            {
                var balloonData = GenerateBalloonData(question);

                for (int i = 0; i < balloonData.Count; i++)
                {
                    var data = balloonData[i];
                    
                    if (cancellationToken.IsCancellationRequested || !_isSpawning)
                    {
                        Debug.Log($"BalloonSpawner: Спавн прерван. Токен отменен: {cancellationToken.IsCancellationRequested}, Спавн активен: {_isSpawning}");
                        return;
                    }

                    var balloon = CreateBalloon(data, i);
                    OnBalloonCreated?.Invoke(balloon);

                    Debug.Log($"BalloonSpawner: Создан шарик {data.Answer}, ожидание {_config.SpawnInterval} сек");
                    await UniTask.WaitForSeconds(_config.SpawnInterval, cancellationToken: cancellationToken);
                    
                    // Дополнительная проверка после ожидания
                    if (cancellationToken.IsCancellationRequested || !_isSpawning)
                    {
                        Debug.Log($"BalloonSpawner: Спавн прерван после ожидания. Токен отменен: {cancellationToken.IsCancellationRequested}, Спавн активен: {_isSpawning}");
                        return;
                    }
                }
            }
            finally
            {
                _isSpawning = false;
            }
        }

        /// <summary>
        /// Остановить спавн
        /// </summary>
        public void StopSpawning()
        {
            _isSpawning = false;
            Debug.Log("BalloonSpawner: Спавн остановлен");
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
            
            Debug.Log($"BalloonSpawner: Очищено {clearedCount} из {childCount} шариков");
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
            } while ((existingAnswers.Contains(wrongAnswer) || wrongAnswer < 0) && attempts < 20);

            return Math.Max(0, wrongAnswer);
        }

        /// <summary>
        /// Создать физический шарик
        /// </summary>
        private BalloonAnswer CreateBalloon(BalloonData data, int balloonIndex)
        {
            var spawnPosition = GetRandomPosition();

            // Пытаемся загрузить префаб шарика
            var balloonPrefab = Resources.Load<GameObject>(BALLOON_PREFAB_PATH);

            if (balloonPrefab == null)
            {
                Debug.LogError("BalloonSpawner: BalloonPrefab is null");
            }
            
            var balloonObject = Object.Instantiate(balloonPrefab, _spawnParent);
            balloonObject.name = $"Balloon_{data.Answer}_{(data.IsCorrect ? "CORRECT" : "WRONG")}";
            var balloon = balloonObject.GetComponent<BalloonAnswer>();
            balloon.RectTransform.anchoredPosition = spawnPosition;

            // Получаем цвет на основе настроек конфига
            var balloonColor = _config.GetBalloonColor(data.IsCorrect, balloonIndex);
            
            balloon.Initialize(data.Answer, data.IsCorrect, _config.BalloonSpeed, _config, balloonColor);

            return balloon;
        }

        private Vector3 GetRandomPosition()
        {
            // Используем настройки отступов из конфига
            var leftBound = _balloonContainer.rect.xMin - _config.SpawnLeftOffset;
            var rightBound = _balloonContainer.rect.xMax + _config.SpawnRightOffset;
            var bottomBound = _balloonContainer.rect.yMin + _config.SpawnBottomOffset;
            
            // Проверяем валидность зоны спавна
            if (leftBound >= rightBound)
            {
                Debug.LogError($"BalloonSpawner: Некорректная зона спавна! Левая граница ({leftBound:F1}) >= правой ({rightBound:F1})");
                // Fallback к центру контейнера
                leftBound = _balloonContainer.rect.center.x - 50f;
                rightBound = _balloonContainer.rect.center.x + 50f;
            }
            
            var randomX = Random.Range(leftBound, rightBound);
            
            Debug.Log($"BalloonSpawner: Спавн позиция X={randomX:F1}, Y={bottomBound:F1}. Границы: L={leftBound:F1}, R={rightBound:F1}");
            
            return new Vector3(randomX, bottomBound, 0);
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