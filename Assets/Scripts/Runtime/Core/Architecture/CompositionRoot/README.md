# Composition Root Architecture

## Overview
The Composition Root pattern manages the lifecycle and dependencies of application components in different scenes/states.

## Key Components

### CompositionRootProvider
Central manager for composition roots with caching capabilities.

**Features:**
- Lazy loading and initialization of composition roots
- Automatic caching of ICacheableSceneCompositionRoot implementations
- Precaching support for background loading
- Status tracking (NotCached, Caching, Cached, InUse)
- Configurable cache size and TTL

### Interfaces

#### ICompositionRoot
Base interface for all composition roots:
- `Initialize()` / `InitializeAsync()` - Setup the root
- `Dispose()` - Cleanup resources

#### IGlobalCompositionRoot
Interface for global composition roots that persist across scene changes:
- Inherits from `ICompositionRoot`
- Managed separately by the provider
- Only disposed when the provider is disposed
- Cannot be cached or precached

#### ISceneCompositionRoot
Interface for scene-specific composition roots:
- Inherits from `ICompositionRoot`
- Can be cached and released
- Disposed when switching scenes or releasing from provider

#### ICacheableSceneCompositionRoot
Extended interface for scene roots that support caching:
- Inherits from `ISceneCompositionRoot`
- `Enable()` / `EnableAsync()` - Re-enable cached root
- `Disable()` / `DisableAsync()` - Disable root for caching

## Caching Strategy

### How It Works
1. **First Access**: Root is created and initialized
   - Global roots are stored permanently in the provider
   - Scene roots can be cached if they implement `ICacheableSceneCompositionRoot`
2. **Release**: 
   - Global roots are not disposed, they persist
   - Cacheable scene roots are disabled and stored in cache
   - Non-cacheable scene roots are disposed immediately
3. **Subsequent Access**: 
   - Global roots return the same instance
   - Cached scene roots are re-enabled and returned
4. **Cache Eviction**: LRU policy with configurable max size and TTL (scene roots only)
5. **Provider Disposal**: All roots (global and cached) are disposed

### Precaching
Preload roots in background to improve performance. Precaching uses `InitializeForCacheAsync` to initialize roots in a disabled state, preventing any UI or active components from appearing during the process:

```csharp
// In MainMenuState - precache city while showing menu
if (!_provider.IsCached<CityCompositionRoot>())
{
    await _provider.PrecacheAsync<CityCompositionRoot>(token);
}
```

### Status Tracking
Monitor caching status for debugging and optimization:

```csharp
var status = _provider.GetCacheStatus<CityCompositionRoot>();
switch (status)
{
    case CachingStatus.NotCached:
        // Root not loaded
        break;
    case CachingStatus.Caching:
        // Currently loading in background
        break;
    case CachingStatus.Cached:
        // Ready for use
        break;
    case CachingStatus.InUse:
        // Currently active
        break;
}
```

## Usage Examples

### Basic Usage
```csharp
// Get root (loads if not cached)
var root = await _provider.GetRootAsync<GameplayCompositionRoot>(token);
await root.EnterRootAsync(token);

// Release root (caches if cacheable)
await _provider.ReleaseAsync(root, token);
```

### Precaching Strategy
```csharp
public class LoadingState : StateBase
{
    protected override async Task EnterAsync(CancellationToken token)
    {
        // Start precaching multiple roots
        var tasks = new[]
        {
            _provider.PrecacheAsync<CityCompositionRoot>(token),
            _provider.PrecacheAsync<GameplayCompositionRoot>(token)
        };
        
        // Show loading UI while precaching
        ShowLoadingScreen();
        
        // Wait for all to complete
        await Task.WhenAll(tasks);
        
        // Transition to next state
        await _stateFacade.GoToGameplayAsync(token);
    }
}
```

### Conditional Caching
```csharp
// Only precache if not already cached
if (!_provider.IsCached<ExpensiveRoot>())
{
    // Check current status
    var status = _provider.GetCacheStatus<ExpensiveRoot>();
    
    if (status == CachingStatus.NotCached)
    {
        await _provider.PrecacheAsync<ExpensiveRoot>(token);
    }
}
```

## Best Practices

1. **Implement ICacheableSceneCompositionRoot** for roots that should be cached
2. **Use precaching** during loading screens or idle times
3. **Configure cache limits** based on memory constraints
4. **Check cache status** before precaching to avoid redundant work
5. **Handle Disable/Enable properly** to pause/resume root functionality
6. **Implement InitializeForCacheAsync carefully** - ensure all UI and active components remain disabled during precache initialization to prevent visual glitches

## Usage Examples

### Persistent Composition Root
```csharp
// Define a persistent root that lives throughout the application
public class GameCompositionRoot : DisposableBase, ICompositionRoot, IPersistentRoot
{
    public void Initialize()
    {
        // Initialize global services, configs, etc.
        // These will persist until the app closes
    }
    
    public ValueTask InitializeAsync(CancellationToken token)
    {
        // Async initialization
        return default;
    }
}

// Usage
var gameRoot = await provider.GetRootAsync<GameCompositionRoot>(token);
// gameRoot persists even after Release()
```

### Transient Cacheable Root
```csharp
// Define a transient root that can be cached
public class CityCompositionRoot : DisposableBase, ICompositionRoot, ICacheable
{
    public void Initialize()
    {
        // Initialize scene-specific resources normally
        // ICacheable handles enable/disable state separately
    }
    
    public void Enable() { /* Re-enable when retrieved from cache */ }
    public void Disable() { /* Disable when cached */ }
}

// Usage
var cityRoot = await provider.GetRootAsync<CityCompositionRoot>(token);
// ... use the root
await provider.ReleaseAsync(cityRoot, token); // Will be cached
```

### Preloadable Root
```csharp
// Define a root with resource preloading capability
public class GameplayCompositionRoot : ICompositionRoot, IPersistentRoot, IPreloadable
{
    public ValueTask InitializeAsync(CancellationToken token)
    {
        // Fast initialization - services, containers, etc.
        RegisterServices();
        return default;
    }
    
    public async ValueTask PreloadAsync(CancellationToken token)
    {
        // Heavy resource preloading
        // Root decides if it needs initialization first
        if (!IsInitialized)
            await InitializeAsync(token);
            
        await LoadHeavyTextures(token);
        await LoadAudioClips(token);
    }
}

// Usage
await provider.PreloadAsync<GameplayCompositionRoot>(token); // Preload resources
var gameplayRoot = await provider.GetRootAsync<GameplayCompositionRoot>(token); // Get ready root
```

## Configuration

```csharp
var provider = new CompositionRootProvider(
    maxCached: 3,                    // Max cached scene roots
    maxIdle: TimeSpan.FromMinutes(5) // TTL for cached scene roots
);
```

## Testing
Unit tests are provided in `CompositionRootProviderTests.cs` covering:
- Caching behavior
- Status tracking
- Precaching functionality
- Concurrent access scenarios
