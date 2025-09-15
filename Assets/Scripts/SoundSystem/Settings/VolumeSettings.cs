using System;
using SoundSystem.Storage;
using UnityEngine;

namespace SoundSystem.Settings
{
    [CreateAssetMenu(menuName = "Audio/VolumeSettings", fileName = "VolumeSettings")]
    [Serializable]
    public class VolumeSettings : ScriptableObject
    {
        [field: SerializeField] public bool MusicEnabled { get; set; }
        [field: SerializeField] public bool SoundEnabled { get; set; }
    }
}