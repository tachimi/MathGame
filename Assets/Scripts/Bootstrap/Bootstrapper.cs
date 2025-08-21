using Bootstrap.Steps.Interfaces;
using MathGame.UI;
using ScreenManager.Core;
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

        [Inject]
        private void Construct()
        {
            _bootstrapSteps = new IBootstrapStep[]
            {
            };
        }

        public void Start()
        {
            foreach (var bootstrapStep in _bootstrapSteps)
            {
                bootstrapStep.Execute();
            }

            //Start app here
            ScreensManager.OpenScreen<MainMenuScreen>();
        }
    }
}