using System;
using System.Threading;
using System.Threading.Tasks;

namespace Runtime.Core.Architecture.GameGlobalState.Contract
{
/// <summary>
/// Port for requesting state transitions by (level, state) identifiers.
/// </summary>
public interface IFlowPort
{
	public event Action<string> StateChanged;
	
	/// <summary>
	/// Single-entry flow API. If <paramref name="id"/> is a main state, switches main.
	/// If it's a sub state, ensures its owner main is active first, then switches the sub.
	/// </summary>
	public Task RequestAsync(string id, CancellationToken token, bool continueOnCapturedContext = true);
}

/// <summary>
/// Port for ticking the active states.
/// </summary>
public interface ITickPort
{
	/// <summary>
	/// Tick the active states.
	/// </summary>
	/// <param name="deltaTime">Time since last tick in seconds.</param>
	void Tick(float deltaTime);

	/// <summary>
	/// Fixed tick the active states.
	/// </summary>
	/// <param name="deltaTime">Fixed time step in seconds.</param>
	void FixedTick(float deltaTime);

	/// <summary>
	/// Late tick the active states.
	/// </summary>
	/// <param name="deltaTime">Time since last tick in seconds.</param>
	void LateTick(float deltaTime);
}
}