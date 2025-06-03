using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Runtime.Core.Architecture.GameGlobalState.Contract;

namespace Runtime.Core.Architecture.GameGlobalState.Core
{
/// <summary>
/// A generic, lock-serialized finite-state machine over an identifier domain.
/// 
/// Design notes:
/// - Transitions are serialized via a semaphore to prevent concurrent state mutations.
/// - <see cref="IState.EnterAsync"/> is invoked *outside* the critical section to avoid
///   re-entrancy deadlocks if a state's Enter triggers nested transitions.
/// - Exit is awaited before disposing the previous state; Enter is awaited before the FSM
///   becomes fully ready to receive ticks.
/// - The FSM is idempotent: switching to the currently active ID is a no-op.
/// </summary>
internal class Fsm<TId> : IDisposable
{
	/// <summary>
	/// Gets the ID of the currently active state. Undefined when <see cref="HasCurrent"/> is false.
	/// </summary>
	public TId CurrentId { get; private set; }

	/// <summary>
	/// Indicates whether the FSM has an active state.
	/// </summary>
	public bool HasCurrent { get; private set; }

	private readonly HashSet<TId> _validIds;
	private readonly Func<TId, IState> _createState;
	private readonly SemaphoreSlim _gate = new(1, 1);

	private IState _current;
	private bool _disposed;

	/// <summary>
	/// Creates a new FSM instance.
	/// </summary>
	/// <param name="validIds">The set of IDs that are valid for this FSM.</param>
	/// <param name="createState">Factory for creating a concrete <see cref="IState"/> by ID.</param>
	/// <exception cref="ArgumentNullException">If <paramref name="validIds"/> or <paramref name="createState"/> is null.</exception>
	public Fsm(HashSet<TId> validIds, Func<TId, IState> createState)
	{
		_validIds = validIds ?? throw new ArgumentNullException(nameof(validIds));
		_createState = createState ?? throw new ArgumentNullException(nameof(createState));
	}
	
	/// <summary>
	/// Disposes the FSM and its current state (if any). After disposal, the FSM cannot be used.
	/// </summary>
	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		_disposed = true;

		// Best-effort synchronous shutdown of the current state
		_current?.Dispose();
		_current = null;
		HasCurrent = false;

		_gate.Dispose();
	}

	/// <summary>
	/// Asynchronously disposes the FSM and its current state (if any). After disposal, the FSM cannot be used.
	/// </summary>
	public async ValueTask DisposeAsync(CancellationToken ct)
	{
		if (_disposed)
		{
			return;
		}

		_disposed = true;

		// Async shutdown of the current state with cancellation token
		if (_current != null)
		{
			try
			{
				await _current.DisposeAsync(ct);
			}
			catch (OperationCanceledException)
			{
				// Fallback to sync disposal if async was cancelled
				_current.Dispose();
			}
			catch
			{
				// Fallback to sync disposal if async failed  
				_current.Dispose();
				throw;
			}

			_current = null;
			HasCurrent = false;
		}

		_gate.Dispose();
	}

	/// <summary>
	/// Changes the active state to <paramref name="id"/>.
	/// The call is idempotent: if <paramref name="id"/> is already active, the method returns immediately.
	/// </summary>
	/// <param name="id">Target state identifier.</param>
	/// <param name="ct">Cancellation token for the transition.</param>
	/// <exception cref="ObjectDisposedException">If the FSM has been disposed.</exception>
	/// <exception cref="InvalidOperationException">If <paramref name="id"/> is not registered or the factory returns null.</exception>
	public virtual async Task ChangeStateAsync(TId id, CancellationToken ct, bool continueOnCapturedContext = true)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(nameof(Fsm<TId>));
		}

		IState nextInstance = null;

		// Serialize transitions
		await _gate.WaitAsync(ct).ConfigureAwait(continueOnCapturedContext);
		try
		{
			// Idempotent quick exit
			if (HasCurrent && EqualityComparer<TId>.Default.Equals(CurrentId, id))
			{
				return;
			}

			// Validate target
			if (!_validIds.Contains(id))
			{
				throw new InvalidOperationException($"State '{id}' is not registered in this FSM.");
			}

			// Exit and dispose old state
			if (_current != null)
			{
				await _current.ExitAsync(ct).ConfigureAwait(continueOnCapturedContext);
				_current.Dispose();
				_current = null;
				HasCurrent = false;
			}

			// Create new state instance under the lock,
			// but call EnterAsync *outside* to avoid re-entrancy deadlocks.
			nextInstance = _createState(id)
							?? throw new InvalidOperationException($"Factory returned null for '{id}'.");

			_current = nextInstance;
			CurrentId = id;
			HasCurrent = true;
		}
		finally
		{
			_gate.Release();
		}

		// Enter outside the lock for deadlock safety
		try
		{
			await nextInstance.EnterAsync(ct).ConfigureAwait(continueOnCapturedContext);
		}
		catch
		{
			// If enter fails, dispose the instance and reset FSM state
			nextInstance.Dispose();
			
			// Reset FSM state since enter failed
			await _gate.WaitAsync(ct).ConfigureAwait(continueOnCapturedContext);
			try
			{
				_current = null;
				HasCurrent = false;
			}
			finally
			{
				_gate.Release();
			}
			
			throw;
		}
	}

	/// <summary>
	/// Per-frame update for the active state. No-ops if no state is active.
	/// </summary>
	public virtual void Tick(float deltaTime)
	{
		_current?.Tick(deltaTime);
	}

	/// <summary>
	/// Fixed-step update for the active state. No-ops if no state is active.
	/// </summary>
	public virtual void FixedTick(float deltaTime)
	{
		_current?.FixedTick(deltaTime);
	}

	/// <summary>
	/// Late update for the active state. No-ops if no state is active.
	/// </summary>
	public virtual void LateTick(float deltaTime)
	{
		_current?.LateTick(deltaTime);
	}
}
}