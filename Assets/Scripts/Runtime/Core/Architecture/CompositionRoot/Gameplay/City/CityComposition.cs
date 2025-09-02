using System;
using System.Threading;
using System.Threading.Tasks;
using Azzazelloqq.Config;
using Azzazelloqq.DetectionService.Source;
using Disposable;
using InGameLogger;
using LightDI.Runtime;
using ResourceLoader;
using Runtime.Core.Architecture.CompositionRoot.Base;
using Runtime.Core.Infrastructure.Config.Local.DayNightConfig;
using Runtime.Core.Infrastructure.Services.CameraService;
using Runtime.Core.Infrastructure.Services.PeopleSpawnService;
using Runtime.Gameplay.Characters.Player;
using Runtime.Gameplay.Characters.Player.Base;
using Runtime.Gameplay.DayNightCycle;
using Runtime.Gameplay.DayNightCycle.Base;
using Scripts.Generated.Addressables;
using TickHandler;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Runtime.Core.Architecture.CompositionRoot.Gameplay.City
{
public class CityComposition : ICompositionRoot, IPreloadable, ICacheable
{
	private readonly IConfig _config;
	private readonly IResourceLoader _resourceLoader;
	private readonly PlayerFactory _playerFactory;
	private readonly IDetectionService _detectionService;
	private readonly ICameraService _cameraService;
	private readonly IInGameLogger _logger;
	private readonly ICompositeDisposable _compositeDisposable = new CompositeDisposable();
	private readonly IСitizenSpawnService _сitizenSpawnService;

	private DayNightCyclePresenterBase _dayNightCyclePresenter;
	private PlayerPresenterBase _player;
	private GameObject _scene;
	private CityRootContext _sceneContext;

	public CityComposition(
		[Inject] ITickHandler tickHandler,
		[Inject] IConfig config,
		[Inject] IResourceLoader resourceLoader,
		[Inject] PlayerFactory playerFactory,
		[Inject] IDetectionService detectionService,
		[Inject] ICameraService cameraService,
		[Inject] IInGameLogger logger)
	{
		_config = config;
		_resourceLoader = resourceLoader;
		_playerFactory = playerFactory;
		_detectionService = detectionService;
		_cameraService = cameraService;
		_logger = logger;

		_сitizenSpawnService = СitizenSpawnServiceFactory.CreateСitizenSpawnService();
	}

	public void Initialize()
	{
		throw new NotImplementedException();
	}

	public async ValueTask InitializeAsync(CancellationToken token)
	{
		if (_sceneContext == null)
		{
			//todo: to think about inject context
			_sceneContext = Object.FindFirstObjectByType<CityRootContext>();
		}

		_scene = await CreateSceneIfNotCreated(token);
		_sceneContext.SceneParent.gameObject.SetActive(true);

		var dayNightViewParent = _sceneContext.DayNightViewParent;

		var playerParent = _sceneContext.PlayerParent;
		await InitializeDayNightCycleAsync(dayNightViewParent, token);

		_player = await _playerFactory.GetOrCreatePlayerAsync(playerParent, token);

		_player.InitializePosition(_sceneContext.PlayerParent.position);
		_dayNightCyclePresenter.StartCycle();

		var spawnParent = _sceneContext.CitizensSpawnParent;
		_сitizenSpawnService.Initialize(new SpawnSettings(
			spawnParent,
			3,
			40,
			5,
			1000,
			20));

		_сitizenSpawnService.Enable();
	}

	public async ValueTask PreloadAsync(CancellationToken token)
	{
		if (_sceneContext == null)
		{
			//todo: to think about inject context
			_sceneContext = Object.FindFirstObjectByType<CityRootContext>();
		}

		_sceneContext.SceneParent.gameObject.SetActive(false);
		var spawnParent = _sceneContext.CitizensSpawnParent;
		_сitizenSpawnService.Initialize(new SpawnSettings(
			spawnParent,
			3,
			40,
			5,
			1000,
			20));

		_scene = await CreateSceneIfNotCreated(token);
	}

	public void Disable()
	{
		_player.Disable();
		_scene.SetActive(false);
		_dayNightCyclePresenter.Disable();
		_сitizenSpawnService.Disable();
	}

	public ValueTask DisableAsync(CancellationToken token)
	{
		_player.Disable();
		_scene.SetActive(false);
		_dayNightCyclePresenter.Disable();
		_сitizenSpawnService.Disable();

		return default;
	}

	public void Enable()
	{
		_player.InitializePosition(_sceneContext.PlayerParent.position);
		_player.Enable();
		_scene.SetActive(true);
		_dayNightCyclePresenter.Enable();
		_сitizenSpawnService.Enable();
	}

	public ValueTask EnableAsync(CancellationToken token)
	{
		_player.InitializePosition(_sceneContext.PlayerParent.position);
		_player.Enable();
		_scene.SetActive(true);
		_dayNightCyclePresenter.Enable();
		_сitizenSpawnService.Enable();

		return default;
	}

	public void Dispose()
	{
		_сitizenSpawnService?.Dispose();
		_compositeDisposable.Dispose();
	}

	private async ValueTask<GameObject> CreateSceneIfNotCreated(CancellationToken token)
	{
		var isCreated = _scene != null;

		if (isCreated)
		{
			return _scene;
		}

		return await CreateScene(token);
	}

	private async Task<GameObject> CreateScene(CancellationToken token)
	{
		var sceneParent = _sceneContext.SceneParent;
		var citySceneResourceId = ResourceIdsContainer.Scenes.CityScene;
		var cityScenePrefab = await _resourceLoader.LoadResourceAsync<GameObject>(citySceneResourceId, token);

		var scene = Object.Instantiate(cityScenePrefab, sceneParent);

		return scene;
	}

	private async ValueTask InitializeDayNightCycleAsync(Transform viewParent, CancellationToken token)
	{
		var dayNightCycleConfigPage = _config.GetConfigPage<DayNightCycleConfigPage>();
		var lightingPeriods = dayNightCycleConfigPage.LightingPeriods;

		var dayNightCycleModel = new DayNightCycleModel(lightingPeriods);

		var viewResourceId = ResourceIdsContainer.CommonGameplay.DayNightCycleView;
		var viewBase =
			await _resourceLoader.LoadAndCreateAsync<DayNightCycleViewBase, Transform>(viewResourceId,
				viewParent, token);

		_dayNightCyclePresenter = DayNightCyclePresenterFactory.CreateDayNightCyclePresenter(viewBase, dayNightCycleModel);

		await _dayNightCyclePresenter.InitializeAsync(token);

		_compositeDisposable.AddDisposable(_dayNightCyclePresenter);
	}
}
}