using System;
using System.Collections.Generic;
using System.Threading;
using Runtime.Core.Architecture.GameGlobalState.Contract;

namespace Runtime.Core.Architecture.GameGlobalState.Core
{
/// <summary>
/// Builder for composing levels and states into a running kernel.
/// </summary>
public interface IKernelBuilder
{
	public IKernelBuilder WithFactory(IStateFactory factory);
	public IKernelBuilder AddMain(string mainId);
	public IKernelBuilder AddSub(string ownerMainId, string subId);
	public IKernelBuilder SetInitialMain(string mainId); 
	public IKernelBuilder SetInitialSub(string ownerMainId, string subId); 
	public IKernel Build();
}

/// <summary>
/// Internal kernel builder implementation.
/// </summary>
internal sealed class KernelBuilder : IKernelBuilder
{
	private IStateFactory _factory;

	private readonly HashSet<string> _mainIds = new(StringComparer.Ordinal);

	private readonly Dictionary<string, HashSet<string>> _subsByMain =
		new(StringComparer.Ordinal);

	private readonly Dictionary<string, string> _subOwner =
		new(StringComparer.Ordinal);

	private string _initialMain;
	private bool _hasInitialMain;
	private readonly Dictionary<string, string> _initialSub = new(StringComparer.Ordinal);

	public IKernelBuilder WithFactory(IStateFactory factory)
	{
		_factory = factory;
		return this;
	}

	public IKernelBuilder AddMain(string mainId)
	{
		if (mainId == null)
		{
			throw new ArgumentNullException(nameof(mainId));
		}

		if (!_mainIds.Add(mainId))
		{
			throw new InvalidOperationException($"Main '{mainId}' already added.");
		}

		_subsByMain.TryAdd(mainId, new HashSet<string>(StringComparer.Ordinal));
		return this;
	}

	public IKernelBuilder AddSub(string ownerMainId, string subId)
	{
		if (ownerMainId == null)
		{
			throw new ArgumentNullException(nameof(ownerMainId));
		}

		if (subId == null)
		{
			throw new ArgumentNullException(nameof(subId));
		}

		if (!_mainIds.Contains(ownerMainId))
		{
			throw new InvalidOperationException($"Owner main '{ownerMainId}' not registered. Add main first.");
		}

		if (!_subsByMain.TryGetValue(ownerMainId, out var set))
		{
			set = new HashSet<string>(StringComparer.Ordinal);
			_subsByMain[ownerMainId] = set;
		}

		if (!set.Add(subId))
		{
			throw new InvalidOperationException($"Sub '{subId}' already added under '{ownerMainId}'.");
		}

		if (_subOwner.TryGetValue(subId, out var value))
		{
			throw new InvalidOperationException(
				$"Sub '{subId}' already bound to '{value}'. Sub IDs must be unique across mains.");
		}

		_subOwner[subId] = ownerMainId;
		return this;
	}

	public IKernelBuilder SetInitialMain(string mainId)
	{
		_initialMain = mainId ?? throw new ArgumentNullException(nameof(mainId));
		_hasInitialMain = true;
		return this;
	}

	public IKernelBuilder SetInitialSub(string ownerMainId, string subId)
	{
		if (ownerMainId == null || subId == null)
		{
			throw new ArgumentNullException();
		}

		_initialSub[ownerMainId] = subId;
		return this;
	}

	public IKernel Build()
	{
		if (_factory == null)
		{
			throw new InvalidOperationException("Factory not set.");
		}

		if (_mainIds.Count == 0)
		{
			throw new InvalidOperationException("No mains added.");
		}

		if (_hasInitialMain && !_mainIds.Contains(_initialMain))
		{
			throw new InvalidOperationException($"Initial main '{_initialMain}' not registered.");
		}

		// Build main FSM
		var mainFsm = new Fsm<string>(
			validIds: new HashSet<string>(_mainIds, StringComparer.Ordinal),
			createState: id => _factory.CreateState(id));

		// Build sub FSMs per main
		var subFsms = new Dictionary<string, Fsm<string>>(StringComparer.Ordinal);
		foreach (var main in _mainIds)
		{
			var set = _subsByMain.TryGetValue(main, out var subs)
				? subs
				: new HashSet<string>(StringComparer.Ordinal);

			var fsm = new Fsm<string>(
				validIds: set,
				createState: subId => _factory.CreateState(subId));

			subFsms[main] = fsm;
		}

		var flow = new FlowPort(new HashSet<string>(_mainIds, StringComparer.Ordinal), mainFsm, subFsms,
			new Dictionary<string, string>(_subOwner, StringComparer.Ordinal));
		var ticks = new TickPort(mainFsm, subFsms);

		var kernel = new KernelImpl(flow, ticks);

		// Activate initial states if specified
		if (!_hasInitialMain)
		{
			return kernel;
		}

		// Activate initial main state
		flow.RequestAsync(_initialMain, CancellationToken.None).Wait();
			
		// Activate initial sub state for this main (if any)
		if (_initialSub.TryGetValue(_initialMain, out var initialSub))
		{
			flow.RequestAsync(initialSub, CancellationToken.None).Wait();
		}

		return kernel;
	}
}
}