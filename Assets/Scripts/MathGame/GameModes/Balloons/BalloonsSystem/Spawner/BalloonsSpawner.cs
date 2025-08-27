using System;
using System.Collections.Generic;
using BalloonsSystem.Factory;
using Events;
using SimpleEventBus.Disposables;
using UniTaskPubSub;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace BalloonsSystem.Spawner
{
    public class BalloonsSpawner : MonoBehaviour
    {
        [SerializeField] private List<Balloon> _balloonPrefabs;
        [SerializeField] private RectTransform _balloonContainer;
        [SerializeField] private SpawnType _spawnType;
        [SerializeField] private float _spawnXOffset = 100f;
        [SerializeField] private float _spawnInterval;

        private BalloonFactory _balloonFactory;
        private IDisposable _subscriptions;
        private List<Balloon> _activeBalloons = new();
        private bool _canSpawn;
        private float _elapsedTime;
        private int _nextPrefabIndex = -1;

        [Inject]
        public void Construct(BalloonFactory balloonFactory, IAsyncSubscriber subscriber)
        {
            _balloonFactory = balloonFactory;

            _subscriptions?.Dispose();
            _subscriptions = new CompositeDisposable()
            {
                subscriber.Subscribe<BalloonExplodedEvent>(OnBalloonExploded)
            };
        }

        public void StartSpawn()
        {
            _nextPrefabIndex = -1;
            _canSpawn = true;
        }

        private void Update()
        {
            if (!_canSpawn) return;

            _elapsedTime += Time.deltaTime;
            if (_elapsedTime >= _spawnInterval)
            {
                SpawnBalloon();
                _elapsedTime = 0;
            }
        }

        private void SpawnBalloon()
        {
            Balloon prefab;

            switch (_spawnType)
            {
                case SpawnType.Random:
                    prefab = GetRandomPrefab();
                    break;
                case SpawnType.InOrder:
                    prefab = GetNextPrefab();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var balloon = _balloonFactory.CreateBalloon(prefab, _balloonContainer);
            balloon.RectTransform.anchoredPosition = GetRandomPosition();
            balloon.transform.rotation = Quaternion.identity;
            balloon.StartFlying();
            _activeBalloons.Add(balloon);
        }

        private Balloon GetRandomPrefab()
        {
            var randomValue = Random.Range(0, _balloonPrefabs.Count);
            return _balloonPrefabs[randomValue];
        }

        private Balloon GetNextPrefab()
        {
            _nextPrefabIndex = (_nextPrefabIndex + 1) % _balloonPrefabs.Count;  
            return _balloonPrefabs[_nextPrefabIndex];
        }
        private void OnBalloonExploded(BalloonExplodedEvent data)
        {
            _activeBalloons.Remove(data.Balloon);
        }

        private Vector3 GetRandomPosition()
        {
            var randomX = Random.Range(_balloonContainer.rect.xMin + _spawnXOffset,
                _balloonContainer.rect.xMax - _spawnXOffset);
            return new Vector3(randomX, _balloonContainer.rect.yMin, 0);
        }

        public void StopSpawn()
        {
            _canSpawn = false;
            _elapsedTime = 0;

            Clear();
        }

        private void Clear()
        {
            foreach (var activeBalloon in _activeBalloons)
            {
                activeBalloon.StopFlying();
            }

            _activeBalloons.Clear();
        }

        private void OnDestroy()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
        }
    }

    public enum SpawnType
    {
        Random = 0,
        InOrder = 1
    }
}