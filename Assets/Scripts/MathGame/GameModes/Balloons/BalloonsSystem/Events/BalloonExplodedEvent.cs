using BalloonsSystem;
using UnityEngine;

namespace Events
{
    public struct BalloonExplodedEvent
    {
        public Balloon Balloon { get; private set; }
        public ParticleSystem ParticleSystem { get; private set; }
        public Vector3 Position { get; private set; }

        public BalloonExplodedEvent(Vector3 position, Balloon balloon, ParticleSystem particleSystem)
        {
            Balloon = balloon;
            ParticleSystem = particleSystem;
            Position = position;
        }
    }
}