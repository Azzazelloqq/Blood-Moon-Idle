using System.Threading;
using System.Threading.Tasks;

namespace Runtime.Core.Architecture.GameGlobalState.Contract
{
/// <summary>
/// Convenience base with no-op ticks and disposal hook.
/// </summary>
public abstract class StateBase : IState
{
	public bool IsDisposed { get; private set; }
	
	/// <summary>
	/// Disposes the state.
	/// </summary>
	public void Dispose()
	{
		if (IsDisposed)
		{
			return;
		}
		
		OnDispose();
		
		IsDisposed = true;
	}
	
	/// <summary>
	/// Async Disposes the state.
	/// </summary>
	public async ValueTask DisposeAsync(CancellationToken token)
	{
		if (IsDisposed)
		{
			return;
		}
		
		await OnDisposeAsync(token);
		
		IsDisposed = true;
	}

	/// <summary>
	/// Override to implement disposal logic.
	/// </summary>
	protected virtual void OnDispose() { }

	/// <summary>
	/// Override to implement async disposal logic.
	/// </summary>
	protected virtual ValueTask OnDisposeAsync(CancellationToken token)
	{
		return default;
	}

	/// <summary>
	/// Override to implement update tick logic.
	/// </summary>
	/// <param name="deltaTime">Delta time in seconds.</param>
	protected virtual void Tick(float deltaTime) {}

	/// <summary>
	/// Override to implement fixed update tick logic.
	/// </summary>
	/// <param name="deltaTime">Fixed delta time in seconds.</param>
	protected virtual void FixedTick(float deltaTime) {}

	/// <summary>
	/// Override to implement late update tick logic.
	/// </summary>
	/// <param name="deltaTime">Delta time in seconds.</param>
	protected virtual void LateTick(float deltaTime) {}

	Task IState.EnterAsync(CancellationToken token) => EnterAsync(token);
	Task IState.ExitAsync(CancellationToken token) => ExitAsync(token);
	void IState.Tick(float deltaTime) => Tick(deltaTime);
	void IState.FixedTick(float deltaTime) => FixedTick(deltaTime);
	void IState.LateTick(float deltaTime) => LateTick(deltaTime);
	ValueTask IState.DisposeAsync(CancellationToken ct) => DisposeAsync(ct);

	/// <summary>
	/// Override to implement state enter logic.
	/// </summary>
	protected virtual Task EnterAsync(CancellationToken token) => Task.CompletedTask;

	/// <summary>
	/// Override to implement state exit logic.
	/// </summary>
	protected virtual Task ExitAsync(CancellationToken token) => Task.CompletedTask;
}
}