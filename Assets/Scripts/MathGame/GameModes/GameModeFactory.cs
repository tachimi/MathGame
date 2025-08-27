using System;
using MathGame.Configs;
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
    public class GameModeFactory
    {
        private readonly BalloonModeConfig _balloonConfig;
        
        /// <summary>
        /// Конструктор с инжекцией зависимостей
        /// </summary>
        public GameModeFactory(BalloonModeConfig balloonConfig = null)
        {
            _balloonConfig = balloonConfig;
        }
        
        /// <summary>
        /// Создает экземпляр игрового режима на основе типа
        /// </summary>
        /// <param name="gameType">Тип игрового режима</param>
        /// <param name="settings">Настройки игры</param>
        /// <param name="parentContainer">Родительский контейнер для UI</param>
        /// <returns>Экземпляр игрового режима</returns>
        public IMathGameMode Create(GameType gameType, GameSettings settings, RectTransform parentContainer)
        {
            IMathGameMode gameMode = gameType switch
            {
                GameType.Cards => new CardGameMode(),
                GameType.Balloons => CreateBalloonMode(),
                GameType.Grid => throw new NotImplementedException("Grid game mode coming soon!"),
                _ => throw new ArgumentException($"Unsupported game type: {gameType}")
            };
            
            // Инициализируем созданный режим
            gameMode.Initialize(settings, parentContainer);
            
            return gameMode;
        }
        
        /// <summary>
        /// Создает режим Balloons с конфигом
        /// </summary>
        private IMathGameMode CreateBalloonMode()
        {
            if (_balloonConfig == null)
            {
                Debug.LogWarning("BalloonModeConfig not injected! Loading from Resources as fallback...");
                var config = Resources.Load<BalloonModeConfig>("Configs/BalloonModeConfig");
                if (config == null)
                {
                    throw new InvalidOperationException("BalloonModeConfig not found! Please create one in Resources/Configs/");
                }
                return new BalloonGameMode(config);
            }
            
            return new BalloonGameMode(_balloonConfig);
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
                GameType.Balloons => true,
                _ => false
            };
        }
    }
}