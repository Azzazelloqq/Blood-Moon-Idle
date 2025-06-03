using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azzazelloqq.Config;
using Disposable;
using InGameLogger;
using LightDI.Runtime;
using LocalSaveSystem;
using ResourceLoader;
using ResourceLoader.AddressableResourceLoader;
using Runtime.Core.Architecture.CompositionRoot.Base;
using Runtime.Core.Architecture.UI;
using Runtime.Core.Infrastructure.Config.Parser;
using Runtime.Core.Infrastructure.Config.Remote.Main;
using Scripts.Generated.Addressables;
using UnityEngine;
using Config = Azzazelloqq.Config.Config;
using Object = UnityEngine.Object;

namespace Runtime.Core.Architecture.CompositionRoot.Main
{
    public class GameCompositionRoot : DisposableBase, ICompositionRoot, IPersistentRoot
    {
        private const string SaveFolder = "SaveData";
        private IDiContainer _container;
        private IInGameLogger _logger;
        private IResourceLoader _resourceLoader;
        private IConfig _config;
        private Transform _rootTransform;
        private UIProvider _uiProvider;
        private RemoteConfigSO _remoteConfig;
        
        public void Initialize()
        {
            var sceneContext = Object.FindFirstObjectByType<GameEntryPointSceneContext>();
            
            _rootTransform = sceneContext.RootTransform;
            _uiProvider = sceneContext.UIProvider;
            
            Object.DontDestroyOnLoad(_rootTransform.gameObject);
            
            ConfigureContainer();
            
            RegisterGlobalServices();
        }

        public async ValueTask InitializeAsync(CancellationToken token)
        {
            var sceneContext = Object.FindFirstObjectByType<GameEntryPointSceneContext>();
            
            _rootTransform = sceneContext.RootTransform;
            _uiProvider = sceneContext.UIProvider;
            
            try
            {
                Object.DontDestroyOnLoad(_rootTransform.gameObject);
                
                ConfigureContainer();
                
                await RegisterGlobalServicesAsync(token);
            }
            catch (Exception e)
            {
                _logger.LogException(e);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            
            _container?.Dispose();
        }

        public override async ValueTask DisposeAsync(CancellationToken token, bool continueOnCapturedContext = false)
        {
            await base.DisposeAsync(token, continueOnCapturedContext);
            
            _container?.Dispose();
        }

        private void ConfigureContainer()
        {
            _container = DiContainerFactory.CreateContainer();
        }
        
        private async ValueTask RegisterGlobalServicesAsync(CancellationToken token)
        {
            _logger = new UnityInGameLogger();
            _container.RegisterAsSingleton(_logger);
            
            _resourceLoader = new AddressableResourceLoader();
            _container.RegisterAsSingleton(_resourceLoader);

            var storagePath = Path.Combine(Application.persistentDataPath, SaveFolder);
            var saveSystem = new UnityBinaryLocalSaveSystem(storagePath, 1);
            _container.RegisterAsSingleton<ILocalSaveSystem>(saveSystem);
            
            _container.RegisterAsSingleton<IUIProvider>(_uiProvider);

            var mainConfigResourceId = ResourceIdsContainer.Config.MainRemoteConfig;
            _remoteConfig = await _resourceLoader.LoadResourceAsync<RemoteConfigSO>(mainConfigResourceId, token);
            
            var configParser = new ConfigParser(_remoteConfig);
            _config = new Config(configParser);
            await _config.InitializeAsync(token);
            
            _container.RegisterAsSingleton(_config);
        }

        private void RegisterGlobalServices()
        {
            _logger = new UnityInGameLogger();
            _container.RegisterAsSingleton(_logger);
            
            _resourceLoader = new AddressableResourceLoader();
            _container.RegisterAsSingleton(_resourceLoader);

            var storagePath = Path.Combine(Application.persistentDataPath, SaveFolder);
            var saveSystem = new UnityBinaryLocalSaveSystem(storagePath, 1);
            _container.RegisterAsSingleton<ILocalSaveSystem>(saveSystem);
            
            _container.RegisterAsSingleton<IUIProvider>(_uiProvider);

            var mainConfigResourceId = ResourceIdsContainer.Config.MainRemoteConfig;
            _remoteConfig = _resourceLoader.LoadResource<RemoteConfigSO>(mainConfigResourceId);
            
            var configParser = new ConfigParser(_remoteConfig);
            _config = new Config(configParser);
            _config.Initialize();
            
            _container.RegisterAsSingleton(_config);
        }
    }
} 