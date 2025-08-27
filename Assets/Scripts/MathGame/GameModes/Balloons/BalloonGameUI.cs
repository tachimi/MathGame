using MathGame.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.GameModes.Balloons
{
    /// <summary>
    /// UI компонент для игрового режима с шариками
    /// Отображает вопрос сверху и управляет общим интерфейсом
    /// </summary>
    public class BalloonGameUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private RectTransform _balloonsContainer;
        [SerializeField] private TextMeshProUGUI _questionText;
        [SerializeField] private TextMeshProUGUI _countdownText;
        [SerializeField] private Image _backgroundImage;
        
        /// <summary>
        /// Инициализация UI
        /// </summary>
        public void Initialize()
        {
            SetupUI();
        }
        
        /// <summary>
        /// Настройка UI компонентов
        /// </summary>
        private void SetupUI()
        {
            SetupCountdownText();
        }
        
        /// <summary>
        /// Настройка текста отсчета
        /// </summary>
        private void SetupCountdownText()
        {
            _countdownText.text = "Приготовься...";
            // Изначально скрываем
            _countdownText.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Установить вопрос для отображения
        /// </summary>
        public void SetQuestion(Question question)
        {
            if (_questionText != null && question != null)
            {
                _questionText.text = question.GetQuestionDisplay();
            }
        }
        
        /// <summary>
        /// Показать игровой UI
        /// </summary>
        public void ShowGameUI()
        {
            gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Показать отсчет перед игрой
        /// </summary>
        public void ShowCountdown()
        {
            if (_countdownText != null)
            {
                _countdownText.gameObject.SetActive(true);
                _countdownText.text = "Приготовься...";
            }
        }
        
        /// <summary>
        /// Обновить текст отсчета
        /// </summary>
        public void UpdateCountdownText(string text)
        {
            if (_countdownText != null)
            {
                _countdownText.text = text;
            }
        }
        
        /// <summary>
        /// Скрыть отсчет
        /// </summary>
        public void HideCountdown()
        {
            if (_countdownText != null)
            {
                _countdownText.gameObject.SetActive(false);
            }
        }

        public RectTransform GetBalloonsContainer()
        {
            return _balloonsContainer;
        }
    }
}