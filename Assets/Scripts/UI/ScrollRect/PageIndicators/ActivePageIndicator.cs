using UnityEngine;
using UnityEngine.UI;

namespace UI.ScrollRect.PageIndicators
{
    public class ActivePageIndicator : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private Color _activeColor;
        [SerializeField] private Color _inactiveColor;

        public void Activate()
        {
            _image.color = _activeColor;
        }

        public void Deactivate()
        {
            _image.color = _inactiveColor;
        }
    }
}