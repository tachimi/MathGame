using Bootstrap.Configs;
using Bootstrap.Steps;
using Bootstrap.Steps.Interfaces;
using MathGame.Core;
using MathGame.Screens;
using MathGame.Services;
using MathGame.UI;
using ScreenManager.Core;
using UI.ScrollRect;
using VContainer;
using VContainer.Unity;

namespace Bootstrap
{
    /// <summary>
    /// Точка запуска приложения
    /// Есть поддержка шагов инициализации перед запуском
    /// </summary>
    public class Bootstrapper : IStartable
    {
        private IBootstrapStep[] _bootstrapSteps;
        private SessionScrollKeeper _sessionScrollKeeper;

        [Inject]
        private void Construct(ProjectSettingsConfig projectSettingsConfig, SessionScrollKeeper sessionScrollKeeper)
        {
            _sessionScrollKeeper = sessionScrollKeeper;
            _bootstrapSteps = new IBootstrapStep[]
            {
                new ProjectSettingsInitializeStep(projectSettingsConfig)
            };
        }

        public void Start()
        {
            foreach (var bootstrapStep in _bootstrapSteps)
            {
                bootstrapStep.Execute();
            }
            
            _sessionScrollKeeper.ClearSession();
            //Start app here
            ScreensManager.OpenScreen<MainMenuScreen>();
        }
    }
}