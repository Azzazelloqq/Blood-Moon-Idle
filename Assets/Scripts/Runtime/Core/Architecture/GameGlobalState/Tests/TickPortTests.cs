using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Runtime.Core.Architecture.GameGlobalState.Contract;
using Runtime.Core.Architecture.GameGlobalState.Core;

namespace Runtime.Core.Architecture.GameGlobalState.Tests
{
[TestFixture]
public class TickPortTests
{
	private Fsm<string> _mainFsm;
	private Dictionary<string, Fsm<string>> _subFsmsByMain;
	private TickPort _tickPort;
	private TestStateFactory _stateFactory;
	
	[SetUp]
	public void SetUp()
	{
		_stateFactory = new TestStateFactory();
		_mainFsm = new TestFsm("main", _stateFactory);
		_subFsmsByMain = new Dictionary<string, Fsm<string>>();
		_tickPort = new TickPort(_mainFsm, _subFsmsByMain);
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
	public async Task Tick_CallsMainAndActiveSub()
	{
		// Arrange
		var mainFsm = (TestFsm)_mainFsm;
		var subFsm = new TestFsm("sub", _stateFactory);
		_subFsmsByMain["main"] = subFsm;
		
		// Make main active so sub gets ticked
		await mainFsm.ChangeStateAsync("main", CancellationToken.None);
		
		// Act
		_tickPort.Tick(0.16f);
		
		// Assert
		Assert.That(mainFsm.TickCount, Is.EqualTo(1));
		Assert.That(mainFsm.LastTickDelta, Is.EqualTo(0.16f));
		Assert.That(subFsm.TickCount, Is.EqualTo(1));
		Assert.That(subFsm.LastTickDelta, Is.EqualTo(0.16f));
	}
	
	[Test]
	public void Tick_EmptyFsms_DoesNotThrow()
	{
		// Act & Assert
		Assert.DoesNotThrow(() => _tickPort.Tick(0.16f));
		Assert.DoesNotThrow(() => _tickPort.FixedTick(0.02f));
		Assert.DoesNotThrow(() => _tickPort.LateTick(0.16f));
	}
	
	[Test]
	public async Task MultipleTicks_AccumulateCorrectly()
	{
		// Arrange
		var mainFsm = (TestFsm)_mainFsm;
		await mainFsm.ChangeStateAsync("main", CancellationToken.None);
		
		// Act
		_tickPort.Tick(0.16f);
		_tickPort.Tick(0.20f);
		_tickPort.FixedTick(0.02f);
		_tickPort.FixedTick(0.02f);
		_tickPort.FixedTick(0.02f);
		_tickPort.LateTick(0.16f);
		
		// Assert
		Assert.That(mainFsm.TickCount, Is.EqualTo(2));
		Assert.That(mainFsm.FixedTickCount, Is.EqualTo(3));
		Assert.That(mainFsm.LateTickCount, Is.EqualTo(1));
		Assert.That(mainFsm.LastTickDelta, Is.EqualTo(0.20f)); // Last tick value
	}
	
	// Test support classes
	private class TestFsm : Fsm<string>
	{
		public int TickCount { get; private set; }
		public int FixedTickCount { get; private set; }
		public int LateTickCount { get; private set; }
		public float LastTickDelta { get; private set; }
		public float LastFixedTickDelta { get; private set; }
		public float LastLateTickDelta { get; private set; }
		
		public TestFsm(string validId, TestStateFactory factory) : base(new HashSet<string> { validId }, factory.CreateState)
		{
		}
		
		public override void Tick(float deltaTime)
		{
			TickCount++;
			LastTickDelta = deltaTime;
			base.Tick(deltaTime);
		}
		
		public override void FixedTick(float deltaTime)
		{
			FixedTickCount++;
			LastFixedTickDelta = deltaTime;
			base.FixedTick(deltaTime);
		}
		
		public override void LateTick(float deltaTime)
		{
			LateTickCount++;
			LastLateTickDelta = deltaTime;
			base.LateTick(deltaTime);
		}
	}
	
	private class TestState : IState
	{
		public void Dispose() { }
		public Task EnterAsync(CancellationToken ct) => Task.CompletedTask;
		public Task ExitAsync(CancellationToken ct) => Task.CompletedTask;
		public void Tick(float deltaTime) { }
		public void FixedTick(float deltaTime) { }
		public void LateTick(float deltaTime) { }
		public ValueTask DisposeAsync(CancellationToken ct) => default;
	}
	
	private class TestStateFactory
	{
		public IState CreateState(string stateId)
		{
			return new TestState();
		}
	}
}
}