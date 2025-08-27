using System;
using UnityEngine;

namespace MathGame.Configs
{
    /// <summary>
    /// Конфигурация визуальной обратной связи для режима шариков
    /// </summary>
    [CreateAssetMenu(fileName = "BalloonFeedbackConfig", menuName = "MathGame/Configs/Balloon Feedback Config")]
    public class BalloonFeedbackConfig : ScriptableObject
    {
        [Header("Timing Settings")]
        [Tooltip("Длительность показа фидбека для правильного ответа (в секундах)")]
        public float correctAnswerDisplayDuration = 2f;
        
        [Tooltip("Длительность показа фидбека для неправильного ответа (в секундах)")]
        public float wrongAnswerDisplayDuration = 3f;
        
        [Tooltip("Длительность показа фидбека для проигрыша раунда (в секундах)")]
        public float roundLostDisplayDuration = 3f;
        
        [Header("Animation Settings")]
        [Tooltip("Время анимации появления панели (в секундах)")]
        public float showAnimationDuration = 0.3f;
        
        [Tooltip("Время анимации скрытия панели (в секундах)")]
        public float hideAnimationDuration = 0.2f;
        
        [Header("Text Settings")]
        [Tooltip("Шрифт для текста фидбека")]
        public TMPro.TMP_FontAsset font;
        
        [Tooltip("Размер шрифта для текста фидбека")]
        public float fontSize = 48f;
        
        [Tooltip("Настройки шрифта для текста фидбека")]
        public TMPro.FontStyles fontStyle = TMPro.FontStyles.Bold;
        
        [Header("Correct Answer Feedback")]
        [Tooltip("Текст для правильного ответа")]
        public string correctAnswerText = "Правильно!";
        
        [Tooltip("Цвет текста для правильного ответа")]
        public Color correctAnswerTextColor = Color.green;
        
        [Tooltip("Цвет фона для правильного ответа")]
        public Color correctAnswerBackgroundColor = new Color(0f, 1f, 0f, 0.3f);
        
        [Header("Wrong Answer Feedback")]
        [Tooltip("Шаблон текста для неправильного ответа. {0} - выбранный ответ, {1} - правильный ответ")]
        public string wrongAnswerTextTemplate = "Неправильно!\nВаш ответ: {0}\nПравильный ответ: {1}";
        
        [Tooltip("Цвет текста для неправильного ответа")]
        public Color wrongAnswerTextColor = Color.red;
        
        [Tooltip("Цвет фона для неправильного ответа")]
        public Color wrongAnswerBackgroundColor = new Color(1f, 0f, 0f, 0.3f);
        
        [Header("Round Lost Feedback")]
        [Tooltip("Шаблон текста для проигрыша раунда. {0} - правильный ответ")]
        public string roundLostTextTemplate = "Время вышло!\nПравильный ответ был: {0}";
        
        [Tooltip("Цвет текста для проигрыша раунда")]
        public Color roundLostTextColor = Color.yellow;
        
        [Tooltip("Цвет фона для проигрыша раунда")]
        public Color roundLostBackgroundColor = new Color(1f, 1f, 0f, 0.3f);
        
        [Header("Panel Settings")]
        [Tooltip("Базовый цвет фона панели фидбека")]
        public Color basePanelBackgroundColor = new Color(0f, 0f, 0f, 0.5f);
        
        [Header("Advanced Settings")]
        [Tooltip("Использовать анимацию отскока при появлении")]
        public bool useBouncyShowAnimation = true;
        
        [Tooltip("Использовать анимацию сжатия при скрытии")]
        public bool useShrinkHideAnimation = true;
        
        [Tooltip("Отключать взаимодействие с UI во время показа фидбека")]
        public bool blockUIInteraction = true;
        
        /// <summary>
        /// Получить форматированный текст для неправильного ответа
        /// </summary>
        public string GetWrongAnswerText(int selectedAnswer, int correctAnswer)
        {
            return string.Format(wrongAnswerTextTemplate, selectedAnswer, correctAnswer);
        }
        
        /// <summary>
        /// Получить форматированный текст для проигрыша раунда
        /// </summary>
        public string GetRoundLostText(int correctAnswer)
        {
            return string.Format(roundLostTextTemplate, correctAnswer);
        }
        
        /// <summary>
        /// Валидация настроек конфига
        /// </summary>
        private void OnValidate()
        {
            // Ограничиваем минимальные значения
            correctAnswerDisplayDuration = Mathf.Max(0.1f, correctAnswerDisplayDuration);
            wrongAnswerDisplayDuration = Mathf.Max(0.1f, wrongAnswerDisplayDuration);
            roundLostDisplayDuration = Mathf.Max(0.1f, roundLostDisplayDuration);
            
            showAnimationDuration = Mathf.Max(0.1f, showAnimationDuration);
            hideAnimationDuration = Mathf.Max(0.1f, hideAnimationDuration);
            
            fontSize = Mathf.Max(12f, fontSize);
            
            // Убеждаемся что альфа каналы фона не превышают 1.0
            correctAnswerBackgroundColor.a = Mathf.Clamp01(correctAnswerBackgroundColor.a);
            wrongAnswerBackgroundColor.a = Mathf.Clamp01(wrongAnswerBackgroundColor.a);
            roundLostBackgroundColor.a = Mathf.Clamp01(roundLostBackgroundColor.a);
            basePanelBackgroundColor.a = Mathf.Clamp01(basePanelBackgroundColor.a);
        }
    }
}