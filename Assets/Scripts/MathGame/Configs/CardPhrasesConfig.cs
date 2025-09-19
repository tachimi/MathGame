using System;
using System.Collections.Generic;
using UnityEngine;
using MathGame.Core;

namespace MathGame.Configs
{
    /// <summary>
    /// ScriptableObject конфигурация для настройки фраз результатов
    /// </summary>
    [CreateAssetMenu(fileName = "ResultPhrasesConfig", menuName = "MathGame/Result Phrases Config")]
    public class CardPhrasesConfig : ScriptableObject
    {
        [Serializable]
        public class ResultPhrase
        {
            [Header("Диапазон точности (%)")]
            [Range(0, 100)]
            [Tooltip("Минимальный процент точности для этой фразы")]
            public float MinAccuracy = 0;
            
            [Range(0, 100)]
            [Tooltip("Максимальный процент точности для этой фразы")]
            public float MaxAccuracy = 100;
            
            [Header("Локализованные фразы")]
            [Tooltip("Список локализованных фраз для случайного выбора")]
            public List<LocalizedString> LocalizedPhrases = new();

            [Header("Фразы (устаревшие - используйте LocalizedPhrases)")]
            [Tooltip("Список фраз для случайного выбора - будет заменено на локализацию")]
            public List<string> Phrases = new();
            
            [Header("Визуальные настройки")]
            [Tooltip("Цвет текста для этого диапазона")]
            public Color TextColor = Color.white;
            
       
            public bool IsInRange(float accuracy)
            {
                return accuracy >= MinAccuracy && accuracy <= MaxAccuracy;
            }
            
            /// <summary>
            /// Получить случайную локализованную фразу
            /// </summary>
            public string GetRandomLocalizedPhrase()
            {
                // Приоритет у локализованных фраз
                if (LocalizedPhrases != null && LocalizedPhrases.Count > 0)
                {
                    // Фильтруем только те, у которых есть ключи
                    var validPhrases = LocalizedPhrases.FindAll(p => p != null && p.HasTerm());

                    if (validPhrases.Count > 0)
                    {
                        int index = UnityEngine.Random.Range(0, validPhrases.Count);
                        return validPhrases[index].GetLocalizedText();
                    }
                }

                // Fallback на старые фразы
                return GetRandomPhrase();
            }

            public string GetRandomPhrase()
            {
                if (Phrases == null || Phrases.Count == 0)
                    return Loc.Get("Config/Phrases/DefaultResult");

                int index = UnityEngine.Random.Range(0, Phrases.Count);
                return Phrases[index];
            }
        }

        [Header("Конфигурация фраз")]
        [SerializeField] private List<ResultPhrase> _resultPhrases = new();
        
        /// <summary>
        /// Получить локализованную фразу и цвет для заданной точности
        /// </summary>
        public (string phrase, Color color) GetPhraseForAccuracy(float accuracy)
        {
            // Ищем подходящий диапазон
            foreach (var resultPhrase in _resultPhrases)
            {
                if (resultPhrase.IsInRange(accuracy))
                {
                    return (resultPhrase.GetRandomLocalizedPhrase(), resultPhrase.TextColor);
                }
            }

            // Если не нашли подходящий диапазон, возвращаем дефолтные локализованные значения
            return (Loc.Get("Config/Phrases/DefaultResult"), Color.white);
        }
    }
}