using SoundSystem.Enums;

namespace SoundSystem.Events
{
    public struct VolumeChangedEvent
    {
        public AudioType AudioType { get; set; }
        public bool Enabled { get; set; }

        public VolumeChangedEvent(AudioType type, bool enabled)
        {
            AudioType = type;
            Enabled = enabled;
        }
    }
}