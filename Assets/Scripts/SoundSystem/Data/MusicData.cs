using System;
using SoundSystem.Enums;
using UnityEngine;

namespace SoundSystem.Data
{
    [Serializable]
    public class MusicData
    {
        [field: SerializeField] public MusicType MusicType { get; private set; }
        [field: SerializeField] public AudioClip AudioClip { get; private set; }
    }
}