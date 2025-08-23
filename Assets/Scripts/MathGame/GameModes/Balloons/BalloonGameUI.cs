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
        [SerializeField] private RectTransform _mainContainer;
        [SerializeField] private TextMeshProUGUI _questionText;
        [SerializeField] private Image _backgroundImage;
        
        [Header("Question Display Settings")]
        [SerializeField] private float _questionFontSize = 48f;
        [SerializeField] private Color _questionTextColor = Color.white;
        
        private Question _currentQuestion;
        
        public Question CurrentQuestion => _currentQuestion;
        
        /// <summary>
        /// Инициализация UI
        /// </summary>
        public void Initialize()
        {
            SetupUI();
            Debug.Log("BalloonGameUI: UI инициализирован");
        }
        
        /// <summary>
        /// Настройка UI компонентов
        /// </summary>
        private void SetupUI()
        {
            // Настраиваем основной контейнер
            if (_mainContainer == null)
            {
                _mainContainer = GetComponent<RectTransform>();
                if (_mainContainer == null)
                {
                    _mainContainer = gameObject.AddComponent<RectTransform>();
                }
            }
            
            // Растягиваем на весь экран
            _mainContainer.anchorMin = Vector2.zero;
            _mainContainer.anchorMax = Vector2.one;
            _mainContainer.sizeDelta = Vector2.zero;
            _mainContainer.anchoredPosition = Vector2.zero;
            
            // Создаем фоновое изображение
            if (_backgroundImage == null)
            {
                var bgObject = new GameObject("Background");
                bgObject.transform.SetParent(transform, false);
                
                _backgroundImage = bgObject.AddComponent<Image>();
                _backgroundImage.color = new Color(0.1f, 0.3f, 0.6f, 0.3f); // Голубоватый полупрозрачный фон
                
                var bgRect = _backgroundImage.rectTransform;
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.sizeDelta = Vector2.zero;
                bgRect.anchoredPosition = Vector2.zero;
            }
            
            // Создаем текст вопроса
            if (_questionText == null)
            {
                var questionObject = new GameObject("QuestionText");
                questionObject.transform.SetParent(transform, false);
                
                _questionText = questionObject.AddComponent<TextMeshProUGUI>();
                SetupQuestionText();
            }
        }
        
        /// <summary>
        /// Настройка текста вопроса
        /// </summary>
        private void SetupQuestionText()
        {
            _questionText.text = "Приготовься...";
            _questionText.fontSize = _questionFontSize;
            _questionText.color = _questionTextColor;
            _questionText.alignment = TextAlignmentOptions.Center;
            _questionText.fontStyle = FontStyles.Bold;
            
            // Размещаем текст в верхней части экрана
            var textRect = _questionText.rectTransform;
            textRect.anchorMin = new Vector2(0, 0.7f);
            textRect.anchorMax = new Vector2(1, 1f);
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            // Добавляем отступы
            textRect.offsetMin = new Vector2(50, 0); // Левый и нижний отступы
            textRect.offsetMax = new Vector2(-50, -20); // Правый и верхний отступы
        }
        
        /// <summary>
        /// Установить вопрос для отображения
        /// </summary>
        public void SetQuestion(Question question)
        {
            _currentQuestion = question;
            
            if (_questionText != null && question != null)
            {
                _questionText.text = question.GetQuestionDisplay();
                Debug.Log($"BalloonGameUI: Установлен вопрос - {question.GetQuestionDisplay()}");
            }
        }
        
        /// <summary>
        /// Показать игровой UI
        /// </summary>
        public void ShowGameUI()
        {
            gameObject.SetActive(true);
            
            // Можно добавить анимацию появления
            if (_questionText != null)
            {
                _questionText.color = _questionTextColor;
            }
            
            Debug.Log("BalloonGameUI: UI показан");
        }
        
        /// <summary>
        /// Скрыть игровой UI
        /// </summary>
        public void HideGameUI()
        {
            gameObject.SetActive(false);
            Debug.Log("BalloonGameUI: UI скрыт");
        }
        
        /// <summary>
        /// Обновить цвет фона
        /// </summary>
        public void SetBackgroundColor(Color color)
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.color = color;
            }
        }
        
        /// <summary>
        /// Обновить размер шрифта вопроса
        /// </summary>
        public void SetQuestionFontSize(float fontSize)
        {
            _questionFontSize = fontSize;
            
            if (_questionText != null)
            {
                _questionText.fontSize = fontSize;
            }
        }
        
        /// <summary>
        /// Обновить цвет текста вопроса
        /// </summary>
        public void SetQuestionTextColor(Color color)
        {
            _questionTextColor = color;
            
            if (_questionText != null)
            {
                _questionText.color = color;
            }
        }
        
        /// <summary>
        /// Получить область для размещения шариков (нижние 70% экрана)
        /// </summary>
        public Rect GetBalloonsArea()
        {
            if (_mainContainer != null)
            {
                var rect = _mainContainer.rect;
                return new Rect(rect.x, rect.y, rect.width, rect.height * 0.7f);
            }
            
            // Fallback размеры
            return new Rect(0, 0, 1920, 756); // 70% от 1080
        }
        
        /// <summary>
        /// Анимация правильного ответа
        /// </summary>
        public void PlayCorrectAnswerAnimation()
        {
            if (_questionText != null)
            {
                // Меняем цвет на зеленый на короткое время
                StartCoroutine(FlashTextColor(Color.green, 1f));
            }
        }
        
        /// <summary>
        /// Анимация неправильного ответа
        /// </summary>
        public void PlayIncorrectAnswerAnimation()
        {
            if (_questionText != null)
            {
                // Меняем цвет на красный на короткое время
                StartCoroutine(FlashTextColor(Color.red, 1f));
            }
        }
        
        /// <summary>
        /// Корутина мигания цвета текста
        /// </summary>
        private System.Collections.IEnumerator FlashTextColor(Color flashColor, float duration)
        {
            Color originalColor = _questionTextColor;
            _questionText.color = flashColor;
            
            yield return new WaitForSeconds(duration);
            
            _questionText.color = originalColor;
        }
        
        private void OnDestroy()
        {
            // Очистка ресурсов
            _currentQuestion = null;
        }
    }
}