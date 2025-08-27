using System;
using MathGame.Models;
using MathGame.Settings;
using UnityEngine;

namespace MathGame.Interfaces
{
    /// <summary>
    /// Интерфейс для всех игровых режимов
    /// </summary>
    public interface IMathGameMode
    {
        /// <summary>
        /// Событие выбора ответа игроком
        /// </summary>
        event Action<int> OnAnswerSelected;
        
        /// <summary>
        /// Событие завершения раунда (готовность к переходу к следующему вопросу)
        /// </summary>
        event Action OnRoundComplete;
        
        /// <summary>
        /// Инициализация режима игры с настройками
        /// </summary>
        /// <param name="settings">Настройки игры</param>
        /// <param name="parentContainer">Родительский контейнер для UI элементов</param>
        void Initialize(GameSettings settings, RectTransform parentContainer);
        
        /// <summary>
        /// Установка нового вопроса
        /// </summary>
        /// <param name="question">Вопрос для отображения</param>
        void SetQuestion(Question question);
        
        /// <summary>
        /// Начало раунда (показ вопроса и активация интерфейса)
        /// </summary>
        void StartRound();
        
        /// <summary>
        /// Завершение раунда (подготовка к следующему вопросу)
        /// </summary>
        void EndRound();
        
        /// <summary>
        /// Очистка ресурсов
        /// </summary>
        void Cleanup();
        
        /// <summary>
        /// Проверка, завершен ли текущий раунд
        /// </summary>
        bool IsRoundComplete { get; }
        
        /// <summary>
        /// Получить текущий вопрос
        /// </summary>
        Question CurrentQuestion { get; }
    }
}