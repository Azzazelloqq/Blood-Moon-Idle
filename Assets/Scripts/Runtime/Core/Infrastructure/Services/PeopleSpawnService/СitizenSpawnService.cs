using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InGameLogger;
using LightDI.Runtime;
using Runtime.Core.Infrastructure.Services.CameraService;
using Runtime.Gameplay.Characters.Base;
using Runtime.Gameplay.Characters.Person;
using Runtime.Gameplay.Characters.Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Core.Infrastructure.Services.PeopleSpawnService
{
public class СitizenSpawnService : IСitizenSpawnService
{
	public event Action<ICharacter> OnCharacterSpawned;
	public event Action<ICharacter> OnCharacterConsumed;

	private readonly ICameraService _cameraService;
	private readonly IInGameLogger _logger;
	private readonly PlayerFactory _playerFactory;
	private SpawnSettings _spawnSettings;

	private readonly List<ICharacter> _activePeople = new();
	private readonly Queue<ICharacter> _peoplePool = new();
	private readonly Queue<Vector3> _pendingSpawns = new();
	private readonly SemaphoreSlim _spawnSemaphore = new(3, 3);

	private bool _isInitialized;
	private bool _isEnabled;
	private CancellationTokenSource _cancellationTokenSource;
	private List<Task> _reusableTaskList;
	private readonly PersonDetectionContext _detectionContextTest;
	
	public СitizenSpawnService(
		[Inject] ICameraService cameraService,
		[Inject] IInGameLogger logger,
		[Inject] PlayerFactory playerFactory)
	{
		_cameraService = cameraService;
		_logger = logger;
		_playerFactory = playerFactory;

		_detectionContextTest = new PersonDetectionContext(
			8f,     
			360f,   
			1 << 5,     
			0.2f,  
			15f,     
			50f,     
			5f);   

	}

	public void Initialize(SpawnSettings spawnSettings)
	{
		_isInitialized = true;
		_spawnSettings = spawnSettings;
	}

	public void Enable()
	{
		if (_isEnabled || !_isInitialized)
		{
			return;
		}

		_isEnabled = true;
		_cancellationTokenSource = new CancellationTokenSource();
		StartUpdateAsync(_cancellationTokenSource.Token);

		_logger.Log("PeopleSpawnService enabled");
	}

	public void Disable()
	{
		if (!_isEnabled)
		{
			return;
		}

		_isEnabled = false;
		_cancellationTokenSource?.Cancel();

		_logger.Log("PeopleSpawnService disabled");
	}

	private async void StartUpdateAsync(CancellationToken cancellationToken)
	{
		try
		{
			while (!cancellationToken.IsCancellationRequested && _isInitialized && _isEnabled)
			{
				await ProcessSpawningAsync(cancellationToken);
				await ProcessPendingSpawnsAsync(cancellationToken);
				CleanupConsumedPeople();

				// Wait for the spawn check interval
				var checkInterval = _spawnSettings.SpawnCheckInterval;
				await Task.Delay(checkInterval, cancellationToken);
			}
		}
		catch (OperationCanceledException)
		{
			// Expected when cancelling
		}
		catch (Exception ex)
		{
			_logger.LogException(ex);
		}
	}

	public int GetActivePeopleCount()
	{
		return _activePeople.Count(p => p.CanBeConsumed && p.IsActive);
	}

	public void SpawnCharacter()
	{
		if (!_isInitialized || !_isEnabled)
		{
			return;
		}

		var spawnPosition = GetSpawnPosition();
		if (spawnPosition.HasValue)
		{
			// Fire-and-forget для совместимости с синхронным API
			_ = CreateCharacterAtAsync(spawnPosition.Value, _cancellationTokenSource?.Token ?? CancellationToken.None);
		}
		else
		{
			_pendingSpawns.Enqueue(Vector3.zero);
		}
	}

	private async Task SpawnCharacterAsync(CancellationToken cancellationToken)
	{
		if (!_isInitialized || !_isEnabled)
		{
			return;
		}

		var spawnPosition = GetSpawnPosition();
		if (spawnPosition.HasValue)
		{
			await CreateCharacterAtAsync(spawnPosition.Value, cancellationToken);
		}
		else
		{
			_pendingSpawns.Enqueue(Vector3.zero);
		}
	}

	public void RemoveCharacter(ICharacter character)
	{
		if (character != null && _activePeople.Contains(character))
		{
			_activePeople.Remove(character);
			OnCharacterConsumed?.Invoke(character);

			ReturnCharacterToPool(character);
		}
	}

	public void Dispose()
	{
		Disable();

		_cancellationTokenSource?.Cancel();
		_cancellationTokenSource?.Dispose();
		_spawnSemaphore?.Dispose();

		DisposeCharacters();

		_pendingSpawns.Clear();
		_logger.Log("PeopleSpawnService disposed");
	}

	private void DisposeCharacters()
	{
		// Dispose all active people
		foreach (var person in _activePeople)
		{
			person.Dispose();
		}
		_activePeople.Clear();

		// Dispose pooled people
		while (_peoplePool.Count > 0)
		{
			var person = _peoplePool.Dequeue();
			person.Dispose();
		}
	}

	private void ProcessSpawning()
	{
		var targetSpawnCount = _spawnSettings.TargetSpawnCount;
		var currentActiveCount = GetActivePeopleCount();
		var peopleToSpawn = targetSpawnCount - currentActiveCount;

		for (var i = 0; i < peopleToSpawn; i++)
		{
			SpawnCharacter();
		}
	}

	private async Task ProcessSpawningAsync(CancellationToken cancellationToken)
	{
		var targetSpawnCount = _spawnSettings.TargetSpawnCount;
		var currentActiveCount = GetActivePeopleCount();
		var peopleToSpawn = targetSpawnCount - currentActiveCount;

		var spawnTasks = new List<Task>();
		
		for (var i = 0; i < peopleToSpawn; i++)
		{
			spawnTasks.Add(SpawnCharacterAsync(cancellationToken));
		}

		if (spawnTasks.Count > 0)
		{
			await Task.WhenAll(spawnTasks);
		}
	}

	private void ProcessPendingSpawns()
	{
		if (_pendingSpawns.Count == 0)
		{
			return;
		}

		var spawnsToProcess = Math.Min(_pendingSpawns.Count, 3);

		for (var i = 0; i < spawnsToProcess; i++)
		{
			_pendingSpawns.Dequeue();

			var spawnPosition = GetSpawnPosition();
			if (spawnPosition.HasValue)
			{
				_ = CreateCharacterAtAsync(spawnPosition.Value, _cancellationTokenSource?.Token ?? CancellationToken.None);
			}
			else
			{
				_pendingSpawns.Enqueue(Vector3.zero);
			}
		}
	}

	private async Task ProcessPendingSpawnsAsync(CancellationToken cancellationToken)
	{
		if (_pendingSpawns.Count == 0) return;

		var spawnsToProcess = Math.Min(_pendingSpawns.Count, 3);
		
		_reusableTaskList.Clear();

		for (var i = 0; i < spawnsToProcess; i++)
		{
			_pendingSpawns.Dequeue();

			var spawnPosition = GetSpawnPosition();
			if (spawnPosition.HasValue)
			{
				_reusableTaskList.Add(CreateCharacterAtAsync(spawnPosition.Value, cancellationToken));
			}
			else
			{
				_pendingSpawns.Enqueue(Vector3.zero);
			}
		}

		if (_reusableTaskList.Count > 0)
		{
			await Task.WhenAll(_reusableTaskList);
		}
	}

	private void CleanupConsumedPeople()
	{
		for (var i = _activePeople.Count - 1; i >= 0; i--)
		{
			var character = _activePeople[i];
			if (character is not { CanBeConsumed: true } || !character.IsActive)
			{
				_activePeople.RemoveAt(i);
			}
		}
	}

	private Vector3? GetSpawnPosition()
	{
		var cameraBehindPosition = GetCameraBehindSpawnPosition();
		if (cameraBehindPosition.HasValue)
		{
			return cameraBehindPosition;
		}

		return GetRandomSpawnPosition();
	}

	private Vector3? GetCameraBehindSpawnPosition()
	{
		if (!_cameraService.HasActiveCamera)
		{
			return null;
		}

		var playerPosition = GetPlayerPosition();
		if (playerPosition == Vector3.zero)
		{
			return null;
		}

		var cameraToPlayer = (playerPosition - GetCameraPosition()).normalized;
		var cameraBehindSpawnDistance = _spawnSettings.CameraBehindSpawnDistance;
		var behindCameraPosition = GetCameraPosition() - cameraToPlayer * cameraBehindSpawnDistance;

		var randomOffset = Random.insideUnitCircle * 10f;
		behindCameraPosition += new Vector3(randomOffset.x, 0, randomOffset.y);

		var minSpawnDistanceFromPlayer = _spawnSettings.MinSpawnDistanceFromPlayer;
		if (Vector3.Distance(behindCameraPosition, playerPosition) < minSpawnDistanceFromPlayer)
		{
			return null;
		}

		return behindCameraPosition;
	}

	private Vector3? GetRandomSpawnPosition()
	{
		var playerPosition = GetPlayerPosition();
		if (playerPosition == Vector3.zero)
		{
			return null;
		}

		for (var attempts = 0; attempts < 10; attempts++)
		{
			var angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
			var minSpawnDistanceFromPlayer = _spawnSettings.MinSpawnDistanceFromPlayer;
			var maxSpawnDistanceFromPlayer = _spawnSettings.MaxSpawnDistanceFromPlayer;
			var distance = Random.Range(minSpawnDistanceFromPlayer, maxSpawnDistanceFromPlayer);

			var spawnPosition = playerPosition + new Vector3(
				Mathf.Cos(angle) * distance,
				0,
				Mathf.Sin(angle) * distance);

			if (IsValidSpawnPosition(spawnPosition))
			{
				return spawnPosition;
			}
		}

		return null;
	}

	private bool IsValidSpawnPosition(Vector3 position)
	{
		foreach (var character in _activePeople)
		{
			if (Vector3.Distance(position, character.Position) < 3f)
			{
				return false;
			}
		}

		return true;
	}

	private async Task CreateCharacterAtAsync(Vector3 position, CancellationToken cancellationToken)
	{
		await _spawnSemaphore.WaitAsync(cancellationToken);
		
		try
		{
			ICharacter character;

			if (_peoplePool.Count > 0)
			{
				character = _peoplePool.Dequeue();
				character.InitializePosition(position);
				character.Enable();
			}
			else
			{
				character = await CreateNewCharacterAsync(position, cancellationToken);
			}

			if (character != null)
			{
				_activePeople.Add(character);
				OnCharacterSpawned?.Invoke(character);
			}
		}
		catch (OperationCanceledException)
		{
			// Expected when cancelling
		}
		catch (Exception ex)
		{
			_logger.LogException(ex);
		}
		finally
		{
			_spawnSemaphore.Release();
		}
	}

	private async Task<ICharacter> CreateNewCharacterAsync(Vector3 position, CancellationToken token)
	{
		try
		{
			var spawnParent = _spawnSettings.SpawnParent;
			var personFactory = CitizenFactoryFactory.CreateCitizenFactory(_detectionContextTest);
			var character = await personFactory.CreatePersonAsync(spawnParent, position, token);

			return character;
		}
		catch (TaskCanceledException)
		{
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogException(ex);
			return null;
		}
	}

	private void ReturnCharacterToPool(ICharacter character)
	{
		if (character == null)
		{
			return;
		}

		character.Disable();
		_peoplePool.Enqueue(character);
	}

	private Vector3 GetPlayerPosition()
	{
		try
		{
			return _playerFactory.GetPlayerPosition();
		}
		catch (Exception ex)
		{
			_logger.LogException(ex);
			return Vector3.zero;
		}
	}

	private Vector3 GetCameraPosition()
	{
		// This is a simplified implementation
		// You'd need to extend ICameraService to provide camera position
		var mainCamera = Camera.main;
		return mainCamera != null ? mainCamera.transform.position : Vector3.zero;
	}
}
}