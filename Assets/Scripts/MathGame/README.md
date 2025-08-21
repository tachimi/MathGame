# Математическая игра для детей

## Структура проекта

```
MathGame/
├── Core/                    # Основная игровая логика
│   └── GameSessionController.cs
├── Settings/               # Настройки игры
│   └── GameSettings.cs
├── Questions/              # Генерация вопросов
│   └── QuestionGenerator.cs
├── AnswerStrategies/       # Стратегии ответов (паттерн Стратегия)
│   ├── IAnswerStrategy.cs
│   ├── MultipleChoiceStrategy.cs
│   ├── TextInputStrategy.cs
│   ├── TrueFalseStrategy.cs
│   └── AnswerStrategyFactory.cs
├── Models/                 # Модели данных
│   ├── Question.cs
│   ├── QuestionResult.cs
│   └── GameSessionResult.cs
├── Enums/                  # Перечисления
│   ├── DifficultyLevel.cs
│   ├── AnswerMode.cs
│   └── MathOperation.cs
├── UI/                     # UI компоненты
│   ├── MainMenuScreen.cs
│   ├── SettingsScreen.cs
│   └── GameScreen.cs
└── DI/                     # Dependency Injection
    └── GameLifetimeScope.cs
```

## Основные компоненты

### 1. GameSettings
Конфигурация игровой сессии:
- **DifficultyLevel** - уровень сложности (Easy/Medium/Hard/Expert)
- **QuestionsCount** - количество вопросов
- **AnswerMode** - режим ответа
- **EnabledOperations** - включенные математические операции

### 2. Режимы ответов (AnswerMode)

#### MultipleChoice (Карточки с вариантами)
- Показывает 4 варианта ответа
- Один правильный, три неправильных
- Игрок выбирает карточку с ответом

#### TextInput (Ввод ответа)
- Текстовое поле для ввода числа
- Кнопка "Ответить" или Enter для подтверждения
- Поддерживает только числовой ввод

#### TrueFalse (Правильно/Неправильно)
- Показывает вопрос с ответом
- Игрок выбирает, правильный ответ или нет
- 50% шанс показать правильный/неправильный ответ

### 3. Уровни сложности

- **Easy (Легко)**: числа от 1 до 10
- **Medium (Средне)**: числа от 11 до 20
- **Hard (Сложно)**: числа от 21 до 50
- **Expert (Эксперт)**: числа от 51 до 100

### 4. Математические операции

- Addition (Сложение)
- Subtraction (Вычитание)
- Multiplication (Умножение)
- Division (Деление)

## Новая архитектура кнопок операций

### OperationButton - Компонент кнопки-держателя данных

Каждая кнопка операции теперь является отдельным компонентом, который хранит свои данные:

```csharp
public class OperationButton : MonoBehaviour
{
    public event Action<List<MathOperation>> OnOperationSelected;
    
    public void Configure(List<MathOperation> operations, string displayText)
    public void Configure(MathOperation operation, string displayText)
}
```

### Преимущества новой архитектуры:
- **Гибкость**: Легко добавлять/изменять кнопки через Inspector
- **Переиспользование**: Один компонент для всех типов операций  
- **Данные в кнопке**: Каждая кнопка знает свои операции
- **Простота расширения**: Новые комбинации без изменения кода

### Доступные пресеты:
- **Addition**: + (сложение)
- **Subtraction**: - (вычитание)
- **Multiplication**: × (умножение) 
- **Division**: ÷ (деление)
- **AddSub**: +- (сложение + вычитание)
- **MulDiv**: ×÷ (умножение + деление)
- **AllOperations**: +-×÷ (все операции)

## Использование в Unity

### Создание игровой сцены

1. **Создайте Canvas** для UI элементов

2. **Создайте GameObject для главного меню**:
   - Добавьте компонент `MainMenuScreen`
   - Создайте GameObjects с компонентами `OperationButton` для каждой операции:
     - Добавьте `Button` и `Text` компоненты
     - Настройте через Inspector с помощью пресетов или вручную
     - Или используйте контекстное меню (ПКМ → Configure Addition/Subtraction/etc.)
   - Перетащите все OperationButton в массив `_operationButtons` в MainMenuScreen
   - Настройте кнопки Settings и Exit

3. **Создайте GameObject для экрана настроек**:
   - Добавьте компонент `SettingsScreen`
   - Настройте UI элементы:
     - Dropdown для сложности
     - Slider для количества вопросов
     - Toggle группу для режима ответа
     - Toggle для математических операций

4. **Создайте GameObject для игрового экрана**:
   - Добавьте компонент `GameScreen`
   - Добавьте компонент `GameSessionController`
   - Настройте UI элементы:
     - Text для вопроса
     - Container для ответов
     - Text для счета и прогресса
     - Panel для результатов

### Настройка DI контейнера

1. Создайте GameObject с компонентом `GameLifetimeScope`
2. Убедитесь, что `RootLifetimeScope` подключен в сцене

### Запуск игры

1. Игра начинается с `MainMenuScreen`
2. При нажатии "Play" открывается `SettingsScreen`
3. После выбора настроек запускается `GameScreen`
4. По завершении показываются результаты

## Расширение функционала

### Добавление нового режима ответа

1. Создайте класс, реализующий `IAnswerStrategy`
2. Реализуйте методы:
   - `Initialize()` - инициализация с вопросом
   - `CreateAnswerUI()` - создание UI
   - `Cleanup()` - очистка ресурсов
3. Добавьте новый режим в `AnswerMode` enum
4. Обновите `AnswerStrategyFactory`

### Добавление новых операций

1. Добавьте операцию в `MathOperation` enum
2. Обновите `QuestionGenerator.CalculateAnswer()`
3. Добавьте UI toggle в `SettingsScreen`

## Архитектурные паттерны

- **Strategy Pattern** - для режимов ответов
- **Factory Pattern** - для создания стратегий
- **MVC/MVP** - разделение логики и представления
- **Dependency Injection** - через VContainer
- **Event System** - для коммуникации между экранами