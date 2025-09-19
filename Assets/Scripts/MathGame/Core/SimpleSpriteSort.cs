using UnityEngine;

namespace MathGame.Core
{
    /// <summary>
    /// Сортирует спрайты по вертикальному положению на ЭКРАНЕ:
    /// чем выше на экране — тем МЕНЬШЕ sortingOrder; чем ниже — тем БОЛЬШЕ.
    /// </summary>
    [ExecuteAlways]
    public class SimpleSpriteSort : MonoBehaviour
    {
        [System.Serializable]
        public class SpriteData
        {
            public SpriteRenderer Sprite;
            public int Offset;
        }

        [Header("Target References")]
        [SerializeField] private Transform _trackedTransform;   // можно дать RectTransform, он наследует Transform

        [Header("Multiple Sprites")]
        [SerializeField] private SpriteData[] _sprites;

        [Header("Sorting Settings")]
        [SerializeField] private float _multiplier = 1f;        // шаг чувствительности сортировки
        [SerializeField] private float _yOffset = 0f;           // смещение базовой линии в пикселях screen-space
        [SerializeField] private bool _autoOffsetHalfScreen = true;

        [Header("Camera")]
        [SerializeField] private Camera _camera;                // если не указана, возьмём Camera.main

        private Camera Cam => _camera != null ? _camera : Camera.main;

        private void Start()
        {
            if (_autoOffsetHalfScreen)
                CalculateScreenOffset();
            UpdateSorting();
        }

        private void Update()
        {
            UpdateSorting();
        }

        private void UpdateSorting()
        {
            if (_trackedTransform == null) return;
            if (Cam == null) return;

            // Экранная Y (0 внизу экрана, растёт вверх)
            float screenY = Cam.WorldToScreenPoint(_trackedTransform.position).y;

            // Вычитаем опорную линию (по умолчанию — середина экрана, см. CalculateScreenOffset)
            float adjustedY = screenY - _yOffset;

            // Чем выше на экране (adjustedY > 0), тем МЕНЬШЕ порядок; ниже — БОЛЬШЕ.
            int baseSortingOrder = Mathf.RoundToInt(-adjustedY * _multiplier);

            if (_sprites != null)
            {
                for (int i = 0; i < _sprites.Length; i++)
                {
                    var data = _sprites[i];
                    if (data?.Sprite == null) continue;
                    data.Sprite.sortingOrder = baseSortingOrder + data.Offset;
                }
            }
        }

        /// <summary>
        /// Положить опорную линию в центр экрана (в пикселях screen-space).
        /// </summary>
        [ContextMenu("Calculate Half Screen Offset")]
        public void CalculateScreenOffset()
        {
            // Для screen-space логики правильнее брать именно Screen.height
            _yOffset = Screen.height * 0.5f; // центр экрана
            _autoOffsetHalfScreen = false;
            UpdateSorting();
        }
    }
}