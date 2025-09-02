using System;
using System.Threading;
using System.Threading.Tasks;
using Azzazelloqq.Config;
using Azzazelloqq.DetectionService.Source;
using LightDI.Runtime;
using ResourceLoader;
using Runtime.Core.Infrastructure.Config.Local.PlayerConfig;
using Runtime.Core.Infrastructure.TransformUtils;
using Runtime.Gameplay.Characters.Player.Base;
using Scripts.Generated.Addressables;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Runtime.Gameplay.Characters.Player
{
public class PlayerFactory : IDisposable
{
	private const int PlayerCurrentLevel = 0;

	private readonly IConfig _config;
	private readonly IResourceLoader _resourceLoader;
	private readonly IDetectionService _detectionService;
	private PlayerPresenterBase _player;

	public PlayerFactory(
		[Inject] IConfig config,
		[Inject] IResourceLoader resourceLoader,
		[Inject] IDetectionService detectionService)
	{
		_config = config;
		_resourceLoader = resourceLoader;
		_detectionService = detectionService;
	}

	public void Dispose()
	{
		_player?.Dispose();
	}

	public async Task<PlayerPresenterBase> GetOrCreatePlayerAsync(Transform parent, CancellationToken token)
	{
		if (_player == null)
		{
			_player = await CreatePlayerPresenterAsync(parent, token);
		}
		else
		{
			_player.UpdateParent(parent);
			_player.Enable();
		}

		return _player;
	}

	public void GetOrCreatePlayer(Transform parent, Action<PlayerPresenterBase> onCreated, CancellationToken token)
	{
		if (_player == null)
		{
			CreatePlayerPresenter(parent, onCreated, token);
		}
		else
		{
			_player.UpdateParent(parent);
			_player.Enable();
		}
	}

	public void InitializePosition(Vector3 position)
	{
		_player.InitializePosition(position);
	}

	public void ReturnPlayer()
	{
		_player?.Disable();
	}

	/// <summary>
	/// Gets current player position. Returns Vector3.zero if no player exists.
	/// </summary>
	/// <returns>Player position or Vector3.zero</returns>
	public Vector3 GetPlayerPosition()
	{
		return _player?.CharacterTransform.Position ?? Vector3.zero;
	}

	/// <summary>
	/// Gets current player transform. Returns null if no player exists.
	/// </summary>
	/// <returns>Player transform or null</returns>
	public ReadOnlyTransform GetPlayerTransform()
	{
		return _player.CharacterTransform;
	}

	private void CreatePlayerPresenter(Transform spawnPoint, Action<PlayerPresenterBase> onCreated, CancellationToken token)
	{
		var moveSpeedByLevel = _config.GetConfigPage<PlayerConfigPage>().MoveSpeedByLevel;
		var playerModel = new PlayerModel(moveSpeedByLevel, PlayerCurrentLevel);
		var playerViewId = ResourceIdsContainer.Characters.Hero;

		_resourceLoader.LoadResource<PlayerView>(playerViewId, (resource) =>
		{
			var view = Object.Instantiate(resource, spawnPoint).GetComponent<PlayerView>();
			_player = PlayerPresenterFactory.CreatePlayerPresenter(view, playerModel);
			_player.Initialize();

			// Register player view with detection service so NPCs can detect it
			_detectionService?.RegisterObject(view);

			onCreated?.Invoke(_player);
		}, token);
	}

	private async Task<PlayerPresenterBase> CreatePlayerPresenterAsync(Transform spawnPoint, CancellationToken token)
	{
		var moveSpeedByLevel = _config.GetConfigPage<PlayerConfigPage>().MoveSpeedByLevel;
		var playerModel = new PlayerModel(moveSpeedByLevel, PlayerCurrentLevel);
		var playerViewId = ResourceIdsContainer.Characters.Hero;

		var playerView = await _resourceLoader.LoadAndCreateAsync<PlayerView, Transform>(playerViewId, spawnPoint, token);

		var player = PlayerPresenterFactory.CreatePlayerPresenter(playerView, playerModel);
		await player.InitializeAsync(token);

		return player;
	}
}
}