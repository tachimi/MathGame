using System;
using System.Collections.Generic;
using UnityEngine;

namespace MathGame.Configs
{
    /// <summary>
    /// ScriptableObject конфигурация для настройки фраз результатов
    /// </summary>
    [CreateAssetMenu(fileName = "ResultPhrasesConfig", menuName = "MathGame/Result Phrases Config")]
    public class ResultPhrasesConfig : ScriptableObject
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
            
            [Header("Фразы")]
            [Tooltip("Список фраз для случайного выбора")]
            public List<string> Phrases = new();
            
            [Header("Визуальные настройки")]
            [Tooltip("Цвет текста для этого диапазона")]
            public Color TextColor = Color.white;
            
       
            public bool IsInRange(float accuracy)
            {
                return accuracy >= MinAccuracy && accuracy <= MaxAccuracy;
            }
            
            public string GetRandomPhrase()
            {
                if (Phrases == null || Phrases.Count == 0)
                    return "Результат";
                    
                int index = UnityEngine.Random.Range(0, Phrases.Count);
                return Phrases[index];
            }
        }

        [Header("Конфигурация фраз")]
        [SerializeField] private List<ResultPhrase> _resultPhrases = new();
        
        /// <summary>
        /// Получить фразу и цвет для заданной точности
        /// </summary>
        public (string phrase, Color color) GetPhraseForAccuracy(float accuracy)
        {
            // Ищем подходящий диапазон
            foreach (var resultPhrase in _resultPhrases)
            {
                if (resultPhrase.IsInRange(accuracy))
                {
                    return (resultPhrase.GetRandomPhrase(), resultPhrase.TextColor);
                }
            }
            
            // Если не нашли подходящий диапазон, возвращаем дефолтные значения
            return ("Результат", Color.white);
        }
    }
}