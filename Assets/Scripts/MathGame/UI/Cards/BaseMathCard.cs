using System;
using System.Collections;
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

        [Header("Base Card Components")]
        [SerializeField] protected RectTransform _cardContainer;
        [SerializeField] protected GameObject _frontSide;
        [SerializeField] protected GameObject _backSide;
        [SerializeField] protected TextMeshProUGUI _questionText;

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

        [Header("Swipe Settings")]
        [SerializeField] protected float _swipeThreshold = 50f;
        [SerializeField] protected float _swipeTimeThreshold = 0.5f;

        protected Question _currentQuestion;
        protected bool _isFlipped = false;
        protected bool _isAnswered = false;
        protected int _selectedAnswer = -1;
        protected bool _isPlayingSwipeAnimation = false;
        protected bool _isPlayingEntryAnimation = false;
        protected bool _isFlipping = false;

        // Swipe detection
        private Vector2 _startTouchPosition;
        private float _touchStartTime;
        private bool _isDragging = false;

        public bool IsAnswered => _isAnswered;
        public bool IsFlipped => _isFlipped;
        public Question CurrentQuestion => _currentQuestion;

        protected virtual void Awake()
        {
            SetupCard();
        }

        /// <summary>
        /// Настройка карточки - переопределяется в наследниках
        /// </summary>
        protected abstract void SetupCard();

        /// <summary>
        /// Установить вопрос для карточки
        /// </summary>
        public virtual void SetQuestion(Question question)
        {
            _currentQuestion = question;
            _isAnswered = false;
            _isFlipped = false;
            _selectedAnswer = -1;
            _isPlayingSwipeAnimation = false;
            _isPlayingEntryAnimation = false;
            _isFlipping = false;

            // Сбрасываем позицию карточки (начинаем справа)
            ResetCardPosition();

            // Обновляем текст вопроса
            if (_questionText != null)
                _questionText.text = question.GetQuestionDisplay();

            // Настраиваем специфичные для режима компоненты
            SetupModeSpecificComponents();

            // Показываем лицевую сторону
            ShowFrontSide();
            
            // Запускаем анимацию появления
            StartCoroutine(PlayEntryAnimation());
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
            return true;
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

            // Активируем компоненты обратной стороны
            ActivateBackSideComponents();
        }

        /// <summary>
        /// Активировать компоненты обратной стороны - переопределяется в наследниках
        /// </summary>
        protected virtual void ActivateBackSideComponents()
        {
            // Базовая реализация - ничего не делает
        }

        /// <summary>
        /// Перевернуть карточку
        /// </summary>
        public virtual void FlipCard()
        {
            if (!CanFlip() || _isFlipping || _isPlayingSwipeAnimation || _isPlayingEntryAnimation) return;

            StartCoroutine(FlipCardAnimation());
        }

        /// <summary>
        /// Анимация переворота карточки
        /// </summary>
        protected virtual IEnumerator FlipCardAnimation()
        {
            _isFlipping = true; // Блокируем дополнительные клики
            
            float elapsed = 0f;
            Vector3 originalScale = _cardContainer.localScale;

            // Сжатие по X
            while (elapsed < _flipDuration / 2)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / (_flipDuration / 2);
                float scaleX = Mathf.Lerp(1f, 0f, _flipCurve.Evaluate(progress));
                _cardContainer.localScale = new Vector3(scaleX, originalScale.y, originalScale.z);
                yield return null;
            }

            // Переключаем сторону
            if (_isFlipped)
                ShowFrontSide();
            else
                ShowBackSide();

            elapsed = 0f;

            // Восстановление по X
            while (elapsed < _flipDuration / 2)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / (_flipDuration / 2);
                float scaleX = Mathf.Lerp(0f, 1f, _flipCurve.Evaluate(progress));
                _cardContainer.localScale = new Vector3(scaleX, originalScale.y, originalScale.z);
                yield return null;
            }

            _cardContainer.localScale = originalScale;
            _isFlipping = false; // Разблокируем клики
        }

        #region Input Handlers

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            _startTouchPosition = eventData.position;
            _touchStartTime = Time.time;
            _isDragging = false;
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (_isDragging)
            {
                HandleSwipe(eventData.position);
            }
            else
            {
                // Клик по карточке
                OnCardClicked();
            }
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            _isDragging = true;
        }

        /// <summary>
        /// Обработка клика по карточке - переопределяется в наследниках
        /// </summary>
        protected virtual void OnCardClicked()
        {
            if (_isPlayingEntryAnimation || _isPlayingSwipeAnimation || _isFlipping) return;
            
            FlipCard();
        }

        /// <summary>
        /// Обработка свайпа
        /// </summary>
        protected virtual void HandleSwipe(Vector2 endPosition)
        {
            Vector2 swipeVector = endPosition - _startTouchPosition;
            float swipeTime = Time.time - _touchStartTime;

            if (swipeVector.magnitude < _swipeThreshold || swipeTime > _swipeTimeThreshold)
                return;

            if (Mathf.Abs(swipeVector.y) > Mathf.Abs(swipeVector.x))
            {
                if (swipeVector.y > 0)
                {
                    OnSwipeUpDetected();
                }
                else
                {
                    OnSwipeDownDetected();
                }
            }
        }

        /// <summary>
        /// Обработка свайпа вверх - переопределяется в наследниках
        /// </summary>
        protected virtual void OnSwipeUpDetected()
        {
            if (_isPlayingSwipeAnimation || _isPlayingEntryAnimation) return;
            
            StartCoroutine(PlaySwipeUpAnimation());
        }

        /// <summary>
        /// Обработка свайпа вниз - переопределяется в наследниках
        /// </summary>
        protected virtual void OnSwipeDownDetected()
        {
            if (_isPlayingSwipeAnimation || _isPlayingEntryAnimation) return;
            
            StartCoroutine(PlaySwipeDownAnimation());
        }
        
        /// <summary>
        /// Анимация свайпа вверх - улетает за верхний край экрана
        /// </summary>
        protected virtual IEnumerator PlaySwipeUpAnimation()
        {
            _isPlayingSwipeAnimation = true;
            
            Vector3 originalPosition = _cardContainer.localPosition;
            Vector3 exitPosition = originalPosition + Vector3.up * (_swipeUpDistance + 200f); // Дополнительное расстояние за экран
            
            float elapsed = 0f;
            
            // Одна фаза: прямо вверх за экран
            while (elapsed < _swipeAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / _swipeAnimationDuration;
                float easedProgress = _swipeAnimationCurve.Evaluate(progress);
                
                _cardContainer.localPosition = Vector3.Lerp(originalPosition, exitPosition, easedProgress);
                
                yield return null;
            }
            
            _cardContainer.localPosition = exitPosition;
            
            // Вызываем событие после завершения анимации
            OnSwipeUp?.Invoke();
            
            _isPlayingSwipeAnimation = false;
        }
        
        /// <summary>
        /// Анимация свайпа вниз - улетает за нижний край экрана
        /// </summary>
        protected virtual IEnumerator PlaySwipeDownAnimation()
        {
            _isPlayingSwipeAnimation = true;
            
            Vector3 originalPosition = _cardContainer.localPosition;
            Vector3 exitPosition = originalPosition + Vector3.down * (_swipeDownDistance + 200f); // Дополнительное расстояние за экран
            
            float elapsed = 0f;
            
            // Одна фаза: прямо вниз за экран
            while (elapsed < _swipeAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / _swipeAnimationDuration;
                float easedProgress = _swipeAnimationCurve.Evaluate(progress);
                
                _cardContainer.localPosition = Vector3.Lerp(originalPosition, exitPosition, easedProgress);
                
                yield return null;
            }
            
            _cardContainer.localPosition = exitPosition;
            
            // Вызываем событие после завершения анимации
            OnSwipeDown?.Invoke();
            
            _isPlayingSwipeAnimation = false;
        }
        
        /// <summary>
        /// Анимация появления карточки справа
        /// </summary>
        public virtual IEnumerator PlayEntryAnimation()
        {
            _isPlayingEntryAnimation = true;
            
            if (_cardContainer != null)
            {
                // Начальная позиция справа
                Vector3 startPosition = Vector3.right * _entryFromRightDistance;
                Vector3 targetPosition = Vector3.zero;
                
                _cardContainer.localPosition = startPosition;
                
                float elapsed = 0f;
                
                while (elapsed < _entryAnimationDuration)
                {
                    elapsed += Time.deltaTime;
                    float progress = elapsed / _entryAnimationDuration;
                    float easedProgress = _entryAnimationCurve.Evaluate(progress);
                    
                    _cardContainer.localPosition = Vector3.Lerp(startPosition, targetPosition, easedProgress);
                    
                    yield return null;
                }
                
                _cardContainer.localPosition = targetPosition;
            }
            
            _isPlayingEntryAnimation = false;
        }
        
        /// <summary>
        /// Сброс позиции карточки для нового вопроса
        /// </summary>
        protected virtual void ResetCardPosition()
        {
            if (_cardContainer != null)
            {
                // Начинаем с позиции справа, анимация появления переместит в центр
                _cardContainer.localPosition = Vector3.right * _entryFromRightDistance;
            }
        }

        #endregion

        #region Answer Handling

        /// <summary>
        /// Выбрать ответ
        /// </summary>
        protected virtual void SelectAnswer(int answer)
        {
            if (_isAnswered) return;

            _isAnswered = true;
            _selectedAnswer = answer;
            OnAnswerSelected?.Invoke(answer);
        }

        /// <summary>
        /// Сбросить состояние ответа
        /// </summary>
        protected virtual void ResetAnswerState()
        {
            _isAnswered = false;
            _selectedAnswer = -1;
        }

        #endregion

        /// <summary>
        /// Очистка ресурсов - переопределяется в наследниках
        /// </summary>
        protected virtual void OnDestroy()
        {
            // Базовая очистка
        }
    }
}