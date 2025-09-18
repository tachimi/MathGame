using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MathGame.Core
{
    [System.Serializable]
    public class SortingSettings
    {
        [Header("Sorting Configuration")]
        [Tooltip("Base sorting order - all objects will be sorted relative to this value")]
        public int baseSortingOrder = 0;

        [Tooltip("Multiplier for Y position to sorting order conversion")]
        public float sortingMultiplier = 100f;

        [Tooltip("Update sorting every frame (performance impact)")]
        public bool updateEveryFrame = false;

        [Tooltip("Update interval in seconds when not updating every frame")]
        public float updateInterval = 0.1f;
    }

    public class SpriteSortingByPosition : MonoBehaviour
    {
        [SerializeField] private SortingSettings settings = new SortingSettings();
        [SerializeField] private List<Transform> trackedParents = new List<Transform>();

        private float lastUpdateTime;
        private Dictionary<Transform, List<SpriteRenderer>> parentSpriteGroups = new Dictionary<Transform, List<SpriteRenderer>>();
        private Dictionary<Transform, Vector2> lastParentPositions = new Dictionary<Transform, Vector2>();

        private void Start()
        {
            InitializeTracking();
            UpdateSortingOrders();
        }

        private void Update()
        {
            if (ShouldUpdate())
            {
                UpdateSortingOrders();
                lastUpdateTime = Time.time;
            }
        }

        private bool ShouldUpdate()
        {
            if (settings.updateEveryFrame)
                return true;

            return Time.time - lastUpdateTime >= settings.updateInterval;
        }

        private void InitializeTracking()
        {
            parentSpriteGroups.Clear();
            lastParentPositions.Clear();

            foreach (var parent in trackedParents)
            {
                if (parent != null)
                {
                    RegisterParent(parent);
                }
            }
        }

        public void UpdateSortingOrders()
        {
            foreach (var parentGroup in parentSpriteGroups)
            {
                var parent = parentGroup.Key;
                var sprites = parentGroup.Value;

                if (parent != null)
                {
                    float yPosition = GetParentYPosition(parent);
                    int baseSortingOrder = CalculateSortingOrder(yPosition);

                    // Применяем сортировку ко всем дочерним спрайтам
                    for (int i = 0; i < sprites.Count; i++)
                    {
                        if (sprites[i] != null)
                        {
                            sprites[i].sortingOrder = baseSortingOrder - i;
                        }
                    }

                    // Обновляем кэшированную позицию родителя
                    UpdateParentPosition(parent);
                }
            }
        }

        private int CalculateSortingOrder(float yPosition)
        {
            // Чем выше Y позиция, тем меньше sorting order (ниже слой)
            return settings.baseSortingOrder - Mathf.RoundToInt(yPosition * settings.sortingMultiplier);
        }

        public void AddParent(Transform parent)
        {
            if (parent == null) return;

            if (!trackedParents.Contains(parent))
            {
                trackedParents.Add(parent);
                RegisterParent(parent);
            }
        }

        public void RemoveParent(Transform parent)
        {
            if (parent == null) return;

            trackedParents.Remove(parent);
            parentSpriteGroups.Remove(parent);
            lastParentPositions.Remove(parent);
        }

        private void RegisterParent(Transform parent)
        {
            if (parent == null) return;

            // Собираем все SpriteRenderer в дочерних объектах
            var sprites = new List<SpriteRenderer>();
            var allSprites = parent.GetComponentsInChildren<SpriteRenderer>();

            foreach (var sprite in allSprites)
            {
                sprites.Add(sprite);
            }

            parentSpriteGroups[parent] = sprites;

            // Сохраняем начальную позицию родителя
            UpdateParentPosition(parent);

            // Немедленно применяем сортировку
            ApplySortingToParentGroup(parent);
        }

        public void RefreshTracking()
        {
            InitializeTracking();
            UpdateSortingOrders();
        }

        public void SetSettings(SortingSettings newSettings)
        {
            settings = newSettings;
            UpdateSortingOrders();
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                UpdateSortingOrders();
            }
        }

        private bool HasParentPositionChanged(Transform parent)
        {
            if (!lastParentPositions.ContainsKey(parent))
                return true;

            float currentYPosition = GetParentYPosition(parent);
            Vector2 lastPosition = lastParentPositions[parent];

            return Mathf.Abs(currentYPosition - lastPosition.y) > 0.1f;
        }

        private float GetParentYPosition(Transform parent)
        {
            var rectTransform = parent.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                return rectTransform.anchoredPosition.y;
            }
            return parent.position.y;
        }

        private void UpdateParentPosition(Transform parent)
        {
            lastParentPositions[parent] = new Vector2(0, GetParentYPosition(parent));
        }

        private void ApplySortingToParentGroup(Transform parent)
        {
            if (!parentSpriteGroups.ContainsKey(parent)) return;

            var sprites = parentSpriteGroups[parent];
            float yPosition = GetParentYPosition(parent);
            int baseSortingOrder = CalculateSortingOrder(yPosition);

            for (int i = 0; i < sprites.Count; i++)
            {
                if (sprites[i] != null)
                {
                    sprites[i].sortingOrder = baseSortingOrder - i;
                }
            }
        }

        // Статический метод для быстрого использования
        public static void SortParentGroup(Transform parent, int baseSortingOrder = 0, float multiplier = 100f)
        {
            if (parent == null) return;

            var sprites = parent.GetComponentsInChildren<SpriteRenderer>();
            float yPosition;

            var rectTransform = parent.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                yPosition = rectTransform.anchoredPosition.y;
            }
            else
            {
                yPosition = parent.position.y;
            }

            int sortingOrder = baseSortingOrder - Mathf.RoundToInt(yPosition * multiplier);

            for (int i = 0; i < sprites.Length; i++)
            {
                sprites[i].sortingOrder = sortingOrder - i;
            }
        }
    }
}