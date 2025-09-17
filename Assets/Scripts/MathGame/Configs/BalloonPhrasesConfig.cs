using System;
using System.Collections.Generic;
using UnityEngine;

namespace MathGame.Configs
{
    /// <summary>
    /// ScriptableObject конфигурация для настройки фраз результатов режима шариков
    /// </summary>
    [CreateAssetMenu(fileName = "BalloonPhrasesConfig", menuName = "MathGame/Balloon Phrases Config")]
    public class BalloonPhrasesConfig : ScriptableObject
    {
        [Serializable]
        public class BalloonScorePhrase
        {
            [Header("Диапазон счета")]
            [Tooltip("Минимальный счет для этой фразы")]
            public int MinScore = 0;

            [Tooltip("Максимальный счет для этой фразы")]
            public int MaxScore = 10;

            [Header("Фразы")]
            [Tooltip("Список фраз для случайного выбора")]
            public List<string> Phrases = new();

            [Header("Визуальные настройки")]
            [Tooltip("Цвет текста для этого диапазона")]
            public Color TextColor = Color.white;

            /// <summary>
            /// Проверить, попадает ли счет в диапазон этой фразы
            /// </summary>
            public bool IsInRange(int score)
            {
                return score >= MinScore && score <= MaxScore;
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

        [Header("Конфигурация фраз для режима шариков")]
        [SerializeField] private List<BalloonScorePhrase> _scorePhrases = new();

        /// <summary>
        /// Получить фразу и цвет для заданного счета
        /// </summary>
        public (string phrase, Color color) GetPhraseForScore(int score)
        {
            // Ищем подходящий диапазон счета
            foreach (var scorePhrase in _scorePhrases)
            {
                if (scorePhrase.IsInRange(score))
                {
                    return (scorePhrase.GetRandomPhrase(), scorePhrase.TextColor);
                }
            }

            // Если не нашли подходящий диапазон, возвращаем дефолтные значения
            return ("Отличный результат!", Color.white);
        }

        /// <summary>
        /// Получить все настроенные диапазоны счета (для отладки)
        /// </summary>
        public List<BalloonScorePhrase> GetAllScorePhrases()
        {
            return _scorePhrases;
        }
    }
}