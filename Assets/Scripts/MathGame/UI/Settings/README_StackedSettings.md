# Стековая архитектура настроек

## Обзор

Новая стековая архитектура настроек заменяет предыдущую систему с `PanelTransitionController` на более масштабируемое решение, которое может обрабатывать любое количество уровней настроек для различных игровых режимов.

## Архитектура

### Основные компоненты

1. **SettingsLevel** - Абстрактный базовый класс для всех уровней настроек
2. **StackedSettingsController** - Контроллер навигации по стеку уровней
3. **GameTypeSettingsLevel** - Корневой уровень (выбор игрового режима и общие настройки)
4. **CardsSettingsLevel** - Уровень настроек для режима карточек
5. **StackedSettingsDebugger** - Отладочный компонент для тестирования

### Принципы работы

- **Стековая навигация**: Каждый переход создает новый уровень в стеке
- **Автосохранение**: Настройки сохраняются при переходах между уровнями
- **Валидация**: Каждый уровень может проверять корректность своих настроек
- **Гибкость**: Легко добавлять новые уровни для других игровых режимов

## Использование

### Базовая настройка в Unity

1. **В SettingsScreen добавьте компоненты:**
   ```
   - StackedSettingsController
   - GameTypeSettingsLevel (с ссылками на все селекторы)
   - CardsSettingsLevel (с ссылкой на AnswerModeSelector)
   ```

2. **Настройте ссылки:**
   ```
   SettingsScreen:
   - stackedController → StackedSettingsController
   - gameTypeLevel → GameTypeSettingsLevel
   - cardsLevel → CardsSettingsLevel
   
   GameTypeSettingsLevel:
   - cardsSettingsLevel → CardsSettingsLevel
   ```

3. **Настройте UI элементы:**
   ```
   GameTypeSettingsLevel:
   - difficultySelector
   - questionCountSelector
   - gameTypeSelector
   - backButton
   
   CardsSettingsLevel:
   - answerModeSelector
   - backButton
   - titleText (опционально)
   ```

### Добавление нового уровня настроек

1. **Создайте класс-наследник SettingsLevel:**
   ```csharp
   public class NewGameModeSettingsLevel : SettingsLevel
   {
       // Переопределите абстрактные методы
       protected override void LoadSettingsToUI() { }
       protected override void SaveSettingsFromUI() { }
       protected override void SetupEventHandlers() { }
       protected override void CleanupEventHandlers() { }
   }
   ```

2. **Добавьте ссылку в родительский уровень:**
   ```csharp
   public override SettingsLevel GetNextLevel(object context)
   {
       if (context is GameType gameType)
       {
           return gameType switch
           {
               GameType.NewMode => _newModeSettingsLevel,
               // ...
           };
       }
       return null;
   }
   ```

## API Reference

### SettingsLevel

**Основные методы:**
- `ActivateLevel()` - Активация уровня
- `DeactivateLevel()` - Деактивация уровня
- `ValidateSettings(out string error)` - Валидация настроек
- `GetNextLevel(object context)` - Получить следующий уровень

**События:**
- `OnLevelRequested` - Запрос перехода на другой уровень
- `OnBackRequested` - Запрос возврата назад
- `OnExitRequested` - Запрос выхода из настроек

### StackedSettingsController

**Основные методы:**
- `SetRootLevel(SettingsLevel)` - Установить корневой уровень
- `NavigateToLevel(SettingsLevel)` - Переход к уровню
- `GoBack()` - Возврат назад
- `GoToRoot()` - Возврат к корню

**Свойства:**
- `CurrentLevel` - Текущий активный уровень
- `StackDepth` - Глубина стека
- `CanGoBack` - Можно ли вернуться назад
- `IsNavigating` - Выполняется ли переход

## Отладка

### Использование StackedSettingsDebugger

1. Добавьте компонент `StackedSettingsDebugger` на сцену
2. Настройте UI элементы:
   - `debugInfoText` - TextMeshPro для отображения информации
   - `goToRootButton` - Кнопка возврата к корню
   - `goBackButton` - Кнопка возврата назад
   - `refreshButton` - Кнопка обновления информации
   - `validateButton` - Кнопка валидации настроек

### Отладочная информация

Debugger показывает:
- Текущий активный уровень
- Глубину стека
- Путь навигации
- Возможность навигации
- Состояние валидации
- Историю переходов

## Преимущества новой архитектуры

### По сравнению с PanelTransitionController

1. **Масштабируемость**: Неограниченное количество уровней вместо двух панелей
2. **Гибкость**: Каждый игровой режим может иметь свою структуру настроек
3. **Автосохранение**: Автоматическое сохранение при переходах
4. **Валидация**: Встроенная система проверки корректности настроек
5. **Отладка**: Полная прозрачность состояния системы

### Для разработчиков

1. **Простота добавления**: Новые уровни добавляются минимальным кодом
2. **Независимость**: Уровни не зависят друг от друга
3. **Переиспользование**: Уровни можно использовать в разных контекстах
4. **Тестируемость**: Каждый уровень можно тестировать независимо

## Миграция со старой системы

Старая система с `PanelTransitionController` сохранена как fallback:

```csharp
if (_stackedController == null || _gameTypeLevel == null)
{
    SetupLegacyUI(); // Fallback к старой системе
}
```

Это обеспечивает обратную совместимость и плавный переход.

## Примеры использования

### Переход к настройкам карточек
```csharp
// В GameTypeSettingsLevel
private void OnGameTypeSettingsRequested(GameType gameType)
{
    var nextLevel = GetNextLevel(gameType);
    if (nextLevel != null)
    {
        NavigateToLevel(nextLevel);
    }
}
```

### Валидация настроек
```csharp
// В любом SettingsLevel
public override bool ValidateSettings(out string errorMessage)
{
    if (someCondition)
    {
        errorMessage = "Описание ошибки";
        return false;
    }
    errorMessage = string.Empty;
    return true;
}
```

### Программная навигация
```csharp
// Возврат к корню
_stackedController.GoToRoot();

// Возврат назад
_stackedController.GoBack();

// Переход к определенному уровню
_stackedController.NavigateToLevel(targetLevel);
```

## Будущие расширения

Архитектура готова для:
- Добавления настроек для режимов Balloons и Grid
- Создания многоуровневых настроек (например, подуровни в настройках карточек)
- Добавления анимаций переходов
- Интеграции с системой локализации
- Добавления пресетов настроек