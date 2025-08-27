using System;
using MathGame.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.UI
{
    /// <summary>
    /// Компонент кнопки цифровой клавиатуры
    /// Содержит информацию о типе кнопки и её значении
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class KeypadButton : MonoBehaviour
    {
        [Header("Button Configuration")]
        [SerializeField] private KeypadButtonType _buttonType = KeypadButtonType.Number;
        [SerializeField] private int _numberValue = 0; // Используется только для Number типа

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _buttonText;
        [SerializeField] private Image _buttonImage;

        private Button _button;

        /// <summary>
        /// Событие нажатия на кнопку
        /// </summary>
        public event Action<KeypadButton> OnButtonClicked;

        /// <summary>
        /// Тип кнопки
        /// </summary>
        public KeypadButtonType ButtonType => _buttonType;

        /// <summary>
        /// Числовое значение (только для Number типа)
        /// </summary>
        public int NumberValue => _numberValue;

        /// <summary>
        /// Компонент Button
        /// </summary>
        public Button Button => _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClicked);

            // Автоматически настраиваем текст кнопки на основе типа
            UpdateButtonAppearance();
        }

        private void OnValidate()
        {
            // Обновляем внешний вид в Editor при изменении параметров
            if (Application.isPlaying) return;

            UpdateButtonAppearance();
        }

        /// <summary>
        /// Настроить кнопку как числовую
        /// </summary>
        /// <param name="number">Число от 0 до 9</param>
        public void ConfigureAsNumber(int number)
        {
            _buttonType = KeypadButtonType.Number;
            _numberValue = Mathf.Clamp(number, 0, 9);
            UpdateButtonAppearance();
        }

        /// <summary>
        /// Настроить кнопку как служебную
        /// </summary>
        /// <param name="buttonType">Тип служебной кнопки</param>
        public void ConfigureAsAction(KeypadButtonType buttonType)
        {
            if (buttonType == KeypadButtonType.Number)
            {
                Debug.LogWarning("Используйте ConfigureAsNumber для числовых кнопок");
                return;
            }

            _buttonType = buttonType;
            UpdateButtonAppearance();
        }

        /// <summary>
        /// Установить интерактивность кнопки
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            _button.interactable = interactable;
        }

        /// <summary>
        /// Получить отображаемый текст для кнопки
        /// </summary>
        private string GetDisplayText()
        {
            return _buttonType switch
            {
                KeypadButtonType.Number => _numberValue.ToString(),
                _ => ""
            };
        }

        private void OnClicked()
        {
            OnButtonClicked?.Invoke(this);
        }

        private void UpdateButtonAppearance()
        {
            if (_buttonText != null)
            {
                var text = GetDisplayText();
                if (string.IsNullOrEmpty(text))
                {
                    return;
                }

                _buttonText.text = GetDisplayText();
            }

            // Можно добавить специальную логику для разных типов кнопок
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveAllListeners();
        }
    }
}