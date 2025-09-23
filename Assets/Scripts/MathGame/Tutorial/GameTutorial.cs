using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer;
using DG.Tweening;
using MathGame.UI.Cards;
using UnityEngine.UI;
using TMPro;

namespace MathGame.Tutorial
{
    /// <summary>
    /// Простая система туториала для математических карточек
    /// </summary>
    public interface IGameTutorial
    {
        UniTask ShowCardTutorial(MultipleChoiceCard card);
        bool IsCompleted(string gameMode);
    }

    public class GameTutorial : IGameTutorial
    {
        private const string SAVE_KEY_PREFIX = "GameTutorial_";
        private GameObject _finger;
        private Canvas _tutorialCanvas;

        public bool IsCompleted(string gameMode)
        {
            return PlayerPrefs.GetInt(SAVE_KEY_PREFIX + gameMode, 0) == 1;
        }

        private void MarkCompleted(string gameMode)
        {
            PlayerPrefs.SetInt(SAVE_KEY_PREFIX + gameMode, 1);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Показать туториал для карточек с выбором ответа
        /// </summary>
        public async UniTask ShowCardTutorial(MultipleChoiceCard card)
        {
            if (IsCompleted("Cards") || card == null)
                return;

            CreateFinger();

            // Шаг 1: Тап на карточку (лицевая сторона)
            await ShowTapAt(card.transform.position);
            await UniTask.Delay(1000);

            // Ждем пока карточка перевернется и покажет кнопки
            await WaitForCardFlip(card);

            // Шаг 2: Найти и тапнуть правильный ответ
            var correctButton = FindCorrectAnswerButton(card);
            if (correctButton != null)
            {
                await ShowTapAt(correctButton.transform.position);
                await UniTask.Delay(1000);
            }

            // Шаг 3: Свайп вверх для следующей карточки
            await ShowSwipeUp(card.transform.position);
            await UniTask.Delay(500);

            DestroyFinger();
            MarkCompleted("Cards");
        }

        /// <summary>
        /// Ждем пока карточка перевернется
        /// </summary>
        private async UniTask WaitForCardFlip(MultipleChoiceCard card)
        {
            // Ждем пока карточка не перевернется или максимум 3 секунды
            float timeout = 3f;
            while (!card.IsFlipped && timeout > 0)
            {
                timeout -= Time.deltaTime;
                await UniTask.Yield();
            }
        }

        /// <summary>
        /// Найти кнопку с правильным ответом
        /// </summary>
        private Button FindCorrectAnswerButton(MultipleChoiceCard card)
        {
            if (card.CurrentQuestion == null) return null;

            var answersContainer = card.GetComponentInChildren<Transform>().Find("AnswersContainer") ??
                                   card.transform.Find("AnswersContainer");

            if (answersContainer == null) return null;

            // Ищем среди дочерних объектов кнопку с правильным ответом
            foreach (Transform child in answersContainer)
            {
                var button = child.GetComponent<Button>();
                var text = child.GetComponentInChildren<TextMeshProUGUI>();

                if (button != null && text != null &&
                    int.TryParse(text.text, out int answer) &&
                    answer == card.CurrentQuestion.CorrectAnswer)
                {
                    return button;
                }
            }

            return null;
        }

        /// <summary>
        /// Показать тап в указанной позиции
        /// </summary>
        private async UniTask ShowTapAt(Vector3 worldPosition)
        {
            var screenPos = Camera.main.WorldToScreenPoint(worldPosition);
            MoveFinger(screenPos);

            // Анимация тапа
            _finger.transform.DOScale(1.2f, 0.2f).SetLoops(2, LoopType.Yoyo);
            await UniTask.Delay(800);
        }

        /// <summary>
        /// Показать свайп вверх
        /// </summary>
        private async UniTask ShowSwipeUp(Vector3 worldPosition)
        {
            var screenPos = Camera.main.WorldToScreenPoint(worldPosition);
            MoveFinger(screenPos);

            // Анимация свайпа вверх
            var startY = screenPos.y;
            var endY = startY + 150f;

            _finger.transform.DOMoveY(endY, 0.8f).SetEase(Ease.OutCubic);
            await UniTask.Delay(800);
        }

        /// <summary>
        /// Переместить палец в позицию
        /// </summary>
        private void MoveFinger(Vector3 screenPosition)
        {
            if (_finger == null) return;

            var rectTransform = _finger.GetComponent<RectTransform>();
            var canvasRect = _tutorialCanvas.GetComponent<RectTransform>();

            // Конвертируем экранную позицию в локальную позицию canvas
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPosition, _tutorialCanvas.worldCamera, out Vector2 localPoint);

            rectTransform.anchoredPosition = localPoint;
        }

        /// <summary>
        /// Создать палец для туториала
        /// </summary>
        private void CreateFinger()
        {
            if (_finger != null) return;

            _tutorialCanvas = GetOrCreateCanvas();

            _finger = new GameObject("TutorialFinger");
            _finger.transform.SetParent(_tutorialCanvas.transform, false);

            var rectTransform = _finger.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(80, 80);

            var image = _finger.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(1, 1, 1, 0.8f);

            // Делаем круглым
            var texture = CreateCircleTexture();
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
            image.sprite = sprite;
        }

        /// <summary>
        /// Создать текстуру круга
        /// </summary>
        private Texture2D CreateCircleTexture()
        {
            int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = size * 0.5f;
            var radius = size * 0.4f;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    var alpha = distance <= radius ? 1f : 0f;
                    texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
            }

            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Удалить палец
        /// </summary>
        private void DestroyFinger()
        {
            if (_finger != null)
            {
                UnityEngine.Object.Destroy(_finger);
                _finger = null;
            }
        }

        /// <summary>
        /// Получить или создать Canvas для туториала
        /// </summary>
        private Canvas GetOrCreateCanvas()
        {
            var existing = GameObject.Find("TutorialCanvas");
            if (existing?.GetComponent<Canvas>() != null)
                return existing.GetComponent<Canvas>();

            var canvasGO = new GameObject("TutorialCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            var scaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            return canvas;
        }

        /// <summary>
        /// Сбросить все туториалы (для тестирования)
        /// </summary>
        public void ResetAll()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY_PREFIX + "Cards");
            PlayerPrefs.DeleteKey(SAVE_KEY_PREFIX + "Balloons");
            PlayerPrefs.DeleteKey(SAVE_KEY_PREFIX + "TextInput");
            PlayerPrefs.Save();
        }
    }
}