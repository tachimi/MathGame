using System;
using System.Collections.Generic;
using MathGame.UI;
using ScreenManager.Core;
using UnityEngine;

namespace MathGame.Services
{
    /// <summary>
    /// Implementation of navigation service using ScreenManager
    /// </summary>
    public class NavigationService : INavigationService
    {
        #region Events
        
        public event Action<Type> OnBeforeNavigate;
        public event Action<Type> OnAfterNavigate;
        
        #endregion
        
        #region Properties
        
        public bool CanNavigateBack => _navigationStack.Count > 1;
        
        #endregion
        
        #region Private Fields
        
        private readonly Stack<Type> _navigationStack = new Stack<Type>();
        private readonly Dictionary<Type, Dictionary<string, object>> _screenParameters = new Dictionary<Type, Dictionary<string, object>>();
        
        #endregion
        
        #region Public Methods
        
        public void NavigateTo<TScreen>() where TScreen : UIScreen
        {
            NavigateTo<TScreen>(null);
        }
        
        public void NavigateTo<TScreen>(Dictionary<string, object> parameters) where TScreen : UIScreen
        {
            var screenType = typeof(TScreen);
            
            OnBeforeNavigate?.Invoke(screenType);
            
            // Store parameters if provided
            if (parameters != null)
            {
                _screenParameters[screenType] = parameters;
            }
            
            // Add to navigation stack
            _navigationStack.Push(screenType);
            
            // Open screen using ScreenManager
            ScreensManager.OpenScreen<TScreen>();
            
            OnAfterNavigate?.Invoke(screenType);
            
            Debug.Log($"NavigationService: Navigated to {screenType.Name}. Stack depth: {_navigationStack.Count}");
        }
        
        public void NavigateBack()
        {
            if (!CanNavigateBack)
            {
                Debug.LogWarning("NavigationService: Cannot navigate back - at root screen");
                return;
            }
            
            // Pop current screen
            var currentScreen = _navigationStack.Pop();
            
            // Clear parameters for the screen we're leaving
            if (_screenParameters.ContainsKey(currentScreen))
            {
                _screenParameters.Remove(currentScreen);
            }
            
            // Get previous screen
            var previousScreen = _navigationStack.Peek();
            
            OnBeforeNavigate?.Invoke(previousScreen);
            
            // Navigate to previous screen
            OpenScreenByType(previousScreen);
            
            OnAfterNavigate?.Invoke(previousScreen);
            
            Debug.Log($"NavigationService: Navigated back to {previousScreen.Name}. Stack depth: {_navigationStack.Count}");
        }
        
        public void SetRoot<TScreen>() where TScreen : UIScreen
        {
            var screenType = typeof(TScreen);
            
            OnBeforeNavigate?.Invoke(screenType);
            
            // Clear navigation stack
            _navigationStack.Clear();
            _screenParameters.Clear();
            
            // Set new root
            _navigationStack.Push(screenType);
            
            // Open screen
            ScreensManager.OpenScreen<TScreen>();
            
            OnAfterNavigate?.Invoke(screenType);
            
            Debug.Log($"NavigationService: Set root to {screenType.Name}");
        }
        
        /// <summary>
        /// Get parameters for a screen
        /// </summary>
        public Dictionary<string, object> GetScreenParameters<TScreen>() where TScreen : UIScreen
        {
            var screenType = typeof(TScreen);
            return _screenParameters.TryGetValue(screenType, out var parameters) ? parameters : null;
        }
        
        /// <summary>
        /// Clear navigation history
        /// </summary>
        public void ClearHistory()
        {
            _navigationStack.Clear();
            _screenParameters.Clear();
            Debug.Log("NavigationService: Navigation history cleared");
        }
        
        #endregion
        
        #region Private Methods
        
        private void OpenScreenByType(Type screenType)
        {
            // Use reflection to call the generic method
            var openScreenMethod = typeof(ScreensManager).GetMethod("OpenScreen");
            var genericMethod = openScreenMethod.MakeGenericMethod(screenType);
            genericMethod.Invoke(null, null);
        }
        
        #endregion
    }
}