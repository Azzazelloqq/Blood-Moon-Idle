using System.Threading;
using System.Threading.Tasks;
using LightDI.Runtime;
using Runtime.Core.Architecture.CompositionRoot.Base;
using Runtime.Core.Architecture.CompositionRoot.Facade;
using Runtime.Core.Architecture.CompositionRoot.Gameplay;
using Runtime.Core.Architecture.CompositionRoot.Gameplay.City;
using Runtime.Core.Architecture.CompositionRoot.Gameplay.Crypt;
using Runtime.Core.Architecture.CompositionRoot.Main;
using Runtime.Core.Architecture.GameGlobalState.Contract;
using Runtime.Core.Architecture.GameGlobalState.Facade;

namespace Runtime.Core.Architecture.GameGlobalState.States.App
{
/// <summary>
/// Initial bootstrap state for application startup.
/// </summary>
public sealed class BootstrapState : StateBase
{
	private readonly CompositionRootProvider _compositionRootProvider;
	private readonly IGameStateFacade _gameStateFacade;
	private ICompositionRoot _gameCompositionRoot;

	internal BootstrapState(
		[Inject] CompositionRootProvider compositionRootProvider,
		[Inject] IGameStateFacade gameStateFacade)
	{
		_compositionRootProvider = compositionRootProvider;
		_gameStateFacade = gameStateFacade;
	}

	protected override async Task EnterAsync(CancellationToken token)
	{
		_gameCompositionRoot = await _compositionRootProvider.GetRootAsync<GameCompositionRoot>(token);

		// Enable the composition root (it was initialized enabled by default for non-cacheable roots)
		if (_gameCompositionRoot is ICacheable cacheable)
		{
			await cacheable.EnableAsync(token);
		}

		await PrecacheScenes(token);

		await _gameStateFacade.GoToMainMenuAsync(token);
	}

	protected override async ValueTask OnDisposeAsync(CancellationToken token)
	{
		await base.OnDisposeAsync(token);

		await _compositionRootProvider.ReleaseAsync(_gameCompositionRoot, token);
	}

	protected override void OnDispose()
	{
		base.OnDispose();

		_compositionRootProvider.Release(_gameCompositionRoot);
	}

	private async Task PrecacheScenes(CancellationToken token)
	{
		await _compositionRootProvider.PreloadAsync<GameplayCompositionRoot>(token);

		Task precacheCityTask = null;
		if (!_compositionRootProvider.IsCached<CityComposition>())
		{
			//precacheCityTask = _compositionRootProvider.PrecacheAsync<CityComposition>(token);
		}

		Task precacheCryptTask = null;
		if (!_compositionRootProvider.IsCached<CryptComposition>())
		{
			precacheCryptTask = _compositionRootProvider.PrecacheAsync<CryptComposition>(token);
		}

		if (precacheCityTask != null && precacheCryptTask != null)
		{
			await Task.WhenAll(precacheCityTask, precacheCryptTask);

			return;
		}

		if (precacheCryptTask != null)
		{
			await precacheCryptTask;
		}

		if (precacheCryptTask != null)
		{
			await precacheCryptTask;
		}
	}
}
}