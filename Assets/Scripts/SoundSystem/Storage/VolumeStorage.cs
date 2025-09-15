using Newtonsoft.Json;
using SoundSystem.Settings;
using UnityEngine;

namespace SoundSystem.Storage
{
    public static class VolumeStorage
    {
        private static readonly string VOLUME_KEY = "Volume";

        public static void SaveVolume(VolumeData volumeData)
        {
            string json = JsonConvert.SerializeObject(volumeData);
            PlayerPrefs.SetString(VOLUME_KEY, json);
            PlayerPrefs.Save();
        }

        public static VolumeData LoadVolume(VolumeSettings fallbackSettings = null)
        {
            if (!PlayerPrefs.HasKey(VOLUME_KEY))
            {
                // Дефолтные значения если настройки не заданы
                bool musicEnabled = fallbackSettings?.MusicEnabled ?? true;
                bool soundEnabled = fallbackSettings?.SoundEnabled ?? true;

                return new VolumeData(musicEnabled, soundEnabled);
            }

            string json = PlayerPrefs.GetString(VOLUME_KEY);
            var volumeData = JsonConvert.DeserializeObject<VolumeData>(json);
            return volumeData;
        }
    }

    [System.Serializable]
    public class VolumeData
    {
        public bool MusicEnabled { get; set; }
        public bool SoundEnabled { get; set; }

        public VolumeData(bool musicEnabled, bool soundEnabled)
        {
            MusicEnabled = musicEnabled;
            SoundEnabled = soundEnabled;
        }
    }
}