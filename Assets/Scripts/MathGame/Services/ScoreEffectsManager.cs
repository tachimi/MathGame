using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SoundSystem.Enums;

namespace MathGame.Services
{
    /// <summary>
    /// Менеджер эффектов при достижении определенных очков
    /// </summary>
    public class ScoreEffectsManager : MonoBehaviour
    {
        [Header("Milestone Settings")]
        [SerializeField] private int[] _milestoneThresholds = { 10, 25, 50, 100, 200 };
        [SerializeField] private ParticleSystem _celebrationEffect;
        [SerializeField] private SoundType _achievementSound;

        [Header("Effect Settings")]
        [SerializeField] private Transform _effectSpawnPoint;

        // События для уведомлений
        public event Action<int> OnMilestoneReached; // score threshold
        public event Action<SoundType> OnPlaySound;

        private HashSet<int> _achievedMilestones = new HashSet<int>();

        private void Awake()
        {
            // Если не назначена точка спавна, используем центр экрана
            if (_effectSpawnPoint == null)
            {
                _effectSpawnPoint = transform;
            }
        }

        /// <summary>
        /// Проверить достижение милестоунов при изменении счета
        /// </summary>
        public void CheckMilestones(int currentScore)
        {
            foreach (var threshold in _milestoneThresholds)
            {
                // Проверяем, достигнут ли порог и не был ли уже получен
                if (currentScore >= threshold && !_achievedMilestones.Contains(threshold))
                {
                    TriggerMilestone(threshold);
                    _achievedMilestones.Add(threshold);
                }
            }
        }

        /// <summary>
        /// Запустить эффект достижения милестоуна
        /// </summary>
        private void TriggerMilestone(int threshold)
        {
            // Воспроизводим звук
            OnPlaySound?.Invoke(_achievementSound);

            // Запускаем эффект салюта
            if (_celebrationEffect != null)
            {
                PlayCelebrationEffect();
            }

            // Уведомляем о достижении
            OnMilestoneReached?.Invoke(threshold);

            Debug.Log($"🎉 Milestone reached: {threshold} points!");
        }

        /// <summary>
        /// Воспроизвести эффект салюта
        /// </summary>
        private void PlayCelebrationEffect()
        {
            if (_celebrationEffect == null) return;

            // Создаем временный эффект
            var effectInstance = Instantiate(_celebrationEffect, _effectSpawnPoint.position, Quaternion.identity);

            // Устанавливаем цвет
            var main = effectInstance.main;

            // Воспроизводим эффект
            effectInstance.Play();

            // Уничтожаем после завершения
            StartCoroutine(DestroyEffectAfterDelay(effectInstance, effectInstance.main.duration + effectInstance.main.startLifetime.constantMax));
        }

        /// <summary>
        /// Уничтожить эффект через заданное время
        /// </summary>
        private IEnumerator DestroyEffectAfterDelay(ParticleSystem effect, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (effect != null)
            {
                Destroy(effect.gameObject);
            }
        }
    }
}