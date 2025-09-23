using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MathGame.CardInteractions;
using MathGame.Interfaces;
using MathGame.Models;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MathGame.UI.Cards
{
    /// <summary>
    /// Базовый класс для всех математических карточек
    /// Содержит общую логику анимации, жестов и базового UI
    /// </summary>
    public abstract class BaseMathCard : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public event Action<int> OnAnswerSelected;
        public Action OnSwipeUp;
        public Action OnSwipeDown;
        
        public bool IsFlipped => _isFlipped;
        public Question CurrentQuestion => _currentQuestion;
        public RectTransform CardContainer => _cardContainer;
        public bool IsFlipping => _isFlipping;
        public bool IsPlayingSwipeAnimation => _isPlayingSwipeAnimation;
        public bool IsPlayingEntryAnimation => _isPlayingEntryAnimation;
        public Vector2 OriginalCardPosition { get => _originalCardPosition; set => _originalCardPosition = value; }

        [Header("Base Card Components")]
        [SerializeField] protected RectTransform _cardContainer;
        [SerializeField] protected GameObject _frontSide;
        [SerializeField] protected GameObject _backSide;
        [SerializeField] protected TextMeshProUGUI _questionDisplay;

        [Header("Question Display Components")]
        [SerializeField] protected TextMeshProUGUI _inputDisplay; // Поле ввода/знак вопроса

        [Header("Animation Settings")]
        [SerializeField] protected float _flipDuration = 0.5f;
        [SerializeField] protected AnimationCurve _flipCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Swipe Animation Settings")]
        [SerializeField] protected float _swipeAnimationDuration = 0.3f;
        [SerializeField] protected AnimationCurve _swipeAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] protected float _swipeUpDistance = 100f;
        [SerializeField] protected float _swipeDownDistance = 100f;

        [Header("Card Entry Animation Settings")]
        [SerializeField] protected float _entryAnimationDuration = 0.5f;
        [SerializeField] protected AnimationCurve _entryAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] protected float _entryFromRightDistance = 300f;

        protected Question _currentQuestion;
        protected Vector2 _originalCardPosition;
        protected ICardInteractionStrategy _interactionStrategy;
        protected CancellationTokenSource _cancellationTokenSource;
        protected bool _isFlipped = false;
        protected bool _isAnswered = false;
        protected bool _isPlayingSwipeAnimation = false;
        protected bool _isPlayingEntryAnimation = false;
        protected bool _isFlipping = false;


        protected virtual void Awake()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            SetupCard();
            InitializeInteractionStrategy();
        }

        /// <summary>
        /// Настройка карточки - переопределяется в наследниках
        /// </summary>
        protected abstract void SetupCard();
        
        /// <summary>
        /// Инициализация стратегии взаимодействия - переопределяется в наследниках
        /// </summary>
        protected abstract void InitializeInteractionStrategy();

        /// <summary>
        /// Установить вопрос для карточки
        /// </summary>
        public virtual void SetQuestion(Question question)
        {
            _currentQuestion = question;
            _isAnswered = false;
            _isFlipped = false;
            _isPlayingSwipeAnimation = false;
            _isPlayingEntryAnimation = false;
            _isFlipping = false;

            // Сбрасываем позицию карточки (начинаем справа)
            ResetCardPosition();

            // Обновляем отображение вопроса и поля ввода
            UpdateQuestionDisplay();
            UpdateInputDisplay();

            // Настраиваем специфичные для режима компоненты
            SetupModeSpecificComponents();

            // Показываем лицевую сторону
            ShowFrontSide();

            // Запускаем анимацию появления
            PlayEntryAnimationAsync(_cancellationTokenSource.Token).Forget();
        }

        /// <summary>
        /// Настройка компонентов специфичных для режима - переопределяется в наследниках
        /// </summary>
        protected abstract void SetupModeSpecificComponents();

        /// <summary>
        /// Проверить, можно ли переворачивать карточку
        /// </summary>
        protected virtual bool CanFlip()
        {
            return _interactionStrategy?.CanFlip ?? true;
        }

        /// <summary>
        /// Показать лицевую сторону карточки
        /// </summary>
        protected virtual void ShowFrontSide()
        {
            if (_frontSide != null) _frontSide.SetActive(true);
            if (_backSide != null) _backSide.SetActive(false);
            _isFlipped = false;
        }

        /// <summary>
        /// Показать обратную сторону карточки
        /// </summary>
        protected virtual void ShowBackSide()
        {
            if (_frontSide != null) _frontSide.SetActive(false);
            if (_backSide != null) _backSide.SetActive(true);
            _isFlipped = true;
        }

        /// <summary>
        /// Перевернуть карточку
        /// </summary>
        public virtual void FlipCard()
        {
            if (!CanFlip() || _isFlipping || _isPlayingSwipeAnimation || _isPlayingEntryAnimation)
            {
                return;
            }

            FlipCardAnimationAsync(_cancellationTokenSource.Token).Forget();
        }

        /// <summary>
        /// Анимация переворота карточки
        /// </summary>
        protected virtual async UniTask FlipCardAnimationAsync(CancellationToken cancellationToken = default)
        {
            if (_cardContainer == null) return;
            
            _isFlipping = true;

            try
            {
                float elapsed = 0f;
                Vector3 originalScale = _cardContainer.localScale;
                float halfDuration = _flipDuration / 2;

                // Сжатие по X
                while (elapsed < halfDuration && _cardContainer != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    elapsed += Time.deltaTime;
                    float progress = elapsed / halfDuration;
                    float scaleX = Mathf.Lerp(1f, 0f, _flipCurve.Evaluate(progress));
                    
                    if (_cardContainer != null)
                        _cardContainer.localScale = new Vector3(scaleX, originalScale.y, originalScale.z);
                        
                    await UniTask.Yield(cancellationToken);
                }

                // Переключаем сторону
                if (_isFlipped)
                    ShowFrontSide();
                else
                    ShowBackSide();

                elapsed = 0f;

                // Восстановление по X
                while (elapsed < halfDuration && _cardContainer != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    elapsed += Time.deltaTime;
                    float progress = elapsed / halfDuration;
                    float scaleX = Mathf.Lerp(0f, 1f, _flipCurve.Evaluate(progress));
                    
                    if (_cardContainer != null)
                        _cardContainer.localScale = new Vector3(scaleX, originalScale.y, originalScale.z);
                        
                    await UniTask.Yield(cancellationToken);
                }

                if (_cardContainer != null)
                    _cardContainer.localScale = originalScale;
            }
            finally
            {
                _isFlipping = false;
            }
        }

        #region Input Handlers

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            _interactionStrategy?.OnPointerDown(eventData);
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            _interactionStrategy?.OnPointerUp(eventData);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            _interactionStrategy?.OnDrag(eventData);
        }

        /// <summary>
        /// Анимация свайпа вверх - улетает за верхний край экрана
        /// </summary>
        public async UniTask PlaySwipeUpAnimationAsync(CancellationToken cancellationToken = default)
        {
            if (_cardContainer == null) 
            {
                return;
            }
            
            _isPlayingSwipeAnimation = true;

            try
            {
                Vector2 startPosition = _cardContainer.anchoredPosition;
                Vector2 exitPosition = startPosition + Vector2.up * _swipeUpDistance; // Дополнительное расстояние за экран

                float elapsed = 0f;

                // Одна фаза: прямо вверх за экран
                while (elapsed < _swipeAnimationDuration && _cardContainer != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    elapsed += Time.deltaTime;
                    float progress = elapsed / _swipeAnimationDuration;
                    float easedProgress = _swipeAnimationCurve.Evaluate(progress);

                    if (_cardContainer != null)
                    {
                        Vector2 currentPos = Vector2.Lerp(startPosition, exitPosition, easedProgress);
                        _cardContainer.anchoredPosition = currentPos;
                    }

                    await UniTask.Yield(cancellationToken);
                }

                if (_cardContainer != null)
                {
                    _cardContainer.anchoredPosition = exitPosition;
                }
                
                OnSwipeUp?.Invoke();
            }
            finally
            {
                _isPlayingSwipeAnimation = false;
            }
        }

        /// <summary>
        /// Анимация свайпа вниз - улетает за нижний край экрана
        /// </summary>
        public async UniTask PlaySwipeDownAnimationAsync(CancellationToken cancellationToken = default)
        {
            if (_cardContainer == null) return;
            
            _isPlayingSwipeAnimation = true;

            try
            {
                // Используем текущую anchoredPosition как стартовую точку (после перетаскивания)
                Vector2 startPosition = _cardContainer.anchoredPosition;
                Vector2 exitPosition =
                    startPosition + Vector2.down * _swipeDownDistance; // Дополнительное расстояние за экран

                float elapsed = 0f;

                // Одна фаза: прямо вниз за экран
                while (elapsed < _swipeAnimationDuration && _cardContainer != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    elapsed += Time.deltaTime;
                    float progress = elapsed / _swipeAnimationDuration;
                    float easedProgress = _swipeAnimationCurve.Evaluate(progress);

                    if (_cardContainer != null)
                        _cardContainer.anchoredPosition = Vector2.Lerp(startPosition, exitPosition, easedProgress);

                    await UniTask.Yield(cancellationToken);
                }

                if (_cardContainer != null)
                    _cardContainer.anchoredPosition = exitPosition;

                // Вызываем событие после завершения анимации
                OnSwipeDown?.Invoke();
            }
            finally
            {
                _isPlayingSwipeAnimation = false;
            }
        }

        /// <summary>
        /// Анимация появления карточки справа
        /// </summary>
        private async UniTask PlayEntryAnimationAsync(CancellationToken cancellationToken = default)
        {
            _isPlayingEntryAnimation = true;

            try
            {
                if (_cardContainer != null)
                {
                    // Начальная позиция справа
                    Vector2 startPosition = Vector2.right * _entryFromRightDistance;
                    Vector2 targetPosition = Vector2.zero;

                    _cardContainer.anchoredPosition = startPosition;

                    float elapsed = 0f;

                    while (elapsed < _entryAnimationDuration && _cardContainer != null)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        
                        elapsed += Time.deltaTime;
                        float progress = elapsed / _entryAnimationDuration;
                        float easedProgress = _entryAnimationCurve.Evaluate(progress);

                        if (_cardContainer != null)
                            _cardContainer.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, easedProgress);

                        await UniTask.Yield(cancellationToken);
                    }

                    if (_cardContainer != null)
                    {
                        _cardContainer.anchoredPosition = targetPosition;
                        // Обновляем оригинальную позицию для системы перетаскивания
                        _originalCardPosition = targetPosition;
                    }
                }
            }
            finally
            {
                _isPlayingEntryAnimation = false;
            }
        }

        /// <summary>
        /// Сброс позиции карточки для нового вопроса
        /// </summary>
        protected void ResetCardPosition()
        {
            if (_cardContainer != null)
            {
                // Начинаем с позиции справа, анимация появления переместит в центр
                _cardContainer.anchoredPosition = Vector2.right * _entryFromRightDistance;

                // Сохраняем эту позицию как оригинальную для системы перетаскивания
                _originalCardPosition = _cardContainer.anchoredPosition;
            }
        }

        #endregion

        #region Answer Handling

        /// <summary>
        /// Выбрать ответ
        /// </summary>
        public void SelectAnswer(int answer)
        {
            if (_isAnswered) return;

            _isAnswered = true;
            OnAnswerSelected?.Invoke(answer);
        }

        #endregion

        #region Question Display Methods

        /// <summary>
        /// Обновить отображение примера
        /// </summary>
        protected void UpdateQuestionDisplay()
        {
            if (_questionDisplay != null && _currentQuestion != null)
            {
                // Форматируем вопрос: первое число на первой строке, операция и второе число на второй
                string formattedQuestion =
                    $"{_currentQuestion.FirstNumber}\n{_currentQuestion.GetOperationSymbol()} {_currentQuestion.SecondNumber}";
                _questionDisplay.text = formattedQuestion;
            }
        }

        /// <summary>
        /// Обновить отображение поля ввода
        /// Для TextInput показывает введенный текст или "?"
        /// Для других режимов просто показывает "?"
        /// </summary>
        protected virtual void UpdateInputDisplay()
        {
            if (_inputDisplay != null)
            {
                _inputDisplay.text = "?";
            }
        }

        #endregion

        /// <summary>
        /// Очистка ресурсов - переопределяется в наследниках
        /// </summary>
        protected virtual void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _interactionStrategy?.Cleanup();
        }
    }
}