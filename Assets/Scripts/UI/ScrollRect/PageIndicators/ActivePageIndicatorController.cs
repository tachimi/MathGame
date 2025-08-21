using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI.ScrollRect.PageIndicators
{
    public class ActivePageIndicatorController : MonoBehaviour
    {
        [SerializeField] private ActivePageIndicator _prefab;
        [SerializeField] private RectTransform _container;
        [SerializeField] private TextMeshProUGUI _pageNameText;
    
        private readonly List<ActivePageIndicator> _indicators = new();
        private ActivePageIndicator _currentIndicator;

        public void Initialize(int pagesCount, ref Action<int, string> onPageChanged)
        {
            CreateIndicators(pagesCount);
            _currentIndicator = _indicators[0];
            _currentIndicator.Activate();

            onPageChanged += HandlePageChanged;
        }

        private void CreateIndicators(int pagesCount)
        {
            if (_indicators != null)
            {
                foreach (var indicator in _indicators)
                {
                    if (indicator != null)
                    {
                        Destroy(indicator.gameObject);
                    }
                }

                _indicators.Clear();
            }

            for (var i = 0; i < pagesCount; i++)
            {
                var indicator = Instantiate(_prefab, _container);
                indicator.Deactivate();
                _indicators.Add(indicator);
            }
        }

        private void HandlePageChanged(int pageIndex, string pageName)
        {
            if (_currentIndicator != null)
            {
                _currentIndicator.Deactivate();
            }

            if (pageName != null && _pageNameText != null)
            {
                _pageNameText.text = pageName;
            }

            if (pageIndex >= 0 && pageIndex < _indicators.Count)
            {
                _currentIndicator = _indicators[pageIndex];
                _currentIndicator.Activate();
            }
        }

        private void OnDestroy()
        {
            if (_indicators != null)
            {
                _indicators.Clear();
            }
        }
    }
}