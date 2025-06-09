using UnityEngine;
using LightDI.Runtime;
using BloodMoonIdle.Core.Input;
using BloodMoonIdle.Gameplay.Character;
using BloodMoonIdle.UI.Joystick;
using BloodMoonIdle.Infrastructure.Services;
using BloodMoonIdle.Infrastructure.Factories;
using BloodMoonIdle.Gameplay.Character.Movement;
using InGameLogger;

namespace BloodMoonIdle.Core.Architecture
{
    public class GameplayCompositionRoot : MonoBehaviour
    {
        [SerializeField] private JoystickView _joystickView;
        [SerializeField] private Transform _characterSpawnPoint;
        
        private IDiContainer _localContainer;
        
        private void Start()
        {
            ConfigureLocalContainer();
            RegisterGameplayServices();
            InitializeGameplay();
        }
        
        private void ConfigureLocalContainer()
        {
            _localContainer = DiContainerFactory.CreateContainer();
        }
        
        private void RegisterGameplayServices()
        {
            // Register services that don't have [Inject] constructors
            _localContainer.RegisterAsTransient<IMovementSystem>(() => new MovementSystem());
            
            // Register MVVM models for DI
            _localContainer.RegisterAsTransient<JoystickModel>(() => new JoystickModel());
        }
        
        private void InitializeGameplay()
        {
            var logger = DiContainerProvider.Resolve<IInGameLogger>();
            
            try
            {
                // Create MVVM joystick
                var joystickFactory = new JoystickFactory();
                var joystickViewModel = joystickFactory.CreateJoystick(_joystickView);
                
                // Create input service and connect with joystick ViewModel
                var inputService = new InputService();
                inputService.SetInputProvider(joystickViewModel);
                inputService.Initialize();
                
                // Create character through generated factory
                var characterFactory = CharacterFactoryFactory.CreateCharacterFactory();
                var character = characterFactory.CreateCharacter(_characterSpawnPoint.position);
                
                if (character != null)
                {
                    logger.Log($"[{LogTags.GAMEPLAY_INIT}] Gameplay initialization completed successfully");
                }
                else
                {
                    logger.LogError($"[{LogTags.GAMEPLAY_INIT}] Failed to create character during gameplay initialization");
                }
            }
            catch (System.Exception ex)
            {
                logger.LogError($"[{LogTags.GAMEPLAY_INIT}] Failed to initialize gameplay: {ex.Message}");
                logger.LogException(ex);
            }
        }
        
        private void OnDestroy()
        {
            _localContainer?.Dispose();
        }
    }
} 