using SimpleEventBus.Disposables;
using SoundSystem.Enums;
using SoundSystem.Events;
using SoundSystem.Settings;
using UniTaskPubSub;
using UnityEngine;
using VContainer;
using DG.Tweening;
using SoundSystem.Storage;
using AudioType = SoundSystem.Enums.AudioType;

namespace SoundSystem.Core
{
    public class SoundPlayer : MonoBehaviour
    {
        public bool IsSoundEnabled { get; private set; } = true;
        public bool IsMusicEnabled { get; private set; } = true;
        

        [SerializeField] private AudioSource _audioSourceForSounds;
        [SerializeField] private AudioSource _audioSourceForLoopedSounds;
        [SerializeField] private AudioSource _audioSourceForMusic;

        private CompositeDisposable _subscriptions;
        private SoundTypeSettings _soundTypeSettings;
        private MusicTypeSettings _musicTypeSettings;
        private MusicType _currentMusic;
        private Tweener _musicFadeTween;
        private VolumeData _currentVolume; 

        [Inject]
        public void Construct(IAsyncSubscriber subscriber, SoundTypeSettings soundTypeSettings,
            MusicTypeSettings musicTypeSettings, VolumeSettings volumeSettings)
        {
            _soundTypeSettings = soundTypeSettings;
            _musicTypeSettings = musicTypeSettings;

            _subscriptions?.Dispose();
            _subscriptions = new CompositeDisposable()
            {
                subscriber.Subscribe<SoundEvent>(OnSoundEvent),
                subscriber.Subscribe<MusicEvent>(OnMusicEvent),
                subscriber.Subscribe<VolumeChangedEvent>(OnVolumeChangedEvent),
            };

            LoadVolumeSettings(volumeSettings);
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void LoadVolumeSettings(VolumeSettings fallbackSettings)
        {
            _currentVolume = VolumeStorage.LoadVolume(fallbackSettings);

            IsSoundEnabled = _currentVolume.SoundEnabled;
            IsMusicEnabled = _currentVolume.MusicEnabled;

            _audioSourceForMusic.mute = !_currentVolume.MusicEnabled;
            _audioSourceForSounds.mute = !_currentVolume.SoundEnabled;
            _audioSourceForLoopedSounds.mute = !_currentVolume.SoundEnabled;
        }

        private void SaveVolumeSettings()
        {
            if (_currentVolume != null)
            {
                VolumeStorage.SaveVolume(_currentVolume);
            }
        }

        public void ToggleSound(bool isEnabled)
        {
            IsSoundEnabled = isEnabled;
            _audioSourceForSounds.mute = !isEnabled;
            _audioSourceForLoopedSounds.mute = !isEnabled;

            if (!IsSoundEnabled)
            {
                _audioSourceForSounds.Stop();
                _audioSourceForLoopedSounds.Stop();
            }

            // Обновляем и сохраняем настройки
            if (_currentVolume != null)
            {
                _currentVolume.SoundEnabled = isEnabled;
                SaveVolumeSettings();
            }
        }

        public void ToggleMusic(bool isEnabled)
        {
            IsMusicEnabled = isEnabled;
            _audioSourceForMusic.mute = !isEnabled;
            
            // Обновляем и сохраняем настройки
            if (_currentVolume != null)
            {
                _currentVolume.MusicEnabled = isEnabled;
                SaveVolumeSettings();
            }
        }

        private void OnSoundEvent(SoundEvent eventData)
        {
            if (!IsSoundEnabled)
                return;

            _audioSourceForSounds.pitch = eventData.Pitch;
            if (eventData.Loop)
            {
                PlayLoopSound(_soundTypeSettings.GetClipByType(eventData.SoundType));
            }
            else
            {
                PlaySound(_soundTypeSettings.GetClipByType(eventData.SoundType));
            }
        }

        private void OnMusicEvent(MusicEvent eventData)
        {
            if (eventData.MusicType == _currentMusic)
            {
                return;
            }

            _currentMusic = eventData.MusicType;

            if (!IsMusicEnabled)
                return;

            AudioClip newClip = _musicTypeSettings.GetClipByType(eventData.MusicType);

            PlayMusic(newClip);
        }

        private void OnVolumeChangedEvent(VolumeChangedEvent changedEventData)
        {
            switch (changedEventData.AudioType)
            {
                case AudioType.Music:
                    ToggleMusic(changedEventData.Enabled);
                    break;
                case AudioType.Sound:
                    ToggleSound(changedEventData.Enabled);
                    break;
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip == null)
            {
                _audioSourceForSounds.Stop();
                return;
            }

            _audioSourceForSounds.PlayOneShot(clip);
        }

        private void PlayLoopSound(AudioClip clip)
        {
            if (clip == null)
            {
                _audioSourceForLoopedSounds.Stop();
                _audioSourceForLoopedSounds.clip = null;
                return;
            }

            _audioSourceForLoopedSounds.clip = clip;
            _audioSourceForLoopedSounds.Play();
        }

        private void PlayMusic(AudioClip clip)
        {
            if (clip == null || !IsMusicEnabled)
            {
                _audioSourceForMusic.Stop();
                return;
            }

            // Простое воспроизведение без fade эффектов
            _audioSourceForMusic.clip = clip;
            _audioSourceForMusic.loop = true;
            _audioSourceForMusic.Play();
        }

        private void OnDestroy()
        {
            _musicFadeTween?.Kill();
            _subscriptions?.Dispose();
            _subscriptions = null;
        }
    }
}