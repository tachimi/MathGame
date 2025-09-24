using System;
using MathGame.CardInteractions;
using MathGame.Tutorial;
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
        public Button[] AnswerButtons => _answerButtons;
        
        [Header("Multiple Choice Components")]
        [SerializeField] private Transform _answersContainer;
        [SerializeField] private Button _answerButtonPrefab;
        [SerializeField] private int _answersAmount = 6;

        [Header("Visual Feedback")]
        [SerializeField] private Color _correctAnswerColor = Color.green;
        [SerializeField] private Color _wrongAnswerColor = Color.red;
        [SerializeField] private Color _disabledColor = Color.gray;

        private Button[] _answerButtons;
        public bool _firstAttemptUsed = false; // Флаг для отслеживания первой попытки

        protected override void SetupCard()
        {
            // Базовая настройка уже выполнена в базовом классе
        }
        
        protected override void InitializeInteractionStrategy()
        {
            _interactionStrategy = new MultipleChoiceInteractionStrategy();
            _interactionStrategy.Initialize(this);
        }

        protected override void SetupModeSpecificComponents()
        {
            if (_answersContainer == null || _answerButtonPrefab == null) return;

            // Сбрасываем флаги для нового вопроса
            _firstAttemptUsed = false;

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
            var options = new int[_answersAmount];
            options[0] = _currentQuestion.CorrectAnswer;

            // Генерируем неправильныe ответы
            for (int i = 1; i < _answersAmount; i++)
            {
                int wrongAnswer;
                do
                {
                    wrongAnswer = _currentQuestion.CorrectAnswer + UnityEngine.Random.Range(-10, 11);
                    if (wrongAnswer < 0) wrongAnswer = UnityEngine.Random.Range(1, 20);
                } while (wrongAnswer == _currentQuestion.CorrectAnswer || Array.IndexOf(options, wrongAnswer) != -1);

                options[i] = wrongAnswer;
            }

            // Перемешиваем варианты только если туториал НЕ активен
            if (!_isTutorial)
            {
                for (int i = 0; i < options.Length; i++)
                {
                    int temp = options[i];
                    int randomIndex = UnityEngine.Random.Range(i, options.Length);
                    options[i] = options[randomIndex];
                    options[randomIndex] = temp;
                }
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


        private void OnAnswerOptionSelected(int answer)
        {
            bool isCorrect = answer == _currentQuestion.CorrectAnswer;

            // Если это первая попытка
            if (!_firstAttemptUsed)
            {
                _firstAttemptUsed = true;

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

        protected override Vector2 GetFirstAnswerButtonPosition()
        {
            if (_answerButtons != null && _answerButtons.Length > 0 && _answerButtons[0] != null)
            {
                return _answerButtons[0].transform.position;
            }
            return base.GetFirstAnswerButtonPosition();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ClearAnswerButtons();
        }
    }
}