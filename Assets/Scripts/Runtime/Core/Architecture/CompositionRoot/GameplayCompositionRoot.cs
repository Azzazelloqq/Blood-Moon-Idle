using System;
using System.Threading;
using System.Threading.Tasks;
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

		private IDiContainer _localContainer;
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
			_localContainer = DiContainerFactory.CreateContainer();
		}

		private void RegisterGameplayServices()
		{
			_localContainer.RegisterAsSingletonLazy<IMovementSystem>(() => new MovementSystem());
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
				_logger.LogError($"[{LogTags.GAMEPLAY_INIT}] Failed to initialize gameplay: {ex.Message}");
				_logger.LogException(ex);
			}
		}

		private void OnDestroy()
		{
			_localContainer?.Dispose();
		}
	}
}