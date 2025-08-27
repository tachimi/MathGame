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
        /// Спавн всех шариков для раунда волнами по колонкам
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
                var columns = GenerateColumnOrder();
                
                Debug.Log($"BalloonSpawner: Начинаем спавн {balloonData.Count} шариков в {_config.SpawnColumns} колонок");
                
                int balloonIndex = 0;
                
                // Спавним волнами пока есть шарики
                while (balloonIndex < balloonData.Count && _isSpawning && !cancellationToken.IsCancellationRequested)
                {
                    // Спавн одной волны шариков
                    int spawned = await SpawnWave(balloonData, balloonIndex, columns, cancellationToken);
                    balloonIndex += spawned;
                    
                    // Проверяем нужна ли еще одна волна
                    if (balloonIndex < balloonData.Count && _isSpawning && !cancellationToken.IsCancellationRequested)
                    {
                        Debug.Log($"BalloonSpawner: Ожидание {_config.WaveDelay} сек до следующей волны");
                        await UniTask.WaitForSeconds(_config.WaveDelay, cancellationToken: cancellationToken);
                    }
                }
                
                Debug.Log($"BalloonSpawner: Спавн завершен. Создано {balloonIndex} шариков");
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
        /// Создать физический шарик в определенной колонке
        /// </summary>
        private BalloonAnswer CreateBalloon(BalloonData data, int balloonIndex, int columnIndex)
        {
            var spawnPosition = GetColumnPosition(columnIndex);

            // Пытаемся загрузить префаб шарика
            var balloonPrefab = Resources.Load<GameObject>(BALLOON_PREFAB_PATH);

            if (balloonPrefab == null)
            {
                Debug.LogError("BalloonSpawner: BalloonPrefab is null");
            }
            
            var balloonObject = Object.Instantiate(balloonPrefab, _spawnParent);
            balloonObject.name = $"Balloon_{data.Answer}_{(data.IsCorrect ? "CORRECT" : "WRONG")}_Col{columnIndex}";
            var balloon = balloonObject.GetComponent<BalloonAnswer>();
            balloon.RectTransform.anchoredPosition = spawnPosition;

            // Получаем цвет на основе настроек конфига
            var balloonColor = _config.GetBalloonColor(data.IsCorrect, balloonIndex);
            
            balloon.Initialize(data.Answer, data.IsCorrect, _config.BalloonSpeed, _config, balloonColor);

            Debug.Log($"BalloonSpawner: Создан шарик {data.Answer} в колонке {columnIndex}, позиция: {spawnPosition}");

            return balloon;
        }

        /// <summary>
        /// Спавн одной волны шариков по колонкам
        /// </summary>
        /// <returns>Количество созданных шариков</returns>
        private async UniTask<int> SpawnWave(List<BalloonData> balloonData, int startIndex, List<int> columnOrder, CancellationToken cancellationToken)
        {
            int balloonsInWave = Mathf.Min(_config.SpawnColumns, balloonData.Count - startIndex);
            int spawned = 0;
            
            for (int i = 0; i < balloonsInWave; i++)
            {
                if (cancellationToken.IsCancellationRequested || !_isSpawning)
                {
                    Debug.Log($"BalloonSpawner: Спавн волны прерван на шарике {i}");
                    break;
                }
                
                var data = balloonData[startIndex + i];
                int columnIndex = columnOrder[i % columnOrder.Count];
                
                var balloon = CreateBalloon(data, startIndex + i, columnIndex);
                OnBalloonCreated?.Invoke(balloon);
                
                spawned++;
                
                // Ждем интервал между колонками (если не последний в волне и интервал > 0)
                if (i < balloonsInWave - 1 && _config.ColumnSpawnInterval > 0f)
                {
                    Debug.Log($"BalloonSpawner: Ожидание {_config.ColumnSpawnInterval} сек до следующей колонки");
                    await UniTask.WaitForSeconds(_config.ColumnSpawnInterval, cancellationToken: cancellationToken);
                }
            }
            
            return spawned;
        }
        
        /// <summary>
        /// Генерирует порядок колонок для спавна
        /// </summary>
        private List<int> GenerateColumnOrder()
        {
            var columns = new List<int>();
            
            for (int i = 0; i < _config.SpawnColumns; i++)
            {
                columns.Add(i);
            }
            
            // Если режим случайный - перемешиваем
            if (_config.ColumnMode == ColumnSpawnMode.Random)
            {
                for (int i = columns.Count - 1; i > 0; i--)
                {
                    int randomIndex = Random.Range(0, i + 1);
                    (columns[i], columns[randomIndex]) = (columns[randomIndex], columns[i]);
                }
            }
            
            return columns;
        }
        
        /// <summary>
        /// Получить позицию для спавна в определенной колонке
        /// </summary>
        private Vector3 GetColumnPosition(int columnIndex)
        {
            // Вычисляем границы с учетом отступов
            var leftBound = _balloonContainer.rect.xMin + _config.SpawnLeftOffset;
            var rightBound = _balloonContainer.rect.xMax - _config.SpawnRightOffset;
            var bottomBound = _balloonContainer.rect.yMin + _config.SpawnBottomOffset;
            
            // Проверяем валидность зоны спавна
            if (leftBound >= rightBound)
            {
                Debug.LogError($"BalloonSpawner: Некорректная зона спавна! Левая граница ({leftBound:F1}) >= правой ({rightBound:F1})");
                // Fallback к центру контейнера
                leftBound = _balloonContainer.rect.center.x - 50f;
                rightBound = _balloonContainer.rect.center.x + 50f;
            }
            
            // Вычисляем ширину одной колонки в доступной области (с учетом отступов)
            float availableWidth = rightBound - leftBound;
            float columnWidth = availableWidth / _config.SpawnColumns;
            
            // Вычисляем X позицию для центра колонки
            float columnCenterX = leftBound + (columnIndex + 0.5f) * columnWidth;
            
            Debug.Log($"BalloonSpawner: Колонка {columnIndex}, позиция X={columnCenterX:F1}, Y={bottomBound:F1}. Доступная ширина: {availableWidth:F1}, отступы L={_config.SpawnLeftOffset}, R={_config.SpawnRightOffset}");
            
            return new Vector3(columnCenterX, bottomBound, 0);
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