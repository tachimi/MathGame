using UnityEngine;
using UnityEngine.UI;
using TMPro;
using I2.Loc;
using MathGame.Core;

namespace MathGame.UI
{
    /// <summary>
    /// Компонент для автоматической локализации текста
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class LocalizedText : MonoBehaviour
    {
        [Header("Localization Settings")]
        [SerializeField] private string localizationKey;
        [SerializeField] private bool updateOnLanguageChange = true;
        [SerializeField] private bool setOnStart = true;

        [Header("Text Format")]
        [SerializeField] private bool useFormatting = false;
        [SerializeField] private string[] formatParameters;

        private Text textComponent;
        private TextMeshProUGUI tmpComponent;
        private bool isInitialized = false;

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            if (setOnStart)
            {
                UpdateText();
            }

            if (updateOnLanguageChange)
            {
                I2.Loc.LocalizationManager.OnLocalizeEvent += OnLanguageChanged;
            }
        }

        private void OnDestroy()
        {
            if (updateOnLanguageChange)
            {
                I2.Loc.LocalizationManager.OnLocalizeEvent -= OnLanguageChanged;
            }
        }

        private void InitializeComponents()
        {
            if (isInitialized) return;

            textComponent = GetComponent<Text>();
            tmpComponent = GetComponent<TextMeshProUGUI>();

            isInitialized = true;
        }

        private void OnLanguageChanged()
        {
            UpdateText();
        }

        /// <summary>
        /// Обновить текст с локализацией
        /// </summary>
        public void UpdateText()
        {
            if (string.IsNullOrEmpty(localizationKey))
                return;

            InitializeComponents();

            string localizedText;

            if (useFormatting && formatParameters != null && formatParameters.Length > 0)
            {
                localizedText = Loc.Get(localizationKey, formatParameters);
            }
            else
            {
                localizedText = Loc.Get(localizationKey);
            }

            if (textComponent != null)
            {
                textComponent.text = localizedText;
            }
            else if (tmpComponent != null)
            {
                tmpComponent.text = localizedText;
            }
        }

        /// <summary>
        /// Установить ключ локализации
        /// </summary>
        public void SetLocalizationKey(string key)
        {
            localizationKey = key;
            UpdateText();
        }

        /// <summary>
        /// Установить параметры форматирования
        /// </summary>
        public void SetFormatParameters(params string[] parameters)
        {
            formatParameters = parameters;
            if (useFormatting)
            {
                UpdateText();
            }
        }

        /// <summary>
        /// Получить текущий ключ локализации
        /// </summary>
        public string GetLocalizationKey()
        {
            return localizationKey;
        }

        /// <summary>
        /// Включить/выключить форматирование
        /// </summary>
        public void SetUseFormatting(bool useFormat)
        {
            useFormatting = useFormat;
            UpdateText();
        }

        /// <summary>
        /// Установить текст напрямую (отключает локализацию)
        /// </summary>
        public void SetDirectText(string text)
        {
            InitializeComponents();

            if (textComponent != null)
            {
                textComponent.text = text;
            }
            else if (tmpComponent != null)
            {
                tmpComponent.text = text;
            }

            // Очищаем ключ локализации, чтобы не перезаписать текст
            localizationKey = "";
        }

        /// <summary>
        /// Проверить, валиден ли ключ локализации
        /// </summary>
        public bool IsValidKey()
        {
            if (string.IsNullOrEmpty(localizationKey))
                return false;

            string translation = LocalizationManager.GetTranslation(localizationKey);
            return !string.IsNullOrEmpty(translation);
        }

        // Редакторские методы
        #if UNITY_EDITOR
        [ContextMenu("Update Text Preview")]
        private void UpdateTextPreview()
        {
            UpdateText();
        }

        [ContextMenu("Clear Localization Key")]
        private void ClearKey()
        {
            localizationKey = "";
        }
        #endif
    }
}