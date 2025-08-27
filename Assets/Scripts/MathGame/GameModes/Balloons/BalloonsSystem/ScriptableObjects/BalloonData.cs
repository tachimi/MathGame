using System;
using UnityEngine;

namespace BalloonsSystem.ScriptableObjects
{
    [CreateAssetMenu(fileName = "New BalloonData", menuName = "BalloonSystem/BalloonData")]
    [Serializable]
    public class BalloonData : ScriptableObject
    {
        [field: SerializeField] public Balloon BalloonPrefab { get; set; }
        [field: SerializeField] public ParticleSystem ParticleSystemPrefab { get; set; }
    }
}