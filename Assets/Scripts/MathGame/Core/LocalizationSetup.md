# Настройка локализации i2 Localization

## 1. Импорт CSV файла в i2 Localization

1. Откройте **Window → I2 Localization → Sources**
2. Нажмите **Import** и выберите файл `Assets/Localization/MathGame_Localization.csv`
3. Убедитесь, что колонки правильно распознаны:
   - Key
   - English
   - Russian

## 2. Настройка языков

1. В окне I2 Languages убедитесь, что добавлены:
   - English
   - Russian (Русский)
2. Установите English как язык по умолчанию

## 3. Использование в Unity

### Для UI текстов:
1. Добавьте компонент `LocalizedText` на GameObject с Text или TextMeshPro
2. В поле `Localization Key` введите ключ из CSV (например: `UI/Common/Score`)
3. Включите `Update On Language Change` для автообновления

### Для конфигов фраз:
1. Откройте ScriptableObject конфига (BalloonPhrasesConfig или CardPhrasesConfig)
2. В каждом диапазоне заполните поле `LocalizationKeys` ключами из CSV:
   - `Config/BalloonPhrases/Perfect`
   - `Config/BalloonPhrases/Amazing`
   - и т.д.

### В коде:
```csharp
// Получить локализованный текст
string text = Loc.Get("UI/Common/Score");

// С параметрами
string formatted = Loc.Get("UI/Game/QuestionProgress", 5, 10);

// Сменить язык
LocalizationManagerWrapper.Instance.SetLanguage("Russian");
```

## 4. Тестирование

1. Добавьте `LocalizationManagerWrapper` на сцену
2. В контекстном меню компонента выберите:
   - "Switch to English"
   - "Switch to Russian"
3. Проверьте, что тексты меняются

## Структура ключей

- `UI/Common/` - общие UI элементы
- `UI/MainMenu/` - главное меню
- `UI/Settings/` - настройки
- `UI/RangeSelection/` - выбор диапазона
- `UI/Game/` - игровой процесс
- `UI/Results/` - экран результатов
- `UI/Keypad/` - клавиатура
- `UI/Operations/` - математические операции
- `Game/BalloonMode/` - режим шариков
- `Game/CardMode/` - режим карточек
- `Config/Phrases/` - общие фразы
- `Config/BalloonPhrases/` - фразы для шариков
- `Config/CardPhrases/` - фразы для карточек
- `Error/` - сообщения об ошибках
- `Debug/` - отладочные сообщения