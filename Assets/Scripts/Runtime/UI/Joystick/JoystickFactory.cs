using System.Threading;
using System.Threading.Tasks;
using BloodMoonIdle.Runtime.Core.Architecture.UI;
using BloodMoonIdle.Runtime.UI.Joystick.Base;
using BloodMoonIdle.UI.Joystick.Base;
using LightDI.Runtime;
using ResourceLoader;
using Scripts.Generated.Addressables;
using UnityEngine;

namespace BloodMoonIdle.UI.Joystick
{
    public class JoystickFactory : IJoystickFactory
    {
        private readonly IResourceLoader _resourceLoader;
        private readonly IUIProvider _uiProvider;

        public JoystickFactory([Inject] IResourceLoader resourceLoader, [Inject] IUIProvider uiProvider)
        {
            _resourceLoader = resourceLoader;
            _uiProvider = uiProvider;
        }
        
        public async Task<JoystickViewModel> CreateJoystickAsync(CancellationToken token)
        {
            var virtualJoystickResourceId = ResourceIdsContainer.GameplayUI.VirtualJoystick;
            var viewPrefab = await _resourceLoader.LoadResourceAsync<GameObject>(virtualJoystickResourceId, token);
            var view = _uiProvider.CreateInCanvas<JoystickView>(viewPrefab);
            
            var model = new JoystickModel();
            
            var viewModel = JoystickViewModelFactory.CreateJoystickViewModel(model);
            await viewModel.InitializeAsync(token); 
            await view.InitializeAsync(viewModel, token);
            
            return viewModel;
        }
    }
} 