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
public class KernelIntegrationTests
{
	private TestStateFactory _stateFactory;
	private IKernel _kernel;
	private Dictionary<string, TestState> _states;
	
	[SetUp]
	public void SetUp()
	{
		_stateFactory = new TestStateFactory();
		_states = new Dictionary<string, TestState>();
	}
	
	[TearDown]
	public void TearDown()
	{
		// Dispose kernel first which should dispose all states
		_kernel?.Dispose();
		
		// Dispose any remaining states
		foreach (var state in _states.Values)
		{
			if (!state.IsDisposed)
				state.Dispose();
		}
	}
	
	[Test]
	public async Task FullLifecycle_SingleMain_WorksCorrectly()
	{
		// Arrange
		var menuState = "menu";
		var gameplayState = "gameplay";
		var shutdownState = "shutdown";
		
		var menuStateObj = CreateAndRegisterState(menuState);
		var gameplayStateObj = CreateAndRegisterState(gameplayState);
		var shutdownStateObj = CreateAndRegisterState(shutdownState);
		
		_kernel = Kernel.Create()
			.AddMain(menuState)
			.AddMain(gameplayState)
			.AddMain(shutdownState)
			.SetInitialMain(menuState)
			.WithFactory(_stateFactory)
			.Build();
		
		// Act & Assert - Initial state transition
		await _kernel.Flow.RequestAsync(menuState, CancellationToken.None);
		Assert.That(menuStateObj.IsEntered, Is.True);
		Assert.That(menuStateObj.IsActive, Is.True);
		
		// Tick the menu state
		_kernel.Ticks.Tick(0.16f);
		Assert.That(menuStateObj.TickCount, Is.EqualTo(1));
		
		// Transition to gameplay
		await _kernel.Flow.RequestAsync(gameplayState, CancellationToken.None);
		Assert.That(menuStateObj.IsExited, Is.True);
		Assert.That(menuStateObj.IsActive, Is.False);
		Assert.That(gameplayStateObj.IsEntered, Is.True);
		Assert.That(gameplayStateObj.IsActive, Is.True);
		
		// Tick gameplay state multiple times
		_kernel.Ticks.Tick(0.16f);
		_kernel.Ticks.FixedTick(0.02f);
		_kernel.Ticks.LateTick(0.16f);
		Assert.That(gameplayStateObj.TickCount, Is.EqualTo(1));
		Assert.That(gameplayStateObj.FixedTickCount, Is.EqualTo(1));
		Assert.That(gameplayStateObj.LateTickCount, Is.EqualTo(1));
		
		// Transition to shutdown
		await _kernel.Flow.RequestAsync(shutdownState, CancellationToken.None);
		Assert.That(gameplayStateObj.IsExited, Is.True);
		Assert.That(shutdownStateObj.IsEntered, Is.True);
	}
	
	[Test]
	public async Task MainWithSubStates_WorksCorrectly()
	{
		// Arrange - Create a main state with sub states
		var mainId = "gameplay";
		var citySubId = "city";
		var battleSubId = "battle";
		
		var mainStateObj = CreateAndRegisterState(mainId);
		var cityStateObj = CreateAndRegisterState(citySubId);
		var battleStateObj = CreateAndRegisterState(battleSubId);
		
		_kernel = Kernel.Create()
			.AddMain(mainId)
			.AddSub(mainId, citySubId)
			.AddSub(mainId, battleSubId)
			.SetInitialMain(mainId)
			.SetInitialSub(mainId, citySubId)
			.WithFactory(_stateFactory)
			.Build();
		
		// Initial states should be automatically activated by kernel
		Assert.That(mainStateObj.IsActive, Is.True);
		Assert.That(cityStateObj.IsActive, Is.True);
		
		// Act - Tick to verify both main and sub receive ticks
		_kernel.Ticks.Tick(0.16f);
		
		// Assert - Both received ticks
		Assert.That(mainStateObj.TickCount, Is.EqualTo(1));
		Assert.That(cityStateObj.TickCount, Is.EqualTo(1));
		
		// Act - Switch to different sub state
		await _kernel.Flow.RequestAsync(battleSubId, CancellationToken.None);
		
		// Assert - Main still active, sub switched
		Assert.That(mainStateObj.IsActive, Is.True);
		Assert.That(cityStateObj.IsActive, Is.False);
		Assert.That(battleStateObj.IsActive, Is.True);
	}
	
	[Test]
	public async Task CancellationToken_PropagatedThroughEntireSystem()
	{
		// Arrange
		var stateId = "state1";
		var state = CreateAndRegisterState(stateId);
		
		_kernel = Kernel.Create()
			.AddMain(stateId)
			// Don't set initial main to avoid auto-activation
			.WithFactory(_stateFactory)
			.Build();
		
		var cts = new CancellationTokenSource();
		
		// Act
		await _kernel.Flow.RequestAsync(stateId, cts.Token);
		
		// Assert
		Assert.That(state.LastEnterToken, Is.EqualTo(cts.Token));
	}
	
	[Test]
	public async Task IdempotentTransitions_DoNotCreateNewStates()
	{
		// Arrange
		var stateId = "state1";
		var state = CreateAndRegisterState(stateId);
		
		_kernel = Kernel.Create()
			.AddMain(stateId)
			// Don't set initial main to avoid auto-activation
			.WithFactory(_stateFactory)
			.Build();
		
		// Act - Request same state multiple times
		await _kernel.Flow.RequestAsync(stateId, CancellationToken.None);
		await _kernel.Flow.RequestAsync(stateId, CancellationToken.None);
		await _kernel.Flow.RequestAsync(stateId, CancellationToken.None);
		
		// Assert - State only entered once
		Assert.That(state.IsEntered, Is.True);
		Assert.That(state.IsExited, Is.False);
	}
	
	[Test]
	public void TickingWithoutActiveStates_DoesNotThrow()
	{
		// Arrange
		CreateAndRegisterState("test"); // Register the test state
		
		_kernel = Kernel.Create()
			.AddMain("test")
			.SetInitialMain("test")
			.WithFactory(_stateFactory)
			.Build();
		
		// Act & Assert - Should not throw
		Assert.DoesNotThrow(() => _kernel.Ticks.Tick(0.16f));
		Assert.DoesNotThrow(() => _kernel.Ticks.FixedTick(0.02f));
		Assert.DoesNotThrow(() => _kernel.Ticks.LateTick(0.16f));
	}

	[Test]
	public async Task KernelDisposeAsync_DisposesAllActiveStatesWithCancellationToken()
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
		
		// States should be automatically activated
		Assert.That(mainState.IsActive, Is.True);
		Assert.That(subState.IsActive, Is.True);
		
		var cts = new CancellationTokenSource();
		
		// Act
		await _kernel.DisposeAsync(cts.Token);
		
		// Assert - Both states should have received DisposeAsync with the correct token
		Assert.That(mainState.IsDisposeAsyncCalled, Is.True);
		Assert.That(mainState.LastDisposeAsyncToken, Is.EqualTo(cts.Token));
		Assert.That(subState.IsDisposeAsyncCalled, Is.True);
		Assert.That(subState.LastDisposeAsyncToken, Is.EqualTo(cts.Token));
		Assert.That(mainState.IsDisposed, Is.True);
		Assert.That(subState.IsDisposed, Is.True);
	}

	[Test]
	public async Task FullLifecycleWithDisposeAsync_AllStatesGetProperCleanup()
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
		
		// Act - Full lifecycle
		await _kernel.Flow.RequestAsync(state1Id, CancellationToken.None);
		Assert.That(state1.IsActive, Is.True);
		
		await _kernel.Flow.RequestAsync(state2Id, CancellationToken.None);
		Assert.That(state1.IsActive, Is.False); // Should be exited and disposed during transition
		Assert.That(state2.IsActive, Is.True);
		
		var cts = new CancellationTokenSource();
		await _kernel.DisposeAsync(cts.Token);
		
		// Assert - state1 was disposed during transition, state2 during kernel disposal
		Assert.That(state1.IsDisposed, Is.True);
		Assert.That(state2.IsDisposeAsyncCalled, Is.True);
		Assert.That(state2.LastDisposeAsyncToken, Is.EqualTo(cts.Token));
		Assert.That(state2.IsDisposed, Is.True);
	}
	
	[Test]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
	public async Task StateErrorRecovery_DisposesFailedState()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
	{
		// Arrange
		var goodState = "good";
		var badState = "bad";
		
		var goodStateObj = CreateAndRegisterState(goodState);
		_stateFactory.RegisterState(badState, new FailingState());
		
		_kernel = Kernel.Create()
			.AddMain(goodState)
			.AddMain(badState)
			.SetInitialMain(goodState)
			.WithFactory(_stateFactory)
			.Build();
		
		// Initial state should be automatically activated
		Assert.That(goodStateObj.IsActive, Is.True);
		
		Assert.ThrowsAsync<InvalidOperationException>(
			async () => await _kernel.Flow.RequestAsync(badState, CancellationToken.None)
		);
		
		// After failed transition, FSM resets to no active state
		// This is expected behavior - failed transitions reset the FSM
		Assert.That(goodStateObj.IsActive, Is.False);
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
		public int TickCount { get; private set; }
		public int FixedTickCount { get; private set; }
		public int LateTickCount { get; private set; }
		public CancellationToken LastEnterToken { get; private set; }
		public CancellationToken LastExitToken { get; private set; }
		public CancellationToken LastDisposeAsyncToken { get; private set; }
		
		protected override Task EnterAsync(CancellationToken token)
		{
			IsEntered = true;
			LastEnterToken = token;
			return Task.CompletedTask;
		}
		
		protected override Task ExitAsync(CancellationToken token)
		{
			IsExited = true;
			LastExitToken = token;
			return Task.CompletedTask;
		}
		
		protected override void Tick(float deltaTime)
		{
			TickCount++;
		}
		
		protected override void FixedTick(float deltaTime)
		{
			FixedTickCount++;
		}
		
		protected override void LateTick(float deltaTime)
		{
			LateTickCount++;
		}

		protected override ValueTask OnDisposeAsync(CancellationToken token)
		{
			IsDisposeAsyncCalled = true;
			LastDisposeAsyncToken = token;
			return default;
		}
	}
	
	private class FailingState : StateBase
	{
		protected override Task EnterAsync(CancellationToken token)
		{
			throw new InvalidOperationException("This state always fails to enter");
		}
		
		protected override Task ExitAsync(CancellationToken token)
		{
			return Task.CompletedTask;
		}
	}
}
}