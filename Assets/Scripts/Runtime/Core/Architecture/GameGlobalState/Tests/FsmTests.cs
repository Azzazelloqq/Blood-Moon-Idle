using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Runtime.Core.Architecture.GameGlobalState.Contract;
using Runtime.Core.Architecture.GameGlobalState.Core;

namespace Runtime.Core.Architecture.GameGlobalState.Tests
{
[TestFixture]
public class FsmTests
{
	private HashSet<string> _validIds;
	private TestStateFactory _stateFactory;
	private Fsm<string> _fsm;
	
	[SetUp]
	public void SetUp()
	{
		_validIds = new HashSet<string>
		{
			"state1",
			"state2",
			"state3"
		};
		_stateFactory = new TestStateFactory();
		_fsm = new Fsm<string>(_validIds, _stateFactory.CreateState);
	}
	
	[TearDown]
	public void TearDown()
	{
		_fsm?.Dispose();
	}
	
	[Test]
	public async Task ChangeStateAsync_ValidState_EntersNewState()
	{
		// Arrange
		var stateId = "state1";
		var state = new TestState();
		_stateFactory.RegisterState(stateId, state);
		
		// Act
		await _fsm.ChangeStateAsync(stateId, CancellationToken.None);
		
		// Assert
		Assert.That(_fsm.HasCurrent, Is.True);
		Assert.That(_fsm.CurrentId, Is.EqualTo(stateId));
		Assert.That(state.IsEntered, Is.True);
		Assert.That(state.IsExited, Is.False);
	}
	
	[Test]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
	public async Task ChangeStateAsync_InvalidState_ThrowsException()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
	{
		// Arrange
		var invalidStateId = "invalid";
		
		// Act & Assert
		var ex = Assert.ThrowsAsync<InvalidOperationException>(
			async () => await _fsm.ChangeStateAsync(invalidStateId, CancellationToken.None)
		);
		Assert.That(ex.Message, Does.Contain("not registered"));
	}
	
	[Test]
	public async Task ChangeStateAsync_TransitionBetweenStates_ExitsOldEntersNew()
	{
		// Arrange
		var state1Id = "state1";
		var state2Id = "state2";
		var state1 = new TestState();
		var state2 = new TestState();
		_stateFactory.RegisterState(state1Id, state1);
		_stateFactory.RegisterState(state2Id, state2);
		
		// Act
		await _fsm.ChangeStateAsync(state1Id, CancellationToken.None);
		await _fsm.ChangeStateAsync(state2Id, CancellationToken.None);
		
		// Assert
		Assert.That(_fsm.HasCurrent, Is.True);
		Assert.That(_fsm.CurrentId, Is.EqualTo(state2Id));
		Assert.That(state1.IsExited, Is.True);
		Assert.That(state1.IsDisposed, Is.True);
		Assert.That(state2.IsEntered, Is.True);
	}
	
	[Test]
	public async Task ChangeStateAsync_SameState_IsIdempotent()
	{
		// Arrange
		var stateId = "state1";
		var state = new TestState();
		_stateFactory.RegisterState(stateId, state);
		
		// Act
		await _fsm.ChangeStateAsync(stateId, CancellationToken.None);
		var enterCount = state.EnterCount;
		await _fsm.ChangeStateAsync(stateId, CancellationToken.None);
		
		// Assert
		Assert.That(state.EnterCount, Is.EqualTo(enterCount));
		Assert.That(state.ExitCount, Is.Zero);
	}
	
	[Test]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
	public async Task ChangeStateAsync_FactoryReturnsNull_ThrowsException()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
	{
		// Arrange
		var stateId = "state1";
		_stateFactory.RegisterState(stateId, null);
		
		// Act & Assert
		var ex = Assert.ThrowsAsync<InvalidOperationException>(
			async () => await _fsm.ChangeStateAsync(stateId, CancellationToken.None)
		);
		Assert.That(ex.Message, Does.Contain("returned null"));
	}
	
	[Test]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
	public async Task ChangeStateAsync_EnterThrowsException_DisposesStateAndRethrows()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
	{
		// Arrange
		var stateId = "state1";
		var state = new TestState { ThrowOnEnter = true };
		_stateFactory.RegisterState(stateId, state);
		
		// Act & Assert
		Assert.ThrowsAsync<InvalidOperationException>(
			async () => await _fsm.ChangeStateAsync(stateId, CancellationToken.None)
		);
		Assert.That(state.IsDisposed, Is.True);

		Assert.That(_fsm.HasCurrent, Is.False);
	}
	
	[Test]
	public async Task ChangeStateAsync_Cancellation_PropagatesToken()
	{
		// Arrange
		var stateId = "state1";
		var state = new TestState();
		_stateFactory.RegisterState(stateId, state);
		var cts = new CancellationTokenSource();
		
		// Act
		await _fsm.ChangeStateAsync(stateId, cts.Token);
		
		// Assert
		Assert.That(state.LastEnterToken, Is.EqualTo(cts.Token));
	}
	
	[Test]
	public void Tick_CallsCurrentStateTick()
	{
		// Arrange
		var stateId = "state1";
		var state = new TestState();
		_stateFactory.RegisterState(stateId, state);
		_fsm.ChangeStateAsync(stateId, CancellationToken.None).Wait();
		
		// Act
		_fsm.Tick(0.16f);
		
		// Assert
		Assert.That(state.TickCount, Is.EqualTo(1));
		Assert.That(state.LastTickDelta, Is.EqualTo(0.16f));
	}
	
	[Test]
	public void FixedTick_CallsCurrentStateFixedTick()
	{
		// Arrange
		var stateId = "state1";
		var state = new TestState();
		_stateFactory.RegisterState(stateId, state);
		_fsm.ChangeStateAsync(stateId, CancellationToken.None).Wait();
		
		// Act
		_fsm.FixedTick(0.02f);
		
		// Assert
		Assert.That(state.FixedTickCount, Is.EqualTo(1));
		Assert.That(state.LastFixedTickDelta, Is.EqualTo(0.02f));
	}
	
	[Test]
	public void LateTick_CallsCurrentStateLateTick()
	{
		// Arrange
		var stateId = "state1";
		var state = new TestState();
		_stateFactory.RegisterState(stateId, state);
		_fsm.ChangeStateAsync(stateId, CancellationToken.None).Wait();
		
		// Act
		_fsm.LateTick(0.16f);
		
		// Assert
		Assert.That(state.LateTickCount, Is.EqualTo(1));
		Assert.That(state.LastLateTickDelta, Is.EqualTo(0.16f));
	}
	
	[Test]
	public void Tick_NoCurrentState_DoesNotThrow()
	{
		// Act & Assert
		Assert.DoesNotThrow(() => _fsm.Tick(0.16f));
		Assert.DoesNotThrow(() => _fsm.FixedTick(0.02f));
		Assert.DoesNotThrow(() => _fsm.LateTick(0.16f));
	}
	
	[Test]
	public void Dispose_DisposesCurrentState()
	{
		// Arrange
		var stateId = "state1";
		var state = new TestState();
		_stateFactory.RegisterState(stateId, state);
		_fsm.ChangeStateAsync(stateId, CancellationToken.None).Wait();
		
		// Act
		_fsm.Dispose();
		
		// Assert
		// Dispose is synchronous and only calls Dispose(), not ExitAsync()
		Assert.That(state.IsDisposed, Is.True);
		Assert.That(state.IsExited, Is.False); // ExitAsync is NOT called during synchronous Dispose
	}
	
	[Test]
	public void Dispose_CalledMultipleTimes_IsIdempotent()
	{
		// Arrange
		var stateId = "state1";
		var state = new TestState();
		_stateFactory.RegisterState(stateId, state);
		_fsm.ChangeStateAsync(stateId, CancellationToken.None).Wait();
		
		// Act
		_fsm.Dispose();
		_fsm.Dispose();
		
		// Assert
		Assert.That(state.DisposeCount, Is.EqualTo(1));
	}
	
	[Test]
	public void ChangeStateAsync_AfterDispose_ThrowsObjectDisposedException()
	{
		// Arrange
		var stateId = "state1";
		_fsm.Dispose();
		
		// Act & Assert
		Assert.ThrowsAsync<ObjectDisposedException>(
			async () => await _fsm.ChangeStateAsync(stateId, CancellationToken.None)
		);
	}

	[Test]
	public async Task DisposeAsync_DisposesCurrentStateWithCancellationToken()
	{
		// Arrange
		var stateId = "state1";
		var state = new TestState();
		_stateFactory.RegisterState(stateId, state);
		await _fsm.ChangeStateAsync(stateId, CancellationToken.None);
		var cts = new CancellationTokenSource();

		// Act
		await _fsm.DisposeAsync(cts.Token);

		// Assert
		Assert.That(state.IsDisposeAsyncCalled, Is.True);
		Assert.That(state.LastDisposeAsyncToken, Is.EqualTo(cts.Token));
		Assert.That(state.IsDisposed, Is.True);
	}

	[Test]
	public async Task DisposeAsync_CalledMultipleTimes_IsIdempotent()
	{
		// Arrange
		var stateId = "state1";
		var state = new TestState();
		_stateFactory.RegisterState(stateId, state);
		await _fsm.ChangeStateAsync(stateId, CancellationToken.None);

		// Act
		await _fsm.DisposeAsync(CancellationToken.None);
		await _fsm.DisposeAsync(CancellationToken.None);

		// Assert
		Assert.That(state.DisposeAsyncCount, Is.EqualTo(1));
	}

	[Test]
	public async Task DisposeAsync_StateDisposeAsyncThrows_FallsBackToSyncDispose()
	{
		// Arrange
		var stateId = "state1";
		var state = new TestState { ThrowOnDisposeAsync = true };
		_stateFactory.RegisterState(stateId, state);
		await _fsm.ChangeStateAsync(stateId, CancellationToken.None);

		// Act & Assert
		Assert.ThrowsAsync<InvalidOperationException>(
			async () => await _fsm.DisposeAsync(CancellationToken.None)
		);

		// State should still be disposed via fallback sync Dispose()
		Assert.That(state.IsDisposed, Is.True);
	}

	[Test]
	public async Task DisposeAsync_CancellationRequested_FallsBackToSyncDispose()
	{
		// Arrange
		var stateId = "state1";
		var state = new TestState();
		_stateFactory.RegisterState(stateId, state);
		await _fsm.ChangeStateAsync(stateId, CancellationToken.None);
		var cts = new CancellationTokenSource();
		cts.Cancel(); // Cancel the token

		// Act
		await _fsm.DisposeAsync(cts.Token);

		// Assert
		// Should fallback to sync dispose when cancellation is requested
		Assert.That(state.IsDisposed, Is.True);
	}

	[Test]
	public void ChangeStateAsync_AfterDisposeAsync_ThrowsObjectDisposedException()
	{
		// Arrange
		var stateId = "state1";
		_fsm.DisposeAsync(CancellationToken.None).AsTask().Wait();
		
		// Act & Assert
		Assert.ThrowsAsync<ObjectDisposedException>(
			async () => await _fsm.ChangeStateAsync(stateId, CancellationToken.None)
		);
	}
	
	// Test support classes
	private class TestStateFactory : IStateFactory
	{
		private readonly Dictionary<string, IState> _states = new();
		
		public void RegisterState(string stateId, IState state)
		{
			_states[stateId] = state;
		}
		
		public IState CreateState(string stateId)
		{
			return _states.TryGetValue(stateId, out var state) ? state : null;
		}
	}
	
	private class TestState : IState
	{
		public bool IsEntered { get; private set; }
		public bool IsExited { get; private set; }
		public bool IsDisposed { get; private set; }
		public bool IsDisposeAsyncCalled { get; private set; }
		public int EnterCount { get; private set; }
		public int ExitCount { get; private set; }
		public int DisposeCount { get; private set; }
		public int DisposeAsyncCount { get; private set; }
		public int TickCount { get; private set; }
		public int FixedTickCount { get; private set; }
		public int LateTickCount { get; private set; }
		public float LastTickDelta { get; private set; }
		public float LastFixedTickDelta { get; private set; }
		public float LastLateTickDelta { get; private set; }
		public CancellationToken LastEnterToken { get; private set; }
		public CancellationToken LastDisposeAsyncToken { get; private set; }
		public bool ThrowOnEnter { get; set; }
		public bool ThrowOnDisposeAsync { get; set; }
		
		public Task EnterAsync(CancellationToken ct)
		{
			if (ThrowOnEnter)
				throw new InvalidOperationException("Enter failed");
				
			IsEntered = true;
			EnterCount++;
			LastEnterToken = ct;
			return Task.CompletedTask;
		}
		
		public Task ExitAsync(CancellationToken ct)
		{
			IsExited = true;
			ExitCount++;
			return Task.CompletedTask;
		}
		
		public void Tick(float deltaTime)
		{
			TickCount++;
			LastTickDelta = deltaTime;
		}
		
		public void FixedTick(float deltaTime)
		{
			FixedTickCount++;
			LastFixedTickDelta = deltaTime;
		}
		
		public void LateTick(float deltaTime)
		{
			LateTickCount++;
			LastLateTickDelta = deltaTime;
		}
		
		public void Dispose()
		{
			if (IsDisposed)
				return;
				
			IsDisposed = true;
			DisposeCount++;
		}

		public ValueTask DisposeAsync(CancellationToken ct)
		{
			if (ThrowOnDisposeAsync)
				throw new InvalidOperationException("DisposeAsync failed");

			IsDisposeAsyncCalled = true;
			DisposeAsyncCount++;
			LastDisposeAsyncToken = ct;

			// Also mark as disposed
			if (!IsDisposed)
			{
				IsDisposed = true;
				DisposeCount++;
			}

			return default;
		}
	}
}
}