using System;
using SoundSystem.Data;
using SoundSystem.Enums;
using UnityEngine;

namespace SoundSystem.Settings
{
    [CreateAssetMenu(menuName = "Audio/MusicTypeSettings", fileName = "MusicTypeSettings")]
    [Serializable]
    public class MusicTypeSettings : ScriptableObject
    {
        [SerializeField] private MusicData[] _musicDatas;

        public AudioClip GetClipByType(MusicType musicType)
        {
            foreach (var musicData in _musicDatas)
            {
                if (musicData.MusicType == musicType)
                {
                    return musicData.AudioClip;
                }
            }

            return null;
        }
    }
}
