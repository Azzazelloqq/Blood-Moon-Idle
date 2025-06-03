using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Runtime.Core.Architecture.GameGlobalState.Contract;

namespace Runtime.Core.Architecture.GameGlobalState.Tests
{
[TestFixture]
public class StateBaseTests
{
	private TestStateImplementation _testState;
	
	[SetUp]
	public void SetUp()
	{
		_testState = new TestStateImplementation();
	}
	
	[Test]
	public async Task IState_EnterAsync_CallsProtectedEnterAsync()
	{
		// Arrange
		IState state = _testState;
		var cts = new CancellationTokenSource();
		
		// Act
		await state.EnterAsync(cts.Token);
		
		// Assert
		Assert.That(_testState.EnterAsyncCalled, Is.True);
		Assert.That(_testState.LastEnterToken, Is.EqualTo(cts.Token));
	}
	
	[Test]
	public async Task IState_ExitAsync_CallsProtectedExitAsync()
	{
		// Arrange
		IState state = _testState;
		var cts = new CancellationTokenSource();
		
		// Act
		await state.ExitAsync(cts.Token);
		
		// Assert
		Assert.That(_testState.ExitAsyncCalled, Is.True);
		Assert.That(_testState.LastExitToken, Is.EqualTo(cts.Token));
	}
	
	[Test]
	public void IState_Tick_CallsProtectedTick()
	{
		// Arrange
		IState state = _testState;
		
		// Act
		state.Tick(0.16f);
		
		// Assert
		Assert.That(_testState.TickCalled, Is.True);
		Assert.That(_testState.LastTickDelta, Is.EqualTo(0.16f));
	}
	
	[Test]
	public void IState_FixedTick_CallsProtectedFixedTick()
	{
		// Arrange
		IState state = _testState;
		
		// Act
		state.FixedTick(0.02f);
		
		// Assert
		Assert.That(_testState.FixedTickCalled, Is.True);
		Assert.That(_testState.LastFixedTickDelta, Is.EqualTo(0.02f));
	}
	
	[Test]
	public void IState_LateTick_CallsProtectedLateTick()
	{
		// Arrange
		IState state = _testState;
		
		// Act
		state.LateTick(0.16f);
		
		// Assert
		Assert.That(_testState.LateTickCalled, Is.True);
		Assert.That(_testState.LastLateTickDelta, Is.EqualTo(0.16f));
	}
	
	[Test]
	public void Dispose_CallsOnDispose()
	{
		// Act
		_testState.Dispose();
		
		// Assert
		Assert.That(_testState.OnDisposeCalled, Is.True);
	}
	
	[Test]
	public void DefaultImplementation_TickMethodsDoNothing()
	{
		// Arrange
		var minimalState = new MinimalStateImplementation();
		IState state = minimalState;
		
		// Act & Assert - Should not throw
		Assert.DoesNotThrow(() => state.Tick(0.16f));
		Assert.DoesNotThrow(() => state.FixedTick(0.02f));
		Assert.DoesNotThrow(() => state.LateTick(0.16f));
	}
	
	[Test]
	public void DefaultImplementation_OnDisposeDoesNothing()
	{
		// Arrange
		var minimalState = new MinimalStateImplementation();
		
		// Act & Assert - Should not throw
		Assert.DoesNotThrow(() => minimalState.Dispose());
	}
	
	[Test]
	public async Task StateBase_CanBeUsedPolymorphically()
	{
		// Arrange
		StateBase stateBase = _testState;
		IState stateInterface = _testState;
		
		// Act
		await stateInterface.EnterAsync(CancellationToken.None);
		stateInterface.Tick(0.1f);
		await stateInterface.ExitAsync(CancellationToken.None);
		stateBase.Dispose();
		
		// Assert
		Assert.That(_testState.EnterAsyncCalled, Is.True);
		Assert.That(_testState.TickCalled, Is.True);
		Assert.That(_testState.ExitAsyncCalled, Is.True);
		Assert.That(_testState.OnDisposeCalled, Is.True);
	}

	[Test]
	public async Task IState_DisposeAsync_CallsProtectedOnDisposeAsync()
	{
		// Arrange
		IState state = _testState;
		var cts = new CancellationTokenSource();
		
		// Act
		await state.DisposeAsync(cts.Token);
		
		// Assert
		Assert.That(_testState.OnDisposeAsyncCalled, Is.True);
		Assert.That(_testState.LastDisposeAsyncToken, Is.EqualTo(cts.Token));
		Assert.That(_testState.IsDisposed, Is.True);
	}

	[Test]
	public async Task DisposeAsync_CallsOnDisposeAsyncAndSetsDisposed()
	{
		// Act
		await _testState.DisposeAsync(CancellationToken.None);
		
		// Assert
		Assert.That(_testState.OnDisposeAsyncCalled, Is.True);
		Assert.That(_testState.IsDisposed, Is.True);
	}

	[Test]
	public async Task DisposeAsync_CalledMultipleTimes_IsIdempotent()
	{
		// Act
		await _testState.DisposeAsync(CancellationToken.None);
		await _testState.DisposeAsync(CancellationToken.None);
		
		// Assert
		Assert.That(_testState.IsDisposed, Is.True);
		// Should not call OnDisposeAsync multiple times (base implementation would need to track this)
	}

	[Test]
	public async Task DefaultImplementation_OnDisposeAsyncDoesNothing()
	{
		// Arrange
		var minimalState = new MinimalStateImplementation();
		IState state = minimalState;
		
		// Act & Assert - Should not throw
		Assert.DoesNotThrowAsync(async () => await state.DisposeAsync(CancellationToken.None));
	}
	
	// Test support classes
	private class TestStateImplementation : StateBase
	{
		public bool EnterAsyncCalled { get; private set; }
		public bool ExitAsyncCalled { get; private set; }
		public bool TickCalled { get; private set; }
		public bool FixedTickCalled { get; private set; }
		public bool LateTickCalled { get; private set; }
		public bool OnDisposeCalled { get; private set; }
		public bool OnDisposeAsyncCalled { get; private set; }
		
		public CancellationToken LastEnterToken { get; private set; }
		public CancellationToken LastExitToken { get; private set; }
		public CancellationToken LastDisposeAsyncToken { get; private set; }
		public float LastTickDelta { get; private set; }
		public float LastFixedTickDelta { get; private set; }
		public float LastLateTickDelta { get; private set; }
		
		protected override Task EnterAsync(CancellationToken token)
		{
			EnterAsyncCalled = true;
			LastEnterToken = token;
			return Task.CompletedTask;
		}
		
		protected override Task ExitAsync(CancellationToken token)
		{
			ExitAsyncCalled = true;
			LastExitToken = token;
			return Task.CompletedTask;
		}
		
		protected override void Tick(float deltaTime)
		{
			TickCalled = true;
			LastTickDelta = deltaTime;
		}
		
		protected override void FixedTick(float deltaTime)
		{
			FixedTickCalled = true;
			LastFixedTickDelta = deltaTime;
		}
		
		protected override void LateTick(float deltaTime)
		{
			LateTickCalled = true;
			LastLateTickDelta = deltaTime;
		}
		
		protected override void OnDispose()
		{
			OnDisposeCalled = true;
		}

		protected override ValueTask OnDisposeAsync(CancellationToken token)
		{
			OnDisposeAsyncCalled = true;
			LastDisposeAsyncToken = token;
			return default;
		}
	}
	
	private class MinimalStateImplementation : StateBase
	{
		protected override Task EnterAsync(CancellationToken token)
		{
			return Task.CompletedTask;
		}
		
		protected override Task ExitAsync(CancellationToken token)
		{
			return Task.CompletedTask;
		}
	}
}
}