using System;
using Cysharp.Threading.Tasks;
using UI.ScrollRect.PageIndicators;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace UI.ScrollRect.Core
{
    public class SnapScrollRect : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        public event Action<int, string> OnPageChanged;

        [SerializeField] private UnityEngine.UI.ScrollRect _scrollRect;
        [SerializeField] private ScrollRectPage[] _pages;
        [SerializeField] private ActivePageIndicatorController _indicatorController;
        [SerializeField] private Button _nextPageButton;
        [SerializeField] private Button _previousPageButton;
        [SerializeField] private float _snapSpeed = 10f;
        [SerializeField] private float _minSwipeDistance = 5f;
        [SerializeField] private bool _forgetLastPage;
        [SerializeField] private bool _infiniteScroll = true;

        private LastPageKeeper _lastPageKeeper;
        private Vector2 _dragStartPosition;
        private int _currentPage;
        private bool _isSnapping;

        [Inject]
        public void Construct(LastPageKeeper lastPageKeeper)
        {
            _lastPageKeeper = lastPageKeeper;
        }

        private async void Awake()
        {
            if (_indicatorController != null)
            {
                _indicatorController.Initialize(_pages.Length, ref OnPageChanged);
            }

            if (!_forgetLastPage)
            {
                _currentPage = Mathf.Clamp(_lastPageKeeper.CurrentPage, 0, _pages.Length - 1);
            }

            if (_currentPage >= 0 && _currentPage < _pages.Length)
            {
                OnPageChanged?.Invoke(_currentPage, _pages[_currentPage].Name);
            }

            if (!_forgetLastPage)
            {
                await UniTask.NextFrame();
                SetInitialScrollPosition();
            }

            if (_nextPageButton != null)
            {
                _nextPageButton.onClick.AddListener(NextPage);
            }

            if (_previousPageButton != null)
            {
                _previousPageButton.onClick.AddListener(PreviousPage);
            }
        }

        private void SetInitialScrollPosition()
        {
            var targetPos = GetPagePosition(_currentPage);
            _scrollRect.horizontalNormalizedPosition = targetPos;
        }

        private void Update()
        {
            if (_isSnapping)
            {
                var targetPos = GetPagePosition(_currentPage);
                _scrollRect.horizontalNormalizedPosition = Mathf.Lerp(
                    _scrollRect.horizontalNormalizedPosition,
                    targetPos,
                    Time.deltaTime * _snapSpeed
                );

                if (Mathf.Abs(_scrollRect.horizontalNormalizedPosition - targetPos) < 0.001f)
                {
                    _scrollRect.horizontalNormalizedPosition = targetPos;
                    _isSnapping = false;
                }
            }
        }

        private void NextPage()
        {
            if (_infiniteScroll)
            {
                _currentPage = (_currentPage + 1) % _pages.Length;
            }
            else if (_currentPage < _pages.Length - 1)
            {
                _currentPage++;
            }

            if (_currentPage >= 0 && _currentPage < _pages.Length)
            {
                OnPageChanged?.Invoke(_currentPage, _pages[_currentPage].Name);
            }

            _isSnapping = true;
        }

        private void PreviousPage()
        {
            if (_infiniteScroll)
            {
                _currentPage = (_currentPage - 1 + _pages.Length) % _pages.Length;
            }
            else if (_currentPage > 0)
            {
                _currentPage--;
            }

            if (_currentPage >= 0 && _currentPage < _pages.Length)
            {
                OnPageChanged?.Invoke(_currentPage, _pages[_currentPage].Name);
            }

            _isSnapping = true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isSnapping = false;
            _dragStartPosition = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var dragDistance = (eventData.position - _dragStartPosition).x;

            if (Mathf.Abs(dragDistance) >= _minSwipeDistance)
            {
                if (dragDistance < 0) // Swipe left (next page)
                {
                    NextPage();
                }
                else if (dragDistance > 0) // Swipe right (previous page)
                {
                    PreviousPage();
                }
            }

            if (!_forgetLastPage)
            {
                _lastPageKeeper.CurrentPage = _currentPage;
            }
        }

        private float GetPagePosition(int pageIndex)
        {
            if (pageIndex < 0 || pageIndex >= _pages.Length)
            {
                Debug.LogError($"Некорректный индекс страницы: {pageIndex}");
                return 0;
            }

            if (_pages.Length == 1)
                return 0;

            var totalWidth = _pages[^1].RectTransform.anchoredPosition.x - _pages[0].RectTransform.anchoredPosition.x;
            if (Mathf.Approximately(totalWidth, 0))
            {
                Debug.LogWarning("Страницы имеют одинаковую позицию по X. Проверьте настройку ScrollRect.");
                return pageIndex / (float)(_pages.Length - 1);
            }

            return (_pages[pageIndex].RectTransform.anchoredPosition.x - _pages[0].RectTransform.anchoredPosition.x) /
                   totalWidth;
        }
    }
}