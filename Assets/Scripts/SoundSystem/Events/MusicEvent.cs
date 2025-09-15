using SoundSystem.Enums;

namespace SoundSystem.Events
{
    public struct MusicEvent 
    {
        public MusicType MusicType { get; }

        public MusicEvent(MusicType musicType)
        {
            MusicType = musicType;
        }
    }
}
