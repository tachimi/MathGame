using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.UI.Cards
{
    /// <summary>
    /// Карточка для режима множественного выбора
    /// Лицевая сторона: вопрос, обратная сторона: варианты ответов
    /// </summary>
    public class MultipleChoiceCard : BaseMathCard
    {
        [Header("Multiple Choice Components")]
        [SerializeField] private Transform _answersContainer;
        [SerializeField] private Button _answerButtonPrefab;
        
        [Header("Visual Feedback")]
        [SerializeField] private Color _correctAnswerColor = Color.green;
        [SerializeField] private Color _wrongAnswerColor = Color.red;
        [SerializeField] private Color _disabledColor = Color.gray;

        private Button[] _answerButtons;
        private bool _firstAttemptUsed = false; // Флаг для отслеживания первой попытки
        private int _firstSelectedAnswer = -1; // Первый выбранный ответ

        protected override void SetupCard()
        {
            // Базовая настройка уже выполнена в базовом классе
        }

        protected override void SetupModeSpecificComponents()
        {
            if (_answersContainer == null || _answerButtonPrefab == null) return;

            // Сбрасываем флаги для нового вопроса
            _firstAttemptUsed = false;
            _firstSelectedAnswer = -1;

            // Очищаем предыдущие кнопки
            ClearAnswerButtons();

            // Генерируем варианты ответов
            var answers = GenerateAnswerOptions();
            _answerButtons = new Button[answers.Length];

            for (int i = 0; i < answers.Length; i++)
            {
                CreateAnswerButton(answers[i], i);
            }
        }

        protected override void ActivateBackSideComponents()
        {
            // Активируем контейнер с ответами
            if (_answersContainer != null)
                _answersContainer.gameObject.SetActive(true);
        }

        private void CreateAnswerButton(int answer, int index)
        {
            var buttonGO = Instantiate(_answerButtonPrefab, _answersContainer);
            var button = buttonGO.GetComponent<Button>();
            var text = buttonGO.GetComponentInChildren<TextMeshProUGUI>();

            if (text != null)
                text.text = answer.ToString();

            // Настраиваем кнопку
            button.onClick.AddListener(() => OnAnswerOptionSelected(answer));
            _answerButtons[index] = button;

            // Изначально скрываем кнопки (показываем только на обратной стороне)
            buttonGO.gameObject.SetActive(false);
        }

        private int[] GenerateAnswerOptions()
        {
            var options = new int[4];
            options[0] = _currentQuestion.CorrectAnswer;

            // Генерируем 3 неправильных ответа
            for (int i = 1; i < 4; i++)
            {
                int wrongAnswer;
                do
                {
                    wrongAnswer = _currentQuestion.CorrectAnswer + UnityEngine.Random.Range(-10, 11);
                    if (wrongAnswer < 0) wrongAnswer = UnityEngine.Random.Range(1, 20);
                } while (wrongAnswer == _currentQuestion.CorrectAnswer || Array.IndexOf(options, wrongAnswer) != -1);

                options[i] = wrongAnswer;
            }

            // Перемешиваем варианты
            for (int i = 0; i < options.Length; i++)
            {
                int temp = options[i];
                int randomIndex = UnityEngine.Random.Range(i, options.Length);
                options[i] = options[randomIndex];
                options[randomIndex] = temp;
            }

            return options;
        }

        protected override void ShowBackSide()
        {
            base.ShowBackSide();

            // Показываем кнопки ответов
            if (_answerButtons != null)
            {
                foreach (var button in _answerButtons)
                {
                    if (button != null)
                        button.gameObject.SetActive(true);
                }
            }
        }

        protected override void ShowFrontSide()
        {
            base.ShowFrontSide();

            // Скрываем кнопки ответов
            if (_answerButtons != null)
            {
                foreach (var button in _answerButtons)
                {
                    if (button != null)
                        button.gameObject.SetActive(false);
                }
            }
        }

        protected override void OnSwipeUpDetected()
        {
            // Свайп работает только на обратной стороне карточки (когда показаны варианты ответов)
            if (!_isFlipped) return;

            // Если не было ни одной попытки - засчитываем неправильный ответ
            if (!_firstAttemptUsed)
            {
                SelectAnswer(-1); // Неправильный ответ (не пытались отвечать)
            }
            // Если была попытка, но не ответили правильно - ответ уже был засчитан при первой попытке

            // Запускаем базовую анимацию (она вызовет событие OnSwipeUp после завершения)
            base.OnSwipeUpDetected();
        }
        
        protected override void OnSwipeDownDetected()
        {
            // Свайп вниз также работает только на обратной стороне
            if (!_isFlipped) return;

            // Если не было ни одной попытки - засчитываем неправильный ответ
            if (!_firstAttemptUsed)
            {
                SelectAnswer(-1); // Неправильный ответ (не пытались отвечать)
            }

            // Запускаем базовую анимацию
            base.OnSwipeDownDetected();
        }

        private void OnAnswerOptionSelected(int answer)
        {
            bool isCorrect = answer == _currentQuestion.CorrectAnswer;
            
            // Если это первая попытка
            if (!_firstAttemptUsed)
            {
                _firstAttemptUsed = true;
                _firstSelectedAnswer = answer;
                
                // Отправляем ответ только на первой попытке
                // Правильный ответ засчитывается только если выбран с первой попытки
                SelectAnswer(answer);
            }
            
            // Подсвечиваем выбранную кнопку
            HighlightSelectedAnswer(answer, isCorrect);
        }

        private void HighlightSelectedAnswer(int selectedAnswer, bool isCorrect)
        {
            if (_answerButtons == null) return;

            foreach (var button in _answerButtons)
            {
                if (button == null) continue;

                var text = button.GetComponentInChildren<TextMeshProUGUI>();
                
                if (text != null && int.TryParse(text.text, out int buttonAnswer))
                {
                    if (buttonAnswer == selectedAnswer)
                    {
                        // Подсвечиваем текст выбранной кнопки
                        text.color = isCorrect ? _correctAnswerColor : _wrongAnswerColor;
                        
                        // Отключаем кнопку после выбора
                        button.interactable = false;
                    }
                    else if (isCorrect)
                    {
                        // Если выбрали правильный ответ, отключаем остальные кнопки
                        button.interactable = false;
                        // Не меняем цвет - оставляем как есть (красные остаются красными)
                    }
                    else if (buttonAnswer == _currentQuestion.CorrectAnswer && _firstAttemptUsed)
                    {
                        // Если выбрали неправильный ответ, можем продолжить выбирать
                        // Но не подсвечиваем правильный ответ сразу
                        button.interactable = true;
                    }
                    else
                    {
                        // Остальные неправильные варианты остаются активными после неправильного выбора
                        button.interactable = !isCorrect;
                    }
                }
            }
            
            // Если нашли правильный ответ (не обязательно с первой попытки), показываем его
            if (isCorrect)
            {
                ShowCorrectAnswer();
            }
        }
        
        private void ShowCorrectAnswer()
        {
            // Отключаем все оставшиеся активные кнопки
            foreach (var button in _answerButtons)
            {
                if (button != null)
                {
                    button.interactable = false;
                    
                    var text = button.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null && int.TryParse(text.text, out int buttonAnswer))
                    {
                        // Серыми делаем только те кнопки, которые не были выбраны
                        // Красные (неправильно выбранные) остаются красными
                        if (text.color != _wrongAnswerColor && text.color != _correctAnswerColor)
                        {
                            text.color = _disabledColor;
                        }
                    }
                }
            }
        }

        private void ClearAnswerButtons()
        {
            if (_answersContainer == null) return;

            foreach (Transform child in _answersContainer)
            {
                if (child != null)
                    Destroy(child.gameObject);
            }

            _answerButtons = null;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ClearAnswerButtons();
        }
    }
}