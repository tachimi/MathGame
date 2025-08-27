using System;
using BalloonsSystem.ScriptableObjects;
using Events;
using Pools.SharedPool;
using UniTaskPubSub;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace BalloonsSystem
{
    public class Balloon : MonoBehaviour, IPoolable, IPointerDownHandler
    {
        public RectTransform RectTransform => _movementController.RectTransform;

        [SerializeField] private BalloonMovementController _movementController;
        [SerializeField] private ParticleSystem _particleSystem;

        private IAsyncPublisher _publisher;
        private Pool<Balloon> _pool;
        private bool _isMoving;

        [Inject]
        private void Construct(IAsyncPublisher publisher)
        {
            _publisher = publisher;
            _movementController.OnHeightLimitReached += StopFlying;
        }

        public void StartFlying()
        {
            _movementController.StartMoving();
        }

        public void StopFlying()
        {
            _movementController.StopMoving();
            Release();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_publisher == null)
            {
                Debug.Log("Pub null");
            }

            _publisher.Publish(new BalloonExplodedEvent(transform.position, this, _particleSystem));
            StopFlying();
        }


        public void InitializePool(object parentPool)
        {
            if (parentPool is Pool<Balloon> pool)
            {
                _pool = pool;
            }
            else
            {
                throw new ArgumentException(
                    $"Expected parentPool to be of type Pool<{typeof(Balloon)}>, but got {parentPool.GetType()}");
            }
        }

        public void OnSpawn()
        {
            gameObject.SetActive(true);
        }

        public void OnDespawn()
        {
            gameObject.SetActive(false);
        }

        public void Release()
        {
            _pool?.Release(this);
        }

        private void OnDestroy()
        {
            _movementController.OnHeightLimitReached -= StopFlying;
        }
    }
}