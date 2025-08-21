using UnityEngine;

namespace UI.ScrollRect.Core
{
    public class ScrollRectPage : MonoBehaviour
    {
        public RectTransform RectTransform => _rectTransform;
        public string Name => _name;

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private string _name;
    }
}