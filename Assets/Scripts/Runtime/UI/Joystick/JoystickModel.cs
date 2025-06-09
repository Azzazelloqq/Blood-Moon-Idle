using UnityEngine;
using Azzazelloqq.MVVM.Source.Core.Model;
using Azzazelloqq.MVVM.Source.ReactiveLibrary.Property;

namespace BloodMoonIdle.UI.Joystick
{
    public class JoystickModel : ModelBase
    {
        public IReactiveProperty<Vector2> InputVector { get; private set; }
        public IReactiveProperty<bool> IsPressed { get; private set; }
        public IReactiveProperty<bool> IsActive { get; private set; }
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // Initialize reactive properties
            InputVector = new ReactiveProperty<Vector2>(Vector2.zero);
            IsPressed = new ReactiveProperty<bool>(false);
            IsActive = new ReactiveProperty<bool>(true);
            
            // Add to composite disposable for automatic cleanup
            compositeDisposable.AddDisposable(InputVector);
            compositeDisposable.AddDisposable(IsPressed);
            compositeDisposable.AddDisposable(IsActive);
        }
        
        public void SetInputVector(Vector2 input)
        {
            InputVector.SetValue(input);
        }
        
        public void SetPressed(bool pressed)
        {
            IsPressed.SetValue(pressed);
        }
        
        public void SetActive(bool active)
        {
            IsActive.SetValue(active);
        }
    }
} 