using System;
using System.Collections.Generic;
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
using BloodMoonIdle.Runtime.Core.Architecture.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace BloodMoonIdle.Core.Architecture
{
    public class GameCompositionRoot : MonoBehaviour
    {
        //todo: to remove temp for test
        [SerializeField]
        private GameplayCompositionRoot _gameplayCompositionRoot;

        [SerializeField]
        private UIProvider _uiProvider;
        
        private IDiContainer _container;
        private IInGameLogger _logger;

        private async void Start()
        {
            try
            {
                Application.quitting += OnApplicationQuitting;
            
                DontDestroyOnLoad(gameObject);
                ConfigureContainer();
                RegisterGlobalServices();
                InitializeGame();

                var exitCancellationToken = Application.exitCancellationToken;
                
                _gameplayCompositionRoot.Initialize(_logger);
                await _gameplayCompositionRoot.EnterAsync(exitCancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogException(e);
            }
        }

        private void OnApplicationQuitting()
        {
            Dispose();
        }

        private void Dispose()
        {
            _container?.Dispose();
        }
        
        private void ConfigureContainer()
        {
            _container = DiContainerFactory.CreateContainer();
        }
        
        private void RegisterGlobalServices()
        {
            _logger = new UnityInGameLogger();
            _container.RegisterAsSingleton(_logger);
            
            var resourceLoader = new AddressableResourceLoader();
            _container.RegisterAsSingleton<IResourceLoader>(resourceLoader);
            
            var dispatcher = gameObject.AddComponent<UnityDispatcherBehaviour>();
            var tickHandler = new UnityTickHandler(dispatcher);
            _container.RegisterAsSingleton<ITickHandler>(tickHandler);
            
            var sceneService = new AddressablesSceneSwitcher();
            _container.RegisterAsSingleton<ISceneSwitcher>(sceneService);
            
            var storagePath = Path.Combine(Application.persistentDataPath, "SaveData");
            var saveSystem = new UnityBinaryLocalSaveSystem(storagePath, 1);
            _container.RegisterAsSingleton<ILocalSaveSystem>(saveSystem);
            
            _container.RegisterAsSingleton<IUIProvider>(_uiProvider);
        }
        private void InitializeGame()
        {
            _logger.Log($"[{LogTags.GAME_INIT}] Game initialized successfully!");
        }
    }
} 