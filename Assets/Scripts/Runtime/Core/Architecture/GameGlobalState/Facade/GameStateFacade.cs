// File: Runtime/Core/Architecture/GameGlobalState/Facade/GameFlow.cs

using System;
using System.Threading;
using System.Threading.Tasks;
using Runtime.Core.Architecture.GameGlobalState.Contract;

namespace Runtime.Core.Architecture.GameGlobalState.Facade
{
/// <summary>
/// Game-specific façade: semantic commands mapped to (level, state) identifiers.
/// </summary>
internal sealed class GameStateFacade : IGameStateFacade
{
	public event Action<string> StateChanged;
	
	private readonly IFlowPort _flow;
	private readonly string _mainMenu;
	private readonly string _gameplay;
	private readonly string _shutdown;
	private readonly string _city;
	private readonly string _crypt;

	public GameStateFacade(
		IFlowPort flow,
		string mainMenu,
		string gameplay,
		string shutdown,
		string city,
		string crypt)
	{
		_flow = flow;
		_mainMenu = mainMenu;
		_gameplay = gameplay;
		_shutdown = shutdown;
		_city = city;
		_crypt = crypt;

		_flow.StateChanged += OnStateChanged;
	}

	public void Dispose()
	{
		_flow.StateChanged -= OnStateChanged;
	}

	public Task GoToMainMenuAsync(CancellationToken ct)
	{
		return _flow.RequestAsync(_mainMenu, ct);
	}

	public Task GoToCityAsync(CancellationToken ct)
	{
		return _flow.RequestAsync(_city, ct);
	}

	public Task GoToCryptAsync(CancellationToken ct)
	{
		return _flow.RequestAsync(_crypt, ct);
	}

	public Task ShutdownAsync(CancellationToken ct)
	{
		return _flow.RequestAsync(_shutdown, ct);
	}

	private void OnStateChanged(string stateId)
	{
		StateChanged?.Invoke(stateId);
	}
}
}