using System;
using System.Collections.Generic;
using UnityEngine;
using MathGame.Core;

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
            [Header("Диапазон счета")] [Tooltip("Минимальный счет для этой фразы")]
            public int MinScore = 0;

            [Tooltip("Максимальный счет для этой фразы")]
            public int MaxScore = 10;

            [Header("Локализованные фразы")] [Tooltip("Список локализованных фраз для случайного выбора")]
            public List<LocalizedString> LocalizedPhrases = new();

            [Header("Визуальные настройки")] [Tooltip("Цвет текста для этого диапазона")]
            public Color TextColor = Color.white;

            /// <summary>
            /// Проверить, попадает ли счет в диапазон этой фразы
            /// </summary>
            public bool IsInRange(int score)
            {
                return score >= MinScore && score <= MaxScore;
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

                return "Bad configuration";
            }
        }

        [Header("Конфигурация фраз для режима шариков")]
        [SerializeField] private List<BalloonScorePhrase> _scorePhrases = new();
        [SerializeField] private LocalizedString _newRecordPhrase;
        
        /// <summary>
        /// Получить локализованную фразу и цвет для заданного счета
        /// </summary>
        public (string phrase, Color color) GetPhraseForScore(int score)
        {
            // Ищем подходящий диапазон счета
            foreach (var scorePhrase in _scorePhrases)
            {
                if (scorePhrase.IsInRange(score))
                {
                    return (scorePhrase.GetRandomLocalizedPhrase(), scorePhrase.TextColor);
                }
            }

            // Если не нашли подходящий диапазон, возвращаем дефолтные локализованные значения
            return (Loc.Get("Config/Phrases/DefaultResult"), Color.white);
        }

        public string GetNewRecordPhrase()
        {
            return _newRecordPhrase.GetLocalizedText();
        }
    }
}