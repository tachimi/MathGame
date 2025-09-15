using System;
using System.Collections.Generic;
using MathGame.Screens;
using MathGame.UI;
using Plugins.ScreenManager.Interfaces;
using ScreenManager.Enums;
using ScreenManager.Interfaces;
using SimpleEventBus.Disposables;
using UnityEngine;

namespace ScreenManager.Loaders
{
    public class SceneScreenSettingsProvider : IScreenSettingsProvider, IDisposable
    {
        private readonly Dictionary<int, SceneScreenSettings> _screenSettings = new Dictionary<int, SceneScreenSettings>();
        private readonly CompositeDisposable _subscriptions;

        public SceneScreenSettingsProvider()
        {
            AddScreenSettings(ScreenId.None, "None");
            AddScreenSettings(typeof(MainMenuScreen), "MainMenu");
            AddScreenSettings(typeof(SettingsScreen), "Settings");
            AddScreenSettings(typeof(OperationSelectionScreen), "OperationSelection");
            AddScreenSettings(typeof(RangeSelectionScreen), "RangeSelection");
            AddScreenSettings(typeof(CardsGameScreen), "CardsGame");
            AddScreenSettings(typeof(BalloonGameScreen), "BalloonGame");
            AddScreenSettings(typeof(ResultScreen), "Result");

            _subscriptions = new CompositeDisposable
            {
                EventStreams.Game.Subscribe<RegisterNewScreenEvent>(RegisterNewScreenHandler),
                EventStreams.Game.Subscribe<GetScreenIdBySceneNameEvent>(GetScreenIdBySceneNameHandler),
            };
        }

        private void GetScreenIdBySceneNameHandler(GetScreenIdBySceneNameEvent eventData)
        {
            foreach (var screenSettings in _screenSettings)
            {
                if (screenSettings.Value.Path == eventData.Scene)
                {
                    eventData.Id = screenSettings.Key;
                    break;
                }
            }
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
        }

        private void RegisterNewScreenHandler(RegisterNewScreenEvent eventData)
        {
            AddScreenSettings(eventData.Id, eventData.Scene, eventData.Name);
        }

        private void AddScreenSettings(Type type, string scene, string customName = null)
        {
            var id = type.Name.GetHashCode();
            _screenSettings[id] = new SceneScreenSettings(scene, customName ?? type.Name);
        }

        private void AddScreenSettings(int id, string scene, string customName = null)
        {
            _screenSettings[id] = new SceneScreenSettings(scene, customName ?? scene);
        }

        public IScreenSettings Get(ScreenId id)
        {
            if (!_screenSettings.TryGetValue(id, out var settings))
            {
                Debug.LogError("Screen settings hasn't found...");
                return null;
            }

            return settings;
        }
    }
}