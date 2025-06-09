using LightDI.Runtime;

namespace BloodMoonIdle.UI.Joystick
{
    public class JoystickFactory : IJoystickFactory
    {
        public JoystickFactory()
        {
        }
        
        public JoystickViewModel CreateJoystick(JoystickView view)
        {
            // Create model manually (no DI needed)
            var model = new JoystickModel();
            
            // Create ViewModel through auto-generated factory
            // Model parameter will be passed, dependencies auto-injected
            var viewModel = JoystickViewModelFactory.CreateJoystickViewModel(model);
            viewModel.Initialize(); // This will automatically call model.Initialize()
            
            // Initialize view with ViewModel
            view.Initialize(viewModel);
            
            return viewModel;
        }
    }
} 