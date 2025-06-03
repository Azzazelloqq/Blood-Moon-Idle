using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Runtime.Core.Architecture.CompositionRoot.Base;
using Runtime.Core.Architecture.CompositionRoot.Factory;

namespace Runtime.Core.Architecture.CompositionRoot.Facade
{
/// <summary>
/// Represents the caching status of a composition root.
/// </summary>
public enum CachingStatus
{
	/// <summary>
	/// The root has not been loaded or cached.
	/// </summary>
	NotCached,
	
	/// <summary>
	/// The root is currently being loaded and cached.
	/// </summary>
	Caching,
	
	/// <summary>
	/// The root is cached and available for use.
	/// </summary>
	Cached,
	
	/// <summary>
	/// The root is cached and currently in use (enabled).
	/// </summary>
	InUse
}

/// <summary>
/// Manages the lifecycle and caching of composition roots.
/// Provides functionality to create, retrieve, and release composition roots with optional caching.
/// Persistent roots persist across scene changes and are only disposed when the provider is disposed.
/// </summary>
public class CompositionRootProvider : IDisposable
{
	private readonly ICompositionRootFactory _factory;
	private readonly int _maxCached;
	private readonly TimeSpan _maxIdle;

	private readonly Dictionary<Type, ICompositionRoot> _cache = new();
	private readonly Dictionary<Type, ICompositionRoot> _persistentRoots = new();
	private readonly Dictionary<Type, DateTime> _lastUsedUtc = new();
	private readonly Dictionary<Type, CachingStatus> _cachingStatuses = new();
	private readonly Dictionary<Type, TaskCompletionSource<ICompositionRoot>> _cachingTasks = new();
	private readonly Dictionary<Type, TaskCompletionSource<ICompositionRoot>> _preloadingTasks = new();
	private readonly HashSet<Type> _initializedRoots = new();

	private bool _disposed;

	public CompositionRootProvider(
		int maxCached = 2,
		TimeSpan? maxIdle = null)
	{
		_factory = new CompositionRootFactory();
		_maxCached = Math.Max(0, maxCached);
		_maxIdle = maxIdle ?? TimeSpan.FromMinutes(5);
	}
	
	public CompositionRootProvider(
		ICompositionRootFactory customFactory,
		int maxCached = 2,
		TimeSpan? maxIdle = null)
	{
		_factory = customFactory;
		_maxCached = Math.Max(0, maxCached);
		_maxIdle = maxIdle ?? TimeSpan.FromMinutes(5);
	}

	/// <summary>
	/// Retrieves a composition root instance of type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">The concrete type of the root to retrieve.</typeparam>
	/// <param name="token">A cancellation token to observe while awaiting the operation.</param>
	/// <param name="continueOnCapturedContext">True to attempt to marshal the continuation back to the original context captured; otherwise, false.</param>
	/// <returns>
	/// A task that resolves to an initialized instance of <typeparamref name="T"/>.
	/// If a cacheable instance is available in the cache, it is returned directly.
	/// Otherwise, a new instance is created and initialized.
	/// </returns>
	/// <remarks>
	/// This method does not manage the enabled/disabled state of roots upon retrieval.
	/// The caller is responsible for calling Enable/Disable methods as needed.
	/// <para/>
	/// If <typeparamref name="T"/> implements <see cref="ICacheable"/> and the
	/// provider is configured with <c>maxCached &gt; 0</c>, the instance can be retained warm in
	/// the internal cache upon <see cref="Release(ICompositionRoot)"/> / <see cref="ReleaseAsync(ICompositionRoot,CancellationToken,bool)"/>.
	/// When released, cacheable roots are automatically disabled for caching.
	/// </remarks>
	/// <exception cref="ObjectDisposedException">Thrown if the provider has been disposed.</exception>
	/// <exception cref="InvalidOperationException">
	/// Thrown if the factory fails to create an instance for <typeparamref name="T"/>.
	/// </exception>
	public async Task<T> GetRootAsync<T>(CancellationToken token, bool continueOnCapturedContext = true)
		where T : ICompositionRoot
	{
		ThrowIfDisposed();
		CleanupExpired();

		var type = typeof(T);

		// Check if it's a persistent root
		if (_persistentRoots.TryGetValue(type, out var persistentRoot))
		{
			// Initialize if not yet initialized (for preloaded roots)
			if (!_initializedRoots.Contains(type))
			{
				await persistentRoot.InitializeAsync(token).ConfigureAwait(continueOnCapturedContext);
				_initializedRoots.Add(type);
			}
			return (T)persistentRoot;
		}

		if (_cache.TryGetValue(type, out var cached))
		{
			_cachingStatuses[type] = CachingStatus.InUse;
			Touch(type);
			
			// Enable the cached root for use
			if (cached is ICacheable cacheable)
			{
				cacheable.Enable();
			}
			
			return (T)cached;
		}
		
		// Check if precaching is in progress
		if (_cachingTasks.TryGetValue(type, out var cachingTask))
		{
			var precached = await cachingTask.Task.ConfigureAwait(continueOnCapturedContext);
			_cachingStatuses[type] = CachingStatus.InUse;
			Touch(type);
			
			// Enable the cached root for use
			if (precached is ICacheable cacheable)
			{
				cacheable.Enable();
			}
			
			return (T)precached;
		}

		var created = _factory.Get(type);
		if (created is null)
		{
			throw new InvalidOperationException($"Factory failed to create {type.FullName}");
		}

		// Initialize the root
		await created.InitializeAsync(token).ConfigureAwait(continueOnCapturedContext);
		_initializedRoots.Add(type);

		// Store persistent roots separately
		if (created is IPersistentRoot)
		{
			_persistentRoots[type] = created;
			return (T)created;
		}

		// Non-persistent roots are considered transient
		_cachingStatuses[type] = CachingStatus.InUse;
		return (T)created;
	}

	/// <summary>
	/// Preloads resources for a composition root of type <typeparamref name="T"/> if it implements IPreloadable.
	/// </summary>
	/// <typeparam name="T">The concrete type of the root to preload.</typeparam>
	/// <param name="token">A cancellation token to observe while awaiting the operation.</param>
	/// <param name="continueOnCapturedContext">True to attempt to marshal the continuation back to the original context captured; otherwise, false.</param>
	/// <returns>A task that completes when the root is preloaded.</returns>
	/// <remarks>
	/// This method creates a root instance (WITHOUT initializing it) and calls PreloadAsync if it implements IPreloadable.
	/// The root is responsible for managing its own initialization state during preloading.
	/// If the root needs services from Initialize(), it should call Initialize() itself within PreloadAsync().
	/// </remarks>
	/// <exception cref="ObjectDisposedException">Thrown if the provider has been disposed.</exception>
	public async Task PreloadAsync<T>(CancellationToken token, bool continueOnCapturedContext = true)
		where T : ICompositionRoot
	{
		ThrowIfDisposed();

		var type = typeof(T);

		// Check if already preloading
		if (_preloadingTasks.TryGetValue(type, out var preloadingTask))
		{
			await preloadingTask.Task.ConfigureAwait(continueOnCapturedContext);
			return;
		}

		// Create a task completion source for this preloading operation
		var tcs = new TaskCompletionSource<ICompositionRoot>();
		_preloadingTasks[type] = tcs;

		try
		{
			// Create root WITHOUT initializing
			var created = _factory.Get(type);
			if (created is null)
			{
				throw new InvalidOperationException($"Factory failed to create {type.FullName}");
			}

			// Preload resources if the root supports it (WITHOUT initialization)
			if (created is IPreloadable preloadable)
			{
				await preloadable.PreloadAsync(token).ConfigureAwait(continueOnCapturedContext);
			}

			// Store based on root type
			if (created is IPersistentRoot)
			{
				_persistentRoots[type] = created;
			}

			tcs.SetResult(created);
		}
		catch (Exception ex)
		{
			tcs.SetException(ex);
			throw;
		}
		finally
		{
			_preloadingTasks.Remove(type);
		}
	}

	/// <summary>
	/// Preloads and caches a composition root of type <typeparamref name="T"/> without activating it.
	/// </summary>
	/// <typeparam name="T">The concrete type of the root to precache.</typeparam>
	/// <param name="token">A cancellation token to observe while awaiting the operation.</param>
	/// <param name="continueOnCapturedContext">True to attempt to marshal the continuation back to the original context captured; otherwise, false.</param>
	/// <returns>A task that completes when the root is precached.</returns>
	/// <remarks>
	/// This method creates and initializes a root instance, then immediately disables it and stores it in the cache.
	/// Only roots implementing <see cref="ICacheable"/> can be precached.
	/// Persistent roots cannot be precached as they are always active.
	/// If the root is already cached or being cached, this method returns immediately.
	/// </remarks>
	/// <exception cref="ObjectDisposedException">Thrown if the provider has been disposed.</exception>
	/// <exception cref="InvalidOperationException">Thrown if the root type doesn't support caching.</exception>
	public async Task PrecacheAsync<T>(CancellationToken token, bool continueOnCapturedContext = true)
		where T : ICompositionRoot
	{
		ThrowIfDisposed();
		CleanupExpired();

		var type = typeof(T);

		// Already cached
		if (_cache.ContainsKey(type))
		{
			return;
		}

		// Already being cached
		if (_cachingTasks.TryGetValue(type, out var task))
		{
			await task.Task.ConfigureAwait(continueOnCapturedContext);
			return;
		}

		// Create a task completion source for this caching operation
		var tcs = new TaskCompletionSource<ICompositionRoot>();
		_cachingTasks[type] = tcs;
		_cachingStatuses[type] = CachingStatus.Caching;

		try
		{
			var created = _factory.Get(type);
			if (created is null)
			{
				throw new InvalidOperationException($"Factory failed to create {type.FullName}");
			}

			// Check if the root can be cached
			if (created is IPersistentRoot)
			{
				created.Dispose();
				throw new InvalidOperationException($"Type {type.FullName} is a persistent root and cannot be precached");
			}

			if (created is not ICacheable cacheable)
			{
				created.Dispose();
				throw new InvalidOperationException($"Type {type.FullName} must implement ICacheable to be precached");
			}

			// Initialize the root
			await created.InitializeAsync(token).ConfigureAwait(continueOnCapturedContext);
			_initializedRoots.Add(type);
			
			// Immediately disable the root for caching to prevent UI flashing
			await cacheable.DisableAsync(token).ConfigureAwait(continueOnCapturedContext);

			_cache[type] = created;
			_lastUsedUtc[type] = DateTime.UtcNow;
			_cachingStatuses[type] = CachingStatus.Cached;

			EnforceCapacity();

			tcs.SetResult(created);
		}
		catch (Exception ex)
		{
			_cachingStatuses[type] = CachingStatus.NotCached;
			tcs.SetException(ex);
			throw;
		}
		finally
		{
			_cachingTasks.Remove(type);
		}
	}

	/// <summary>
	/// Gets the caching status for a specific root type.
	/// </summary>
	/// <typeparam name="T">The concrete type of the root to check.</typeparam>
	/// <returns>The current caching status of the root.</returns>
	public CachingStatus GetCacheStatus<T>() where T : ICompositionRoot
	{
		return GetCacheStatus(typeof(T));
	}

	/// <summary>
	/// Gets the caching status for a specific root type.
	/// </summary>
	/// <param name="rootType">The type of the root to check.</param>
	/// <returns>The current caching status of the root.</returns>
	public CachingStatus GetCacheStatus(Type rootType)
	{
		ThrowIfDisposed();
		
		if (_cachingStatuses.TryGetValue(rootType, out var status))
		{
			return status;
		}

		return CachingStatus.NotCached;
	}

	/// <summary>
	/// Checks if a root type is currently cached.
	/// </summary>
	/// <typeparam name="T">The concrete type of the root to check.</typeparam>
	/// <returns>True if the root is cached; otherwise, false.</returns>
	public bool IsCached<T>() where T : ICompositionRoot
	{
		return IsCached(typeof(T));
	}

	/// <summary>
	/// Checks if a root type is currently cached.
	/// </summary>
	/// <param name="rootType">The type of the root to check.</param>
	/// <returns>True if the root is cached; otherwise, false.</returns>
	public bool IsCached(Type rootType)
	{
		ThrowIfDisposed();
		return _cache.ContainsKey(rootType);
	}

	/// <summary>
	/// Releases a root instance back to the provider.
	/// </summary>
	/// <param name="root">The root instance to release.</param>
	/// <remarks>
	/// Persistent roots are not disposed when released.
	/// If <paramref name="root"/> implements <see cref="ICacheable"/>,
	/// the provider calls <see cref="ICacheable.Disable"/> and stores
	/// the instance in the cache (subject to capacity and TTL policies).
	/// Otherwise, the provider calls <see cref="ICompositionRoot.Dispose"/> and the instance
	/// is not cached.
	/// </remarks>
	/// <exception cref="ObjectDisposedException">Thrown if the provider has been disposed.</exception>
	public void Release(ICompositionRoot root)
	{
		if (root is null)
		{
			return;
		}

		ThrowIfDisposed();
		CleanupExpired();

		var type = root.GetType();

		// Don't dispose persistent roots on release
		if (root is IPersistentRoot)
		{
			return;
		}

		if (root is ICacheable cacheable)
		{
			if (_cache.TryGetValue(type, out var old) && !ReferenceEquals(old, root))
			{
				old.Dispose();
			}

			// Disable the root for caching
			cacheable.Disable();
			
			_cache[type] = root;
			_lastUsedUtc[type] = DateTime.UtcNow;
			_cachingStatuses[type] = CachingStatus.Cached;

			EnforceCapacity();
		}
		else
		{
			root.Dispose();
			_cachingStatuses[type] = CachingStatus.NotCached;
			_initializedRoots.Remove(type);
		}
	}

	/// <summary>
	/// Asynchronously releases a root instance back to the provider.
	/// </summary>
	/// <param name="root">The root instance to release.</param>
	/// <param name="token">A cancellation token to observe while awaiting the operation.</param>
	/// <param name="continueOnCapturedContext">True to attempt to marshal the continuation back to the original context captured; otherwise, false.</param>
	/// <remarks>
	/// Persistent roots are not disposed when released; they persist until the provider is disposed.
	/// If <paramref name="root"/> implements <see cref="ICacheable"/>,
	/// the provider calls <see cref="ICacheable.DisableAsync(CancellationToken)"/>
	/// and stores the instance in the cache (subject to capacity and TTL policies).
	/// Otherwise, the provider calls <see cref="ICompositionRoot.Dispose"/> and the instance
	/// is not cached.
	/// </remarks>
	/// <exception cref="ObjectDisposedException">Thrown if the provider has been disposed.</exception>
	public async Task ReleaseAsync(ICompositionRoot root, CancellationToken token, bool continueOnCapturedContext = true)
	{
		if (root is null)
		{
			return;
		}

		ThrowIfDisposed();
		CleanupExpired();

		var type = root.GetType();

		// Don't dispose persistent roots on release
		if (root is IPersistentRoot)
		{
			return;
		}

		if (root is ICacheable cacheable)
		{
			if (_cache.TryGetValue(type, out var old) && !ReferenceEquals(old, root))
			{
				old.Dispose();
			}

			// Disable the root for caching
			await cacheable.DisableAsync(token).ConfigureAwait(continueOnCapturedContext);
			
			_cache[type] = root;
			_lastUsedUtc[type] = DateTime.UtcNow;
			_cachingStatuses[type] = CachingStatus.Cached;

			EnforceCapacity();
		}
		else
		{
			root.Dispose();
			_cachingStatuses[type] = CachingStatus.NotCached;
			_initializedRoots.Remove(type);
		}
	}
	
	/// <summary>
	/// Releases all cached roots and persistent roots, then marks the provider as disposed.
	/// </summary>
	/// <remarks>
	/// After disposal, further calls to public methods will throw <see cref="ObjectDisposedException"/>.
	/// This method disposes all cached transient roots and all persistent roots immediately.
	/// </remarks>
	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		_disposed = true;

		// Dispose cached transient roots
		foreach (var kv in _cache)
		{
			kv.Value.Dispose();
		}

		// Dispose persistent roots
		foreach (var kv in _persistentRoots)
		{
			kv.Value.Dispose();
		}

		_cache.Clear();
		_persistentRoots.Clear();
		_preloadingTasks.Clear();
		_lastUsedUtc.Clear();
		_cachingStatuses.Clear();
		_cachingTasks.Clear();
		_initializedRoots.Clear();
	}

	private void Touch(Type type)
	{
		_lastUsedUtc[type] = DateTime.UtcNow;
	}

	private void EnforceCapacity()
	{
		var iterations = 0;
		const int maxIterations = 1000;


		while (_cache.Count > _maxCached)
		{
			if (++iterations > maxIterations)
			{
				throw new InvalidOperationException(
					$"EnforceCapacity exceeded maximum iterations ({maxIterations}). " +
					$"Cache count: {_cache.Count}, MaxCached: {_maxCached}, LastUsed count: {_lastUsedUtc.Count}");
			}

			Type victimType = null;
			var oldestTime = DateTime.MaxValue;
		
			foreach (var kv in _lastUsedUtc)
			{
				if (kv.Value < oldestTime)
				{
					oldestTime = kv.Value;
					victimType = kv.Key;
				}
			}
		
			if (victimType != null && _cache.Remove(victimType, out var victim))
			{
				victim.Dispose();
				_cachingStatuses[victimType] = CachingStatus.NotCached;
				_initializedRoots.Remove(victimType);
			}

			_lastUsedUtc.Remove(victimType);
		}
	}

	private void CleanupExpired()
	{
		if (_maxIdle <= TimeSpan.Zero)
		{
			return;
		}

		var now = DateTime.UtcNow;
		var toEvict = new List<Type>();

		foreach (var kv in _lastUsedUtc)
		{
			if (now - kv.Value > _maxIdle)
			{
				toEvict.Add(kv.Key);
			}
		}

		foreach (var type in toEvict)
		{
			if (_cache.Remove(type, out var victim))
			{
				victim.Dispose();
				_cachingStatuses[type] = CachingStatus.NotCached;
				_initializedRoots.Remove(type);
			}

			_lastUsedUtc.Remove(type);
		}
	}

	private void ThrowIfDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(GetType().FullName);
		}
	}
}
}