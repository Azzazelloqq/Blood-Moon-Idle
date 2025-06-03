using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Runtime.Core.Architecture.CompositionRoot.Base;
using Runtime.Core.Architecture.CompositionRoot.Facade;
using Runtime.Core.Architecture.CompositionRoot.Factory;

namespace Runtime.Core.Architecture.CompositionRoot.Tests
{
[TestFixture]
public class CompositionRootProviderTests
{
	private CompositionRootProvider _provider;
	private TestCompositionRootFactory _testFactory;
	
	[SetUp]
	public void SetUp()
	{
		_testFactory = new TestCompositionRootFactory();
		_provider = new CompositionRootProvider(_testFactory, maxCached: 2, maxIdle: TimeSpan.FromSeconds(30));
	}
	
	[TearDown]
	public void TearDown()
	{
		_provider?.Dispose();
	}
	
	[Test]
	public void GetCacheStatus_WithoutCaching_ReturnsNotCached()
	{
		// Act
		var status = _provider.GetCacheStatus<TestNonCacheable>();
		
		// Assert
		Assert.AreEqual(CachingStatus.NotCached, status);
	}
	
	[Test]
	public async Task GetRootAsync_NonCacheableRoot_SetsStatusToInUse()
	{
		// Act
		var root = await _provider.GetRootAsync<TestNonCacheable>(CancellationToken.None);
		var status = _provider.GetCacheStatus<TestNonCacheable>();
		
		// Assert
		Assert.IsNotNull(root);
		Assert.AreEqual(CachingStatus.InUse, status);
	}
	
	[Test]
	public async Task PrecacheAsync_CacheableRoot_SetsStatusToCached()
	{
		// Act
		await _provider.PrecacheAsync<TestCacheable>(CancellationToken.None);
		var status = _provider.GetCacheStatus<TestCacheable>();
		var isCached = _provider.IsCached<TestCacheable>();
		
		// Assert
		Assert.AreEqual(CachingStatus.Cached, status);
		Assert.IsTrue(isCached);
	}
	
	[Test]
	public async Task PrecacheAsync_InitializesAndDisablesRoot()
	{
		// Act
		await _provider.PrecacheAsync<TestCacheable>(CancellationToken.None);
		
		// Verify the root is cached
		var status = _provider.GetCacheStatus<TestCacheable>();
		Assert.AreEqual(CachingStatus.Cached, status);
		
		// Get the root from cache
		var root = await _provider.GetRootAsync<TestCacheable>(CancellationToken.None);
		var testRoot = root as TestCacheable;
		
		// Assert - Verify that the root was initialized and then disabled for caching
		Assert.IsNotNull(testRoot);
		Assert.IsTrue(testRoot.IsInitialized, "Initialize should have been called during precaching");
		// After GetRootAsync, the cached root should be enabled for use
		Assert.IsFalse(testRoot.IsDisabled, "Root should be enabled after retrieval from cache");
	}
	
	[Test]
	public async Task GetRootAsync_AfterPrecache_ReusesInstance()
	{
		// Arrange
		await _provider.PrecacheAsync<TestCacheable>(CancellationToken.None);
		
		// Act
		var root1 = await _provider.GetRootAsync<TestCacheable>(CancellationToken.None);
		await _provider.ReleaseAsync(root1, CancellationToken.None);
		
		var root2 = await _provider.GetRootAsync<TestCacheable>(CancellationToken.None);
		
		// Assert
		Assert.AreSame(root1, root2);
	}
	
	[Test]
	public async Task PrecacheAsync_MultipleCalls_ReturnsImmediately()
	{
		// Arrange
		var task1 = _provider.PrecacheAsync<TestCacheable>(CancellationToken.None);
		var task2 = _provider.PrecacheAsync<TestCacheable>(CancellationToken.None);
		
		// Act
		await Task.WhenAll(task1, task2);
		var status = _provider.GetCacheStatus<TestCacheable>();
		
		// Assert
		Assert.AreEqual(CachingStatus.Cached, status);
	}
	
	[Test]
	public async Task ReleaseAsync_CacheableRoot_SetsStatusToCached()
	{
		// Arrange
		var root = await _provider.GetRootAsync<TestCacheable>(CancellationToken.None);
		
		// Act
		await _provider.ReleaseAsync(root, CancellationToken.None);
		var status = _provider.GetCacheStatus<TestCacheable>();
		var testRoot = root as TestCacheable;
		
		// Assert
		Assert.AreEqual(CachingStatus.Cached, status);
		Assert.IsTrue(testRoot.IsDisabled, "Root should be disabled when released for caching");
	}
	
	[Test]
	public async Task ReleaseAsync_NonCacheableRoot_SetsStatusToNotCached()
	{
		// Arrange
		var root = await _provider.GetRootAsync<TestNonCacheable>(CancellationToken.None);
		
		// Act
		await _provider.ReleaseAsync(root, CancellationToken.None);
		var status = _provider.GetCacheStatus<TestNonCacheable>();
		
		// Assert
		Assert.AreEqual(CachingStatus.NotCached, status);
	}
	
	[Test]
	public void IsCached_WithCachedRoot_ReturnsTrue()
	{
		// Arrange
		var task = _provider.PrecacheAsync<TestCacheable>(CancellationToken.None);
		task.Wait();
		
		// Act
		var isCached = _provider.IsCached<TestCacheable>();
		
		// Assert
		Assert.IsTrue(isCached);
	}
	
	[Test]
	public void IsCached_WithoutCachedRoot_ReturnsFalse()
	{
		// Act
		var isCached = _provider.IsCached<TestCacheable>();
		
		// Assert
		Assert.IsFalse(isCached);
	}
	
	[Test]
	public async Task GetRootAsync_WhilePrecaching_WaitsForCompletion()
	{
		// Arrange
		var precacheStarted = new TaskCompletionSource<bool>();
		var precacheCompleted = new TaskCompletionSource<bool>();
		
		// Start precaching in background
		var precacheTask = Task.Run(async () =>
		{
			precacheStarted.SetResult(true);
			await _provider.PrecacheAsync<TestSlowCacheable>(CancellationToken.None);
			precacheCompleted.SetResult(true);
		});
		
		// Wait for precache to start
		await precacheStarted.Task;
		
		// Act - Try to get root while precaching is in progress
		var getRootTask = _provider.GetRootAsync<TestSlowCacheable>(CancellationToken.None);
		
		// Assert - GetRootAsync should wait for precache to complete
		await Task.WhenAll(precacheTask, getRootTask);
		Assert.IsTrue(precacheCompleted.Task.IsCompleted);
		Assert.IsNotNull(getRootTask.Result);
	}
	
	[Test]
	public async Task Release_CacheableRoot_CallsDisable()
	{
		// Arrange
		var root = await _provider.GetRootAsync<TestCacheable>(CancellationToken.None);
		var testRoot = root as TestCacheable;
		
		// Enable the root to ensure it's in active state
		await testRoot.EnableAsync(CancellationToken.None);
		Assert.IsTrue(testRoot.IsEnabled);
		Assert.IsFalse(testRoot.IsDisabled);
		
		// Act
		_provider.Release(root);
		
		// Assert
		Assert.IsTrue(testRoot.IsDisabled, "Release should call Disable()");
		Assert.IsFalse(testRoot.IsEnabled, "Root should no longer be enabled after Release");
	}
	
	[Test]
	public async Task ReleaseAsync_CacheableRoot_CallsDisableAsync()
	{
		// Arrange
		var root = await _provider.GetRootAsync<TestCacheable>(CancellationToken.None);
		var testRoot = root as TestCacheable;
		
		// Enable the root to ensure it's in active state
		await testRoot.EnableAsync(CancellationToken.None);
		Assert.IsTrue(testRoot.IsEnabled);
		Assert.IsFalse(testRoot.IsDisabled);
		
		// Act
		await _provider.ReleaseAsync(root, CancellationToken.None);
		
		// Assert
		Assert.IsTrue(testRoot.IsDisabled, "ReleaseAsync should call DisableAsync()");
		Assert.IsFalse(testRoot.IsEnabled, "Root should no longer be enabled after ReleaseAsync");
	}
	
	[Test]
	public async Task GetRootAsync_PersistentRoot_PersistsAcrossReleases()
	{
		// Arrange & Act
		var root1 = await _provider.GetRootAsync<TestPersistentRoot>(CancellationToken.None);
		var testRoot1 = root1 as TestPersistentRoot;
		
		// Release should not dispose persistent root
		await _provider.ReleaseAsync(root1, CancellationToken.None);
		Assert.IsFalse(testRoot1.IsDisposed, "Persistent root should not be disposed on release");
		
		// Get again should return the same instance
		var root2 = await _provider.GetRootAsync<TestPersistentRoot>(CancellationToken.None);
		
		// Assert
		Assert.AreSame(root1, root2, "Should return the same persistent root instance");
	}
	
	[Test]
	public void Dispose_Provider_DisposesPersistentRoots()
	{
		// Arrange
		var task = _provider.GetRootAsync<TestPersistentRoot>(CancellationToken.None);
		task.Wait();
		var root = task.Result as TestPersistentRoot;
		
		// Act
		_provider.Dispose();
		
		// Assert
		Assert.IsTrue(root.IsDisposed, "Persistent root should be disposed when provider is disposed");
	}
	
	[Test]
	public async Task PrecacheAsync_PersistentRoot_ThrowsInvalidOperationException()
	{
		// Act & Assert
		Assert.ThrowsAsync<InvalidOperationException>(async () =>
		{
			await _provider.PrecacheAsync<TestPersistentRoot>(CancellationToken.None);
		});
	}
	
	[Test]
	public async Task PreloadAsync_CallsPreloadOnPreloadableRoot()
	{
		// Arrange & Act
		await _provider.PreloadAsync<TestPreloadableRoot>(CancellationToken.None);
		
		// Get the preloaded root
		var root = await _provider.GetRootAsync<TestPreloadableRoot>(CancellationToken.None);
		var testRoot = root as TestPreloadableRoot;
		
		// Assert
		Assert.IsNotNull(testRoot);
		Assert.IsTrue(testRoot.IsPreloaded, "PreloadAsync should have been called");
		// Should return the same instance since it's persistent
		var root2 = await _provider.GetRootAsync<TestPreloadableRoot>(CancellationToken.None);
		Assert.AreSame(root, root2);
	}
	
	[Test]
	public async Task PreloadAsync_DoesNotAutoInitialize()
	{
		// Arrange & Act
		await _provider.PreloadAsync<TestPreloadableWithInit>(CancellationToken.None);
		
		// Get the preloaded root
		var root = await _provider.GetRootAsync<TestPreloadableWithInit>(CancellationToken.None);
		var testRoot = root as TestPreloadableWithInit;
		
		// Assert
		Assert.IsNotNull(testRoot);
		Assert.IsTrue(testRoot.IsPreloaded, "PreloadAsync should have been called");
		// PreloadAsync should NOT automatically call Initialize
		Assert.IsFalse(testRoot.WasInitializedDuringPreload, "Initialize should not be called automatically during PreloadAsync");
		// But it should be initialized when retrieved via GetRootAsync
		Assert.IsTrue(testRoot.IsInitialized, "Initialize should be called when getting root via GetRootAsync");
	}
	
	[Test]
	public async Task PreloadAsync_RootCanInitializeItself()
	{
		// Arrange & Act
		await _provider.PreloadAsync<TestSelfInitializingPreloadable>(CancellationToken.None);
		
		// Get the preloaded root
		var root = await _provider.GetRootAsync<TestSelfInitializingPreloadable>(CancellationToken.None);
		var testRoot = root as TestSelfInitializingPreloadable;
		
		// Assert
		Assert.IsNotNull(testRoot);
		Assert.IsTrue(testRoot.IsPreloaded, "PreloadAsync should have been called");
		Assert.IsTrue(testRoot.WasInitializedDuringPreload, "Root should have initialized itself during PreloadAsync");
		Assert.IsTrue(testRoot.IsInitialized, "Root should be initialized");
	}
	
	[Test]
	public async Task GetRootAsync_WithoutPreload_DoesNotCallPreload()
	{
		// Arrange & Act - Get root normally without preload
		var normalRoot = await _provider.GetRootAsync<TestPreloadableWithInit>(CancellationToken.None);
		var normalTestRoot = normalRoot as TestPreloadableWithInit;
		
		// Assert normal behavior
		Assert.IsNotNull(normalTestRoot);
		Assert.IsTrue(normalTestRoot.IsInitialized, "Root should be initialized via normal GetRootAsync");
		Assert.IsFalse(normalTestRoot.IsPreloaded, "PreloadAsync should not have been called");
		Assert.IsFalse(normalTestRoot.WasInitializedDuringPreload, "Initialize should not have been during preload");
	}
	
	// Test implementations
	private class TestNonCacheable : ICompositionRoot
	{
		public bool IsInitialized { get; private set; }
		public bool WasInitializedDisabled { get; private set; }
		public bool IsDisposed { get; private set; }
		
		public void Initialize()
		{
			IsInitialized = true;
			WasInitializedDisabled = false;
		}
		
		public ValueTask InitializeAsync(CancellationToken token)
		{
			IsInitialized = true;
			WasInitializedDisabled = false;
			return default;
		}
		
		public void Dispose()
		{
			IsDisposed = true;
		}
	}
	
	private class TestCacheable : ICompositionRoot, ICacheable
	{
		public bool IsInitialized { get; private set; }
		public bool WasInitializedDisabled { get; private set; }
		public bool IsDisabled { get; private set; }
		public bool IsEnabled { get; private set; }
		public bool IsDisposed { get; private set; }
		
		public void Initialize()
		{
			IsInitialized = true;
			WasInitializedDisabled = false;
			IsDisabled = false;
		}
		
		public ValueTask InitializeAsync(CancellationToken token)
		{
			IsInitialized = true;
			WasInitializedDisabled = false;
			IsDisabled = false;
			return default;
		}
		
		public void Disable()
		{
			IsDisabled = true;
			IsEnabled = false;
		}
		
		public ValueTask DisableAsync(CancellationToken token)
		{
			IsDisabled = true;
			IsEnabled = false;
			return default;
		}
		
		public void Enable()
		{
			IsEnabled = true;
			IsDisabled = false;
		}
		
		public ValueTask EnableAsync(CancellationToken token)
		{
			IsEnabled = true;
			IsDisabled = false;
			return default;
		}
		
		public void Dispose()
		{
			IsDisposed = true;
		}
	}
	
	private class TestSlowCacheable : ICompositionRoot, ICacheable
	{
		public void Initialize()
		{
			Thread.Sleep(100);
		}

		public async ValueTask InitializeAsync(CancellationToken token)
		{
			await Task.Delay(100, token);
		}
	
		public void Disable() { }
		
		public ValueTask DisableAsync(CancellationToken token) => default;
		
		public void Enable() { }
		
		public ValueTask EnableAsync(CancellationToken token) => default;
		
		public void Dispose() { }
	}
	
	private class TestPersistentRoot : ICompositionRoot, IPersistentRoot
	{
		public bool IsInitialized { get; private set; }
		public bool IsDisposed { get; private set; }
		
		public void Initialize()
		{
			IsInitialized = true;
		}
		
		public ValueTask InitializeAsync(CancellationToken token)
		{
			IsInitialized = true;
			return default;
		}
		
		public void Dispose()
		{
			IsDisposed = true;
		}
	}
	
	private class TestPreloadableRoot : ICompositionRoot, IPersistentRoot, IPreloadable
	{
		public bool IsInitialized { get; private set; }
		public bool IsPreloaded { get; private set; }
		public bool IsDisposed { get; private set; }
		
		public void Initialize()
		{
			IsInitialized = true;
		}
		
		public ValueTask InitializeAsync(CancellationToken token)
		{
			IsInitialized = true;
			return default;
		}
		
			public async ValueTask PreloadAsync(CancellationToken token)
	{
		// PreloadAsync does NOT call Initialize - this tests the new behavior
		await Task.Delay(10, token); // Simulate some async work
		IsPreloaded = true;
	}
		
		public void Dispose()
		{
			IsDisposed = true;
		}
	}
	
	private class TestPreloadableWithInit : ICompositionRoot, IPersistentRoot, IPreloadable
	{
		public bool IsInitialized { get; private set; }
		public bool IsPreloaded { get; private set; }
		public bool WasInitializedDuringPreload { get; private set; }
		public bool IsDisposed { get; private set; }
		
		public void Initialize()
		{
			IsInitialized = true;
		}
		
		public ValueTask InitializeAsync(CancellationToken token)
		{
			IsInitialized = true;
			return default;
		}
		
		public async ValueTask PreloadAsync(CancellationToken token)
		{
			// Track if Initialize was called during preload
			WasInitializedDuringPreload = IsInitialized;
			
			await Task.Delay(10, token);
			IsPreloaded = true;
		}
		
		public void Dispose()
		{
			IsDisposed = true;
		}
	}
	
	private class TestSelfInitializingPreloadable : ICompositionRoot, IPersistentRoot, IPreloadable
	{
		public bool IsInitialized { get; private set; }
		public bool IsPreloaded { get; private set; }
		public bool WasInitializedDuringPreload { get; private set; }
		public bool IsDisposed { get; private set; }
		
		public void Initialize()
		{
			IsInitialized = true;
		}
		
		public ValueTask InitializeAsync(CancellationToken token)
		{
			IsInitialized = true;
			return default;
		}
		
		public async ValueTask PreloadAsync(CancellationToken token)
		{
			// This root decides to initialize itself during preload
			await InitializeAsync(token);
			WasInitializedDuringPreload = true;
			
			await Task.Delay(10, token);
			IsPreloaded = true;
		}
		
		public void Dispose()
		{
			IsDisposed = true;
		}
	}
	
	// Test factory that can create test roots using reflection
	private class TestCompositionRootFactory : ICompositionRootFactory
	{
		public ICompositionRoot Get(Type rootType)
		{
			// For test types, create them using Activator
			if (rootType.IsNested && rootType.DeclaringType == typeof(CompositionRootProviderTests))
			{
				return (ICompositionRoot)Activator.CreateInstance(rootType);
			}
			
			// For unknown types, throw NotSupportedException like the real factory
			throw new NotSupportedException($"Test factory doesn't support type: {rootType.FullName}");
		}
	}
}
}
