# Game Modes Prefabs Structure

Эта папка содержит префабы UI для различных игровых режимов.

## Структура префабов

### CardsUI.prefab
Префаб для режима карточек. Должен содержать:
- **MathCardFactory** компонент (обязательно)
- Ссылки на префабы карточек:
  - MultipleChoiceCard prefab
  - TextInputCard prefab
  - FlashCard prefab
- CardContainer (Transform для размещения карточек)

**Настройка в Unity:**
1. Создайте пустой GameObject
2. Добавьте MathCardFactory компонент
3. Создайте дочерний объект CardContainer
4. Назначьте префабы карточек в MathCardFactory
5. Сохраните как префаб в `Assets/Resources/GameModes/CardsUI.prefab`

### BalloonsUI.prefab
Префаб для режима шариков. Должен содержать:
- **BalloonGameUI** компонент (обязательно)
- Background Image
- Question Text (TextMeshProUGUI)
- Область для шариков

**Настройка в Unity:**
1. Создайте Canvas или Panel
2. Добавьте BalloonGameUI компонент
3. Настройте фон и текст вопроса
4. Сохраните как префаб в `Assets/Resources/GameModes/BalloonsUI.prefab`

### BalloonAnswer.prefab (опционально)
Префаб отдельного шарика:
- **BalloonAnswer** компонент
- Image для визуала шарика
- TextMeshProUGUI для отображения ответа
- Collider для обработки нажатий

**Настройка в Unity:**
1. Создайте UI элемент (Image)
2. Добавьте BalloonAnswer компонент
3. Добавьте дочерний Text для ответа
4. Сохраните как префаб в `Assets/Resources/GameModes/BalloonAnswer.prefab`

## Fallback механизм

Если префабы не найдены:
- **CardGameMode** попытается найти MathCardFactory на сцене
- **BalloonGameMode** создаст UI программно

## Важные замечания

1. Все префабы должны быть в папке `Resources/GameModes/`
2. Имена префабов чувствительны к регистру
3. Префабы загружаются через `Resources.Load()`
4. После сборки игры префабы включаются в билд автоматически