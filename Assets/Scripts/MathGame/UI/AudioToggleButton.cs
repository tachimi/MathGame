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
            SetupComponents();
        }

        private void OnEnable()
        {
            SetupEventHandlers();
            UpdateButtonState();
        }

        private void OnDisable()
        {
            CleanupEventHandlers();
        }

        private void SetupComponents()
        {
            // Автоматически находим компоненты если не назначены
            if (_button == null)
                _button = GetComponent<Button>();

            if (_buttonImage == null)
                _buttonImage = GetComponent<Image>();
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

        private void OnButtonClicked()
        {
            if (_soundPlayer == null || _publisher == null) return;

            bool currentState = GetCurrentState();
            bool newState = !currentState;

            // Отправляем событие изменения
            _publisher.Publish(new VolumeChangedEvent(_audioType, newState));

            // Обновляем состояние кнопки
            UpdateButtonState();
        }

        private bool GetCurrentState()
        {
            if (_soundPlayer == null) return false;

            return _audioType switch
            {
                AudioType.Sound => _soundPlayer.IsSoundEnabled,
                AudioType.Music => _soundPlayer.IsMusicEnabled,
                _ => false
            };
        }

        private void UpdateButtonState()
        {
            bool isEnabled = GetCurrentState();

            // Обновляем спрайт
            if (_buttonImage != null)
            {
                _buttonImage.sprite = isEnabled ? _onSprite : _offSprite;
            }
        }

        /// <summary>
        /// Принудительное обновление состояния кнопки (для внешнего вызова)
        /// </summary>
        public void RefreshState()
        {
            UpdateButtonState();
        }
    }
}