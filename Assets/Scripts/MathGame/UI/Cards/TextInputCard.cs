using System.Threading;
using Cysharp.Threading.Tasks;
using MathGame.CardInteractions;
using MathGame.Enums;
using MathGame.Models;
using TMPro;
using UnityEngine;

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
        
        [Header("Keypad Buttons")]
        [SerializeField] private KeypadButton[] _keypadButtons;

        [Header("Visual settings")]
        [SerializeField] private Color _correctAnswerColor;
        [SerializeField] private Color _wrongAnswerColor;
        
        [Header("Feedback Settings")]
        [SerializeField] private float _feedbackDuration = 2f;
        [SerializeField] private int _maxInputLength = 4;

        private string _currentInput = "";

        protected override void SetupCard()
        {
            // Настраиваем кнопки клавиатуры
            SetupKeypadButtons();
        }
        
        protected override void InitializeInteractionStrategy()
        {
            _interactionStrategy = new TextInputInteractionStrategy();
            _interactionStrategy.Initialize(this);
        }

        protected override void SetupModeSpecificComponents()
        {
            // Активируем контейнер ввода
            if (_inputContainer != null)
                _inputContainer.gameObject.SetActive(true);

            // Сбрасываем ввод
            _currentInput = "";
        }


        private void SetupKeypadButtons()
        {
            if (_keypadButtons == null || _keypadButtons.Length == 0)
            {
                Debug.LogWarning("TextInputCard: Массив keypad buttons пуст! Настройте кнопки в Inspector.");
                return;
            }

            // Подписываемся на события всех кнопок
            foreach (var keypadButton in _keypadButtons)
            {
                if (keypadButton != null)
                {
                    keypadButton.OnButtonClicked += OnKeypadButtonClicked;
                }
            }
        }

        private void OnKeypadButtonClicked(KeypadButton keypadButton)
        {
            if (_isAnswered) return;

            switch (keypadButton.ButtonType)
            {
                case KeypadButtonType.Number:
                    OnNumberClicked(keypadButton.NumberValue);
                    break;
                    
                case KeypadButtonType.Delete:
                    OnDeleteClicked();
                    break;
                    
                case KeypadButtonType.Clear:
                    OnClearClicked();
                    break;
                    
                case KeypadButtonType.Submit:
                    OnSubmitClicked();
                    break;
            }
        }

        private void OnNumberClicked(int number)
        {
            if (_currentInput.Length < _maxInputLength)
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

        private void OnClearClicked()
        {
            _currentInput = "";
            UpdateInputDisplay();
            UpdateSubmitButtonState();
        }

        private void OnSubmitClicked()
        {
            if (_isAnswered || string.IsNullOrEmpty(_currentInput)) return;

            if (int.TryParse(_currentInput, out int answer))
            {
                SelectAnswer(answer);

                // Отключаем кнопки после ответа
                SetButtonsInteractable(false);

                // Если ответ неправильный, показываем красный -> правильный ответ -> автоматический переход
                if (answer != _currentQuestion.CorrectAnswer)
                {
                    ShowIncorrectThenCorrectAndProceedAsync(this.GetCancellationTokenOnDestroy()).Forget();
                }
                else
                {
                    // Правильный ответ - показываем зеленый и автоматически переходим
                    ShowCorrectAndProceedAsync(this.GetCancellationTokenOnDestroy()).Forget();
                }
            }
        }

        protected override void UpdateInputDisplay()
        {
            // Вызываем базовый метод для обновления _questionDisplay
            base.UpdateInputDisplay();
            
            // Переопределяем логику для поля ввода - показываем введенный текст или "?"
            if (_inputDisplay != null)
            {
                _inputDisplay.text = string.IsNullOrEmpty(_currentInput) ? "?" : _currentInput;
            }
        }

        private void UpdateSubmitButtonState()
        {
            // Находим кнопку Submit и обновляем её состояние
            foreach (var keypadButton in _keypadButtons)
            {
                if (keypadButton != null && keypadButton.ButtonType == KeypadButtonType.Submit)
                {
                    bool canSubmit = !string.IsNullOrEmpty(_currentInput) && !_isAnswered;
                    keypadButton.SetInteractable(canSubmit);
                    break;
                }
            }
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (_keypadButtons == null) return;

            foreach (var keypadButton in _keypadButtons)
            {
                if (keypadButton == null) continue;

                // Для кнопки Submit особая логика - она активна только при наличии ввода
                if (keypadButton.ButtonType == KeypadButtonType.Submit)
                {
                    bool canSubmit = interactable && !string.IsNullOrEmpty(_currentInput) && !_isAnswered;
                    keypadButton.SetInteractable(canSubmit);
                }
                else
                {
                    keypadButton.SetInteractable(interactable);
                }
            }
        }

        private async UniTask ShowCorrectAndProceedAsync(CancellationToken cancellationToken = default)
        {
            // Показываем зеленый цвет для правильного ответа
            if (_inputDisplay != null)
            {
                _inputDisplay.color = _correctAnswerColor;
            }

            await UniTask.Delay((int)(_feedbackDuration * 1000), cancellationToken: cancellationToken);

            // Автоматически переходим к следующему вопросу
            AutoProceedToNext();
        }

        private async UniTask ShowIncorrectThenCorrectAndProceedAsync(CancellationToken cancellationToken = default)
        {
            // Показываем красный цвет для неправильного ответа
            if (_inputDisplay != null)
            {
                _inputDisplay.color = _wrongAnswerColor;
            }

            await UniTask.Delay((int)(_feedbackDuration * 1000), cancellationToken: cancellationToken);

            // Показываем правильный ответ зеленым цветом
            if (_inputDisplay != null)
            {
                _inputDisplay.text = _currentQuestion.CorrectAnswer.ToString();
                _inputDisplay.color = _correctAnswerColor;
            }

            await UniTask.Delay((int)(_feedbackDuration * 1000), cancellationToken: cancellationToken);

            // Автоматически переходим к следующему вопросу
            AutoProceedToNext();
        }

        private void AutoProceedToNext()
        {
            // Запускаем анимацию исчезновения карточки и вызываем событие завершения раунда
            PlaySwipeUpAnimationAsync().Forget();
        }


        public override void SetQuestion(Question question, bool isTutorial)
        {
            base.SetQuestion(question, isTutorial);
            
            // Сбрасываем ввод для нового вопроса
            _currentInput = "";
            
            // Включаем кнопки для нового вопроса
            SetButtonsInteractable(true);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Отписываемся от событий keypad кнопок
            if (_keypadButtons != null)
            {
                foreach (var keypadButton in _keypadButtons)
                {
                    if (keypadButton != null)
                        keypadButton.OnButtonClicked -= OnKeypadButtonClicked;
                }
            }
        }
    }
}