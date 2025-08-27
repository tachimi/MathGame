using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MathGame.Configs;
using MathGame.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MathGame.GameModes.Balloons
{
    /// <summary>
    /// Компонент отдельного шарика с ответом
    /// Летит снизу вверх, при нажатии лопается и передает ответ
    /// </summary>
    public class BalloonAnswer : MonoBehaviour, IPointerClickHandler
    {
        public event Action<BalloonAnswer, int, bool> OnBalloonTapped;
        public event Action<BalloonAnswer> OnBalloonDestroyed;
        public event Action<BalloonAnswer, int, bool> OnBalloonReachedTop;

        public RectTransform RectTransform => _rectTransform;

        [Header("Balloon Components")]
        [SerializeField] private Image _balloonImage;
        [SerializeField] private TextMeshProUGUI _answerText;
        [SerializeField] private RectTransform _rectTransform;
        
        [Header("Animation Settings")]
        [SerializeField] private float _popAnimationDuration = 0.5f;
        [SerializeField] private AnimationCurve _popScaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        [SerializeField] private AnimationCurve _floatCurve = AnimationCurve.Linear(0, 0, 1, 1);
        
        private BalloonModeConfig _config;
        private Vector3 _targetPosition;
        private CancellationTokenSource _movementCancellation;
        private CancellationTokenSource _animationCancellation;
        private int _answer;
        private bool _isCorrect;
        private float _speed;
        private bool _isPopped;
        private bool _isMoving;
        private bool _interactionEnabled = true;
        
        public int Answer => _answer;
        public bool IsCorrectAnswer => _isCorrect;
        public bool IsPopped => _isPopped;
        
        private void Awake()
        {
            SetupComponents();
        }
        
        /// <summary>
        /// Настройка компонентов шарика
        /// </summary>
        private void SetupComponents()
        {
        }
        
        /// <summary>
        /// Инициализация шарика с параметрами
        /// </summary>
        public void Initialize(int answer, bool isCorrect, float speed, BalloonModeConfig config, Color balloonColor)
        {
            _answer = answer;
            _isCorrect = isCorrect;
            _speed = speed;
            _config = config;
            _isPopped = false;
            _isMoving = false;
            _interactionEnabled = true;
            
            // Обновляем текст ответа
            if (_answerText != null)
                _answerText.text = answer.ToString();
            
            // Устанавливаем цвет шарика из параметра
            if (_balloonImage != null)
            {
                _balloonImage.color = balloonColor;
            }
            
            // Позиция устанавливается спавнером, просто запускаем движение
            StartMovement();
            
            Debug.Log($"BalloonAnswer: Инициализирован шарик с ответом {answer}, правильный: {isCorrect}, цвет: {balloonColor}");
        }
        
        /// <summary>
        /// Настроить целевую позицию для движения
        /// </summary>
        private void SetupTargetPosition()
        {
            // Движемся вверх от текущей позиции
            var canvasRect = GetCanvasRect();
            var currentPos = _rectTransform.anchoredPosition;
            _targetPosition = new Vector3(currentPos.x, canvasRect.height * 0.6f, 0);
        }
        
        /// <summary>
        /// Запуск движения шарика
        /// </summary>
        private void StartMovement()
        {
            _isMoving = true;
            
            // Настраиваем целевую позицию
            SetupTargetPosition();
            
            // Создаем токен отмены для движения
            _movementCancellation = new CancellationTokenSource();
            
            // Запускаем асинхронное движение
            MoveBalloonAsync(_movementCancellation.Token).Forget();
        }
        
        /// <summary>
        /// Остановить движение шарика
        /// </summary>
        public void StopMovement()
        {
            _isMoving = false;
            _movementCancellation?.Cancel();
        }
        
        /// <summary>
        /// Отключить взаимодействие с шариком
        /// </summary>
        public void DisableInteraction()
        {
            _interactionEnabled = false;
        }
        
        /// <summary>
        /// Асинхронное движение шарика
        /// </summary>
        private async UniTaskVoid MoveBalloonAsync(CancellationToken cancellationToken)
        {
            var startPosition = _rectTransform.anchoredPosition;
            float journeyLength = Vector3.Distance(startPosition, _targetPosition);
            float journeyTime = journeyLength / _speed;
            float journey = 0f;
            
            while (journey <= journeyTime && !_isPopped && _isMoving && !cancellationToken.IsCancellationRequested)
            {
                float fractionOfJourney = journey / journeyTime;
                float easedProgress = _floatCurve.Evaluate(fractionOfJourney);
                
                _rectTransform.anchoredPosition = Vector2.Lerp(startPosition, _targetPosition, easedProgress);
                
                // Проверяем выход за границы экрана
                bool shouldAutoPop = false;
                
                if (_config != null)
                {
                    shouldAutoPop = UIScreenBounds.IsAboveScreen(_rectTransform, _config.ScreenBoundsPadding);
                }
                if (shouldAutoPop)
                {
                    Debug.Log($"BalloonAnswer: Шарик {_answer} вышел за границы экрана");
                    
                    // Уведомляем о достижении верхней границы
                    OnBalloonReachedTop?.Invoke(this, _answer, _isCorrect);
                    
                    // Лопаем шарик
                    PlayPopAnimation();
                    
                    // Прерываем движение
                    return;
                }
                
                journey += Time.deltaTime;
                await UniTask.NextFrame(cancellationToken);
            }
        }
        
        
        /// <summary>
        /// Обработчик нажатия на шарик
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"BalloonAnswer: Попытка клика на шарик {_answer}. Popped: {_isPopped}, InteractionEnabled: {_interactionEnabled}");
            
            if (_isPopped || !_interactionEnabled) 
            {
                Debug.Log($"BalloonAnswer: Клик игнорирован для шарика {_answer}");
                return;
            }
            
            Debug.Log($"BalloonAnswer: Шарик нажат - ответ {_answer}, правильный: {_isCorrect}");
            
            // Уведомляем о нажатии
            OnBalloonTapped?.Invoke(this, _answer, _isCorrect);
        }
        
        /// <summary>
        /// Анимация лопания шарика
        /// </summary>
        public void PlayPopAnimation()
        {
            if (_isPopped) return;
            
            _isPopped = true;
            _isMoving = false;
            
            // Останавливаем движение
            _movementCancellation?.Cancel();
            
            PopAnimationAsync().Forget();
        }
        
        /// <summary>
        /// Асинхронная анимация лопания
        /// </summary>
        private async UniTaskVoid PopAnimationAsync()
        {
            float elapsed = 0f;
            Vector3 originalScale = transform.localScale;
            
            while (elapsed < _popAnimationDuration)
            {
                // Проверяем, что объект еще существует
                if (this == null || transform == null)
                    return;
                    
                elapsed += Time.deltaTime;
                float progress = elapsed / _popAnimationDuration;
                float scaleMultiplier = _popScaleCurve.Evaluate(progress);
                
                transform.localScale = originalScale * scaleMultiplier;
                
                // Постепенно делаем прозрачным
                if (_balloonImage != null)
                {
                    var color = _balloonImage.color;
                    color.a = 1f - progress;
                    _balloonImage.color = color;
                }
                
                if (_answerText != null)
                {
                    var textColor = _answerText.color;
                    textColor.a = 1f - progress;
                    _answerText.color = textColor;
                }
                
                await UniTask.NextFrame();
            }
            
            // Уничтожаем после анимации
            if (this != null)
            {
                DestroyBalloon();
            }
        }
        
        /// <summary>
        /// Показать что это был правильный ответ (подсветка)
        /// </summary>
        public void ShowAsCorrectAnswer()
        {
            if (_balloonImage != null)
            {
                // Делаем шарик зеленым
                _balloonImage.color = Color.green;
            }
            
            if (_answerText != null)
            {
                // Увеличиваем текст и делаем жирным
                _answerText.fontSize = _answerText.fontSize * 1.5f;
                _answerText.fontStyle = FontStyles.Bold;
            }
            
            // Отменяем предыдущую анимацию
            _animationCancellation?.Cancel();
            _animationCancellation = new CancellationTokenSource();
            
            // Анимация пульсации
            PulseAnimationAsync(_animationCancellation.Token).Forget();
        }
        
        /// <summary>
        /// Анимация пульсации для правильного ответа
        /// </summary>
        private async UniTaskVoid PulseAnimationAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Проверяем, что объект существует
                if (this == null || transform == null)
                    return;
                    
                var originalScale = transform.localScale;
                float pulseTime = 0.3f;
                int pulseCount = 3;
                
                for (int i = 0; i < pulseCount; i++)
                {
                    // Проверяем отмену
                    if (cancellationToken.IsCancellationRequested)
                        return;
                        
                    // Проверяем, что объект еще существует
                    if (this == null || transform == null)
                        return;
                    
                    // Увеличиваем
                    float elapsed = 0f;
                    while (elapsed < pulseTime)
                    {
                        // Проверяем отмену и существование объекта
                        if (cancellationToken.IsCancellationRequested || this == null || transform == null)
                            return;
                            
                        elapsed += Time.deltaTime;
                        float scale = 1f + (0.3f * Mathf.Sin(elapsed / pulseTime * Mathf.PI));
                        transform.localScale = originalScale * scale;
                        await UniTask.NextFrame(cancellationToken);
                    }
                    
                    // Проверяем перед восстановлением масштаба
                    if (this != null && transform != null)
                    {
                        transform.localScale = originalScale;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ожидаемая отмена - нормально
            }
        }

        
        /// <summary>
        /// Уничтожить шарик
        /// </summary>
        private void DestroyBalloon()
        {
            // Уведомляем об уничтожении
            OnBalloonDestroyed?.Invoke(this);
            
            // Уничтожаем GameObject
            Destroy(gameObject);
        }
        
        
        /// <summary>
        /// Получить размеры Canvas
        /// </summary>
        private Rect GetCanvasRect()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                return canvas.GetComponent<RectTransform>().rect;
            }
            
            // Fallback размеры
            return new Rect(0, 0, 1920, 1080);
        }
        
        private void OnDestroy()
        {
            // Отменяем все анимации
            _movementCancellation?.Cancel();
            _movementCancellation?.Dispose();
            _animationCancellation?.Cancel();
            _animationCancellation?.Dispose();
        }
    }
}