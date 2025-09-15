using System;
using System.Collections.Generic;
using MathGame.UI;
using ScreenManager.Core;

namespace MathGame.Services
{
    /// <summary>
    /// Navigation service for managing screen transitions
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Navigate to a specific screen
        /// </summary>
        void NavigateTo<TScreen>() where TScreen : UIScreen;
        
        /// <summary>
        /// Navigate to a specific screen with parameters
        /// </summary>
        void NavigateTo<TScreen>(Dictionary<string, object> parameters) where TScreen : UIScreen;
        
        /// <summary>
        /// Navigate back to the previous screen
        /// </summary>
        void NavigateBack();
        
        /// <summary>
        /// Set root screen (clears navigation stack)
        /// </summary>
        void SetRoot<TScreen>() where TScreen : UIScreen;
        
        /// <summary>
        /// Check if can navigate back
        /// </summary>
        bool CanNavigateBack { get; }
        
        /// <summary>
        /// Event fired before navigation
        /// </summary>
        event Action<Type> OnBeforeNavigate;
        
        /// <summary>
        /// Event fired after navigation
        /// </summary>
        event Action<Type> OnAfterNavigate;
    }
}