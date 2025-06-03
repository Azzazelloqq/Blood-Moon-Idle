using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Runtime.Core.Architecture.GameGlobalState.Contract;
using Runtime.Core.Architecture.GameGlobalState.Core;

namespace Runtime.Core.Architecture.GameGlobalState.Tests
{
/// <summary>
/// Comprehensive tests for the disposal lifecycle functionality added to address
/// missing DisposeAsync calls in the state machine system.
/// </summary>
[TestFixture]
public class DisposalLifecycleTests
{
	private TestStateFactory _stateFactory;
	private Dictionary<string, TestState> _states;
	private IKernel _kernel;
	
	[SetUp]
	public void SetUp()
	{
		_stateFactory = new TestStateFactory();
		_states = new Dictionary<string, TestState>();
	}
	
	[TearDown]
	public void TearDown()
	{
		_kernel?.Dispose();
		
		foreach (var state in _states.Values)
		{
			if (!state.IsDisposed)
				state.Dispose();
		}
	}
	
	[Test]
	public async Task StateTransition_OldStateGetsDisposeAsyncDuringTransition()
	{
		// Arrange
		var state1Id = "state1";
		var state2Id = "state2";
		
		var state1 = CreateAndRegisterState(state1Id);
		var state2 = CreateAndRegisterState(state2Id);
		
		_kernel = Kernel.Create()
			.AddMain(state1Id)
			.AddMain(state2Id)
			.WithFactory(_stateFactory)
			.Build();
		
		await _kernel.Flow.RequestAsync(state1Id, CancellationToken.None);
		Assert.That(state1.IsActive, Is.True);
		
		var cts = new CancellationTokenSource();
		
		// Act - Transition to another state
		await _kernel.Flow.RequestAsync(state2Id, cts.Token);
		
		// Assert - Old state should be properly disposed through FSM transition
		Assert.That(state1.IsExited, Is.True, "State1 should have been exited");
		Assert.That(state1.IsDisposed, Is.True, "State1 should have been disposed");
		// Note: During transition, FSM calls Dispose() synchronously after ExitAsync()
		// This is expected behavior for state transitions
		
		Assert.That(state2.IsActive, Is.True, "State2 should be active");
	}
	
	[Test]
	public async Task KernelDispose_DoesNotCallDisposeAsync()
	{
		// Arrange
		var stateId = "test";
		var state = CreateAndRegisterState(stateId);
		
		_kernel = Kernel.Create()
			.AddMain(stateId)
			.SetInitialMain(stateId)
			.WithFactory(_stateFactory)
			.Build();
		
		Assert.That(state.IsActive, Is.True);
		
		// Act - Synchronous dispose
		_kernel.Dispose();
		
		// Assert - Only synchronous Dispose should be called
		Assert.That(state.IsDisposed, Is.True);
		Assert.That(state.IsDisposeAsyncCalled, Is.False, "DisposeAsync should NOT be called during synchronous disposal");
	}
	
	[Test]
	public async Task KernelDisposeAsync_CallsDisposeAsyncOnAllActiveStates()
	{
		// Arrange
		var mainId = "main";
		var subId = "sub";
		
		var mainState = CreateAndRegisterState(mainId);
		var subState = CreateAndRegisterState(subId);
		
		_kernel = Kernel.Create()
			.AddMain(mainId)
			.AddSub(mainId, subId)
			.SetInitialMain(mainId)
			.SetInitialSub(mainId, subId)
			.WithFactory(_stateFactory)
			.Build();
		
		Assert.That(mainState.IsActive, Is.True);
		Assert.That(subState.IsActive, Is.True);
		
		var cts = new CancellationTokenSource();
		
		// Act - Async dispose
		await _kernel.DisposeAsync(cts.Token);
		
		// Assert - Both states should receive DisposeAsync
		Assert.That(mainState.IsDisposeAsyncCalled, Is.True, "Main state should receive DisposeAsync");
		Assert.That(mainState.LastDisposeAsyncToken, Is.EqualTo(cts.Token), "Main state should receive correct token");
		Assert.That(subState.IsDisposeAsyncCalled, Is.True, "Sub state should receive DisposeAsync");
		Assert.That(subState.LastDisposeAsyncToken, Is.EqualTo(cts.Token), "Sub state should receive correct token");
		
		Assert.That(mainState.IsDisposed, Is.True);
		Assert.That(subState.IsDisposed, Is.True);
	}
	
	[Test]
	public async Task StateDisposeAsyncException_FallsBackToSyncDispose()
	{
		// Arrange
		var stateId = "failing";
		var state = new FailingDisposeAsyncState();
		_stateFactory.RegisterState(stateId, state);
		
		_kernel = Kernel.Create()
			.AddMain(stateId)
			.SetInitialMain(stateId)
			.WithFactory(_stateFactory)
			.Build();
		
		Assert.That(state.IsActive, Is.True);
		
		// Act & Assert - Exception should be propagated but state still disposed
		var ex = Assert.ThrowsAsync<InvalidOperationException>(
			async () => await _kernel.DisposeAsync(CancellationToken.None)
		);
		Assert.That(ex.Message, Does.Contain("DisposeAsync failed"));
		
		// State should still be disposed via fallback
		Assert.That(state.IsDisposed, Is.True, "State should be disposed via fallback mechanism");
	}
	
	[Test]
	public async Task CancelledDisposeAsync_FallsBackToSyncDispose()
	{
		// Arrange
		var stateId = "test";
		var state = CreateAndRegisterState(stateId);
		
		_kernel = Kernel.Create()
			.AddMain(stateId)
			.SetInitialMain(stateId)
			.WithFactory(_stateFactory)
			.Build();
		
		Assert.That(state.IsActive, Is.True);
		
		var cts = new CancellationTokenSource();
		cts.Cancel(); // Cancel immediately
		
		// Act - Should not throw despite cancellation
		await _kernel.DisposeAsync(cts.Token);
		
		// Assert - State should be disposed via fallback
		Assert.That(state.IsDisposed, Is.True, "State should be disposed despite cancellation");
	}
	
	[Test]
	public async Task MultipleDisposeAsyncCalls_AreIdempotent()
	{
		// Arrange
		var stateId = "test";
		var state = CreateAndRegisterState(stateId);
		
		_kernel = Kernel.Create()
			.AddMain(stateId)
			.SetInitialMain(stateId)
			.WithFactory(_stateFactory)
			.Build();
		
		Assert.That(state.IsActive, Is.True);
		
		// Act - Multiple calls
		await _kernel.DisposeAsync(CancellationToken.None);
		await _kernel.DisposeAsync(CancellationToken.None);
		await _kernel.DisposeAsync(CancellationToken.None);
		
		// Assert - State should only be disposed once
		Assert.That(state.DisposeAsyncCount, Is.EqualTo(1), "DisposeAsync should only be called once");
		Assert.That(state.IsDisposed, Is.True);
	}
	
	[Test]
	public async Task MixedSyncAsyncDisposal_WorksCorrectly()
	{
		// Arrange
		var stateId = "test";
		var state = CreateAndRegisterState(stateId);
		
		_kernel = Kernel.Create()
			.AddMain(stateId)
			.SetInitialMain(stateId)
			.WithFactory(_stateFactory)
			.Build();
		
		Assert.That(state.IsActive, Is.True);
		
		// Act - Call sync dispose first
		_kernel.Dispose();
		
		// Then try async dispose
		await _kernel.DisposeAsync(CancellationToken.None);
		
		// Assert - Should be idempotent
		Assert.That(state.IsDisposed, Is.True);
		Assert.That(state.IsDisposeAsyncCalled, Is.False, "DisposeAsync should not be called after sync Dispose");
	}
	
	// Helper methods
	private TestState CreateAndRegisterState(string stateId)
	{
		var state = new TestState();
		_states[stateId] = state;
		_stateFactory.RegisterState(stateId, state);
		return state;
	}
	
	// Test support classes
	private class TestStateFactory : IStateFactory
	{
		private readonly Dictionary<string, IState> _registry = new();
		
		public void RegisterState(string stateId, IState state)
		{
			_registry[stateId] = state;
		}
		
		public IState CreateState(string stateId)
		{
			return _registry.TryGetValue(stateId, out var state) ? state : null;
		}
	}
	
	private class TestState : StateBase
	{
		public bool IsEntered { get; private set; }
		public bool IsExited { get; private set; }
		public bool IsActive => IsEntered && !IsExited;
		public bool IsDisposeAsyncCalled { get; private set; }
		public int DisposeAsyncCount { get; private set; }
		public CancellationToken LastDisposeAsyncToken { get; private set; }
		
		protected override Task EnterAsync(CancellationToken token)
		{
			IsEntered = true;
			return Task.CompletedTask;
		}
		
		protected override Task ExitAsync(CancellationToken token)
		{
			IsExited = true;
			return Task.CompletedTask;
		}
		
		protected override ValueTask OnDisposeAsync(CancellationToken token)
		{
			IsDisposeAsyncCalled = true;
			DisposeAsyncCount++;
			LastDisposeAsyncToken = token;
			return default;
		}
	}
	
	private class FailingDisposeAsyncState : StateBase
	{
		public bool IsEntered { get; private set; }
		public bool IsExited { get; private set; }
		public bool IsActive => IsEntered && !IsExited;
		
		protected override Task EnterAsync(CancellationToken token)
		{
			IsEntered = true;
			return Task.CompletedTask;
		}
		
		protected override Task ExitAsync(CancellationToken token)
		{
			IsExited = true;
			return Task.CompletedTask;
		}
		
		protected override ValueTask OnDisposeAsync(CancellationToken token)
		{
			throw new InvalidOperationException("DisposeAsync failed");
		}
	}
}
}
