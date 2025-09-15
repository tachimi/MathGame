using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.UI
{
    /// <summary>
    /// UI компонент для отображения количества жизней в виде спрайтов (например, сердечки)
    /// </summary>
    public class LivesDisplay : MonoBehaviour
    {
        /// <summary>
        /// Получить текущее количество жизней
        /// </summary>
        public int CurrentLives => _currentLives;
        
        /// <summary>
        /// Получить максимальное количество жизней
        /// </summary>
        public int MaxLives => _maxLives;
        
        [Header("Lives Configuration")]
        [SerializeField] private GameObject _lifeIconPrefab;
        [SerializeField] private RectTransform _livesContainer;
        
        [Header("Visual Settings")]
        [SerializeField] private Sprite _fullLifeSprite;
        [SerializeField] private Sprite _emptyLifeSprite;

        [Header("Shake Animation Settings")]
        [SerializeField] private bool _animateLifeLoss = true;
        [SerializeField] private float _shakeAnimationDuration = 0.6f;
        [SerializeField] private float _shakeStrength = 20f;
        [SerializeField] private int _shakeVibrato = 10;
        
        private List<Image> _lifeIcons = new List<Image>();
        private List<Vector3> _originalPositions = new List<Vector3>(); // Оригинальные позиции иконок
        private int _currentLives;
        private int _maxLives;
        
        /// <summary>
        /// Инициализация компонента с максимальным количеством жизней
        /// </summary>
        /// <param name="maxLives">Максимальное количество жизней</param>
        public void Initialize(int maxLives)
        {
            _maxLives = maxLives;
            _currentLives = maxLives;
            
            CreateLifeIcons();
            UpdateDisplay();
        }
        
        /// <summary>
        /// Обновление отображения количества жизней
        /// </summary>
        /// <param name="currentLives">Текущее количество жизней</param>
        public void UpdateLives(int currentLives)
        {
            if (currentLives == _currentLives) return;
            
            int previousLives = _currentLives;
            _currentLives = Mathf.Clamp(currentLives, 0, _maxLives);
            
            // Если жизней стало меньше и включена анимация потери
            if (_animateLifeLoss && currentLives < previousLives)
            {
                AnimateLifeLoss();
            }
            else
            {
                UpdateDisplay();
            }
        }
        
        /// <summary>
        /// Создание иконок жизней в контейнере
        /// </summary>
        private void CreateLifeIcons()
        {
            // Очищаем существующие иконки
            ClearLifeIcons();

            if (_lifeIconPrefab == null || _livesContainer == null)
            {
                Debug.LogError("LivesDisplay: Не назначены префаб иконки или контейнер!");
                return;
            }

            // Создаем иконки для каждой жизни
            for (int i = 0; i < _maxLives; i++)
            {
                var iconGo = Instantiate(_lifeIconPrefab, _livesContainer);
                var iconImage = iconGo.GetComponent<Image>();

                if (iconImage == null)
                {
                    Debug.LogError("LivesDisplay: Префаб иконки должен содержать компонент Image!");
                    continue;
                }

                _lifeIcons.Add(iconImage);
                // Позиции сохраним после того, как Layout Group их расставит
                _originalPositions.Add(Vector3.zero); // заполняем пока нулями
            }

            // Сохраняем позиции после работы Layout Group
            DOVirtual.DelayedCall(0.1f, () => {
                SaveIconPositions();
            });
        }
        
        /// <summary>
        /// Очистка существующих иконок жизней
        /// </summary>
        private void ClearLifeIcons()
        {
            foreach (var icon in _lifeIcons)
            {
                if (icon != null)
                    DestroyImmediate(icon.gameObject);
            }
            _lifeIcons.Clear();
            _originalPositions.Clear();
        }
        
        /// <summary>
        /// Сохраняем текущие позиции иконок (после работы Layout Group)
        /// </summary>
        private void SaveIconPositions()
        {
            for (int i = 0; i < _lifeIcons.Count && i < _originalPositions.Count; i++)
            {
                if (_lifeIcons[i] != null)
                {
                    _originalPositions[i] = _lifeIcons[i].transform.localPosition;
                }
            }
        }

        /// <summary>
        /// Обновление визуального отображения жизней
        /// </summary>
        private void UpdateDisplay()
        {
            for (int i = 0; i < _lifeIcons.Count; i++)
            {
                if (_lifeIcons[i] == null) continue;

                bool isAlive = i < _currentLives;

                // Устанавливаем спрайт
                if (_fullLifeSprite != null && _emptyLifeSprite != null)
                {
                    _lifeIcons[i].sprite = isAlive ? _fullLifeSprite : _emptyLifeSprite;
                }

                // Сбрасываем масштаб
                _lifeIcons[i].transform.localScale = Vector3.one;

                // Не трогаем позицию - пусть Layout Group ею управляет
                // Позиция восстанавливается только после анимации в AnimateShakeEffect
            }
        }
        
        /// <summary>
        /// Анимация потери жизни с тряской
        /// </summary>
        private void AnimateLifeLoss()
        {
            // Анимируем все иконки жизней (эффект получения урона)
            AnimateShakeEffect();

            // Обновляем отображение после завершения анимации
            DOVirtual.DelayedCall(_shakeAnimationDuration, () => {
                UpdateDisplay();
            });
        }

        /// <summary>
        /// Анимация тряски всех иконок жизней с изменением цвета
        /// </summary>
        private void AnimateShakeEffect()
        {
            for (int i = 0; i < _lifeIcons.Count; i++)
            {
                var lifeIcon = _lifeIcons[i];
                if (lifeIcon == null || i >= _originalPositions.Count) continue;

                // Останавливаем любые текущие анимации для этой иконки
                lifeIcon.transform.DOKill();

                // Получаем оригинальную позицию из сохраненного списка
                var originalPosition = _originalPositions[i];

                // Создаем последовательность анимации
                var sequence = DOTween.Sequence();

                // Тряска влево-вправо (только по X оси)
                sequence.Append(lifeIcon.transform.DOShakePosition(
                    _shakeAnimationDuration,
                    new Vector3(_shakeStrength, 0, 0),
                    _shakeVibrato,
                    90f, // randomness
                    false, // snapping
                    true   // fadeOut
                ));

                // Возвращаем оригинальную позицию в конце
                sequence.AppendCallback(() => {
                    if (lifeIcon != null)
                    {
                        lifeIcon.transform.localPosition = originalPosition;
                    }
                });

                // Устанавливаем ID для возможности остановки
                sequence.SetId(lifeIcon.transform);
            }
        }
        
        /// <summary>
        /// Сброс к максимальному количеству жизней
        /// </summary>
        public void ResetToMaxLives()
        {
            _currentLives = _maxLives;
            UpdateDisplay();
        }
        
        /// <summary>
        /// Изменение максимального количества жизней (пересоздает иконки)
        /// </summary>
        /// <param name="newMaxLives">Новое максимальное количество жизней</param>
        public void SetMaxLives(int newMaxLives)
        {
            if (newMaxLives != _maxLives)
            {
                _maxLives = newMaxLives;
                _currentLives = Mathf.Min(_currentLives, _maxLives);
                CreateLifeIcons();
                UpdateDisplay();
            }
        }
        
        private void OnDestroy()
        {
            // Останавливаем все DOTween анимации этого объекта
            transform.DOKill();

            // Останавливаем анимации всех иконок жизней
            foreach (var lifeIcon in _lifeIcons)
            {
                if (lifeIcon != null)
                {
                    lifeIcon.transform.DOKill();
                }
            }
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// Для тестирования в редакторе
        /// </summary>
        [ContextMenu("Test Life Loss")]
        private void TestLifeLoss()
        {
            if (Application.isPlaying && _currentLives > 0)
            {
                UpdateLives(_currentLives - 1);
            }
        }
        
        [ContextMenu("Test Reset Lives")]
        private void TestResetLives()
        {
            if (Application.isPlaying)
            {
                ResetToMaxLives();
            }
        }
        #endif
    }
}