using SoundSystem.Enums;
using SoundSystem.Events;
using UniTaskPubSub;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;
using Random = UnityEngine.Random;

namespace SoundSystem.UI
{
    public class ClickSound : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private SoundType _soundType = SoundType.Select;
        [SerializeField] private bool _loop;
        [SerializeField] private Vector2 _pitch = new(0.8f, 1.2f);

        private IAsyncPublisher _publisher;

        [Inject]
        public void Construct(IAsyncPublisher publisher)
        {
            _publisher = publisher;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var pitch = Random.Range(_pitch.x, _pitch.y);
            _publisher.Publish(new SoundEvent(_soundType, _loop, pitch));
        }
    }
}