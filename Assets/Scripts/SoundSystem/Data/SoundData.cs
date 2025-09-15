using System;
using SoundSystem.Enums;
using UnityEngine;

namespace SoundSystem.Data
{
    [Serializable]
    public class SoundData
    {
        [field: SerializeField] public SoundType SoundType { get; private set; }
        [field: SerializeField] public AudioClip AudioClip { get; private set; }
    }
}