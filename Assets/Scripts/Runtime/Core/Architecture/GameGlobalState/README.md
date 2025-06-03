# Game Global State - Kernel-as-a-Service Architecture

A closed, portable, and game-agnostic state machine kernel for Unity games.

## Overview

This implementation provides a kernel-as-a-service architecture where:
- The FSM and registries live inside a sealed kernel
- External code can only interact through ports (IFlowPort, ITickPort)
- State identification uses IDs instead of types (LevelId, StateId)
- Games define their own states and facades on top of the kernel

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                     Game Layer                          │
│  ┌─────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │   States    │  │   Installer  │  │    Facade    │  │
│  │ (App, GP)   │  │  (Compose)   │  │  (GameFlow)  │  │
│  └─────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                            │
                     Ports (IFlowPort, ITickPort)
                            │
┌─────────────────────────────────────────────────────────┐
│                    Kernel (Internal)                    │
│  ┌─────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │     FSM     │  │  Registries  │  │   Builder    │  │
│  │  (Per Level)│  │  (States)    │  │  (Compose)   │  │
│  └─────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
```

## Folder Structure

```
GameGlobalState/
├── Contract/           # Public contracts (interfaces, types)
│   ├── IState.cs
│   ├── StateBase.cs
│   ├── Ports.cs       # IFlowPort, ITickPort
│   ├── Ids.cs         # LevelId, StateId
│   └── IGameFlow.cs
├── Core/
│   └── Kernel/        # Internal kernel implementation
│       ├── Kernel.cs
│       ├── KernelBuilder.cs
│       ├── Fsm.cs
│       ├── FlowPort.cs
│       └── TickPort.cs
├── States/            # Game-specific state implementations
│   ├── App/          # Application-level states
│   └── Gameplay/     # Gameplay-level states
├── Facade/           # Game-specific flow facade
│   └── GameFlow.cs
├── Installer/        # Kernel composition
│   └── GameGlobalStateInstaller.cs
└── Tests/
    └── StateMachineTests.cs
```

## Quick Start

### 1. Define Your States

```csharp
public class MainMenuState : StateBase
{
    protected override async Task EnterAsync(CancellationToken ct)
    {
        // Initialize main menu UI
        await LoadMenuResourcesAsync(ct);
    }

    protected override async Task ExitAsync(CancellationToken ct)
    {
        // Cleanup
        await UnloadMenuResourcesAsync(ct);
    }

    protected override void Tick(float dt)
    {
        // Update menu animations
    }
}
```

### 2. Install the Kernel

```csharp
// In your game entry point
var gameFlow = GameGlobalStateInstaller.Install(serviceProvider);

// Hook up to Unity's update loop
void Update() => kernel.Ticks.Tick(Time.deltaTime);
```

### 3. Use the Game Flow

```csharp
// Transition to main menu
await gameFlow.GoToMainMenuAsync(cancellationToken);

// Start gameplay
await gameFlow.GoToCityAsync(cancellationToken);
```

## Key Concepts

### ID-Based Routing

Instead of type-based routing, states are identified by:
- `LevelId`: Identifies a state machine scope (e.g., "app", "gameplay")
- `StateId`: Identifies a state within a level (e.g., "app/mainmenu")

### Ports Pattern

The kernel exposes only two ports:
- `IFlowPort`: For requesting state transitions
- `ITickPort`: For updating active states

### Levels

The system supports multiple parallel state machines (levels):
- **App Level**: Top-level application states (menu, gameplay, shutdown)
- **Gameplay Level**: Gameplay-specific states (city, dungeon, battle)

### Serialized Transitions

All state transitions are serialized using `SemaphoreSlim` to prevent:
- Race conditions
- Overlapping enter/exit calls
- State corruption

## Design Principles

1. **Encapsulation**: Kernel internals are completely hidden
2. **Portability**: No Unity-specific code in the kernel
3. **Extensibility**: Games extend via states and facades, not kernel modification
4. **Type Safety**: Compile-time checking via IDs and factories
5. **Async-First**: All transitions are async with proper cancellation support

## Advanced Usage

### Custom Service Provider

```csharp
var kernel = Kernel.Create()
    .WithServiceProvider(() => myServiceProvider)
    .Build();
```

### Direct Port Usage

```csharp
// Skip the facade for advanced scenarios
await kernel.Flow.RequestAsync(
    new LevelId("app"), 
    new StateId("app/battle"), 
    cancellationToken
);
```

### Multi-Level Coordination

```csharp
public async Task StartBattleAsync(CancellationToken ct)
{
    // Transition app level to battle state
    await _flow.RequestAsync(_appLevel, _battleState, ct);
    
    // Initialize battle subsystem
    await _flow.RequestAsync(_battleLevel, _setupState, ct);
}
```

## Dependency Injection with State Factory

The system uses a factory pattern for creating states with dependencies:

### 1. Define State Factory

Your DI framework should generate a factory implementing `IStateFactory`:

```csharp
public interface IStateFactory
{
    IState CreateState(StateId stateId);
}
```

### 2. Install with Factory

```csharp
// Create factory through your DI framework
var stateFactory = DIFramework.Create<IStateFactory>();

// Install the state system
var gameFlow = GameGlobalStateInstaller.Install(stateFactory);
```

### 3. State Dependencies

States receive dependencies through constructor injection:

```csharp
public class MainMenuState : StateBase
{
    private readonly IUIProvider _uiProvider;
    private readonly IAudioService _audioService;
    
    public MainMenuState(IUIProvider uiProvider, IAudioService audioService)
    {
        _uiProvider = uiProvider;
        _audioService = audioService;
    }
}
```

## Migration from Type-Based System

If migrating from a type-based state machine:

1. Replace `ChangeState<T>()` with `RequestAsync(level, state, ct)`
2. Define IDs for all states
3. Create a state factory for dependency injection
4. Create a facade for semantic transitions

## Testing

The system includes comprehensive tests for:
- Basic state transitions
- Idempotency (same state requests)
- Concurrent transition serialization
- Tick routing to active states
- Error handling with informative messages

## Performance Considerations

- Transitions are async but lightweight
- Ticks are synchronous for performance
- No reflection or runtime type checks
- Minimal allocations during steady state