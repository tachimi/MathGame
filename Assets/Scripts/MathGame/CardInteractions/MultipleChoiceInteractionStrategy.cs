using Cysharp.Threading.Tasks;
using MathGame.Interfaces;
using MathGame.UI.Cards;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MathGame.CardInteractions
{
    /// <summary>
    /// Стратегия взаимодействия для карточек с выбором ответа
    /// - Можно тапнуть чтобы развернуть
    /// - Не имеет событий Drag
    /// - Если детектили свайп ВВЕРХ, она уезжает
    /// </summary>
    public class MultipleChoiceInteractionStrategy : ICardInteractionStrategy
    {
        public bool CanFlip => !IsFlipBlocked;
        public bool CanDrag => false; // Drag события отключены
        public bool IsFlipBlocked { get; set; } = false;
        public bool IsSwipeBlocked { get; set; } = false;

        private BaseMathCard _card;
        private Vector2 _startTouchPosition;
        private float _touchStartTime;
        private float _swipeThreshold = 50f;
        private float _swipeTimeThreshold = 0.5f;
        
        public void Initialize(MonoBehaviour cardComponent)
        {
            _card = cardComponent as BaseMathCard;
            
            if (_card == null)
            {
                Debug.LogError("MultipleChoiceInteractionStrategy: cardComponent должен быть BaseMathCard");
            }
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (_card == null) return;
            
            _startTouchPosition = eventData.position;
            _touchStartTime = Time.time;
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            if (_card == null) return;
            
            // Проверяем, был ли это свайп вверх
            if (IsSwipeUp(eventData.position))
            {
                OnSwipeUpDetected();
            }
            else
            {
                // Обычный клик
                OnCardClicked();
            }
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            // MultipleChoice карточка не поддерживает перетаскивание
            // Drag события полностью отключены
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
            if (_card == null || !_card.IsFlipped || IsSwipeBlocked) return;

            // MultipleChoice: свайп вверх работает только на обратной стороне
            var multipleChoiceCard = _card as MultipleChoiceCard;
            if (multipleChoiceCard != null)
            {
                // Если не было попытки ответить - засчитываем неправильный ответ
                if (!multipleChoiceCard._firstAttemptUsed)
                {
                    _card.SelectAnswer(-1);
                }
            }


            // Запускаем анимацию исчезновения вверх
            _card.PlaySwipeUpAnimationAsync().Forget();
        }
        
        public void OnSwipeDownDetected()
        {
            // MultipleChoice карточка не поддерживает свайп вниз
            // Она может уехать только вверх
        }
        
        private bool IsSwipeUp(Vector2 endPosition)
        {
            Vector2 swipeVector = endPosition - _startTouchPosition;
            float swipeTime = Time.time - _touchStartTime;
            
            // Проверяем, что свайп достаточно длинный, быстрый и направлен вверх
            bool isLongEnough = swipeVector.magnitude >= _swipeThreshold;
            bool isFastEnough = swipeTime <= _swipeTimeThreshold;
            bool isUpward = swipeVector.y > 0 && Mathf.Abs(swipeVector.y) > Mathf.Abs(swipeVector.x);
            
            return isLongEnough && isFastEnough && isUpward;
        }
        

        public void Cleanup()
        {
            _card = null;
        }
    }
}