using System.Threading;
using Cysharp.Threading.Tasks;
using MathGame.Interfaces;
using MathGame.UI.Cards;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MathGame.CardInteractions
{
    /// <summary>
    /// Стратегия взаимодействия для Flash карточек
    /// - Можно тапнуть чтобы перевернуть
    /// - На обратной стороне можно тянуть вверх/вниз
    /// - При отпускании плавно уезжает по направлению
    /// </summary>
    public class FlashInteractionStrategy : ICardInteractionStrategy
    {
        private BaseMathCard _card;
        private FlashCard _flashCard;
        
        // Состояние перетаскивания
        private bool _isDragging = false;
        private Vector2 _startTouchPosition;
        private Vector2 _startLocalPosition;
        private Vector2 _originalCardPosition;
        private float _swipeThreshold = 120f;
        
        public bool CanFlip => true;
        public bool CanDrag => _card != null && _card.IsFlipped;
        
        public void Initialize(MonoBehaviour cardComponent)
        {
            _card = cardComponent as BaseMathCard;
            _flashCard = cardComponent as FlashCard;
            
            if (_card == null)
            {
                Debug.LogError("FlashInteractionStrategy: cardComponent должен быть BaseMathCard");
            }
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (_card == null) return;
            
            _startTouchPosition = eventData.position;
            _isDragging = false;
            
            // Сохраняем изначальную позицию для перетаскивания
            if (CanDrag && _card.CardContainer != null)
            {
                _originalCardPosition = _card.CardContainer.anchoredPosition;
                _card.OriginalCardPosition = _originalCardPosition;
                
                // Также сохраняем начальную позицию в local space
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _card.CardContainer.parent as RectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out _startLocalPosition
                );
            }
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            if (_card == null) return;
            
            if (_isDragging && CanDrag)
            {
                // Проверяем, достаточно ли далеко потянули для свайпа
                HandleSwipeEnd(eventData.position);
            }
            else if (!_isDragging)
            {
                // Обычный клик
                OnCardClicked();
            }
            
            _isDragging = false;
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (_card == null || !CanDrag) return;
            
            _isDragging = true;
            
            // Вычисляем расстояние перетаскивания по Y оси
            float dragDistanceY = eventData.position.y - _startTouchPosition.y;
            
            // Конвертируем screen space в local space для RectTransform
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _card.CardContainer.parent as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint
            );
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _card.CardContainer.parent as RectTransform,
                _startTouchPosition,
                eventData.pressEventCamera,
                out Vector2 startLocalPoint
            );
            
            // Обновляем позицию карточки только по Y оси
            Vector2 newPosition = _originalCardPosition;
            newPosition.y += (localPoint.y - startLocalPoint.y);
            _card.CardContainer.anchoredPosition = newPosition;
            
            // Визуальная обратная связь для FlashCard
            if (_flashCard != null)
            {
                _flashCard.OnDragFeedback(dragDistanceY);
            }
        }
        
        public void OnCardClicked()
        {
            if (_card == null) return;
            
            // Переворачиваем карточку при клике
            if (CanFlip && !_card.IsFlipping && !_card.IsPlayingSwipeAnimation && !_card.IsPlayingEntryAnimation)
            {
                _card.FlipCard();
            }
        }
        
        public void OnSwipeUpDetected()
        {
            if (_card == null || !_card.IsFlipped)
            {
                return;
            }
            
            // Сохраняем правильный ответ для отложенной обработки
            int correctAnswer = _card.CurrentQuestion.CorrectAnswer;
            
            // Показываем визуальную обратную связь
            if (_flashCard != null)
            {
                _flashCard.ShowRememberedFeedback();
            }
            
            // Подписываемся на событие завершения анимации для выбора ответа
            // НЕ переопределяем OnSwipeUp, а добавляем к нему
            System.Action originalOnSwipeUp = _card.OnSwipeUp;
            _card.OnSwipeUp = () => {
                _card.SelectAnswer(correctAnswer);
                
                // Вызываем оригинальные обработчики
                originalOnSwipeUp?.Invoke();
            };
            
            // Запускаем анимацию исчезновения вверх
            _card.PlaySwipeUpAnimationAsync().Forget();
        }
        
        public void OnSwipeDownDetected()
        {
            if (_card == null || !_card.IsFlipped) return;
            
            // Показываем визуальную обратную связь
            if (_flashCard != null)
            {
                _flashCard.ShowNotRememberedFeedback();
            }
            
            // Подписываемся на событие завершения анимации для выбора ответа
            // НЕ переопределяем OnSwipeDown, а добавляем к нему
            System.Action originalOnSwipeDown = _card.OnSwipeDown;
            _card.OnSwipeDown = () => {
                _card.SelectAnswer(-1); // Неправильный ответ
                
                // Вызываем оригинальные обработчики
                originalOnSwipeDown?.Invoke();
            };
            
            // Запускаем анимацию исчезновения вниз
            _card.PlaySwipeDownAnimationAsync().Forget();
        }
        
        private void HandleSwipeEnd(Vector2 endPosition)
        {
            // Используем простое вычисление расстояния в screen space
            float swipeDistanceY = endPosition.y - _startTouchPosition.y;
            
            if (Mathf.Abs(swipeDistanceY) >= _swipeThreshold)
            {
                if (swipeDistanceY > 0)
                {
                    OnSwipeUpDetected();
                }
                else
                {
                    OnSwipeDownDetected();
                }
            }
            else
            {
                // Не достигли порога - возвращаем карточку на место
                ReturnCardToOriginalPosition();
            }
        }
        
        private void ReturnCardToOriginalPosition()
        {
            if (_card?.CardContainer != null)
            {
                _card.OriginalCardPosition = _originalCardPosition; // Синхронизируем
                AnimateCardReturnAsync().Forget();
            }
            
            // Сбрасываем визуальную обратную связь
            if (_flashCard != null)
            {
                _flashCard.OnDragFeedback(0f);
            }
        }
        
        private async UniTask AnimateCardReturnAsync(CancellationToken cancellationToken = default)
        {
            Vector2 startPos = _card.CardContainer.anchoredPosition;
            Vector2 targetPos = _originalCardPosition;
            float elapsedTime = 0f;
            float duration = 0.3f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                _card.CardContainer.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                await UniTask.Yield(cancellationToken);
            }
            
            _card.CardContainer.anchoredPosition = targetPos;
        }
        
        public void Cleanup()
        {
            _card = null;
            _flashCard = null;
        }
    }
}