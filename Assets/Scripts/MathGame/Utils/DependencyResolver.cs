using SoundSystem.Core;
using UniTaskPubSub;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MathGame.Utils
{
    /// <summary>
    /// Утилитарный класс для разрешения зависимостей когда VContainer инъекция не работает
    /// </summary>
    public static class DependencyResolver
    {
        /// <summary>
        /// Попытаться получить IAsyncPublisher
        /// </summary>
        public static IAsyncPublisher TryGetPublisher()
        {
            try
            {
                // Пытаемся получить через VContainer
                var rootScope = VContainerSettings.Instance?.RootLifetimeScope;
                if (rootScope != null && rootScope.Container.TryResolve<IAsyncPublisher>(out var publisher))
                {
                    return publisher;
                }
            }
            catch
            {
                // Игнорируем ошибки
            }

            return null;
        }

        /// <summary>
        /// Попытаться получить SoundPlayer
        /// </summary>
        public static SoundPlayer TryGetSoundPlayer()
        {
            try
            {
                // Пытаемся получить через VContainer
                var rootScope = VContainerSettings.Instance?.RootLifetimeScope;
                if (rootScope != null && rootScope.Container.TryResolve<SoundPlayer>(out var soundPlayer))
                {
                    return soundPlayer;
                }
            }
            catch
            {
                // Игнорируем ошибки
            }

            // Если VContainer не работает, пытаемся найти через FindObjectOfType
            try
            {
                return Object.FindObjectOfType<SoundPlayer>();
            }
            catch
            {
                // Игнорируем ошибки
            }

            return null;
        }
    }
}