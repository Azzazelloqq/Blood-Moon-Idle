using System.Threading;
using System.Threading.Tasks;
using LightDI.Runtime;
using ResourceLoader;
using Runtime.Core.Architecture.CompositionRoot.Base;
using Runtime.Gameplay.Characters.Player;
using Runtime.Gameplay.Characters.Player.Base;
using Scripts.Generated.Addressables;
using UnityEngine;

namespace Runtime.Core.Architecture.CompositionRoot.Gameplay.Crypt
{
public class CryptComposition : ICompositionRoot, ICacheable, IPreloadable
{
	private readonly PlayerFactory _playerFactory;
	private readonly IResourceLoader _resourceLoader;
	private GameObject _scene;
	private PlayerPresenterBase _player;
	private CryptRootContext _context;

	public CryptComposition(
		[Inject] PlayerFactory playerFactory,
		[Inject] IResourceLoader resourceLoader)
	{
		_playerFactory = playerFactory;
		_resourceLoader = resourceLoader;
	}
	
	public void Initialize()
	{
		throw new System.NotImplementedException();
	}

	public async ValueTask InitializeAsync(CancellationToken token)
	{
		if (_context == null)
		{
				//todo: to think about inject context
			_context = Object.FindFirstObjectByType<CryptRootContext>();
		}

		_scene = await CreateSceneIfNotCreated(token);
		
		var playerParent = _context.PlayerParent;
		_player = await _playerFactory.GetOrCreatePlayerAsync(playerParent, token);
		_player.InitializePosition(_context.PlayerParent.position);
		
		_scene.SetActive(false);
		_player.Disable();
	}
	
	public async ValueTask PreloadAsync(CancellationToken token)
	{
		if (_context == null)
		{
				//todo: to think about inject context
			_context = Object.FindFirstObjectByType<CryptRootContext>();
		}

		_scene = await CreateSceneIfNotCreated(token);

		_context.PlayerParent.gameObject.SetActive(false);
	}

	public void Dispose()
	{
	}

	public void Disable()
	{
		_scene.SetActive(false);
		_player.Disable();
	}

	public ValueTask DisableAsync(CancellationToken token)
	{
		_scene.SetActive(false);
		_player.Disable();
		
		return default;
	}

	public void Enable()
	{
		_player.InitializePosition(_context.PlayerParent.position);
		_scene.SetActive(true);
		_player.Enable();
	}

	public ValueTask EnableAsync(CancellationToken token)
	{
		_player.InitializePosition(_context.PlayerParent.position);
		_scene.SetActive(true);
		_player.Enable();
		
		return default;
	}
	
	private async ValueTask<GameObject> CreateSceneIfNotCreated(CancellationToken token)
	{
		var isCreated = _scene != null;

		if (isCreated)
		{
			return _scene;
		}

		var sceneParent = _context.SceneParent;
		var cryptSceneResourceId = ResourceIdsContainer.Scenes.CryptScene;
		var scenePrefab = await _resourceLoader.LoadResourceAsync<GameObject>(cryptSceneResourceId, token);

		var scene = Object.Instantiate(scenePrefab, sceneParent);

		return scene;
	}
}
}