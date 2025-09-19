# Использование LocalizedString с выпадающим списком

## Преимущества LocalizedString

Класс `LocalizedString` использует атрибут `[TermsPopup]` из i2 Localization, который создает выпадающий список всех доступных ключей локализации прямо в инспекторе Unity.

## Как это работает в Unity

### В конфигах (ScriptableObject):

1. **BalloonPhrasesConfig** и **CardPhrasesConfig** теперь используют `List<LocalizedString>` вместо `List<string>`

2. В инспекторе Unity вы увидите:
   ```
   Localized Phrases (List)
   ├─ Element 0
   │  └─ Term: [Dropdown список всех ключей]
   ├─ Element 1
   │  └─ Term: [Dropdown список всех ключей]
   └─ Element 2
      └─ Term: [Dropdown список всех ключей]
   ```

3. При клике на поле Term появится выпадающий список со всеми доступными ключами локализации из i2

## Настройка в Unity

### Для BalloonPhrasesConfig:
1. Откройте ScriptableObject в инспекторе
2. В каждом диапазоне счёта:
   - Нажмите "+" в списке `Localized Phrases`
   - Кликните на поле `Term`
   - Выберите из выпадающего списка:
     - `Config/BalloonPhrases/Perfect`
     - `Config/BalloonPhrases/Amazing`
     - и т.д.

### Для CardPhrasesConfig:
1. Откройте ScriptableObject в инспекторе
2. В каждом диапазоне точности:
   - Нажмите "+" в списке `Localized Phrases`
   - Кликните на поле `Term`
   - Выберите из выпадающего списка:
     - `Config/CardPhrases/Outstanding`
     - `Config/CardPhrases/Fantastic`
     - и т.д.

## Преимущества этого подхода

1. **Визуальный выбор** - не нужно помнить или копировать ключи
2. **Защита от опечаток** - выбираете из существующих ключей
3. **Автодополнение** - можно начать вводить текст и список отфильтруется
4. **Группировка** - ключи группируются по категориям (UI/, Config/, и т.д.)
5. **Валидация** - видно сразу, если ключ не существует

## Использование в коде

```csharp
// Создать локализованную строку
LocalizedString localizedPhrase = new LocalizedString("Config/Phrases/Excellent");

// Получить локализованный текст
string text = localizedPhrase.GetLocalizedText();

// С параметрами
string formatted = localizedPhrase.GetLocalizedText(10, 20);

// Проверить, установлен ли ключ
if (localizedPhrase.HasTerm())
{
    // Использовать локализацию
}

// Неявное преобразование в string
string autoText = localizedPhrase; // автоматически вызовет GetLocalizedText()
```

## Структура в инспекторе

После настройки вы увидите в инспекторе:

```
Balloon Phrases Config
└─ Score Phrases
   └─ Element 0
      ├─ Min Score: 90
      ├─ Max Score: 100
      ├─ Localized Phrases (Size: 3)
      │  ├─ [0] Term: Config/BalloonPhrases/Perfect ▼
      │  ├─ [1] Term: Config/BalloonPhrases/Amazing ▼
      │  └─ [2] Term: Config/BalloonPhrases/Wonderful ▼
      └─ Text Color: [White]
```

Где ▼ означает выпадающий список с возможностью выбора любого ключа из i2 Localization.