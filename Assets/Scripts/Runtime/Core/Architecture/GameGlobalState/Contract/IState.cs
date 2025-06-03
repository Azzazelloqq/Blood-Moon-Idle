using System;
using System.Threading;
using System.Threading.Tasks;

namespace Runtime.Core.Architecture.GameGlobalState.Contract
{
/// <summary>
/// Runtime behavior of a state; must be side-effect free before EnterAsync completes.
/// </summary>
public interface IState : IDisposable
{
	/// <summary>
	/// Enter the state. State should initialize and be ready for ticks when this completes.
	/// </summary>
	/// <param name="ct">Cancellation token.</param>
	/// <returns>Task representing the async operation.</returns>
	Task EnterAsync(CancellationToken ct);

	/// <summary>
	/// Exit the state. Must be idempotent.
	/// </summary>
	/// <param name="ct">Cancellation token.</param>
	/// <returns>Task representing the async operation.</returns>
	Task ExitAsync(CancellationToken ct);

	/// <summary>
	/// Update tick for the state.
	/// </summary>
	/// <param name="deltaTime">Time since last tick in seconds.</param>
	void Tick(float deltaTime);

	/// <summary>
	/// Fixed update tick for the state.
	/// </summary>
	/// <param name="deltaTime">Fixed time step in seconds.</param>
	void FixedTick(float deltaTime);

	/// <summary>
	/// Late update tick for the state.
	/// </summary>
	/// <param name="deltaTime">Time since last tick in seconds.</param>
	void LateTick(float deltaTime);

	/// <summary>
	/// Asynchronously disposes the state with a cancellation token.
	/// </summary>
	/// <param name="ct">Cancellation token.</param>
	/// <returns>ValueTask representing the async disposal operation.</returns>
	ValueTask DisposeAsync(CancellationToken ct);
}
}