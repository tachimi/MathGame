 using UnityEngine;

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
    /// Режим выбора колонок для спавна
    /// </summary>
    public enum ColumnSpawnMode
    {
        [Tooltip("Спавн по колонкам по порядку слева направо")]
        Sequential = 0,
        
        [Tooltip("Случайный выбор колонки для спавна")]
        Random = 1
    }
    /// <summary>
    /// Конфигурация для игрового режима Balloons
    /// </summary>
    [CreateAssetMenu(fileName = "BalloonModeConfig", menuName = "MathGame/GameModeConfigs/BalloonModeConfig")]
    public class BalloonModeConfig : ScriptableObject
    {
        [Header("Настройки шариков")]
        [field: SerializeField] public int BalloonsCount = 10;
        [field: SerializeField] public float BalloonSpeed = 100f;
        
        [Header("Настройки спавна по колонкам")]
        [Tooltip("Количество колонок для спавна шариков")]
        [field: SerializeField] public int SpawnColumns = 3;
        [Tooltip("Интервал между спавном в колонках (0 = спавн во всех колонках сразу)")]
        [field: SerializeField] public float ColumnSpawnInterval = 0.5f;
        [Tooltip("Задержка между волнами спавна")]
        [field: SerializeField] public float WaveDelay = 1f;
        [Tooltip("Режим выбора колонок для спавна")]
        [field: SerializeField] public ColumnSpawnMode ColumnMode = ColumnSpawnMode.Sequential;
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
        
        [Header("Настройка зоны спавна")]
        [Tooltip("Отступ снизу для зоны спавна (положительное значение поднимает зону спавна выше)")]
        [field: SerializeField] public int SpawnBottomOffset = 50;
        [Tooltip("Отступ слева для зоны спавна")]
        [field: SerializeField] public int SpawnLeftOffset = 80;
        [Tooltip("Отступ справа для зоны спавна")]
        [field: SerializeField] public int SpawnRightOffset = 80;
        [Tooltip("Множитель границ экрана для автоматического лопания шариков (1.1 = 10% за экраном)")]
        [field: SerializeField] public float ScreenBoundsPadding = 1.1f;
        
        [Header("Настройка задержек")]
        [field: SerializeField] public float RoundEndDelay = 2f;
        [field: SerializeField] public float CountdownDelay = 1f;
        
        [Header("Конфиг фидбека")]
        [field: SerializeField] public BalloonFeedbackConfig FeedbackConfig;
        
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
    }
}