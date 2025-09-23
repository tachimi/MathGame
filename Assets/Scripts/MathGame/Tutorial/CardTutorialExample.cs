using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer;
using MathGame.UI.Cards;

namespace MathGame.Tutorial
{
    /// <summary>
    /// Пример использования туториала для карточек
    /// Добавь этот компонент на экран с карточками
    /// </summary>
    public class CardTutorialExample : MonoBehaviour
    {
        [Header("Card Tutorial Setup")]
        [SerializeField] private MultipleChoiceCard _multipleChoiceCard;

        [Inject] private IGameTutorial _gameTutorial;

        private async void Start()
        {
            // Ждем немного, чтобы UI загрузился
            await UniTask.Delay(1500);

            // Показываем туториал если он еще не был показан
            if (_multipleChoiceCard != null)
            {
                await _gameTutorial.ShowCardTutorial(_multipleChoiceCard);
            }
        }

        /// <summary>
        /// Запустить туториал вручную (для тестирования)
        /// </summary>
        [ContextMenu("Start Tutorial")]
        private async void StartTutorial()
        {
            if (_multipleChoiceCard != null)
            {
                await _gameTutorial.ShowCardTutorial(_multipleChoiceCard);
            }
        }

        /// <summary>
        /// Сбросить туториал (для тестирования)
        /// </summary>
        [ContextMenu("Reset Tutorial")]
        private void ResetTutorial()
        {
            if (_gameTutorial is GameTutorial gameTutorial)
            {
                gameTutorial.ResetAll();
                Debug.Log("Tutorial reset!");
            }
        }
    }
}