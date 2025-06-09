using UnityEngine;
using LightDI.Runtime;
using ResourceLoader;
using ResourceLoader.AddressableResourceLoader;
using TickHandler;
using TickHandler.UnityTickHandler;
using LocalSaveSystem;
using InGameLogger;
using BloodMoonIdle.Infrastructure.Services;
using SceneSwitcher;
using System.IO;

namespace BloodMoonIdle.Core.Architecture
{
    public class GameCompositionRoot : MonoBehaviour
    {
        [SerializeField] private GameplayCompositionRoot _gameplayCompositionRoot;
        
        private IDiContainer _container;
        private IInGameLogger _logger;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            ConfigureContainer();
            RegisterGlobalServices();
            InitializeGame();
        }
        
        private void ConfigureContainer()
        {
            _container = DiContainerFactory.CreateContainer();
        }
        
        private void RegisterGlobalServices()
        {
            // Logging system
            _logger = new UnityInGameLogger();
            _container.RegisterAsSingleton(_logger);
            
            // Resource system
            var resourceLoader = new AddressableResourceLoader();
            _container.RegisterAsSingleton<IResourceLoader>(resourceLoader);
            
            // Tick system
            var dispatcher = gameObject.AddComponent<UnityDispatcherBehaviour>();
            var tickHandler = new UnityTickHandler(dispatcher);
            _container.RegisterAsSingleton<ITickHandler>(tickHandler);
            
            // Scene system
            var sceneService = new AddressablesSceneSwitcher();
            _container.RegisterAsSingleton<ISceneSwitcher>(sceneService);
            
            // Save system
            var storagePath = Path.Combine(Application.persistentDataPath, "SaveData");
            var saveSystem = new UnityBinaryLocalSaveSystem(storagePath, 1);
            _container.RegisterAsSingleton<ILocalSaveSystem>(saveSystem);
        }
        private void InitializeGame()
        {
            // Get logger for initialization logging
            // Initialization completed, can load scene
            // Scene will be loaded through SceneService, which is already registered
            _logger.Log($"[{LogTags.GAME_INIT}] Game initialized successfully!");
        }
        
        private void OnDestroy()
        {
            _container?.Dispose();
        }
    }
} 