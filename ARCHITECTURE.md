# Blood Moon Idle - Project Architecture

## Architecture Overview

The project is built on **CompositionRoot** architecture principles using **MVP** pattern and **Dependency Injection**.

## Core Principles

1. **Separation of Concerns** - each class has one clearly defined responsibility
2. **Dependency Inversion** - dependencies are injected through constructor with [Inject] attribute
3. **Input Abstraction** - input system is abstracted from concrete implementation
4. **MVP Pattern** - clear separation of Model, View and Presenter
5. **Source Generation** - automatic factory generation for classes with [Inject]

## Folder Structure

```
Assets/
├── Scripts/
│   ├── Runtime/
│   │   ├── Core/
│   │   │   ├── Architecture/
│   │   │   │   └── CompositionRoot/     # Root architecture components
│   │   │   └── Input/                   # Input system abstraction
│   │   ├── Gameplay/
│   │   │   └── Character/              # Character gameplay logic (MVP)
│   │   │       └── Movement/           # Movement system
│   │   ├── UI/
│   │   │   └── Joystick/              # UI joystick
│   │   └── Infrastructure/
│   │       ├── Services/              # Application services
│   │       └── Factories/             # Object creation factories
│   └── Editor/                        # Editor-only code
└── GameContent/                       # Resources for Addressables (not Resources!)
```

## Used Packages

### Custom Packages
- `com.azzazello.mvp` - MVP pattern with abstract classes
- `com.azzazello.disposable` - Resource management system
- `com.azzazello.resourceloader` - Resource loading through Addressables
- `com.azzazello.lightdi` - Lightweight DI container with SourceGenerator
- `com.azzazello.tickhandler` - Update system
- `com.azzazello.sceneswitcher` - Scene management
- `com.azzazello.localsavesystem` - Local save system

### Unity Packages
- `com.unity.inputsystem` - Unity's new input system
- `com.unity.addressables` - Addressable assets system

## LightDI - Dependency Injection

### How It Works
LightDI uses **SourceGenerator** for automatic factory generation. This provides:
- ✅ **Compile-time resolution** - dependencies resolved without reflection
- ✅ **High performance** - no runtime overhead
- ✅ **Type Safety** - errors caught at compile time

### Proper [Inject] Attribute Usage

#### Constructor injection with parameters
```csharp
public class CharacterPresenter : Presenter<CharacterView, CharacterModel>
{
    public CharacterPresenter(
        CharacterView view,                          // Regular parameter (goes to factory)
        CharacterModel model,                        // Regular parameter (goes to factory)
        [Inject] IMovementSystem movementSystem,     // Auto-resolved from container
        [Inject] IInputService inputService,         // Auto-resolved from container
        [Inject] ITickHandler tickHandler)           // Auto-resolved from container
    {
        // ...
    }
}
```

#### Constructor with dependencies only
```csharp
public class InputService : IInputService
{
    [Inject]
    public InputService()  // Generates InputServiceFactory.CreateInputService()
    {
    }
}

public class CharacterFactory : ICharacterFactory
{
    private readonly IResourceLoader _resourceLoader;
    
    public CharacterFactory([Inject] IResourceLoader resourceLoader)  // Auto-resolved
    {
        _resourceLoader = resourceLoader;
    }
}
```

### Auto-generated Factories

#### For class with regular parameters + [Inject]:
```csharp
// Source class
public CharacterPresenter(
    CharacterView view,                          // Regular parameter
    CharacterModel model,                        // Regular parameter  
    [Inject] IMovementSystem movementSystem,     // [Inject] parameter
    [Inject] IInputService inputService,         // [Inject] parameter
    [Inject] ITickHandler tickHandler)           // [Inject] parameter

// Auto-generated:
public static class CharacterPresenterFactory
{
    public static CharacterPresenter CreateCharacterPresenter(CharacterView view, CharacterModel model)
    {
        // [Inject] parameters resolved automatically from container
        var movementSystem = DiContainerProvider.Resolve<IMovementSystem>();
        var inputService = DiContainerProvider.Resolve<IInputService>();
        var tickHandler = DiContainerProvider.Resolve<ITickHandler>();
        
        return new CharacterPresenter(view, model, movementSystem, inputService, tickHandler);
    }
}
```

#### For class with [Inject] only:
```csharp
// Source class
[Inject]
public InputService()

// Auto-generated:
public static class InputServiceFactory
{
    public static InputService CreateInputService()
    {
        return new InputService();
    }
}
```

## CompositionRoot Structure

### GameCompositionRoot
Main application root. Registers **global services only**:
```csharp
private void RegisterGlobalServices()
{
    var container = DiContainerFactory.CreateContainer();
    
    // Register as Singleton (one instance per container)
    container.RegisterAsSingleton<IResourceLoader>(() => new AddressableResourceLoader());
    container.RegisterAsSingleton<ITickHandler>(() => new UnityTickHandler(dispatcher));
    container.RegisterAsSingleton<ISceneService>(() => new SceneService());
    container.RegisterAsSingleton<ILocalSaveSystem>(() => new LocalSaveSystem());
}
```

### GameplayCompositionRoot
Registers **only services needed for container**, everything else through auto-generated factories:
```csharp
private void RegisterGameplayServices()
{
    // Register only services that don't have [Inject] constructors
    // or have complex creation logic
    _localContainer.RegisterAsTransient<IMovementSystem>(() => new MovementSystem());
}

private void InitializeGameplay()
{
    // Use auto-generated factories instead of container
    var inputService = InputServiceFactory.CreateInputService();
    var joystickController = JoystickControllerFactory.CreateJoystickController(_joystickView);
    var characterFactory = CharacterFactoryFactory.CreateCharacterFactory();
    
    // Wire components
    inputService.SetInputProvider(joystickController);
    var character = characterFactory.CreateCharacter(_characterSpawnPoint.position);
}
```

## Character MVP Architecture

### CharacterModel : Model
- Stores character data (position, direction, speed)
- Inherits from `Model` from MVP package
- Automatic resource management
- **Has [Inject] constructor for auto-factory generation**

### CharacterView : ViewMonoBehaviour<CharacterPresenter>
- Unity component for character display
- Manages animations and visualization
- Inherits from `ViewMonoBehaviour` from MVP package

### CharacterPresenter : Presenter<CharacterView, CharacterModel>
- Links Model and View
- Handles business logic
- Subscribes to update system through ITickHandler
- Inherits from `Presenter` from MVP package
- **Uses [Inject] for automatic system dependency injection**

## Character Creation Example

```csharp
public class CharacterFactory : ICharacterFactory
{
    private readonly IResourceLoader _resourceLoader;
    
    // Dependencies automatically injected
    public CharacterFactory([Inject] IResourceLoader resourceLoader)
    {
        _resourceLoader = resourceLoader;
    }
    
    public CharacterPresenter CreateCharacter(Vector3 position)
    {
        // Load prefab
        var characterPrefab = _resourceLoader.LoadResource<GameObject>("Character");
        var characterGameObject = Object.Instantiate(characterPrefab, position, Quaternion.identity);
        var characterView = characterGameObject.GetComponent<CharacterView>();
        
        // Create model through auto-generated factory
        var characterModel = CharacterModelFactory.CreateCharacterModel();
        
        // Create presenter through auto-generated factory
        // view and model - regular parameters, other dependencies auto-injected
        var presenter = CharacterPresenterFactory.CreateCharacterPresenter(characterView, characterModel);
        
        presenter.Initialize();
        return presenter;
    }
}

// Usage:
var characterFactory = CharacterFactoryFactory.CreateCharacterFactory();  // IResourceLoader auto-injected
var character = characterFactory.CreateCharacter(Vector3.zero);
```

## Important LightDI Rules

### ✅ Correct
1. **[Inject] in constructor** - for dependencies resolved from container
2. **Regular parameters** - for runtime data (position, ID, etc.)
3. **Auto-generated factories** - `ClassNameFactory.CreateClassName(parameters)`
4. **DiContainerProvider.Resolve() ONLY inside auto-generated factories**

### ❌ Incorrect
1. **Direct calls** `DiContainerProvider.Resolve<T>()` in your code
2. **Mixing** manual creation through `new` and DI system
3. **Excessive registration** - if [Inject] constructor exists, registration not needed

## Extension Principles

1. **New services** - add `[Inject]` constructor, use auto-generated factory
2. **New systems** - same thing, register in container only when necessary
3. **New gameplay logic** - use MVP pattern with [Inject] for dependencies
4. **Runtime data passing** - regular constructor parameters without [Inject]

This architecture provides:
- ✅ Maximum performance through compile-time DI
- ✅ Ease of use through auto-generated factories
- ✅ Clear separation between dependencies and data
- ✅ Automatic resource management
- ✅ Type Safety at compile time 