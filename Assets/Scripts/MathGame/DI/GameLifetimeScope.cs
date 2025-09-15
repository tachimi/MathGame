using MathGame.Configs;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MathGame.DI
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private BalloonModeConfig _balloonModeConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            // Configurations
            builder.RegisterInstance(_balloonModeConfig);
        }
    }
}