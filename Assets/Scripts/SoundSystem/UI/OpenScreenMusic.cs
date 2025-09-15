using SoundSystem.Enums;
using SoundSystem.Events;
using UniTaskPubSub;
using UnityEngine;
using VContainer;

namespace SoundSystem.UI
{
    public class OpenScreenMusic : MonoBehaviour
    {
        [SerializeField] private MusicType _musicType;

        private IAsyncPublisher _publisher;

        [Inject]
        public void Construct(IAsyncPublisher publisher)
        {
            _publisher = publisher;
        }

        private void OnEnable()
        {
            _publisher.Publish(new MusicEvent(_musicType));
        }
    }
}