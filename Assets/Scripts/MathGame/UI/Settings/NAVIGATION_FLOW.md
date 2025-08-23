# Схема навигации стековой архитектуры настроек

## 🔄 Полный поток навигации

### 1. Инициализация
```
SettingsScreen.OnEnable()
    ↓
StackedSettingsController.SetRootLevel(GameTypeSettingsLevel)
    ↓
GameTypeSettingsLevel.ActivateLevel()
    ↓
Показывается корневой уровень с селекторами:
- DifficultySelector
- QuestionCountSelector  
- GameTypeSelector (с кнопкой ⚙️ только у Cards)
- Back Button
```

### 2. Переход к настройкам карточек
```
User: Клик на кнопку ⚙️ рядом с "Карточки"
    ↓
GameTypeButton.OnSettingsButtonClicked()
    ↓
GameTypeSelector.OnSettingsRequested(GameTypeButton)
    ↓
GameTypeSettingsLevel.OnGameTypeSettingsRequested(GameType.Cards)
    ↓
GameTypeSettingsLevel.NavigateToLevel(cardsSettingsLevel)
    ↓
StackedSettingsController.PushLevel(CardsSettingsLevel)
    ↓
CardsSettingsLevel.ActivateLevel()
    ↓
Показывается уровень настроек карточек:
- AnswerModeSelector
- Back Button
```

### 3. Возврат назад из настроек карточек
```
User: Клик на Back Button в CardsSettingsLevel
    ↓
CardsSettingsLevel.OnBackButtonClicked()
    ↓
CardsSettingsLevel.RequestBack()
    ↓
StackedSettingsController.GoBack()
    ↓
StackedSettingsController.PopLevel()
    ↓
GameTypeSettingsLevel.ActivateLevel()
    ↓
Возврат к корневому уровню
```

### 4. Выход из настроек
```
User: Клик на Back Button в GameTypeSettingsLevel
    ↓
GameTypeSettingsLevel.OnBackButtonClicked()
    ↓
GameTypeSettingsLevel.RequestExit()
    ↓
StackedSettingsController.HandleExitRequest()
    ↓
StackedSettingsController.ClearStack()
    ↓
SettingsScreen.OnStackCleared()
    ↓
Переход в MainMenuScreen
```

## 🎯 Кнопки навигации по уровням

### GameTypeSettingsLevel (корневой уровень)
- **Back Button** → Выход в главное меню
- **Gear Button (⚙️)** рядом с "Карточки" → Переход к CardsSettingsLevel

### CardsSettingsLevel (настройки карточек)
- **Back Button** → Возврат к GameTypeSettingsLevel

## 🏗️ Архитектурная схема

```
SettingsScreen (контейнер)
├── StackedSettingsController (управление навигацией)
├── GameTypeSettingsLevel (корневой уровень)
│   ├── DifficultySelector
│   ├── QuestionCountSelector
│   ├── GameTypeSelector
│   │   ├── GameTypeButton (Cards) + ⚙️ Gear Button
│   │   ├── GameTypeButton (Balloons)
│   │   └── GameTypeButton (Grid)
│   └── Back Button → Exit
└── CardsSettingsLevel (уровень настроек карточек)
    ├── AnswerModeSelector
    └── Back Button → GoBack()
```

## 🔧 Ключевые компоненты навигации

### StackedSettingsController
- **Методы навигации:** `NavigateToLevel()`, `GoBack()`, `GoToRoot()`, `SetRootLevel()`
- **События:** `OnLevelChanged`, `OnStackCleared`, `OnLevelPushed`, `OnLevelPopped`
- **Стек:** Управляет историей переходов между уровнями

### SettingsLevel (базовый класс)
- **События навигации:** `OnLevelRequested`, `OnBackRequested`, `OnExitRequested`
- **Методы навигации:** `NavigateToLevel()`, `RequestBack()`, `RequestExit()`
- **Lifecycle:** `ActivateLevel()`, `DeactivateLevel()`

## 🎮 Пользовательский опыт

### Сценарий 1: Настройка карточек
1. Пользователь открывает настройки
2. Видит главный экран с выбором игры и общими настройками
3. Кликает на ⚙️ рядом с "Карточки"
4. Попадает на экран настроек карточек
5. Настраивает режим ответа
6. Кликает Back для возврата
7. Кликает Back для выхода в меню

### Сценарий 2: Только общие настройки
1. Пользователь открывает настройки
2. Настраивает сложность и количество вопросов
3. Выбирает тип игры
4. Кликает Back для выхода в меню

## 📋 Legacy код (Fallback)

Если стековая система не настроена:
- SettingsScreen переключается на legacy режим
- Используются старые селекторы напрямую
- Навигация по настройкам карточек недоступна
- Показывается предупреждение в консоли

## 🔮 Будущие расширения

### Добавление BalloonSettingsLevel
```
GameTypeSettingsLevel.GetNextLevel()
{
    return gameType switch
    {
        GameType.Cards => _cardsSettingsLevel,
        GameType.Balloons => _balloonsSettingsLevel, // ← Новый уровень
        GameType.Grid => null,
        _ => null
    };
}
```

### Многоуровневые настройки
```
CardsSettingsLevel → CardsDifficultyLevel → CardsAnimationLevel
```

Стек может содержать любую глубину уровней!