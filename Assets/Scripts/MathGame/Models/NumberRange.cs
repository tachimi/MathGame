using System;
using UnityEngine;

namespace MathGame.Models
{
    [Serializable]
    public class NumberRange
    {
        [field: SerializeField] public int Min { get; set; }
        [field: SerializeField] public int Max { get; set; }

        public NumberRange()
        {
        }

        public NumberRange(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public bool Contains(int number)
        {
            return number >= Min && number <= Max;
        }

        /// <summary>
        /// Получить отображаемый текст для кнопки
        /// </summary>
        public string GetDisplayText()
        {
            return Min == Max ? $"{Min}" : $"{Min}-{Max}";
        }

        /// <summary>
        /// Проверить, входит ли число в диапазон (альтернативное название для Contains)
        /// </summary>
        public bool IsInRange(int number)
        {
            return Contains(number);
        }

        /// <summary>
        /// Получить случайное число из диапазона
        /// </summary>
        public int GetRandomNumber()
        {
            return UnityEngine.Random.Range(Min, Max + 1);
        }
    }
}