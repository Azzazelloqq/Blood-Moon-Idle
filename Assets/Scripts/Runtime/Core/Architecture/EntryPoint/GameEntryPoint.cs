using System;
using System.Threading.Tasks;
using LightDI.Runtime;
using Runtime.Core.Architecture.CompositionRoot.Facade;
using Runtime.Core.Architecture.CompositionRoot.Main;
using Runtime.Core.Architecture.GameGlobalState.Factor;
using Runtime.Core.Architecture.GameGlobalState.Installer;
using TickHandler;
using TickHandler.UnityTickHandler;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Runtime.Core.Architecture.EntryPoint
{
public class GameEntryPoint : MonoBehaviour
{
	[SerializeField]
	private GameEntryPointSceneContext _gameEntryPointSceneContext;
	
	private bool _isDestroyed;
	private bool _isCleaningUp;
	private bool _cleanupCompleted;
	private IDiContainer  _diContainer;
	private ITickHandler _tickHandler;
	private GameGlobalStateInstaller _gameGlobalStateInstaller;

	private async void Start()
	{ try
		{
			_diContainer = DiContainerFactory.CreateContainer();
			
			var gameExitToken = Application.exitCancellationToken;

			SetupCompositionRoot();
			
			SetupTickHandler();

			var stateFactory = new StateFactory();
			_gameGlobalStateInstaller = new GameGlobalStateInstaller();
			await _gameGlobalStateInstaller.InstallAsync(stateFactory, _diContainer, OnStateChanged, gameExitToken);
			
			// Subscribe to application lifecycle events
			Application.wantsToQuit += OnWantsToQuit;
			Application.quitting += OnApplicationQuitting;
			
			DontDestroyOnLoad(gameObject);
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}
	}

	private void SetupCompositionRoot()
	{
		var compositionRootProvider = new CompositionRootProvider();
		_diContainer.RegisterAsSingleton(compositionRootProvider);
	}

	private void OnStateChanged(string stateId)
	{
		Debug.Log($"[State] state changed to {stateId}");
	}

	private void SetupTickHandler()
	{
		var dispatcher = gameObject.AddComponent<UnityDispatcherBehaviour>();
		_tickHandler = new UnityTickHandler(dispatcher);
		_diContainer.RegisterAsSingleton(_tickHandler);
	}

	private void OnDestroy()
	{
		if (_isDestroyed)
		{
			return;
		}
		
		DiContainerProvider.Dispose();
		
		Application.wantsToQuit -= OnWantsToQuit;
		Application.quitting -= OnApplicationQuitting;
		
		// If cleanup wasn't triggered by wantsToQuit, run it now
		if (!_isCleaningUp && !_cleanupCompleted)
		{
			CleanupAsync();
		}
		
		_isDestroyed = true;
	}

	/// <summary>
	/// Called when Unity wants to quit. Returns false to delay quitting until cleanup is complete.
	/// </summary>
	private bool OnWantsToQuit()
	{
		if (_cleanupCompleted)
		{
			Debug.Log("[GameEntryPoint] Cleanup already completed, allowing quit.");
			return true; // Allow quit, cleanup already done
		}

		if (!_isCleaningUp)
		{
			// Start async cleanup and delay quit
			Debug.Log("[GameEntryPoint] Starting async cleanup, delaying quit...");
			_isCleaningUp = true;
			CleanupAsync();
		}
		else
		{
			Debug.Log("[GameEntryPoint] Cleanup in progress, still delaying quit...");
		}

		return false; // Delay quit until cleanup completes
	}

	private async void CleanupAsync()
	{
		if (_cleanupCompleted)
			return;

		try
		{
			Debug.Log("[GameEntryPoint] Starting async cleanup...");

			if (_gameGlobalStateInstaller != null)
				await _gameGlobalStateInstaller.DisposeAsync(Application.exitCancellationToken);
			
			_diContainer?.Dispose();
			DiContainerProvider.Dispose();

			Debug.Log("[GameEntryPoint] Async cleanup completed.");
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}
		finally
		{
			_cleanupCompleted = true;
			
			// If we're in the middle of quitting, allow Unity to continue
			if (_isCleaningUp)
			{
				// Force quit if we were delaying it
				#if UNITY_EDITOR
				if (Application.isPlaying)
				{
					Debug.Log("[GameEntryPoint] Stopping play mode after cleanup completion.");
					EditorApplication.isPlaying = false;
				}
				#else
				Debug.Log("[GameEntryPoint] Quitting application after cleanup completion.");
				Application.Quit();
				#endif
			}
		}
	}

	private void OnApplicationQuitting()
	{
		// Final fallback - immediate cleanup if still not destroyed
		if (!_isDestroyed)
		{
			DestroyImmediate(gameObject);
		}
	}
}
}