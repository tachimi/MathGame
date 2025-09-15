using UnityEngine;
using System.Collections.Generic;
using MathGame.Configs;
using VContainer;

namespace MathGame.GameModes.Balloons.BalloonsSystem
{
    /// <summary>
    /// Менеджер эффектов для шариков (партиклы, звуки)
    /// </summary>
    public class BalloonEffectsManager : MonoBehaviour
    {
        [SerializeField] private int _particlePoolSize = 10;
        [SerializeField] private Transform _effectsContainer;
        
        private Queue<ParticleSystem> _particlePool = new Queue<ParticleSystem>();
        private BalloonModeConfig _config;

        [Inject]
        public void Construct(BalloonModeConfig config)
        {
            _config = config;
        }
        
        private void Awake()
        {
            InitializeParticlePool();
        }

        private void InitializeParticlePool()
        {
            for (int i = 0; i < _particlePoolSize; i++)
            {
                var particle = Instantiate(_config.PopEffectPrefab, _effectsContainer);
                particle.gameObject.SetActive(false);
                _particlePool.Enqueue(particle);
            }
        }

        /// <summary>
        /// Воспроизвести эффект лопания шарика
        /// </summary>
        public void PlayPopEffect(Vector3 position, Color balloonColor)
        {
            if (_particlePool.Count == 0) return;

            ParticleSystem particle = GetPooledParticle();
            if (particle == null) return;

            // Устанавливаем позицию
            particle.transform.position = position;

            // Активируем и воспроизводим
            particle.gameObject.SetActive(true);
            particle.Play();

            // Возвращаем в пул через время жизни
            StartCoroutine(ReturnToPoolAfterDelay(particle, particle.main.duration));
        }

        private ParticleSystem GetPooledParticle()
        {
            if (_particlePool.Count > 0)
            {
                return _particlePool.Dequeue();
            }

            // Если пул пуст, создаем новую частицу
            var newParticle = Instantiate(_config.PopEffectPrefab, _effectsContainer);
            return newParticle;
        }

        private void SetParticleColor(ParticleSystem particle, Color color)
        {
            var main = particle.main;
            main.startColor = color;

            // Если есть дочерние системы частиц, меняем и их цвет
            ParticleSystem[] childParticles = particle.GetComponentsInChildren<ParticleSystem>();
            foreach (var child in childParticles)
            {
                var childMain = child.main;
                childMain.startColor = color;
            }
        }

        private System.Collections.IEnumerator ReturnToPoolAfterDelay(ParticleSystem particle, float delay)
        {
            yield return new WaitForSeconds(delay);

            particle.Stop();
            particle.Clear();
            particle.gameObject.SetActive(false);
            _particlePool.Enqueue(particle);
        }

        /// <summary>
        /// Подписаться на события шариков
        /// </summary>
        public void SubscribeToBalloon(BalloonAnswer balloon)
        {
            if (balloon == null) return;

            // Подписываемся на событие лопания
            balloon.OnBalloonPopped += OnBalloonPopped;
        }

        /// <summary>
        /// Отписаться от событий шарика
        /// </summary>
        public void UnsubscribeFromBalloon(BalloonAnswer balloon)
        {
            if (balloon == null) return;

            balloon.OnBalloonPopped -= OnBalloonPopped;
        }

        private void OnBalloonPopped(BalloonAnswer balloon, Vector3 position, Color color)
        {
            PlayPopEffect(position, color);
        }

        private void OnDestroy()
        {
            // Очищаем пул
            while (_particlePool.Count > 0)
            {
                var particle = _particlePool.Dequeue();
                if (particle != null)
                {
                    Destroy(particle.gameObject);
                }
            }
        }
    }
}