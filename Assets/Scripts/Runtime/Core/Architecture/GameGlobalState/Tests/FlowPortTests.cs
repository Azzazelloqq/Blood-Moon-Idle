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
public class FlowPortTests
{
	private FlowPort _flowPort;
	private TestStateFactory _stateFactory;
	private Fsm<string> _mainFsm;
	private Dictionary<string, Fsm<string>> _subFsmsByMain;
	private HashSet<string> _mainIds;
	private Dictionary<string, string> _subOwner;
	
	[SetUp]
	public void SetUp()
	{
		_stateFactory = new TestStateFactory();
		_mainIds = new HashSet<string>();
		_subFsmsByMain = new Dictionary<string, Fsm<string>>();
		_subOwner = new Dictionary<string, string>();
	}
	
	[TearDown]
	public void TearDown()
	{
		_mainFsm?.Dispose();
		foreach (var fsm in _subFsmsByMain.Values)
		{
			fsm.Dispose();
		}
	}
	
	[Test]
	public async Task RequestAsync_MainState_ChangesState()
	{
		// Arrange
		var mainId = "app";
		_mainIds.Add(mainId);
		_mainFsm = new Fsm<string>(new HashSet<string> { mainId }, _stateFactory.CreateState);
		_flowPort = new FlowPort(_mainIds, _mainFsm, _subFsmsByMain, _subOwner);
		
		_stateFactory.RegisterState(mainId, new TestState());
		
		// Act
		await _flowPort.RequestAsync(mainId, CancellationToken.None);
		
		// Assert
		Assert.That(_mainFsm.CurrentId, Is.EqualTo(mainId));
	}
	
	[Test]
	public void RequestAsync_UnknownState_ThrowsWithMessage()
	{
		// Arrange
		var mainId = "app";
		_mainIds.Add(mainId);
		_mainFsm = new Fsm<string>(new HashSet<string> { mainId }, _stateFactory.CreateState);
		_flowPort = new FlowPort(_mainIds, _mainFsm, _subFsmsByMain, _subOwner);
		
		var unknownId = "unknown";
		
		// Act & Assert
		var ex = Assert.ThrowsAsync<InvalidOperationException>(
			async () => await _flowPort.RequestAsync(unknownId, CancellationToken.None)
		);
		Assert.That(ex.Message, Does.Contain($"Unknown state id '{unknownId}'"));
		Assert.That(ex.Message, Does.Contain("Not registered as main or sub"));
	}
	
	[Test]
	public void RequestAsync_SubStateWithoutOwnerFsm_ThrowsWithMessage()
	{
		// Arrange
		var mainId = "app";
		var subId = "city";
		_mainIds.Add(mainId);
		_subOwner[subId] = mainId;
		_mainFsm = new Fsm<string>(new HashSet<string> { mainId }, _stateFactory.CreateState);
		_flowPort = new FlowPort(_mainIds, _mainFsm, _subFsmsByMain, _subOwner);
		
		_stateFactory.RegisterState(mainId, new TestState());
		_stateFactory.RegisterState(subId, new TestState());
		
		// Act & Assert - SubFsm for main 'app' not found
		var ex = Assert.ThrowsAsync<InvalidOperationException>(
			async () => await _flowPort.RequestAsync(subId, CancellationToken.None)
		);
		Assert.That(ex.Message, Does.Contain($"Sub-FSM for main '{mainId}' not found"));
	}
	
	[Test]
	public void Constructor_NullArguments_ThrowsArgumentNullException()
	{
		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => new FlowPort(null, _mainFsm, _subFsmsByMain, _subOwner));
		Assert.Throws<ArgumentNullException>(() => new FlowPort(_mainIds, null, _subFsmsByMain, _subOwner));
		Assert.Throws<ArgumentNullException>(() => new FlowPort(_mainIds, _mainFsm, null, _subOwner));
		Assert.Throws<ArgumentNullException>(() => new FlowPort(_mainIds, _mainFsm, _subFsmsByMain, null));
	}
	
	[Test]
	public async Task RequestAsync_MainAndSubStates_RoutesToCorrectFsm()
	{
		// Arrange
		var mainId = "app";
		var subId = "city";
		
		_mainIds.Add(mainId);
		_subOwner[subId] = mainId;
		
		_mainFsm = new Fsm<string>(new HashSet<string> { mainId }, _stateFactory.CreateState);
		var subFsm = new Fsm<string>(new HashSet<string> { subId }, _stateFactory.CreateState);
		_subFsmsByMain[mainId] = subFsm;
		
		_flowPort = new FlowPort(_mainIds, _mainFsm, _subFsmsByMain, _subOwner);
		
		_stateFactory.RegisterState(mainId, new TestState());
		_stateFactory.RegisterState(subId, new TestState());
		
		// Act
		await _flowPort.RequestAsync(mainId, CancellationToken.None);
		await _flowPort.RequestAsync(subId, CancellationToken.None);
		
		// Assert
		Assert.That(_mainFsm.CurrentId, Is.EqualTo(mainId));
		Assert.That(subFsm.CurrentId, Is.EqualTo(subId));
	}
	
	[Test]
	public async Task RequestAsync_PropagatesCancellationToken()
	{
		// Arrange
		var mainId = "app";
		_mainIds.Add(mainId);
		
		var testFsm = new TestFsm();
		_flowPort = new FlowPort(_mainIds, testFsm, _subFsmsByMain, _subOwner);
		
		_stateFactory.RegisterState(mainId, new TestState());
		
		var cts = new CancellationTokenSource();
		
		// Act
		await _flowPort.RequestAsync(mainId, cts.Token);
		
		// Assert
		Assert.That(testFsm.LastChangeStateToken, Is.EqualTo(cts.Token));
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
		protected override Task EnterAsync(CancellationToken token)
		{
			return Task.CompletedTask;
		}
		
		protected override Task ExitAsync(CancellationToken token)
		{
			return Task.CompletedTask;
		}
	}
	
	private class TestFsm : Fsm<string>
	{
		public CancellationToken LastChangeStateToken { get; private set; }
		
		public TestFsm() : base(new HashSet<string> { "app" }, id => new TestState())
		{
		}
		
		public override async Task ChangeStateAsync(string id, CancellationToken ct, bool continueOnCapturedContext = true)
		{
			LastChangeStateToken = ct;
			await base.ChangeStateAsync(id, ct, continueOnCapturedContext);
		}
	}
}
}