# Characters Architecture - Обновлённая версия

## Обзор изменений

Архитектура была обновлена в соответствии с принципами чистой архитектуры:

1. **Убраны CharacterModelBase, CharacterPresenterBase, CharacterViewBase** - теперь каждый тип персонажа имеет собственные контракты
2. **Model содержит только бизнес-логику** - никаких зависимостей на внешние сервисы
3. **IDetectable реализуется в View** - View отвечает за взаимодействие с внешними системами
4. **Presenter управляет внешними сервисами** - связывает Model с внешним миром

## Структура Person

### PersonModelBase и PersonModel
- Наследуются от `Model` (MVP библиотека)
- **Только бизнес-логика**: Position, CurrentState, ProcessMovement()
- **БЕЗ зависимостей** на DetectionService или другие внешние сервисы
- Чистые данные и алгоритмы

### PersonViewBase и PersonView
- Наследуются от `ViewMonoBehaviour<PersonPresenterBase>` (MVP библиотека)
- **Реализуют IDetectable** - могут быть обнаружены другими объектами
- Unity-специфичная логика: анимации, эффекты, коллизии
- Интерфейс с внешним миром Unity

### PersonPresenterBase и PersonPresenter
- Наследуются от `Presenter<PersonViewBase, PersonModelBase>` (MVP библиотека)
- **Управляют внешними сервисами**: DetectionService, TickHandler
- Связывают Model с внешним миром
- Обрабатывают логику обнаружения игрока и убегания

## Структура Player

### PlayerModelBase и PlayerModel
- Аналогично Person, но для игрока
- **Только бизнес-логика**: Position, Direction, MovementSpeed
- **БЕЗ зависимостей** на DetectionService
- Чистая логика движения

### PlayerViewBase и PlayerView  
- **Реализуют IDetectable** - могут быть обнаружены NPC
- Unity-специфичная логика отображения игрока
- Анимации, эффекты, взаимодействие с миром

### PlayerPresenterBase и PlayerPresenter
- **Управляют InputService, TickHandler**
- Обрабатывают ввод пользователя
- Связывают модель игрока с системами

## Принципы архитектуры

### Разделение ответственности
- **Model** - чистая бизнес-логика, алгоритмы, данные
- **View** - Unity-специфичная логика, визуализация, IDetectable  
- **Presenter** - связь с внешними сервисами, координация

### Инверсия зависимостей
- Model НЕ знает о внешних сервисах
- Presenter вводит зависимости и управляет ими
- View реализует интерфейсы для внешнего взаимодействия

### Обнаружение объектов
- **IDetectable реализуется в View**, а не в Model
- DetectionService работает с View-объектами
- Presenter регистрирует/разрегистрирует View в DetectionService

## Фабрики

### PersonFactory
- Создает полную MVP триаду Person
- Настраивает PersonDetectionContext с параметрами обнаружения
- Регистрация в DetectionService происходит в Presenter.OnInitialize()

### PlayerFactory  
- Создает полную MVP триаду Player
- Регистрирует PlayerView в DetectionService для обнаружения NPC
- Управляет жизненным циклом Player

## Преимущества новой архитектуры

1. **Чистота Model** - только бизнес-логика, легко тестировать
2. **Отдельные контракты** - нет лишних зависимостей между типами персонажей  
3. **Правильное разделение** - View отвечает за взаимодействие с внешним миром
4. **Расширяемость** - легко добавлять новые типы персонажей
5. **Тестируемость** - Model можно тестировать изолированно

## Взаимодействие с DetectionService

```csharp
// PersonPresenter регистрирует PersonView
protected override void OnInitialize()
{
    _detectionService?.RegisterObject(personView); // View, не Model!
}

// PersonView реализует IDetectable
public class PersonView : PersonViewBase, IDetectable
{
    public Vector3 Position => presenter.Position;
    public bool IsDead => presenter.IdDead;
}
```

Эта архитектура соответствует принципам SOLID и обеспечивает четкое разделение ответственности между слоями MVP.

