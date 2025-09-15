using SoundSystem.Enums;

namespace SoundSystem.Events
{
    public struct SoundEvent
    {
        public SoundType SoundType { get; }
        public bool Loop { get; }
        public float Pitch { get; }

        public SoundEvent(SoundType soundType, bool loop = false, float pitch = 1f)
        {
            SoundType = soundType;
            Loop = loop;
            Pitch = pitch;
        }
    }
}