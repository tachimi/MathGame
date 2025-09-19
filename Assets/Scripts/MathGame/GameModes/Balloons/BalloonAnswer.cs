using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MathGame.Configs;
using MathGame.Utils;
using SoundSystem.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

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
        public event Action<BalloonAnswer, Vector3, Color> OnBalloonPopped;
        public event Action<SoundType> OnPlaySound;

        public RectTransform RectTransform => _rectTransform;

        [Header("Balloon Components")]
        [SerializeField] private SpriteRenderer _balloonSprite;
        [SerializeField] private TextMeshProUGUI _answerText;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private RectTransform _clickDetectionRect;
        [SerializeField] private Rigidbody2D _rigidbody2D;
        
        [Header("Animation Settings")]
        [SerializeField] private float _popAnimationDuration = 0.5f;
        [SerializeField] private AnimationCurve _popScaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        
        private BalloonDifficultySettings _difficultySettings;
        private BalloonModeConfig _config;
        private CancellationTokenSource _animationCancellation;
        private int _answer;
        private bool _isCorrect;
        private bool _isPopped;
        private bool _interactionEnabled = true;
        private bool _boundsCheckEnabled = true;
        
        public int Answer => _answer;
        public bool IsCorrectAnswer => _isCorrect;
        public bool IsPopped => _isPopped;
        
        /// <summary>
        /// Инициализация шарика с параметрами
        /// </summary>
        public void Initialize(int answer, bool isCorrect, BalloonModeConfig config, Color balloonColor, BalloonDifficultySettings difficultySettings, float customGravity = -1f, float customDrag = -1f)
        {
            _answer = answer;
            _isCorrect = isCorrect;
            _config = config;
            _difficultySettings = difficultySettings;
            _isPopped = false;
            _interactionEnabled = true;
            _answerText.text = answer.ToString();
            _balloonSprite.color = balloonColor;
            SetupPhysics(customGravity, customDrag);
        }
        
        /// <summary>
        /// Настройка физики для шарика
        /// </summary>
        private void SetupPhysics(float customGravity = -1f, float customDrag = -1f)
        {
            // Устанавливаем гравитацию (кастомную или из настроек)
            var gravity = customGravity > 0 ? customGravity : _difficultySettings.Gravity;
            _rigidbody2D.gravityScale = -gravity; // Отрицательная - шары замедляются при полете вверх

            // Устанавливаем сопротивление воздуха
            var drag = customDrag >= 0 ? customDrag : _difficultySettings.Drag;
            _rigidbody2D.drag = drag;

            // Запрещаем вращение
            _rigidbody2D.freezeRotation = true;
        }
        
        /// <summary>
        /// Остановить движение шарика
        /// </summary>
        public void StopMovement()
        {
            if (_rigidbody2D != null)
            {
                _rigidbody2D.velocity = Vector2.zero;
                _rigidbody2D.gravityScale = 0f; // Отключаем гравитацию
            }
        }

        /// <summary>
        /// Плавно исчезнуть за указанное время
        /// </summary>
        public void FadeOut()
        {
            if (_isPopped) return;

            // Воспроизводим анимацию лопания без эффектов
            PlayPopAnimation(false);
        }
        
        /// <summary>
        /// Отключить взаимодействие с шариком
        /// </summary>
        public void DisableInteraction()
        {
            _interactionEnabled = false;
        }

        /// <summary>
        /// Добавить начальную силу к шарику
        /// </summary>
        public void AddInitialForce(Vector2 force)
        {
            _rigidbody2D.AddForce(force, ForceMode2D.Impulse);
        }
        
        /// <summary>
        /// Проверка границ экрана и автоматическое лопание
        /// </summary>
        private void Update()
        {
            // Быстрая проверка на необходимость выполнения
            if (!_boundsCheckEnabled || _isPopped || !_interactionEnabled || this == null || _config == null)
                return;
            
            // Проверяем выход за верхнюю границу экрана (шарик улетел вверх)
            if (UIScreenBounds.IsAboveScreen(_rectTransform, _config.ScreenBoundsPadding))
            {
                // Отключаем дальнейшие проверки границ
                _boundsCheckEnabled = false;
                
                // Уведомляем о достижении верхней границы (шарик улетел вверх)
                OnBalloonReachedTop?.Invoke(this, _answer, _isCorrect);
                
                // Лопаем шарик
                PlayPopAnimation();
            }
        }
        
        
        /// <summary>
        /// Обработчик нажатия на шарик
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isPopped || !_interactionEnabled)
            {
                return;
            }

            // Проверяем, что клик действительно попал в круглую область шарика
            if (!IsClickInsideCircle(eventData))
            {
                return;
            }

            // Уведомляем о нажатии
            OnBalloonTapped?.Invoke(this, _answer, _isCorrect);
        }

        /// <summary>
        /// Проверяет, попал ли клик в круглую область шарика
        /// </summary>
        private bool IsClickInsideCircle(PointerEventData eventData)
        {
            // Получаем позицию клика в локальных координатах
            Vector2 localClickPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _clickDetectionRect,
                eventData.position,
                eventData.pressEventCamera,
                out localClickPosition
            );

            // Получаем радиус шарика с учетом настройки из конфига
            float baseRadius = Mathf.Min(_clickDetectionRect.rect.width, _clickDetectionRect.rect.height) * 0.5f;
            float clickRadius = baseRadius * (_config?.ClickRadiusMultiplier ?? 0.9f);

            // Проверяем расстояние от центра до точки клика
            float distanceFromCenter = Vector2.Distance(Vector2.zero, localClickPosition);

            return distanceFromCenter <= clickRadius;
        }
        
        /// <summary>
        /// Анимация лопания шарика
        /// </summary>
        public void PlayPopAnimation(bool withEffects = true)
        {
            if (_isPopped || this == null) return;

            _isPopped = true;

            // Останавливаем движение
            StopMovement();

            // Вызываем события для эффектов и звука только если нужно
            if (withEffects)
            {
                OnBalloonPopped?.Invoke(this, transform.position, _balloonSprite.color);
                OnPlaySound?.Invoke(_config.PopSoundType);
            }

            PopAnimationAsync().Forget();
        }
        
        /// <summary>
        /// Асинхронная анимация лопания
        /// </summary>
        private async UniTaskVoid PopAnimationAsync()
        {
            try
            {
                float elapsed = 0f;
                Vector3 originalScale = transform.localScale;
                
                while (elapsed < _popAnimationDuration)
                {
                    // Проверяем, что объект еще существует
                    if (this == null || transform == null || gameObject == null)
                        return;
                        
                    elapsed += Time.deltaTime;
                    float progress = elapsed / _popAnimationDuration;
                    float scaleMultiplier = _popScaleCurve.Evaluate(progress);
                    
                    // Проверяем перед использованием transform
                    if (transform != null)
                        transform.localScale = originalScale * scaleMultiplier;
                    
                    // Постепенно делаем прозрачным
                    if (_balloonSprite != null)
                    {
                        var color = _balloonSprite.color;
                        color.a = 1f - progress;
                        _balloonSprite.color = color;
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
                if (this != null && gameObject != null)
                {
                    DestroyBalloon();
                }
            }
            catch (MissingReferenceException)
            {
                // Объект был уничтожен - это нормально, просто выходим
                return;
            }
        }

        /// <summary>
        /// Показать что это был правильный ответ (подсветка)
        /// </summary>
        public void ShowAsCorrectAnswer()
        {
            if (_balloonSprite != null)
            {
                // Делаем шарик зеленым
                _balloonSprite.color = Color.green;
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
        
        private void OnDestroy()
        {
            // Отменяем все анимации
            _animationCancellation?.Cancel();
            _animationCancellation?.Dispose();
        }
    }
}