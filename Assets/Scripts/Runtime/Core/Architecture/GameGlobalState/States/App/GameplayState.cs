using System;
using System.Threading;
using System.Threading.Tasks;
using LightDI.Runtime;
using Runtime.Core.Architecture.CompositionRoot.Base;
using Runtime.Core.Architecture.CompositionRoot.Facade;
using Runtime.Core.Architecture.CompositionRoot.Gameplay;
using Runtime.Core.Architecture.GameGlobalState.Contract;
using Runtime.Core.Architecture.GameGlobalState.Facade;
using Runtime.Dev;
using UnityEngine;

namespace Runtime.Core.Architecture.GameGlobalState.States.App
{
/// <summary>
/// Gameplay container state that manages the gameplay subsystem.
/// </summary>
public sealed class GameplayState : StateBase
{
	private readonly CompositionRootProvider _compositionRootProvider;
	private readonly IGameStateFacade _gameStateFacade;

	private ICompositionRoot _gameplayCompositionRoot;

	#if UNITY_EDITOR
	private bool _isDevSwitchSceneInProgress;
	#endif
	
	public GameplayState(
		[Inject] CompositionRootProvider compositionRootProvider,
		[Inject] IGameStateFacade gameStateFacade)
	{
		_compositionRootProvider = compositionRootProvider;
		_gameStateFacade = gameStateFacade;
	}

	protected override async Task EnterAsync(CancellationToken token)
	{
		#if UNITY_EDITOR
		SwitchScenesDevConsole.SwitchToScene += OnSwitchToScene;
		#endif

		_gameplayCompositionRoot = await _compositionRootProvider.GetRootAsync<GameplayCompositionRoot>(token);

		if (_gameplayCompositionRoot is ICacheable cacheable)
		{
			await cacheable.EnableAsync(token);
		}
	}

	protected override async Task ExitAsync(CancellationToken token)
	{
		#if UNITY_EDITOR
		SwitchScenesDevConsole.SwitchToScene += OnSwitchToScene;
		#endif

		await _compositionRootProvider.ReleaseAsync(_gameplayCompositionRoot, token);
	}

	private async void OnSwitchToScene(string scene)
	{
		try
		{
			#if UNITY_EDITOR
			if (_isDevSwitchSceneInProgress)
			{
				return;
			}
		
			var exitCancellationToken = Application.exitCancellationToken;
			_isDevSwitchSceneInProgress = true;
		
			switch (scene)
			{
				case "City":
					await _gameStateFacade.GoToCityAsync(exitCancellationToken);
					break;
				case "Crypt":
					await _gameStateFacade.GoToCryptAsync(exitCancellationToken);
					break;
			}
		
			_isDevSwitchSceneInProgress = false;
			#endif
		}
		catch (Exception e)
		{
			Debug.LogError(e);
		}
	}
}
}