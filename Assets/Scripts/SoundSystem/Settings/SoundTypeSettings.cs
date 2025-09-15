using System;
using SoundSystem.Data;
using SoundSystem.Enums;
using UnityEngine;

namespace SoundSystem.Settings
{
    [CreateAssetMenu(menuName = "Audio/SoundTypeSettings", fileName = "SoundTypeSettings")]
    [Serializable]
    public class SoundTypeSettings : ScriptableObject
    {
        [SerializeField] private SoundData[] _soundDatas;

        public AudioClip GetClipByType(SoundType soundType)
        {
            foreach (var soundData in _soundDatas)
            {
                if (soundData.SoundType == soundType)
                {
                    return soundData.AudioClip;
                }
            }

            return null;
        }
    }
}