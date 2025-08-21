using System;
using System.Collections;
using System.Linq;
using MicroRx.Core;
using ScreenManager.Enums;
using ScreenManager.Events;
using ScreenManager.Interfaces;
using SimpleBus.Extensions;
using SimpleEventBus.Disposables;
using UnityEngine;
using UnityEngine.Assertions;

namespace ScreenManager.Core
{
    public class UIScreen : MonoBehaviour, IScreen, IScreenIdSetter
    {
        public ScreenId Id { get; set; }
        public Guid Guid { get; set; }
        public IObservableSubject<ScreenState> State => _screenState;

        [SerializeField] private Camera _camera;
        
        private readonly Subject<ScreenState> _screenState = new(ScreenState.Hidden);
        private IScreenComponent[] _screenComponents;
        private IAppearingScreenBehaviour _appearingStrategy;
        private CompositeDisposable _subscriptions;

        public virtual void Initialize(object context)
        {
            foreach (var screenComponent in _screenComponents)
            {
                screenComponent.Initialize(context);
            }
        }

        protected virtual void Awake()
        {
            _camera.enabled = false;
            
            var appearingScreenBehaviour = GetComponents<IAppearingScreenBehaviour>()
                .FirstOrDefault(behaviour => !(behaviour is IScreen));

            _appearingStrategy = appearingScreenBehaviour ?? new EmptyAppearingBehaviour();
            _screenComponents = GetComponentsInChildren<IScreenComponent>().Where(component => (UnityEngine.Object)component != this).ToArray();

            _subscriptions = new CompositeDisposable
            {
                State.Subscribe(OnStateChanged)
            };
        }

        protected virtual void OnDestroy()
        {
            _subscriptions?.Dispose();
        }
        
        #region IAppearingScreenBehaviour
        IEnumerator IAppearingScreenBehaviour.SetActiveAsync(bool show)
        {
            Assert.IsTrue(_screenState.CurrentValue is ScreenState.Hidden or ScreenState.Shown);

            if (show)
            {
                _camera.enabled = true;
            }
            
            State.CurrentValue = show ? ScreenState.Appearing : ScreenState.Disappearing;

            yield return show ? ShowAsync() : HideAsync();

            State.CurrentValue = show ? ScreenState.Shown : ScreenState.Hidden;
            
            if (!show)
            {
                _camera.enabled = false;
            }
        }

        protected virtual IEnumerator ShowAsync()
        {
            return _appearingStrategy.SetActiveAsync(true);
        }

        protected virtual IEnumerator HideAsync()
        {
            return _appearingStrategy.SetActiveAsync(false);
        }
        #endregion

        #region Screen State Callbacks
        private void OnStateChanged(ScreenState state)
        {
            switch (state)
            {
                case ScreenState.Hidden:
                    OnScreenHidden();
                    break;
                case ScreenState.Shown:
                    OnScreenShown();
                    break;
                case ScreenState.Appearing:
                    OnScreenStartShow();
                    break;
                case ScreenState.Disappearing:
                    OnScreenStartHide();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        protected virtual void OnScreenStartShow() {}
        protected virtual void OnScreenStartHide() {}
        protected virtual void OnScreenHidden() {}
        protected virtual void OnScreenShown() {}
        #endregion

        public virtual void SetDrawingOrder(int order)
        {
        }

        public virtual void CloseScreen()
        {
            CloseScreenByGuidEvent.Create(Guid).Publish(EventStreams.Game);
        }
    }
}
