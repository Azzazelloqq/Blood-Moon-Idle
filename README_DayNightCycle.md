# Day/Night Cycle System

Система смены дня и ночи реализована по паттерну MVP с использованием вашей архитектуры конфигурации.

## Структура системы

### Конфигурация
- **Remote Config**: `DayNightCycleRemoteConfigPage` - ScriptableObject для настройки в Unity Editor
- **Local Config**: `DayNightCycleConfigPage` - обработанная конфигурация для рантайма
- **Config Parser**: интегрирован в `ConfigParser.cs`

### MVP Архитектура
- **Model**: `DayNightCycleModel` - логика смены времени суток
- **View**: `DayNightCycleView` - визуализация освещения и эффектов
- **Presenter**: `DayNightCyclePresenter` - связь между Model и View

### Фабрика
- **Factory**: `DayNightCycleFactory` - создание компонентов через LightDI

## Функциональность

### События времени суток
- `OnDayStarted` - начался день (рассвет)
- `OnNoonReached` - достигнут полдень
- `OnDayEnded` - день закончился (закат)
- `OnNightStarted` - началась ночь
- `OnMidnightReached` - достигнута полночь
- `OnNightEnded` - ночь закончилась
- `OnTimeChanged` - изменилось время (normalized 0-1)
- `OnLightingChanged` - изменились настройки освещения

### Управление циклом
- `StartCycle()` - запуск цикла
- `StopCycle()` - остановка цикла
- `PauseCycle()` - пауза цикла
- `ResumeCycle()` - возобновление цикла
- `SetTime(float normalizedTime)` - установка времени

### Настройки освещения
Каждый период времени суток имеет настройки:
- **Temperature** - цветовая температура света (1000-20000K)
- **Filter** - цветовой фильтр
- **Intensity** - интенсивность света
- **Time Range** - временной промежуток (normalized 0-1)

## Custom Editor

Создан красивый редактор для `DayNightCycleRemoteConfigPage`:
- Удобная настройка времени (часы:минуты:секунды)
- Визуальная настройка периодов освещения
- Превью цветов и временной шкалы
- Кнопки добавления/сброса периодов

## Использование

### 1. Создание конфигурации
```
Assets → Create → BloodMoonIdle → Config → RemotePages → DayNightCycleRemoteConfigPage
```

### 2. Настройка в RemoteConfigSO
Добавьте созданную конфигурацию в MainRemoteConfig.

### 3. Интеграция в GameplayCompositionRoot
```csharp
// Получаем фабрику через DI
var dayNightFactory = DayNightCycleFactoryFactory.CreateDayNightCycleFactory();

// Создаем и инициализируем систему
var dayNightSystem = GetComponent<DayNightCycleSystem>();
await dayNightSystem.InitializeAsync(dayNightFactory, _config, token);
```

### 4. Подписка на события
```csharp
var model = dayNightFactory.CreateDayNightCyclePresenter(config, view).Model;
model.Events.OnDayStarted += () => Debug.Log("Настал новый день!");
model.Events.OnNightStarted += () => Debug.Log("Наступила ночь...");
```

## Особенности

### Визуальные эффекты
- Автоматическое изменение параметров главного света
- Поддержка URP Post-Processing (White Balance, Color Adjustments)
- Модификация Skybox материала
- Плавные переходы между состояниями

### Производительность
- Использует TickHandler для оптимизированного обновления
- Минимальные аллокации в рантайме
- Кэширование компонентов Post-Processing

### Расширяемость
- Легко добавить новые времена суток в enum `TimeOfDay`
- Простое добавление новых визуальных эффектов через View
- Настраиваемые периоды освещения любой длительности

## Пример конфигурации

По умолчанию создается 6 периодов:
1. **Dawn** (0-12.5%) - Рассвет, теплый свет
2. **Day** (12.5-37.5%) - День, яркий белый свет  
3. **Noon** (37.5-62.5%) - Полдень, максимальная яркость
4. **Dusk** (62.5-75%) - Закат, оранжевый свет
5. **Night** (75-87.5%) - Ночь, холодный синий свет
6. **Midnight** (87.5-100%) - Полночь, минимальная освещенность

Продолжительность цикла по умолчанию: День - 5 минут, Ночь - 3 минуты (общий цикл 8 минут). 