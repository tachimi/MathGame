using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MathGame.Core
{
    public class AutoSpriteSorter : MonoBehaviour
    {
        [Header("Auto Sorting Configuration")]
        [SerializeField] private RectTransform trackedRectTransform;
        [SerializeField] private bool sortOnEnable = true;
        [SerializeField] private bool trackPositionChanges = true;
        [SerializeField] private SortingSettings sortingSettings = new SortingSettings();

        private SpriteSortingByPosition sortingSystem;
        private Coroutine trackingCoroutine;
        private Vector2 lastTrackedPosition;

        private void Awake()
        {
            InitializeSortingSystem();
        }

        private void OnEnable()
        {
            if (trackedRectTransform == null)
            {
                trackedRectTransform = GetComponent<RectTransform>();
            }

            if (sortOnEnable)
            {
                StartCoroutine(DelayedSort());
            }

            if (trackPositionChanges)
            {
                StartTracking();
            }
        }

        private void OnDisable()
        {
            StopTracking();
        }

        private IEnumerator DelayedSort()
        {
            yield return new WaitForEndOfFrame();
            RefreshAndSort();
        }

        private void InitializeSortingSystem()
        {
            sortingSystem = GetComponent<SpriteSortingByPosition>();
            if (sortingSystem == null)
            {
                sortingSystem = gameObject.AddComponent<SpriteSortingByPosition>();
            }

            sortingSystem.SetSettings(sortingSettings);
        }

        public void RefreshAndSort()
        {
            if (trackedRectTransform != null)
            {
                sortingSystem.AddParent(trackedRectTransform);
            }

            sortingSystem.RefreshTracking();
            UpdateLastTrackedPosition();
        }

        private void StartTracking()
        {
            if (trackingCoroutine != null)
            {
                StopCoroutine(trackingCoroutine);
            }

            trackingCoroutine = StartCoroutine(TrackPositionChanges());
        }

        private void StopTracking()
        {
            if (trackingCoroutine != null)
            {
                StopCoroutine(trackingCoroutine);
                trackingCoroutine = null;
            }
        }

        private IEnumerator TrackPositionChanges()
        {
            while (true)
            {
                yield return new WaitForSeconds(sortingSettings.updateInterval);

                bool hasChanges = CheckForPositionChanges();
                if (hasChanges)
                {
                    sortingSystem.UpdateSortingOrders();
                    UpdateLastTrackedPosition();
                }
            }
        }

        private bool CheckForPositionChanges()
        {
            if (trackedRectTransform == null) return false;

            Vector2 currentPosition = trackedRectTransform.anchoredPosition;
            return Vector2.Distance(lastTrackedPosition, currentPosition) > 0.1f;
        }

        private void UpdateLastTrackedPosition()
        {
            if (trackedRectTransform != null)
            {
                lastTrackedPosition = trackedRectTransform.anchoredPosition;
            }
        }

        [ContextMenu("Force Refresh Sorting")]
        public void ForceRefreshSorting()
        {
            RefreshAndSort();
        }
    }
}