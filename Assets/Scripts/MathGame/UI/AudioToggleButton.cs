using MathGame.Utils;
using SoundSystem.Core;
using SoundSystem.Events;
using TMPro;
using UniTaskPubSub;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using AudioType = SoundSystem.Enums.AudioType;

namespace MathGame.UI
{
    /// <summary>
    /// Кнопка переключения звука/музыки с автоматическим обновлением спрайта и текста
    /// </summary>
    public class AudioToggleButton : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioType _audioType = AudioType.Sound;

        [Header("Visual Components")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _buttonImage;

        [Header("Sprites")]
        [SerializeField] private Sprite _onSprite;
        [SerializeField] private Sprite _offSprite;

        private SoundPlayer _soundPlayer;
        private IAsyncPublisher _publisher;

        [Inject]
        public void Construct(SoundPlayer soundPlayer, IAsyncPublisher publisher)
        {
            _soundPlayer = soundPlayer;
            _publisher = publisher;
        }

        private void Awake()
        {
            if (_publisher == null || _soundPlayer == null)
            {
                TryFindDependencies();
            }

            SetupEventHandlers();
            UpdateButtonState();
        }

        private void OnDisable()
        {
            CleanupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(OnButtonClicked);
            }
        }

        private void CleanupEventHandlers()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnButtonClicked);
            }
        }

        private void TryFindDependencies()
        {
            if (_publisher == null)
            {
                _publisher = DependencyResolver.TryGetPublisher();
            }

            if (_soundPlayer == null)
            {
                _soundPlayer = DependencyResolver.TryGetSoundPlayer();
            }
        }

        private void OnButtonClicked()
        {
            var currentState = GetCurrentState();
            var newState = !currentState;

            _publisher.Publish(new VolumeChangedEvent(_audioType, newState));

            UpdateButtonState();
        }

        private bool GetCurrentState()
        {
            return _audioType switch
            {
                AudioType.Sound => _soundPlayer.IsSoundEnabled,
                AudioType.Music => _soundPlayer.IsMusicEnabled,
                _ => false
            };
        }

        private void UpdateButtonState()
        {
            var isEnabled = GetCurrentState();
            _buttonImage.sprite = isEnabled ? _onSprite : _offSprite;
        }
    }
}