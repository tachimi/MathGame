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
            public List<string> Phrases = new List<string>();
            
            [Header("Визуальные настройки")]
            [Tooltip("Цвет текста для этого диапазона")]
            public Color TextColor = Color.white;
            
            /// <summary>
            /// Проверить, попадает ли точность в этот диапазон
            /// </summary>
            public bool IsInRange(float accuracy)
            {
                return accuracy >= MinAccuracy && accuracy <= MaxAccuracy;
            }
            
            /// <summary>
            /// Получить случайную фразу из списка
            /// </summary>
            public string GetRandomPhrase()
            {
                if (Phrases == null || Phrases.Count == 0)
                    return "Результат";
                    
                int index = UnityEngine.Random.Range(0, Phrases.Count);
                return Phrases[index];
            }
        }
        
        [Header("Конфигурация фраз")]
        [SerializeField] private List<ResultPhrase> _resultPhrases = new List<ResultPhrase>()
        {
            new ResultPhrase 
            { 
                MinAccuracy = 90, 
                MaxAccuracy = 100, 
                Phrases = new List<string> { "Отлично!", "Превосходно!", "Великолепно!", "Блестяще!" },
                TextColor = new Color(0.2f, 0.8f, 0.2f) // Зеленый
            },
            new ResultPhrase 
            { 
                MinAccuracy = 70, 
                MaxAccuracy = 89, 
                Phrases = new List<string> { "Хорошо!", "Неплохо!", "Молодец!", "Хороший результат!" },
                TextColor = new Color(0.4f, 0.7f, 0.4f) // Светло-зеленый
            },
            new ResultPhrase 
            { 
                MinAccuracy = 50, 
                MaxAccuracy = 69, 
                Phrases = new List<string> { "Можно лучше!", "Старайся!", "Не сдавайся!", "Продолжай тренироваться!" },
                TextColor = new Color(1f, 0.8f, 0.2f) // Желтый
            },
            new ResultPhrase 
            { 
                MinAccuracy = 30, 
                MaxAccuracy = 49, 
                Phrases = new List<string> { "Нужна практика!", "Попробуй еще раз!", "Тренируйся больше!" },
                TextColor = new Color(1f, 0.5f, 0.2f) // Оранжевый
            },
            new ResultPhrase 
            { 
                MinAccuracy = 0, 
                MaxAccuracy = 29, 
                Phrases = new List<string> { "Попробуй снова!", "Не расстраивайся!", "В следующий раз получится лучше!" },
                TextColor = new Color(0.8f, 0.3f, 0.3f) // Красный
            }
        };
        
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
        
        /// <summary>
        /// Получить только фразу для заданной точности
        /// </summary>
        public string GetPhrase(float accuracy)
        {
            var (phrase, _) = GetPhraseForAccuracy(accuracy);
            return phrase;
        }
        
        /// <summary>
        /// Получить только цвет для заданной точности
        /// </summary>
        public Color GetColor(float accuracy)
        {
            var (_, color) = GetPhraseForAccuracy(accuracy);
            return color;
        }
    }
}