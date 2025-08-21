using System.Collections;
using MathGame.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.UI.Cards
{
    /// <summary>
    /// Карточка для режима ввода текста
    /// Статичная карточка с цифровой клавиатурой
    /// </summary>
    public class TextInputCard : BaseMathCard
    {
        [Header("Text Input Components")]
        [SerializeField] private Transform _inputContainer;
        [SerializeField] private TextMeshProUGUI _inputDisplay;
        [SerializeField] private Transform _numbersContainer;
        [SerializeField] private Button _numberButtonPrefab;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private Button _submitButton;

        [Header("Feedback Settings")]
        [SerializeField] private float _feedbackDuration = 2f;

        private string _currentInput = "";
        private Button[] _numberButtons;

        protected override void SetupCard()
        {
            // Настраиваем кнопки
            if (_deleteButton != null)
                _deleteButton.onClick.AddListener(OnDeleteClicked);

            if (_submitButton != null)
                _submitButton.onClick.AddListener(OnSubmitClicked);

            // Создаем кнопки с цифрами
            CreateNumberButtons();
        }

        protected override void SetupModeSpecificComponents()
        {
            // Активируем контейнер ввода
            if (_inputContainer != null)
                _inputContainer.gameObject.SetActive(true);

            // Сбрасываем ввод
            _currentInput = "";
            UpdateInputDisplay();
            ResetAnswerState();
        }

        protected override bool CanFlip()
        {
            // Карточка статична в режиме ввода текста
            return false;
        }

        protected override void OnCardClicked()
        {
            // В режиме ввода текста клик не переворачивает карточку
            // Можно добавить другую логику, например, фокус на поле ввода
        }

        private void CreateNumberButtons()
        {
            if (_numbersContainer == null || _numberButtonPrefab == null) return;

            // Очищаем предыдущие кнопки
            foreach (Transform child in _numbersContainer)
            {
                Destroy(child.gameObject);
            }

            _numberButtons = new Button[10];

            // Создаем кнопки 0-9
            for (int i = 0; i <= 9; i++)
            {
                var buttonGO = Instantiate(_numberButtonPrefab, _numbersContainer);
                var button = buttonGO.GetComponent<Button>();
                var text = buttonGO.GetComponentInChildren<TextMeshProUGUI>();

                if (text != null)
                    text.text = i.ToString();

                int number = i; // Захватываем значение для замыкания
                button.onClick.AddListener(() => OnNumberClicked(number));
                _numberButtons[i] = button;
            }
        }

        private void OnNumberClicked(int number)
        {
            if (_currentInput.Length < 4) // Ограничиваем длину ввода
            {
                _currentInput += number.ToString();
                UpdateInputDisplay();
                UpdateSubmitButtonState();
            }
        }

        private void OnDeleteClicked()
        {
            if (_currentInput.Length > 0)
            {
                _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
                UpdateInputDisplay();
                UpdateSubmitButtonState();
            }
        }

        private void OnSubmitClicked()
        {
            if (_isAnswered || string.IsNullOrEmpty(_currentInput)) return;

            if (int.TryParse(_currentInput, out int answer))
            {
                SelectAnswer(answer);

                // Если ответ неправильный, показываем правильный ответ
                if (answer != _currentQuestion.CorrectAnswer)
                {
                    StartCoroutine(ShowCorrectAnswerAndProceed());
                }
                else
                {
                    // Правильный ответ - показываем зеленый цвет
                    ShowCorrectFeedback();
                }

                // Отключаем кнопки после ответа
                SetButtonsInteractable(false);
            }
        }

        private void UpdateInputDisplay()
        {
            if (_inputDisplay != null)
            {
                _inputDisplay.text = string.IsNullOrEmpty(_currentInput) ? "?" : _currentInput;
                _inputDisplay.color = Color.black;
            }
        }

        private void UpdateSubmitButtonState()
        {
            if (_submitButton != null)
            {
                _submitButton.interactable = !string.IsNullOrEmpty(_currentInput) && !_isAnswered;
            }
        }

        private void SetButtonsInteractable(bool interactable)
        {
            // Отключаем/включаем цифровые кнопки
            if (_numberButtons != null)
            {
                foreach (var button in _numberButtons)
                {
                    if (button != null)
                        button.interactable = interactable;
                }
            }

            // Отключаем кнопки управления
            if (_deleteButton != null)
                _deleteButton.interactable = interactable;

            if (_submitButton != null)
                _submitButton.interactable = interactable && !string.IsNullOrEmpty(_currentInput);
        }

        private void ShowCorrectFeedback()
        {
            if (_inputDisplay != null)
            {
                _inputDisplay.color = Color.green;
            }
        }

        private IEnumerator ShowCorrectAnswerAndProceed()
        {
            if (_inputDisplay != null)
            {
                _inputDisplay.text = $"Правильно: {_currentQuestion.CorrectAnswer}";
                _inputDisplay.color = Color.red;
            }

            yield return new WaitForSeconds(_feedbackDuration);

            // Запускаем анимацию свайпа (она сама вызовет OnSwipeUp после завершения)
            StartCoroutine(PlaySwipeUpAnimation());
        }

        protected override void OnSwipeUpDetected()
        {
            // Если не ответили - обрабатываем ввод перед анимацией
            if (!_isAnswered && !string.IsNullOrEmpty(_currentInput))
            {
                OnSubmitClicked(); // Пытаемся отправить текущий ввод
            }
            else if (!_isAnswered)
            {
                SelectAnswer(-1); // Неправильный ответ, если ничего не введено
            }
            
            // Запускаем базовую анимацию (она вызовет событие OnSwipeUp после завершения)
            base.OnSwipeUpDetected();
        }

        public override void SetQuestion(Question question)
        {
            base.SetQuestion(question);
            
            // Включаем кнопки для нового вопроса
            SetButtonsInteractable(true);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_deleteButton != null)
                _deleteButton.onClick.RemoveAllListeners();

            if (_submitButton != null)
                _submitButton.onClick.RemoveAllListeners();

            // Очищаем цифровые кнопки
            if (_numberButtons != null)
            {
                foreach (var button in _numberButtons)
                {
                    if (button != null)
                        button.onClick.RemoveAllListeners();
                }
            }
        }
    }
}