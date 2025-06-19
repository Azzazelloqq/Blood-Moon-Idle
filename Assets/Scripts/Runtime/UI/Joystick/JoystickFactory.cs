using System.Threading;
using System.Threading.Tasks;
using BloodMoonIdle.Runtime.UI.Joystick.Base;
using BloodMoonIdle.UI.Joystick.Base;
using LightDI.Runtime;
using ResourceLoader;
using Scripts.Generated.Addressables;

namespace BloodMoonIdle.UI.Joystick
{
    public class JoystickFactory : IJoystickFactory
    {
        private readonly IResourceLoader _resourceLoader;

        public JoystickFactory([Inject] IResourceLoader resourceLoader)
        {
            _resourceLoader = resourceLoader;
        }
        
        public async Task<JoystickViewModel> CreateJoystickAsync(CancellationToken token)
        {
            var virtualJoystickResourceId = ResourceIdsContainer.GameplayUI.VirtualJoystick;
            var view = await _resourceLoader.LoadResourceAsync<JoystickViewBase>(virtualJoystickResourceId, token);
            var model = new JoystickModel();
            
            var viewModel = JoystickViewModelFactory.CreateJoystickViewModel(model);
            await viewModel.InitializeAsync(token); 
            await view.InitializeAsync(viewModel, token);
            
            return viewModel;
        }
    }
} 