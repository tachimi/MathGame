using Bootstrap.Configs;
using Bootstrap.Steps.Interfaces;
using UnityEngine;

namespace Bootstrap.Steps
{
    public class ProjectSettingsInitializeStep : IBootstrapStep
    {
        private readonly ProjectSettingsConfig _config;

        public ProjectSettingsInitializeStep(ProjectSettingsConfig config)
        {
            _config = config;
        }

        public void Execute()
        {
            Input.multiTouchEnabled = _config.AllowMultitouch;
            Screen.orientation = _config.Orientation;
            Application.targetFrameRate = _config.TargetFPS;
            Screen.sleepTimeout = _config.SleepTimeout;
        }
    }
}