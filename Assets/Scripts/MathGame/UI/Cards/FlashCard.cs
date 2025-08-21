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
        [Header("Flash Components")]
        [SerializeField] private TextMeshProUGUI _answerText;
        [SerializeField] private GameObject _answerContainer;

        [Header("Visual Feedback")]
        [SerializeField] private Color _rememberedColor = Color.green;
        [SerializeField] private Color _notRememberedColor = Color.red;

        protected override void SetupCard()
        {
            // Настройка Flash карточки
            if (_answerContainer != null)
                _answerContainer.SetActive(false);
        }

        protected override void SetupModeSpecificComponents()
        {
            // Устанавливаем правильный ответ для показа на обратной стороне
            if (_answerText != null)
            {
                _answerText.text = _currentQuestion.CorrectAnswer.ToString();
                _answerText.color = Color.black; // Сбрасываем цвет
            }

            // Скрываем контейнер с ответом (показываем только на обратной стороне)
            if (_answerContainer != null)
                _answerContainer.SetActive(false);
        }

        protected override void ActivateBackSideComponents()
        {
            // Активируем показ правильного ответа
            if (_answerContainer != null)
                _answerContainer.SetActive(true);
        }

        protected override void ShowFrontSide()
        {
            base.ShowFrontSide();

            // Скрываем ответ на лицевой стороне
            if (_answerContainer != null)
                _answerContainer.SetActive(false);
        }

        protected override void ShowBackSide()
        {
            base.ShowBackSide();

            // Показываем ответ на обратной стороне
            if (_answerContainer != null)
                _answerContainer.SetActive(true);
        }

        protected override void OnSwipeUpDetected()
        {
            // Свайп вверх = "Запомнил" = правильный ответ
            if (_isFlipped) // Можно отвечать только когда карточка перевернута
            {
                SelectAnswer(_currentQuestion.CorrectAnswer);
                ShowRememberedFeedback();
                
                // Запускаем базовую анимацию (она вызовет событие OnSwipeUp после завершения)
                base.OnSwipeUpDetected();
            }
        }

        protected override void OnSwipeDownDetected()
        {
            // Свайп вниз = "Не запомнил" = неправильный ответ
            if (_isFlipped) // Можно отвечать только когда карточка перевернута
            {
                SelectAnswer(-1); // Неправильный ответ
                ShowNotRememberedFeedback();
                
                // Запускаем базовую анимацию (она вызовет событие OnSwipeDown после завершения)
                base.OnSwipeDownDetected();
            }
        }

        private void ShowRememberedFeedback()
        {
            // Визуальная обратная связь для "запомнил"
            if (_answerText != null)
            {
                _answerText.color = _rememberedColor;
            }

            // Можно добавить дополнительные эффекты (анимацию, звук и т.д.)
            ShowPositiveFeedback();
        }

        private void ShowNotRememberedFeedback()
        {
            // Визуальная обратная связь для "не запомнил"
            if (_answerText != null)
            {
                _answerText.color = _notRememberedColor;
            }

            // Можно добавить дополнительные эффекты
            ShowNegativeFeedback();
        }

        private void ShowPositiveFeedback()
        {
            // Дополнительные эффекты для положительного ответа
            // Например, увеличение текста или мигание зеленым
            if (_answerContainer != null)
            {
                var canvasGroup = _answerContainer.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = _answerContainer.AddComponent<CanvasGroup>();

                StartCoroutine(FlashEffect(canvasGroup, _rememberedColor));
            }
        }

        private void ShowNegativeFeedback()
        {
            // Дополнительные эффекты для отрицательного ответа
            if (_answerContainer != null)
            {
                var canvasGroup = _answerContainer.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = _answerContainer.AddComponent<CanvasGroup>();

                StartCoroutine(FlashEffect(canvasGroup, _notRememberedColor));
            }
        }

        private System.Collections.IEnumerator FlashEffect(CanvasGroup canvasGroup, Color color)
        {
            // Простой эффект мигания
            float originalAlpha = canvasGroup.alpha;
            
            for (int i = 0; i < 3; i++)
            {
                canvasGroup.alpha = 0.5f;
                yield return new WaitForSeconds(0.1f);
                canvasGroup.alpha = originalAlpha;
                yield return new WaitForSeconds(0.1f);
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