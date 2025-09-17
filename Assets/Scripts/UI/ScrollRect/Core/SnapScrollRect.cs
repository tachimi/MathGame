using System;
using Cysharp.Threading.Tasks;
using UI.ScrollRect.PageIndicators;
using UI.ScrollRect;
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
        [SerializeField] private ScrollPositionMode _positionMode = ScrollPositionMode.SessionMemory;
        [SerializeField] private string _scrollKey = "default";
        [SerializeField] private bool _infiniteScroll = true;

        private SessionScrollKeeper _sessionScrollKeeper;
        private Vector2 _dragStartPosition;
        private int _currentPage;
        private bool _isSnapping;

        [Inject]
        public void Construct(SessionScrollKeeper sessionScrollKeeper)
        {
            _sessionScrollKeeper = sessionScrollKeeper;
        }

        private async void Awake()
        {
            if (_indicatorController != null)
            {
                _indicatorController.Initialize(_pages.Length, ref OnPageChanged);
            }

            // Загружаем сохраненную позицию в зависимости от режима
            _currentPage = GetSavedPosition();
            _currentPage = Mathf.Clamp(_currentPage, 0, _pages.Length - 1);

            if (_currentPage >= 0 && _currentPage < _pages.Length)
            {
                OnPageChanged?.Invoke(_currentPage, _pages[_currentPage].Name);
            }

            if (_positionMode != ScrollPositionMode.ForgetPosition)
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
            SaveCurrentPosition();
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
            SaveCurrentPosition();
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
            else
            {
                // Если свайп недостаточно длинный, возвращаемся к текущей странице
                SnapToCurrentPage();
            }

            SaveCurrentPosition();
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

        /// <summary>
        /// Получить сохраненную позицию в зависимости от режима
        /// </summary>
        private int GetSavedPosition()
        {
            return _positionMode switch
            {
                ScrollPositionMode.ForgetPosition => 0,
                ScrollPositionMode.SessionMemory => _sessionScrollKeeper?.GetSessionPosition(_scrollKey) ?? 0,
                ScrollPositionMode.PersistentMemory => PlayerPrefs.GetInt($"ScrollPosition_{_scrollKey}", 0),
                _ => 0
            };
        }

        /// <summary>
        /// Сохранить текущую позицию в зависимости от режима
        /// </summary>
        private void SaveCurrentPosition()
        {
            switch (_positionMode)
            {
                case ScrollPositionMode.ForgetPosition:
                    // Ничего не сохраняем
                    break;

                case ScrollPositionMode.SessionMemory:
                    _sessionScrollKeeper?.SetSessionPosition(_scrollKey, _currentPage);
                    break;

                case ScrollPositionMode.PersistentMemory:
                    PlayerPrefs.SetInt($"ScrollPosition_{_scrollKey}", _currentPage);
                    PlayerPrefs.Save();
                    break;
            }
        }

        /// <summary>
        /// Привязать скролл к текущей странице (используется при недостаточном свайпе)
        /// </summary>
        private void SnapToCurrentPage()
        {
            _isSnapping = true;
        }
    }
}