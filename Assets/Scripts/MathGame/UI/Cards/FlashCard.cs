using MathGame.CardInteractions;
using MathGame.Models;
using TMPro;
using UnityEngine;

namespace MathGame.UI.Cards
{
    /// <summary>
    /// Карточка для Flash режима
    /// Лицевая сторона: вопрос, обратная сторона: правильный ответ
    /// Управление свайпами: вверх = запомнил, вниз = не запомнил
    /// </summary>
    public class FlashCard : BaseMathCard
    {
        [Header("Flash Components")] [SerializeField]
        private TextMeshProUGUI _answerText;

        [Header("Visual Feedback")] [SerializeField]
        private Color _rememberedColor = Color.green;

        [SerializeField] private Color _notRememberedColor = Color.red;

        [SerializeField]
        private float _maxDragDistance = 100f; // Максимальное расстояние для полной интенсивности цвета

        protected override void SetupCard()
        {
            // Настройка Flash карточки
        }
        
        protected override void InitializeInteractionStrategy()
        {
            _interactionStrategy = new FlashInteractionStrategy();
            _interactionStrategy.Initialize(this);
        }

        protected override void SetupModeSpecificComponents()
        {
            // Устанавливаем правильный ответ для показа на обратной стороне
            if (_answerText != null)
            {
                _answerText.text = _currentQuestion.CorrectAnswer.ToString();
                _answerText.color = Color.black; // Сбрасываем цвет
            }
        }

        public override void OnDragFeedback(float dragDistance)
        {
            // Применяем визуальную обратную связь только если карточка перевернута и есть текст ответа
            if (!_isFlipped || _answerText == null) return;

            // Вычисляем интенсивность цвета на основе расстояния перетаскивания
            float intensity = Mathf.Clamp01(Mathf.Abs(dragDistance) / _maxDragDistance);

            if (dragDistance > 0)
            {
                // Тянем вверх - зеленый цвет (запомнил)
                Color feedbackColor = Color.Lerp(Color.black, _rememberedColor, intensity);
                _answerText.color = feedbackColor;
            }
            else if (dragDistance < 0)
            {
                // Тянем вниз - красный цвет (не запомнил)
                Color feedbackColor = Color.Lerp(Color.black, _notRememberedColor, intensity);
                _answerText.color = feedbackColor;
            }
            else
            {
                // Нет перетаскивания - возвращаем черный цвет
                _answerText.color = Color.black;
            }
        }

        public void ShowRememberedFeedback()
        {
            // Визуальная обратная связь для "запомнил"
            if (_answerText != null)
            {
                _answerText.color = _rememberedColor;
            }

            // Можно добавить дополнительные эффекты (анимацию, звук и т.д.)
        }

        public void ShowNotRememberedFeedback()
        {
            // Визуальная обратная связь для "не запомнил"
            if (_answerText != null)
            {
                _answerText.color = _notRememberedColor;
            }
        }

        public override void SetQuestion(Question question)
        {
            base.SetQuestion(question);

            // Сбрасываем цвета для нового вопроса
            if (_answerText != null)
            {
                _answerText.color = Color.black;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            // Дополнительная очистка для Flash карточки, если необходимо
        }
    }
}