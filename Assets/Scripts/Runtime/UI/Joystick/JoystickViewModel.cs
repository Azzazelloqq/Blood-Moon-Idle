using UnityEngine;
using LightDI.Runtime;
using Azzazelloqq.MVVM.Source.Core.ViewModel;
using Azzazelloqq.MVVM.Source.ReactiveLibrary.Property;
using Azzazelloqq.MVVM.Source.Core.Command.Commands;
using Azzazelloqq.MVVM.Source.Core.Command.Base;
using BloodMoonIdle.Core.Input;
using InGameLogger;
using BloodMoonIdle.Infrastructure.Services;

namespace BloodMoonIdle.UI.Joystick
{
    public class JoystickViewModel : ViewModelBase<JoystickModel>, IInputProvider
    {
        // Expose read-only reactive properties for binding
        public IReadOnlyReactiveProperty<Vector2> InputVector => model.InputVector;
        public IReadOnlyReactiveProperty<bool> IsPressed => model.IsPressed;
        public IReadOnlyReactiveProperty<bool> IsActive => model.IsActive;
        
        // Commands for UI interactions
        public IRelayCommand<Vector2> OnPointerDownCommand { get; private set; }
        public IActionCommand OnPointerUpCommand { get; private set; }
        public IRelayCommand<Vector2> OnDragCommand { get; private set; }
        public IRelayCommand<bool> SetActiveCommand { get; private set; }
        
        // Input provider interface implementation
        Vector2 IInputProvider.MovementDirection => model.InputVector.Value;
        bool IInputProvider.IsActive => model.IsPressed.Value;
        
        private readonly IInGameLogger _logger;
        
        public JoystickViewModel(
            [Inject] IInGameLogger logger, 
            JoystickModel model ) : base(model)
        {
            _logger = logger;
        }
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // Initialize commands
            InitializeCommands();
        }
        
        private void InitializeCommands()
        {
            // Use method references instead of lambdas for better performance
            OnPointerDownCommand = new RelayCommand<Vector2>(ExecutePointerDown);
            OnPointerUpCommand = new ActionCommand(ExecutePointerUp);
            OnDragCommand = new RelayCommand<Vector2>(ExecuteDrag);
            SetActiveCommand = new RelayCommand<bool>(ExecuteSetActive);
            
            // Add commands to composite disposable for automatic cleanup
            compositeDisposable.AddDisposable(OnPointerDownCommand);
            compositeDisposable.AddDisposable(OnPointerUpCommand);
            compositeDisposable.AddDisposable(OnDragCommand);
            compositeDisposable.AddDisposable(SetActiveCommand);
        }
        
        // Command execution methods - private for encapsulation
        private void ExecutePointerDown(Vector2 position)
        {
            model.SetPressed(true);
        }
        
        private void ExecutePointerUp()
        {
            model.SetPressed(false);
            model.SetInputVector(Vector2.zero);
        }
        
        private void ExecuteDrag(Vector2 inputVector)
        {
            model.SetInputVector(inputVector);
        }
        
        private void ExecuteSetActive(bool active)
        {
            _logger.Log($"[{LogTags.JOYSTICK}] Set active: {active}");
            model.SetActive(active);
        }
        
        // Legacy methods for backward compatibility - delegate to commands efficiently
        public void OnPointerDown(Vector2 position)
        {
            OnPointerDownCommand.Execute(position);
        }
        
        public void OnPointerUp()
        {
            OnPointerUpCommand.Execute();
        }
        
        public void OnDrag(Vector2 inputVector)
        {
            OnDragCommand.Execute(inputVector);
        }
        
        public void SetActive(bool active)
        {
            SetActiveCommand.Execute(active);
        }
    }
} 