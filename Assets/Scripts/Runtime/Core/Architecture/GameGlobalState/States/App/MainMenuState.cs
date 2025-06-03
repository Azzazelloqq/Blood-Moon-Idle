using System.Threading;
using System.Threading.Tasks;
using LightDI.Runtime;
using Runtime.Core.Architecture.CompositionRoot.Facade;
using Runtime.Core.Architecture.CompositionRoot.Gameplay;
using Runtime.Core.Architecture.GameGlobalState.Contract;
using Runtime.Core.Architecture.GameGlobalState.Facade;

namespace Runtime.Core.Architecture.GameGlobalState.States.App
{
/// <summary>
/// Main menu state for the application.
/// </summary>
public sealed class MainMenuState : StateBase
{
	private readonly IGameStateFacade _gameStateFacade;
	private readonly CompositionRootProvider _compositionRootProvider;

	internal MainMenuState(
		[Inject] IGameStateFacade gameStateFacade,
		[Inject] CompositionRootProvider compositionRootProvider)
	{
		_gameStateFacade = gameStateFacade;
		_compositionRootProvider = compositionRootProvider;
	}

	protected override async Task EnterAsync(CancellationToken token)
	{
		await _gameStateFacade.GoToCityAsync(token);
	}
}
}