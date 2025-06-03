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
public class KernelBuilderTests
{
	private IKernelBuilder _builder;
	private TestStateFactory _stateFactory;
	
	[SetUp]
	public void SetUp()
	{
		_builder = Kernel.Create();
		_stateFactory = new TestStateFactory();
	}
	
	[Test]
	public void AddMain_NewMain_AddsSuccessfully()
	{
		// Arrange
		var mainId = "app";
		
		// Act & Assert
		Assert.DoesNotThrow(() => _builder.AddMain(mainId));
	}
	
	[Test]
	public void AddMain_DuplicateMain_ThrowsException()
	{
		// Arrange
		var mainId = "app";
		_builder.AddMain(mainId);
		
		// Act & Assert
		var ex = Assert.Throws<InvalidOperationException>(() => _builder.AddMain(mainId));
		Assert.That(ex.Message, Does.Contain("already added"));
	}
	
	[Test]
	public void AddSub_ExistingMain_AddsSuccessfully()
	{
		// Arrange
		var mainId = "app";
		var subId = "city";
		_builder.AddMain(mainId);
		
		// Act & Assert
		Assert.DoesNotThrow(() => _builder.AddSub(mainId, subId));
	}
	
	[Test]
	public void AddSub_NonExistentMain_ThrowsException()
	{
		// Arrange
		var mainId = "app";
		var subId = "city";
		
		// Act & Assert
		var ex = Assert.Throws<InvalidOperationException>(() => _builder.AddSub(mainId, subId));
		Assert.That(ex.Message, Does.Contain("not registered"));
	}
	
	[Test]
	public void SetInitialMain_ExistingMain_SetsSuccessfully()
	{
		// Arrange
		var mainId = "app";
		_builder.AddMain(mainId);
		
		// Act & Assert
		Assert.DoesNotThrow(() => _builder.SetInitialMain(mainId));
	}
	
	[Test]
	public void SetInitialMain_NonExistentMain_ThrowsException()
	{
		// Arrange
		var existingMainId = "existing";
		var nonExistentMainId = "app";
		var factory = new TestStateFactory();
		
		// Act & Assert
		_builder.AddMain(existingMainId).SetInitialMain(nonExistentMainId).WithFactory(factory);
		var ex = Assert.Throws<InvalidOperationException>(() => _builder.Build());
		Assert.That(ex.Message, Does.Contain("not registered"));
	}
	
	[Test]
	public void SetInitialSub_ExistingMainAndSub_SetsSuccessfully()
	{
		// Arrange
		var mainId = "app";
		var subId = "city";
		_builder.AddMain(mainId);
		_builder.AddSub(mainId, subId);
		
		// Act & Assert
		Assert.DoesNotThrow(() => _builder.SetInitialSub(mainId, subId));
	}
	
	[Test]
	public void WithFactory_ValidFactory_SetsSuccessfully()
	{
		// Act & Assert
		Assert.DoesNotThrow(() => _builder.WithFactory(_stateFactory));
	}
	
	[Test]
	public void Build_WithoutStateFactory_ThrowsException()
	{
		// Arrange
		var mainId = "app";
		_builder.AddMain(mainId)
			.SetInitialMain(mainId);
		
		// Act & Assert
		var ex = Assert.Throws<InvalidOperationException>(() => _builder.Build());
		Assert.That(ex.Message, Does.Contain("Factory not set"));
	}
	
	[Test]
	public void Build_MainWithoutInitialState_CreatesKernelSuccessfully()
	{
		// Arrange
		var mainId = "app";
		_builder.AddMain(mainId)
			.WithFactory(_stateFactory);
		
		// Act
		var kernel = _builder.Build();
		
		// Assert
		Assert.That(kernel, Is.Not.Null);
		Assert.That(kernel.Flow, Is.Not.Null);
		Assert.That(kernel.Ticks, Is.Not.Null);
	}
	
	[Test]
	public void Build_ValidConfiguration_ReturnsKernel()
	{
		// Arrange
		var mainId = "app";
		_builder.AddMain(mainId)
			.SetInitialMain(mainId)
			.WithFactory(_stateFactory);
		
		// Act
		var kernel = _builder.Build();
		
		// Assert
		Assert.That(kernel, Is.Not.Null);
		Assert.That(kernel.Flow, Is.Not.Null);
		Assert.That(kernel.Ticks, Is.Not.Null);
	}
	
	[Test]
	public void Build_MainWithSubStates_CreatesCorrectly()
	{
		// Arrange
		var mainId = "app";
		var sub1 = "city";
		var sub2 = "dungeon";
		
		_builder.AddMain(mainId)
			.AddSub(mainId, sub1)
			.AddSub(mainId, sub2)
			.SetInitialMain(mainId)
			.SetInitialSub(mainId, sub1)
			.WithFactory(_stateFactory);
		
		// Act
		var kernel = _builder.Build();
		
		// Assert
		Assert.That(kernel, Is.Not.Null);
	}
	
	[Test]
	public void FluentInterface_WorksCorrectly()
	{
		// Arrange
		var mainId = "app";
		var sub1 = "city";
		var sub2 = "dungeon";
		
		// Act
		var kernel = _builder
			.AddMain(mainId)
			.AddSub(mainId, sub1)
			.AddSub(mainId, sub2)
			.SetInitialMain(mainId)
			.SetInitialSub(mainId, sub1)
			.WithFactory(_stateFactory)
			.Build();
		
		// Assert
		Assert.That(kernel, Is.Not.Null);
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
			return _states.TryGetValue(stateId, out var state) ? state : new TestState();
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
}
}