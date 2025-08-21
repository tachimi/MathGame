using System;
using UnityEngine;

namespace Bootstrap.Configs
{
    [CreateAssetMenu(fileName = "ProjectSettingsConfig", menuName = "Project/ProjectSettingsConfig")]
    [Serializable]
    public class ProjectSettingsConfig : ScriptableObject
    {
        [field: SerializeField] public bool AllowMultitouch { get; private set; }
        [field: SerializeField] public ScreenOrientation Orientation { get; private set; }
        [field: SerializeField] public int TargetFPS { get; private set; }
        [field: SerializeField] public int SleepTimeout { get; private set; } = -1;
        [field: SerializeField] public bool CursorVisible { get; private set; } = true;
        [field: SerializeField] public CursorLockMode CursorLockMode { get; private set; } = CursorLockMode.None;
    }
}