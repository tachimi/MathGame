using Bootstrap;
using Bootstrap.Configs;
using SoundSystem.Core;
using SoundSystem.Settings;
using UI.ScrollRect;
using UniTaskPubSub;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MathGame.DI
{
    public class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] private SoundPlayer _soundPlayer;
        [SerializeField] private ProjectSettingsConfig _projectSettingsConfig;
        [SerializeField] private SoundTypeSettings _soundTypeSettings;
        [SerializeField] private MusicTypeSettings _musicTypeSettings;
        [SerializeField] private VolumeSettings _volumeSettings;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<AsyncMessageBus>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.RegisterInstance(_soundPlayer);
            builder.RegisterInstance(_projectSettingsConfig);
            builder.RegisterInstance(_soundTypeSettings);
            builder.RegisterInstance(_musicTypeSettings);
            builder.RegisterInstance(_volumeSettings);

            builder.Register<SessionScrollKeeper>(Lifetime.Singleton);

            builder.RegisterEntryPoint<Bootstrapper>();
        }
    }
}