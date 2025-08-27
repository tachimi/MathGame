using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BalloonsSystem
{
    public class BalloonMovementController : MonoBehaviour
    {
        private const float HEIGHT_LIMIT = 1.1f;

        public event Action OnHeightLimitReached;
        public RectTransform RectTransform => _rectTransform;

        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private float _minSpeed;
        [SerializeField] private float _maxSpeed;

        private Camera _camera;
        private float _currentSpeed;
        private bool _isMoving;

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void FixedUpdate()
        {
            if (!_isMoving) return;

            _rigidbody.AddForce(Vector2.up * _currentSpeed);

            if (ScreenUtils.IsOutOfBounds(transform.position, _camera))
            {
                OnHeightLimitReached?.Invoke();
            }
        }

        public void StartMoving()
        {
            _isMoving = true;
            _currentSpeed = Random.Range(_minSpeed, _maxSpeed);
        }

        public void StopMoving()
        {
            _rigidbody.velocity = Vector2.zero;
            _isMoving = false;
        }
    }
}