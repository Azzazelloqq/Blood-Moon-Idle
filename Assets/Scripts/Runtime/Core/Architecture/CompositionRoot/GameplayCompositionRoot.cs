using System;
using System.Threading;
using System.Threading.Tasks;
using BloodMoonIdle.Core.Input;
using BloodMoonIdle.Gameplay.Character.Movement;
using BloodMoonIdle.Infrastructure.Factories;
using BloodMoonIdle.Infrastructure.Services;
using BloodMoonIdle.UI.Joystick;
using InGameLogger;
using LightDI.Runtime;
using UnityEngine;

namespace BloodMoonIdle.Core.Architecture
{
	public class GameplayCompositionRoot : MonoBehaviour
	{
		[SerializeField]
		private Transform _characterSpawnPoint;

		private IDiContainer _gameplayDIContainer;
		private IInGameLogger _logger;

		public void Initialize(IInGameLogger logger)
		{
			_logger = logger;
		}
		
		public async ValueTask EnterAsync(CancellationToken token)
		{
			try
			{
				ConfigureLocalContainer();
				RegisterGameplayServices();
				
				await InitializeGameplayAsync(token);
			}
			catch (Exception e)
			{
				_logger.LogException(e);
			}
		}

		private void ConfigureLocalContainer()
		{
			_gameplayDIContainer = DiContainerFactory.CreateContainer();
		}

		private void RegisterGameplayServices()
		{
			var movementSystem = new MovementSystem();
			_gameplayDIContainer.RegisterAsSingleton<IMovementSystem>(movementSystem);
			
			var inputService = new InputService();
			_gameplayDIContainer.RegisterAsSingleton<IInputService>(inputService);
		}

		private async ValueTask InitializeGameplayAsync(CancellationToken token)
		{
			try
			{
				var joystickFactory = JoystickFactoryFactory.CreateJoystickFactory();
				var joystickViewModel = await joystickFactory.CreateJoystickAsync(token);

				var inputService = new InputService();
				inputService.SetInputProvider(joystickViewModel);
				inputService.Initialize();

				var characterFactory = CharacterFactoryFactory.CreateCharacterFactory();
				var character = characterFactory.CreateCharacter(_characterSpawnPoint.position);

				if (character != null)
				{
					_logger.Log($"[{LogTags.GAMEPLAY_INIT}] Gameplay initialization completed successfully");
				}
				else
				{
					_logger.LogError($"[{LogTags.GAMEPLAY_INIT}] Failed to create character during gameplay initialization");
				}
			}
			catch (Exception ex)
			{
				_logger.LogException(ex);
			}
		}

		private void OnDestroy()
		{
			_gameplayDIContainer?.Dispose();
		}
	}
}