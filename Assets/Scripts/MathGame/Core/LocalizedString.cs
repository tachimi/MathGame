using System;
using UnityEngine;
using I2.Loc;

namespace MathGame.Core
{
    /// <summary>
    /// Сериализуемая строка локализации с выпадающим списком ключей в инспекторе
    /// </summary>
    [Serializable]
    public class LocalizedString
    {
        [SerializeField]
        [TermsPopup("")]
        private string mTerm = string.Empty;

        [SerializeField]
        private string mTermSecondary = string.Empty;

        /// <summary>
        /// Ключ локализации
        /// </summary>
        public string Term
        {
            get => mTerm;
            set => mTerm = value;
        }

        /// <summary>
        /// Вторичный ключ (для шрифтов, спрайтов и т.д.)
        /// </summary>
        public string SecondaryTerm
        {
            get => mTermSecondary;
            set => mTermSecondary = value;
        }

        /// <summary>
        /// Получить локализованный текст
        /// </summary>
        public string GetLocalizedText()
        {
            if (string.IsNullOrEmpty(mTerm))
                return string.Empty;

            return LocalizationManager.GetTranslation(mTerm);
        }

        /// <summary>
        /// Получить локализованный текст с параметрами
        /// </summary>
        public string GetLocalizedText(params object[] parameters)
        {
            string translation = GetLocalizedText();

            if (string.IsNullOrEmpty(translation))
                return string.Empty;

            if (parameters != null && parameters.Length > 0)
            {
                try
                {
                    return string.Format(translation, parameters);
                }
                catch
                {
                    Debug.LogWarning($"Failed to format localized string: {mTerm}");
                    return translation;
                }
            }

            return translation;
        }

        /// <summary>
        /// Проверить, установлен ли ключ
        /// </summary>
        public bool HasTerm()
        {
            return !string.IsNullOrEmpty(mTerm);
        }

        /// <summary>
        /// Проверить, существует ли перевод
        /// </summary>
        public bool HasTranslation()
        {
            if (string.IsNullOrEmpty(mTerm))
                return false;

            string translation = LocalizationManager.GetTranslation(mTerm);
            return !string.IsNullOrEmpty(translation);
        }

        /// <summary>
        /// Очистить ключ
        /// </summary>
        public void Clear()
        {
            mTerm = string.Empty;
            mTermSecondary = string.Empty;
        }

        // Конструкторы
        public LocalizedString()
        {
            mTerm = string.Empty;
            mTermSecondary = string.Empty;
        }

        public LocalizedString(string term)
        {
            mTerm = term;
            mTermSecondary = string.Empty;
        }

        // Неявное преобразование
        public static implicit operator string(LocalizedString localizedString)
        {
            return localizedString?.GetLocalizedText() ?? string.Empty;
        }

        public override string ToString()
        {
            return GetLocalizedText();
        }
    }
}