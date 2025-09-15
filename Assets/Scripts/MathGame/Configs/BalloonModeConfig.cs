 using UnityEngine;
using MathGame.Enums;
using SoundSystem.Enums;

namespace MathGame.Configs
{
    /// <summary>
    /// Режим выбора цветов для шариков
    /// </summary>
    public enum BalloonColorMode
    {
        [Tooltip("Случайный выбор цвета для каждого шарика")]
        Random = 0,
        
        [Tooltip("Циклический выбор по порядку из палитры")]
        Sequential = 1,
        
        [Tooltip("Один цвет для всех неправильных ответов, другой для правильного")]
        Themed = 2
    }
    
    /// <summary>
    /// Настройки для конкретного уровня сложности
    /// </summary>
    [System.Serializable]
    public class BalloonDifficultySettings
    {
        [Tooltip("Уровень сложности")]
        public DifficultyLevel DifficultyLevel = DifficultyLevel.Medium;

        [Header("Настройки игры")]
        [Tooltip("Время раунда в секундах")]
        public float RoundTime = 30f;

        [Tooltip("Количество жизней")]
        public int Lives = 3;

        [Header("Настройки физики")]
        [Tooltip("Минимальная сила гравитации (для полета вверх)")]
        public float MinGravity = 7f;

        [Tooltip("Максимальная сила гравитации (для полета вверх)")]
        public float MaxGravity = 12f;

        [Tooltip("Сопротивление воздуха")]
        public float Drag = 0.5f;

        /// <summary>
        /// Свойство для совместимости со старым кодом
        /// </summary>
        public float Gravity => (MinGravity + MaxGravity) * 0.5f;
    }
    /// <summary>
    /// Конфигурация для игрового режима Balloons
    /// </summary>
    [CreateAssetMenu(fileName = "BalloonModeConfig", menuName = "MathGame/GameModeConfigs/BalloonModeConfig")]
    public class BalloonModeConfig : ScriptableObject
    {
        [Header("Настройки шариков")]
        [field: SerializeField] public GameObject BalloonPrefab;
        [field: SerializeField] public int BalloonsCount = 12;
        
        [Header("Настройки спавна")]
        [Tooltip("Случайный разброс по вертикали при спавне внизу экрана")]
        [field: SerializeField] public float SpawnVerticalRandomness = 50f;
        [Tooltip("Диапазон начальной силы для полета шариков вверх")]
        [field: SerializeField] public float InitialForceRange = 500f;
        [Tooltip("Интервал между спавном шариков в миллисекундах")]
        [field: SerializeField, Range(50, 500)] public int SpawnIntervalMs = 100;
        
        [Header("Настройки сложности")]
        [Tooltip("Массив настроек для разных уровней сложности")]
        [field: SerializeField] public BalloonDifficultySettings[] DifficultySettings = new BalloonDifficultySettings[]
        {
            new() { DifficultyLevel = DifficultyLevel.Easy, RoundTime = 45f, Lives = 5, MinGravity = 5f, MaxGravity = 8f, Drag = 1f },
            new() { DifficultyLevel = DifficultyLevel.Medium, RoundTime = 30f, Lives = 3, MinGravity = 7f, MaxGravity = 12f, Drag = 0.5f },
            new() { DifficultyLevel = DifficultyLevel.Hard, RoundTime = 20f, Lives = 1, MinGravity = 10f, MaxGravity = 15f, Drag = 0.2f }
        };
        [Tooltip("Палитра цветов для шариков")]
        public Color[] balloonColors = {
            Color.red, Color.blue, Color.green, Color.yellow,
            Color.magenta, Color.cyan, new Color(1f, 0.5f, 0f), // оранжевый
            new Color(0.5f, 0f, 1f), // фиолетовый
            new Color(1f, 0.75f, 0.8f), // розовый
            new Color(0f, 0.8f, 0.4f) // темно-зеленый
        };
        [Tooltip("Режим выбора цветов шариков")]
        [field: SerializeField] public BalloonColorMode ColorMode = BalloonColorMode.Random;
        [Header("Themed Mode Colors (используется только для режима Themed)")]
        [Tooltip("Цвет для шарика с правильным ответом в режиме Themed")]
        [field: SerializeField] public Color CorrectAnswerBalloonColor = Color.green;
        [Tooltip("Цвет для шариков с неправильными ответами в режиме Themed")]
        [field: SerializeField] public Color WrongAnswerBalloonColor = Color.red;
        
        [Tooltip("Множитель границ экрана для автоматического лопания шариков (1.1 = 10% за экраном)")]
        [field: SerializeField] public float ScreenBoundsPadding = 1.1f;
        
        [Tooltip("Время показа результата после попадания в правильный/неправильный шарик")]
        [field: SerializeField] public float AnswerFeedbackDelay = 1.5f;
        
        [Header("Физические константы")]
        [Tooltip("Отступ от границ контейнера для безопасного размещения шариков")]
        [field: SerializeField] public float SafeBoundsPadding = 50f;
        [Tooltip("Множитель радиуса для точности клика (1.0 = точно по кругу, 0.8 = чуть меньше)")]
        [field: SerializeField, Range(0.5f, 1.2f)] public float ClickRadiusMultiplier = 0.9f;
        [Tooltip("Максимальное количество попыток генерации уникального неправильного ответа")]
        [field: SerializeField] public int MaxWrongAnswerAttempts = 20;

        [Header("Эффекты")]
        [Tooltip("Префаб партикл системы для эффекта лопания")]
        [field: SerializeField] public ParticleSystem PopEffectPrefab;
        [Tooltip("Звук лопания шарика")]
        [field: SerializeField] public SoundType PopSoundType;
        
        /// <summary>
        /// Получить цвет для шарика на основе настроек режима
        /// </summary>
        /// <param name="isCorrectAnswer">Правильный ли это ответ</param>
        /// <param name="balloonIndex">Индекс шарика (для Sequential режима)</param>
        /// <returns>Цвет для шарика</returns>
        public Color GetBalloonColor(bool isCorrectAnswer, int balloonIndex = 0)
        {
            if (balloonColors == null || balloonColors.Length == 0)
            {
                return Color.white; // Fallback цвет
            }
            
            return ColorMode switch
            {
                BalloonColorMode.Random => GetRandomColor(),
                BalloonColorMode.Sequential => GetSequentialColor(balloonIndex),
                BalloonColorMode.Themed => isCorrectAnswer ? CorrectAnswerBalloonColor : WrongAnswerBalloonColor,
                _ => GetRandomColor()
            };
        }
        
        /// <summary>
        /// Получить случайный цвет из палитры
        /// </summary>
        private Color GetRandomColor()
        {
            return balloonColors[Random.Range(0, balloonColors.Length)];
        }
        
        /// <summary>
        /// Получить цвет по порядку из палитры
        /// </summary>
        private Color GetSequentialColor(int index)
        {
            return balloonColors[index % balloonColors.Length];
        }
        
        /// <summary>
        /// Получить настройки для указанного уровня сложности
        /// </summary>
        public BalloonDifficultySettings GetDifficultySettings(DifficultyLevel difficulty)
        {
            // Ищем настройки для указанного уровня в массиве
            foreach (var settings in DifficultySettings)
            {
                if (settings.DifficultyLevel == difficulty)
                {
                    return settings;
                }
            }
            
            // Если не нашли, возвращаем первый элемент или создаем дефолтный
            if (DifficultySettings != null && DifficultySettings.Length > 0)
            {
                return DifficultySettings[0];
            }
            
            // Fallback на дефолтные настройки
            return new BalloonDifficultySettings
            {
                DifficultyLevel = DifficultyLevel.Medium,
                RoundTime = 30f,
                Lives = 3,
                MinGravity = 7f,
                MaxGravity = 12f,
                Drag = 0.5f
            };
        }
    }
}