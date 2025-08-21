using System.Collections;
using ScreenManager.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace ScreenManager.AppearingBeahviour
{
    public class CustomAppearingBehaviour : MonoBehaviour, IAppearingScreenBehaviour
    {
        [SerializeField]
        private UnityEvent _hide = new UnityEvent();
        [SerializeField]
        private UnityEvent _show = new UnityEvent();
        [SerializeField]
        private float _hideDuration = 0.5f;
        [SerializeField]
        private float _showDuration = 0.5f;

        public IEnumerator SetActiveAsync(bool state)
        {
            if (state)
            {
                _show?.Invoke();
            }
            else
            {
                _hide?.Invoke();
            }

            yield return new WaitForSeconds(state ? _showDuration : _hideDuration);
        }
    }
}
