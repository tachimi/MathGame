using System;
using System.Collections;
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
        
        [Header("Balloon Components")]
        [SerializeField] private Image _balloonImage;
        [SerializeField] private TextMeshProUGUI _answerText;
        [SerializeField] private RectTransform _rectTransform;
        
        [Header("Animation Settings")]
        [SerializeField] private float _popAnimationDuration = 0.5f;
        [SerializeField] private AnimationCurve _popScaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        [SerializeField] private AnimationCurve _floatCurve = AnimationCurve.Linear(0, 0, 1, 1);
        
        private int _answer;
        private bool _isCorrect;
        private float _speed;
        private float _lifetime;
        private bool _isPopped;
        private bool _isMoving;
        
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private float _moveStartTime;
        
        public int Answer => _answer;
        public bool IsCorrect => _isCorrect;
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
            // Получаем или создаем RectTransform
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();
            
            if (_rectTransform == null)
                _rectTransform = gameObject.AddComponent<RectTransform>();
            
            // Создаем Image если нет
            if (_balloonImage == null)
            {
                _balloonImage = gameObject.GetComponent<Image>();
                if (_balloonImage == null)
                {
                    _balloonImage = gameObject.AddComponent<Image>();
                    // TODO: Назначить спрайт шарика из ресурсов
                    _balloonImage.color = GetRandomBalloonColor();
                }
            }
            
            // Создаем TextMeshPro для ответа
            if (_answerText == null)
            {
                var textObject = new GameObject("AnswerText");
                textObject.transform.SetParent(transform, false);
                
                _answerText = textObject.AddComponent<TextMeshProUGUI>();
                _answerText.text = "?";
                _answerText.fontSize = 24;
                _answerText.color = Color.white;
                _answerText.alignment = TextAlignmentOptions.Center;
                
                // Настраиваем RectTransform для текста
                var textRect = _answerText.rectTransform;
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
                textRect.anchoredPosition = Vector2.zero;
            }
            
            // Настраиваем размер шарика
            _rectTransform.sizeDelta = new Vector2(80, 80);
        }
        
        /// <summary>
        /// Инициализация шарика с параметрами
        /// </summary>
        public void Initialize(int answer, bool isCorrect, float speed, float lifetime)
        {
            _answer = answer;
            _isCorrect = isCorrect;
            _speed = speed;
            _lifetime = lifetime;
            _isPopped = false;
            _isMoving = false;
            
            // Обновляем текст ответа
            if (_answerText != null)
                _answerText.text = answer.ToString();
            
            // Устанавливаем цвет шарика (правильный ответ может быть особенным)
            if (_balloonImage != null)
            {
                _balloonImage.color = isCorrect ? GetCorrectAnswerColor() : GetRandomBalloonColor();
            }
            
            // Устанавливаем начальную позицию (снизу экрана)
            SetupInitialPosition();
            
            // Запускаем движение
            StartMovement();
            
            Debug.Log($"BalloonAnswer: Инициализирован шарик с ответом {answer}, правильный: {isCorrect}");
        }
        
        /// <summary>
        /// Настройка начальной позиции
        /// </summary>
        private void SetupInitialPosition()
        {
            // Случайная X позиция в пределах экрана
            var canvasRect = GetCanvasRect();
            float randomX = UnityEngine.Random.Range(-canvasRect.width * 0.4f, canvasRect.width * 0.4f);
            
            _startPosition = new Vector3(randomX, -canvasRect.height * 0.6f, 0);
            _targetPosition = new Vector3(randomX, canvasRect.height * 0.6f, 0);
            
            _rectTransform.localPosition = _startPosition;
        }
        
        /// <summary>
        /// Запуск движения шарика
        /// </summary>
        private void StartMovement()
        {
            _isMoving = true;
            _moveStartTime = Time.time;
            
            // Запускаем корутину движения
            StartCoroutine(MoveBalloonCoroutine());
            
            // Запускаем корутину автоуничтожения
            StartCoroutine(AutoDestroyCoroutine());
        }
        
        /// <summary>
        /// Корутина движения шарика
        /// </summary>
        private IEnumerator MoveBalloonCoroutine()
        {
            float journey = 0f;
            float journeyLength = Vector3.Distance(_startPosition, _targetPosition);
            float journeyTime = journeyLength / _speed;
            
            while (journey <= journeyTime && !_isPopped && _isMoving)
            {
                float fractionOfJourney = journey / journeyTime;
                float easedProgress = _floatCurve.Evaluate(fractionOfJourney);
                
                _rectTransform.localPosition = Vector3.Lerp(_startPosition, _targetPosition, easedProgress);
                
                journey += Time.deltaTime;
                yield return null;
            }
            
            // Шарик долетел до верха - уничтожаем
            if (!_isPopped)
            {
                DestroyBalloon();
            }
        }
        
        /// <summary>
        /// Корутина автоуничтожения через время
        /// </summary>
        private IEnumerator AutoDestroyCoroutine()
        {
            yield return new WaitForSeconds(_lifetime);
            
            if (!_isPopped)
            {
                DestroyBalloon();
            }
        }
        
        /// <summary>
        /// Обработчик нажатия на шарик
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isPopped) return;
            
            Debug.Log($"BalloonAnswer: Шарик нажат - ответ {_answer}");
            
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
            
            StartCoroutine(PopAnimationCoroutine());
        }
        
        /// <summary>
        /// Корутина анимации лопания
        /// </summary>
        private IEnumerator PopAnimationCoroutine()
        {
            float elapsed = 0f;
            Vector3 originalScale = transform.localScale;
            
            while (elapsed < _popAnimationDuration)
            {
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
                
                yield return null;
            }
            
            // Уничтожаем после анимации
            DestroyBalloon();
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
        /// Получить случайный цвет шарика
        /// </summary>
        private Color GetRandomBalloonColor()
        {
            Color[] balloonColors = {
                Color.red, Color.blue, Color.green, Color.yellow,
                Color.magenta, Color.cyan, new Color(1f, 0.5f, 0f) // оранжевый
            };
            
            return balloonColors[UnityEngine.Random.Range(0, balloonColors.Length)];
        }
        
        /// <summary>
        /// Получить цвет для правильного ответа
        /// </summary>
        private Color GetCorrectAnswerColor()
        {
            return Color.green; // Правильные ответы всегда зеленые
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
            // Очистка подписок происходит автоматически при уничтожении
        }
    }
}