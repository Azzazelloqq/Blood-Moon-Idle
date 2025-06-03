using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Runtime.Core.Architecture.GameGlobalState.Contract;

namespace Runtime.Core.Architecture.GameGlobalState.Core
{
/// <summary>
/// Internal implementation of IFlowPort with serialized transitions.
/// </summary>
internal sealed class FlowPort : IFlowPort, IDisposable
{
	public event Action<string> StateChanged;

	private readonly HashSet<string> _mainIds;
	private readonly Fsm<string> _mainFsm;

	private readonly Dictionary<string, Fsm<string>> _subFsmsByMain;

	private readonly Dictionary<string, string> _subOwner;
	private bool _disposed;

	public FlowPort(
		HashSet<string> mainIds,
		Fsm<string> mainFsm,
		Dictionary<string, Fsm<string>> subFsmsByMain,
		Dictionary<string, string> subOwner)
	{
		_mainIds = mainIds ?? throw new ArgumentNullException(nameof(mainIds));
		_mainFsm = mainFsm ?? throw new ArgumentNullException(nameof(mainFsm));
		_subFsmsByMain = subFsmsByMain ?? throw new ArgumentNullException(nameof(subFsmsByMain));
		_subOwner = subOwner ?? throw new ArgumentNullException(nameof(subOwner));
	}

	public async Task RequestAsync(string id, CancellationToken ct, bool continueOnCapturedContext = true)
	{
		try
		{
			if (id == null)
			{
				throw new ArgumentNullException(nameof(id));
			}

			if (_mainIds.Contains(id))
			{
				await _mainFsm.ChangeStateAsync(id, ct).ConfigureAwait(continueOnCapturedContext);

				StateChanged?.Invoke(id);

				return;
			}

			if (_subOwner.TryGetValue(id, out var ownerMain))
			{
				await _mainFsm.ChangeStateAsync(ownerMain, ct).ConfigureAwait(continueOnCapturedContext);

				if (!_subFsmsByMain.TryGetValue(ownerMain, out var subFsm))
				{
					throw new InvalidOperationException($"Sub-FSM for main '{ownerMain}' not found.");
				}

				await subFsm.ChangeStateAsync(id, ct).ConfigureAwait(continueOnCapturedContext);
				StateChanged?.Invoke(id);

				return;
			}

			throw new InvalidOperationException($"Unknown state id '{id}'. Not registered as main or sub.");
		}
		catch (Exception e)
		{
			throw e;
		}
	}

	/// <summary>
	/// Disposes the flow port and all its FSMs.
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
	/// Asynchronously disposes the flow port and all its FSMs.
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