using System;
using System.Threading;
using System.Threading.Tasks;

namespace Runtime.Core.Architecture.GameGlobalState.Facade
{
/// <summary>
/// Game-specific facade for state transitions.
/// </summary>
public interface IGameStateFacade : IDisposable
{
	/// <summary>
	/// Event that is triggered when the game state changes.
	/// </summary>
	/// <remarks>
	/// The Type parameter indicates the new state type that was transitioned to.
	/// </remarks>
	public event Action<string> StateChanged;
	
	/// <summary>
	/// Transition to the main menu.
	/// </summary>
	/// <param name="token">Cancellation token.</param>
	/// <returns>Task representing the async operation.</returns>
	public Task GoToMainMenuAsync(CancellationToken token);

	/// <summary>
	/// Transition to the city gameplay state.
	/// </summary>
	/// <param name="token">Cancellation token.</param>
	/// <returns>Task representing the async operation.</returns>
	public Task GoToCityAsync(CancellationToken token);

	/// <summary>
	/// Transition to the crypt gameplay state.
	/// </summary>
	/// <param name="token">Cancellation token.</param>
	/// <returns>Task representing the async operation.</returns>
	public Task GoToCryptAsync(CancellationToken token);

	/// <summary>
	/// Shutdown the game.
	/// </summary>
	/// <param name="token">Cancellation token.</param>
	/// <returns>Task representing the async operation.</returns>
	public Task ShutdownAsync(CancellationToken token);
}
}