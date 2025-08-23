using System;
using MathGame.Enums;
using MathGame.GameModes.Balloons;
using MathGame.GameModes.Cards;
using MathGame.Interfaces;
using MathGame.Settings;
using UnityEngine;

namespace MathGame.GameModes
{
    /// <summary>
    /// Фабрика для создания различных игровых режимов
    /// </summary>
    public static class GameModeFactory
    {
        /// <summary>
        /// Создает экземпляр игрового режима на основе типа
        /// </summary>
        /// <param name="gameType">Тип игрового режима</param>
        /// <param name="settings">Настройки игры</param>
        /// <param name="parentContainer">Родительский контейнер для UI</param>
        /// <returns>Экземпляр игрового режима</returns>
        public static IMathGameMode Create(GameType gameType, GameSettings settings, Transform parentContainer)
        {
            IMathGameMode gameMode = gameType switch
            {
                GameType.Cards => new CardGameMode(),
                GameType.Balloons => new BalloonGameMode(),
                GameType.Grid => throw new NotImplementedException("Grid game mode coming soon!"),
                _ => throw new ArgumentException($"Unsupported game type: {gameType}")
            };
            
            // Инициализируем созданный режим
            gameMode.Initialize(settings, parentContainer);
            
            return gameMode;
        }
        
        /// <summary>
        /// Проверяет, поддерживается ли указанный тип игры
        /// </summary>
        /// <param name="gameType">Тип игрового режима</param>
        /// <returns>True если поддерживается</returns>
        public static bool IsSupported(GameType gameType)
        {
            return gameType switch
            {
                GameType.Cards => true,
                GameType.Balloons => true, // Теперь реализован
                GameType.Grid => false,    // Пока не реализован
                _ => false
            };
        }
        
        /// <summary>
        /// Получает отображаемое имя режима игры
        /// </summary>
        /// <param name="gameType">Тип игрового режима</param>
        /// <returns>Отображаемое имя</returns>
        public static string GetDisplayName(GameType gameType)
        {
            return gameType switch
            {
                GameType.Cards => "Карточки",
                GameType.Balloons => "Шарики",
                GameType.Grid => "Сетка",
                _ => "Неизвестный режим"
            };
        }
    }
}