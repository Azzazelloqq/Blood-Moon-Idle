using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azzazelloqq.Config;
using Azzazelloqq.DetectionService.Source;
using Disposable;
using InGameLogger;
using LightDI.Runtime;
using ResourceLoader;
using Runtime.Core.Architecture.CompositionRoot.Base;
using Runtime.Core.Architecture.CompositionRoot.Gameplay.Main.Context;
using Runtime.Core.Architecture.Input;
using Runtime.Core.Architecture.UI;
using Runtime.Core.Infrastructure.Config.Local.DayNightConfig;
using Runtime.Core.Infrastructure.Services.CameraService;
using Runtime.Core.Infrastructure.Services.DayNightCycleService;
using Runtime.Core.Infrastructure.Services.MovementService;

using Runtime.Core.Infrastructure.TransformUtils;
using Runtime.Gameplay.Camera;
using Runtime.Gameplay.Camera.FollowCamera;
using Runtime.Gameplay.Characters.Player;
using Runtime.Gameplay.Characters.Player.Base;
using Runtime.UI.Joystick;
using Scripts.Generated.Addressables;
using TickHandler;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Runtime.Core.Architecture.CompositionRoot.Gameplay
{
public class GameplayCompositionRoot : ICompositionRoot, IPersistentRoot, IPreloadable
{
	private readonly IDiContainer _gameplayDiContainer;
	private readonly IInGameLogger _logger;
	private readonly IResourceLoader _resourceLoader;
	private readonly IUIProvider _uiProvider;
	private readonly ICompositeDisposable _compositeDisposable = new CompositeDisposable();
	private readonly IDayNightCycleService _dayNightCycleService;
	private readonly IDetectionService _detectionService;

	private ICameraService _cameraService;
	private PlayerPresenterBase _player;
	private GameplaySceneContext _sceneContext;
	private IInputService _inputService;


	public GameplayCompositionRoot(
		[Inject] IInGameLogger logger,
		[Inject] ITickHandler tickHandler,
		[Inject] IResourceLoader resourceLoader,
		[Inject] IConfig config,
		[Inject] IUIProvider uiProvider)
	{
		_logger = logger;
		_resourceLoader = resourceLoader;
		_uiProvider = uiProvider;
		_gameplayDiContainer = DiContainerFactory.CreateContainer();
		
		var movementSystem = new GenericMovementService(tickHandler, _logger);
		_gameplayDiContainer.RegisterAsSingleton<IMovementService>(movementSystem);
		
		// Create detection service with grid cell size of 10 units
		_detectionService = new DetectionService(10f);
		_gameplayDiContainer.RegisterAsSingleton(_detectionService);
		
		var dayNightCycleConfigPage = config.GetConfigPage<DayNightCycleConfigPage>();

		var timeOfDayPeriods = dayNightCycleConfigPage.LightingPeriods
			.Select(period => new TimeOfDayPeriod(
				period.DayPhase,
				period.NormalizedTimeStart,
				period.NormalizedTimeEnd))
			.ToList();

		_dayNightCycleService = DayNightCycleServiceFactory.CreateDayNightCycleService(
			dayNightCycleConfigPage.TotalCycleDurationMilliseconds, timeOfDayPeriods);
		
		_gameplayDiContainer.RegisterAsSingleton(_dayNightCycleService);
	}

	public void Initialize()
	{
		throw new NotImplementedException();
	}

	public async ValueTask InitializeAsync(CancellationToken token)
	{
		try
		{
			if (_sceneContext == null)
			{
				//todo: to think about inject context
				_sceneContext = Object.FindFirstObjectByType<GameplaySceneContext>();
			}

			await InitializeGameplayIfNotRegisteredAsync(token);

			StartGameplay(_player.CharacterTransform);
		}
		catch (Exception e)
		{
			_logger.LogException(e);
		}
	}
	
	public async ValueTask PreloadAsync(CancellationToken token)
	{
		try
		{
			if (_sceneContext == null)
			{
				//todo: to think about inject context
				_sceneContext = Object.FindFirstObjectByType<GameplaySceneContext>();
			}
			
			await InitializeGameplayIfNotRegisteredAsync(token);
		}
		catch (Exception e)
		{
			_logger.LogException(e);
		}
	}

	public void Dispose()
	{
		StopGameplay();
		
		_gameplayDiContainer?.Dispose();
		_compositeDisposable.Dispose();
	}

	private async ValueTask InitializeGameplayIfNotRegisteredAsync(CancellationToken token)
	{
		try
		{	
			if (_inputService == null)
			{
				_inputService = await InitializeInputAsync(token);
				_gameplayDiContainer.RegisterAsSingleton(_inputService);
			}
			
			if (_player == null)
			{
				_player = await InitializePlayerAsync(token);
				_gameplayDiContainer.RegisterAsSingleton(_player);
			}

			_cameraService ??= await InitializeCameraAsync(token);
			_gameplayDiContainer.RegisterAsSingleton(_cameraService);
			
		}
		catch (Exception ex)
		{
			_logger.LogException(ex);
		}
	}

	private async Task<IInputService> InitializeInputAsync(CancellationToken token)
	{
		var joystickViewId = ResourceIdsContainer.GameplayUI.VirtualJoystick;
		var joystickView =
			await _resourceLoader.LoadAndCreateAsync<JoystickView, Transform>(joystickViewId, _uiProvider.CanvasParent,
				token);

		var joystickModel = new JoystickModel();
		var joystickViewModel = JoystickViewModelFactory.CreateJoystickViewModel(joystickModel);
		await joystickViewModel.InitializeAsync(token);

		await joystickView.InitializeAsync(joystickViewModel, token);

		_compositeDisposable.AddDisposable(joystickViewModel);
		
		return new InputService(joystickViewModel);
	}

	private async Task<PlayerPresenterBase> InitializePlayerAsync(CancellationToken token)
	{
		var characterSpawnPoint = _sceneContext.CharacterSpawnPoint;
		var playerFacade = PlayerFactoryFactory.CreatePlayerFactory();

		_gameplayDiContainer.RegisterAsSingleton(playerFacade);

		var player = await playerFacade.GetOrCreatePlayerAsync(characterSpawnPoint, token);
		
		return player;
	}

	private async Task<CameraService> InitializeCameraAsync(CancellationToken token)
	{
		var characterSpawnPoint = _sceneContext.CharacterSpawnPoint;
		
		var cameraFactory = CameraFactoryFactory.CreateCameraFactory();
		var cameraViewId = ResourceIdsContainer.CommonGameplay.MainFollowCamera;
		var cameraView =
			await _resourceLoader.LoadAndCreateAsync<FollowCameraView, Transform>(cameraViewId, characterSpawnPoint,
				token);
		var cameraPresenter = cameraFactory.CreateFollowCameraPresenter(cameraView);

		await cameraPresenter.InitializeAsync(token);

		_compositeDisposable.AddDisposable(cameraPresenter);
		var cameraService = new CameraService();

		cameraService.SetGameplayCamera(cameraPresenter);

		return cameraService;
	}

	private void StartGameplay(ReadOnlyTransform playerTransform)
	{
		_player.Enable();

		_dayNightCycleService.StartCycle();

		StartCameraFollowing(playerTransform);
	}

	private void StopGameplay()
	{
		_player.Disable();

		_dayNightCycleService.StopCycle();

		StopCameraFollowing();
	}

	private void StartCameraFollowing(ReadOnlyTransform playerTransform)
	{
		_cameraService.SetFollowTarget(playerTransform);
		_cameraService.StartFollowing();
	}

	private void StopCameraFollowing()
	{
		_cameraService.StopFollowing();
	}


}
}