using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Runtime.Core.Architecture.GameGlobalState.Contract;

namespace Runtime.Core.Architecture.GameGlobalState.Core
{
/// <summary>
/// Routes ticks to:
/// 1) the main FSM, and
/// 2) the sub-FSM that belongs to the currently active main (if any).
///
/// Design notes:
/// - If no main is active, sub ticks are skipped (no-op).
/// - If a main is active but has no registered sub-FSM, sub ticks are skipped (no-op).
/// - This preserves the invariant that only the sub-tree of the active main receives updates.
/// </summary>
internal sealed class TickPort : ITickPort, IDisposable
{
	private readonly Fsm<string> _mainFsm;
	private readonly Dictionary<string, Fsm<string>> _subFsmsByMain;
	private bool _disposed;

	public TickPort(Fsm<string> mainFsm, Dictionary<string, Fsm<string>> subFsmsByMain)
	{
		_mainFsm = mainFsm;
		_subFsmsByMain = subFsmsByMain;
	}

	/// <summary>
	/// Per-frame updates: first main, then the sub-FSM of the current main (if any).
	/// </summary>
	public void Tick(float deltaTime)
	{
		_mainFsm.Tick(deltaTime);

		if (_mainFsm.HasCurrent &&
			_subFsmsByMain.TryGetValue(_mainFsm.CurrentId, out var sub))
		{
			sub.Tick(deltaTime);
		}
	}

	/// <summary>
	/// Fixed-step updates: first main, then the sub-FSM of the current main (if any).
	/// </summary>
	public void FixedTick(float deltaTime)
	{
		_mainFsm.FixedTick(deltaTime);

		if (_mainFsm.HasCurrent &&
			_subFsmsByMain.TryGetValue(_mainFsm.CurrentId, out var sub))
		{
			sub.FixedTick(deltaTime);
		}
	}

	/// <summary>
	/// Late updates: first main, then the sub-FSM of the current main (if any).
	/// </summary>
	public void LateTick(float deltaTime)
	{
		_mainFsm.LateTick(deltaTime);

		if (_mainFsm.HasCurrent &&
			_subFsmsByMain.TryGetValue(_mainFsm.CurrentId, out var sub))
		{
			sub.LateTick(deltaTime);
		}
	}

	/// <summary>
	/// Disposes the tick port and all its FSMs.
	/// </summary>
	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;

		_mainFsm?.Dispose();

		foreach (var subFsm in _subFsmsByMain.Values)
		{
			subFsm?.Dispose();
		}
	}

	/// <summary>
	/// Asynchronously disposes the tick port and all its FSMs.
	/// </summary>
	public async ValueTask DisposeAsync(CancellationToken ct)
	{
		if (_disposed)
			return;

		_disposed = true;

		if (_mainFsm != null)
			await _mainFsm.DisposeAsync(ct);

		foreach (var subFsm in _subFsmsByMain.Values)
		{
			if (subFsm != null)
				await subFsm.DisposeAsync(ct);
		}
	}
}
}