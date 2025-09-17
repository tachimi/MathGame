using MathGame.Utils;
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

        private void Start()
        {
            // Если инъекция не сработала, пытаемся найти publisher вручную
            if (_publisher == null)
            {
                TryFindPublisher();
            }
        }

        private void TryFindPublisher()
        {
            _publisher = DependencyResolver.TryGetPublisher();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Проверяем наличие publisher перед использованием
            if (_publisher == null)
            {
                TryFindPublisher();
            }

            if (_publisher != null)
            {
                var pitch = Random.Range(_pitch.x, _pitch.y);
                _publisher.Publish(new SoundEvent(_soundType, _loop, pitch));
            }
        }
    }
}