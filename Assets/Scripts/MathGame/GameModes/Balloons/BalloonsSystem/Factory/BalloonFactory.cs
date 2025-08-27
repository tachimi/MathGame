using Pools.SharedPool;
using UnityEngine;
using VContainer;

namespace BalloonsSystem.Factory
{
    public class BalloonFactory
    {
        private readonly GenericSharedPool<Balloon> _balloonPool;

        [Inject]
        public BalloonFactory(GenericSharedPool<Balloon> balloonPool)
        {
            _balloonPool = balloonPool;
        }

        public Balloon CreateBalloon(Balloon prefab, RectTransform parent)
        {
            return _balloonPool.Take(prefab, parent);
        }
    }
}