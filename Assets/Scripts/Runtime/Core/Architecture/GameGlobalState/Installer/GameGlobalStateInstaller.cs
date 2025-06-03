using System;
using System.Threading;
using System.Threading.Tasks;
using LightDI.Runtime;
using Runtime.Core.Architecture.GameGlobalState.Contract;
using Runtime.Core.Architecture.GameGlobalState.Core;
using Runtime.Core.Architecture.GameGlobalState.Facade;

namespace Runtime.Core.Architecture.GameGlobalState.Installer
{
/// <summary>
/// Composes the state kernel for the game and returns the game-specific flow façade.
/// </summary>
public class GameGlobalStateInstaller : IDisposable
{
	private Action<string> _stateChangedLogEvent;
	private GameStateFacade _gameStateFacade;
	private IKernel _kernel;
	private bool _disposed;

	/// <summary>
	/// Builds the kernel, initializes the first state, and returns an <see cref="IGameStateFacade"/> facade.
	/// </summary>
	/// <param name="stateFactory">
	///     The factory responsible for creating game state instances using the DI container.
	/// </param>
	/// <param name="diContainer"></param>
	/// <param name="stateChangedLog">state changed log event</param>
	/// <param name="token">
	///     A token to cancel the bootstrap process if needed.
	/// </param>
	/// <returns>
	/// A fully initialized <see cref="IGameStateFacade"/> that exposes semantic transition methods.
	/// </returns>
	public async Task InstallAsync(
		IStateFactory stateFactory,
		IDiContainer diContainer,
		Action<string> stateChangedLog,
		CancellationToken token = default)
	{
		_stateChangedLogEvent = stateChangedLog;
		const string appBootstrap = "app/bootstrap";
		const string appMenu = "app/mainmenu";
		const string appGameplay = "app/gameplay";
		const string appShutdown = "app/shutdown";

		const string gpCity = "gameplay/city";
		const string gpCrypt = "gameplay/crypt";

		_kernel = Kernel.Create()
			.WithFactory(stateFactory)
			.AddMain(appBootstrap)
			.AddMain(appMenu)
			.AddMain(appGameplay)
			.AddMain(appShutdown)
			.AddSub(appGameplay, gpCity)
			.AddSub(appGameplay, gpCrypt)
			.SetInitialSub(appGameplay, gpCity)
			.Build();

		_gameStateFacade = new GameStateFacade(_kernel.Flow,
			appMenu,
			appGameplay,
			appShutdown,
			gpCity,
			gpCrypt);

		_gameStateFacade.StateChanged += _stateChangedLogEvent;

		diContainer.RegisterAsSingleton<IGameStateFacade>(_gameStateFacade);

		await _kernel.Flow.RequestAsync(appBootstrap, token);
	}

	/// <summary>
	/// Disposes the installer and its kernel.
	/// </summary>
	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;

		if (_gameStateFacade != null && _stateChangedLogEvent != null)
		{
			_gameStateFacade.StateChanged -= _stateChangedLogEvent;
		}

		_gameStateFacade?.Dispose();
		_kernel?.Dispose();
	}

	/// <summary>
	/// Asynchronously disposes the installer and its kernel.
	/// </summary>
	public async ValueTask DisposeAsync(CancellationToken ct)
	{
		if (_disposed)
			return;

		_disposed = true;

		if (_gameStateFacade != null && _stateChangedLogEvent != null)
		{
			_gameStateFacade.StateChanged -= _stateChangedLogEvent;
		}

		_gameStateFacade?.Dispose();
		
		if (_kernel != null)
			await _kernel.DisposeAsync(ct);
	}
}
}