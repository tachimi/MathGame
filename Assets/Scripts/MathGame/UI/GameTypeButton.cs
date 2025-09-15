using System;
using MathGame.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.UI
{
    /// <summary>
    /// Кнопка-держатель данных для типа игрового режима
    /// </summary>
    public class GameTypeButton : MonoBehaviour
    {
        public event Action<GameType> OnGameTypeSelected;
        
        [Header("UI Components")]
        [SerializeField] private Button _button;
        
        [Header("Game Type Data")]
        [SerializeField] private GameType _gameType;
        
        private void Awake()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(OnButtonClicked);
            }
        }
        
        private void OnButtonClicked()
        {
            OnGameTypeSelected?.Invoke(_gameType);
        }
        
        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveAllListeners();
        }
    }
}