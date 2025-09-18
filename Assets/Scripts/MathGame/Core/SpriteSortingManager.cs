using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace MathGame.Core
{
    public class SpriteSortingManager : MonoBehaviour
    {
        private static SpriteSortingManager instance;
        public static SpriteSortingManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<SpriteSortingManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("SpriteSortingManager");
                        instance = go.AddComponent<SpriteSortingManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        [Header("Performance Settings")]
        [SerializeField] private int maxUpdatesPerFrame = 10;
        [SerializeField] private float batchUpdateInterval = 0.05f;
        [SerializeField] private bool useParentOptimization = true;

        private Queue<Transform> updateQueue = new Queue<Transform>();
        private HashSet<Transform> queuedParents = new HashSet<Transform>();
        private Dictionary<Transform, ParentSortingData> cachedParentData = new Dictionary<Transform, ParentSortingData>();
        private Coroutine batchUpdateCoroutine;

        private struct ParentSortingData
        {
            public Transform parentTransform;
            public List<SpriteRenderer> childSprites;
            public int baseSortingOrder;
            public float multiplier;
            public Vector2 lastPosition;
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                StartBatchUpdate();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        public void RegisterParent(Transform parent, int baseSortingOrder = 0, float multiplier = 100f)
        {
            if (parent == null) return;

            var childSprites = new List<SpriteRenderer>();
            var allSprites = parent.GetComponentsInChildren<SpriteRenderer>();

            foreach (var sprite in allSprites)
            {
                childSprites.Add(sprite);
            }

            var parentData = new ParentSortingData
            {
                parentTransform = parent,
                childSprites = childSprites,
                baseSortingOrder = baseSortingOrder,
                multiplier = multiplier,
                lastPosition = GetParentPosition(parent)
            };

            cachedParentData[parent] = parentData;
            QueueForUpdate(parent);
        }

        public void UnregisterParent(Transform parent)
        {
            if (parent == null) return;

            cachedParentData.Remove(parent);
            queuedParents.Remove(parent);
        }

        public void QueueForUpdate(Transform parent)
        {
            if (parent == null || queuedParents.Contains(parent)) return;

            updateQueue.Enqueue(parent);
            queuedParents.Add(parent);
        }

        public void UpdateParentSorting(Transform parent)
        {
            if (parent == null || !cachedParentData.ContainsKey(parent)) return;

            var data = cachedParentData[parent];
            if (data.parentTransform == null) return;

            Vector2 currentPosition = GetParentPosition(parent);

            // Проверяем, изменилась ли позиция достаточно для обновления
            if (Vector2.Distance(data.lastPosition, currentPosition) < 0.1f) return;

            int baseSortingOrder = data.baseSortingOrder - Mathf.RoundToInt(currentPosition.y * data.multiplier);

            // Применяем сортировку ко всем дочерним спрайтам
            for (int i = 0; i < data.childSprites.Count; i++)
            {
                if (data.childSprites[i] != null)
                {
                    data.childSprites[i].sortingOrder = baseSortingOrder - i;
                }
            }

            // Обновляем кэшированную позицию
            data.lastPosition = currentPosition;
            cachedParentData[parent] = data;
        }

        private Vector2 GetParentPosition(Transform parent)
        {
            var rectTransform = parent.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                return rectTransform.anchoredPosition;
            }
            return parent.position;
        }

        private void StartBatchUpdate()
        {
            if (batchUpdateCoroutine != null)
            {
                StopCoroutine(batchUpdateCoroutine);
            }

            batchUpdateCoroutine = StartCoroutine(BatchUpdateCoroutine());
        }

        private IEnumerator BatchUpdateCoroutine()
        {
            while (true)
            {
                int updatesThisFrame = 0;

                while (updateQueue.Count > 0 && updatesThisFrame < maxUpdatesPerFrame)
                {
                    var parent = updateQueue.Dequeue();
                    queuedParents.Remove(parent);

                    if (parent != null)
                    {
                        UpdateParentSorting(parent);
                        updatesThisFrame++;
                    }
                }

                yield return new WaitForSeconds(batchUpdateInterval);
            }
        }

        public void ForceUpdateAll()
        {
            foreach (var kvp in cachedParentData)
            {
                UpdateParentSorting(kvp.Key);
            }
        }

        public void RefreshParent(Transform parent)
        {
            if (parent == null || !cachedParentData.ContainsKey(parent)) return;

            var data = cachedParentData[parent];

            // Обновляем список дочерних спрайтов
            data.childSprites.Clear();
            var allSprites = parent.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sprite in allSprites)
            {
                data.childSprites.Add(sprite);
            }

            cachedParentData[parent] = data;
            QueueForUpdate(parent);
        }

        public void SetGlobalSettings(int maxUpdates, float updateInterval)
        {
            maxUpdatesPerFrame = maxUpdates;
            batchUpdateInterval = updateInterval;
        }

        // Статические методы для удобного использования
        public static void RegisterParentStatic(Transform parent, int baseSortingOrder = 0, float multiplier = 100f)
        {
            Instance.RegisterParent(parent, baseSortingOrder, multiplier);
        }

        public static void UnregisterParentStatic(Transform parent)
        {
            Instance.UnregisterParent(parent);
        }

        public static void QueueUpdateStatic(Transform parent)
        {
            Instance.QueueForUpdate(parent);
        }

        public static void RefreshParentStatic(Transform parent)
        {
            Instance.RefreshParent(parent);
        }

        // Debug информация
        [Header("Debug Info")]
        [SerializeField] private bool showDebugInfo = false;

        private void OnGUI()
        {
            if (!showDebugInfo) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"Registered Parents: {cachedParentData.Count}");
            GUILayout.Label($"Update Queue: {updateQueue.Count}");
            GUILayout.Label($"Max Updates/Frame: {maxUpdatesPerFrame}");
            GUILayout.Label($"Update Interval: {batchUpdateInterval:F3}s");

            if (GUILayout.Button("Force Update All"))
            {
                ForceUpdateAll();
            }

            GUILayout.EndArea();
        }
    }
}