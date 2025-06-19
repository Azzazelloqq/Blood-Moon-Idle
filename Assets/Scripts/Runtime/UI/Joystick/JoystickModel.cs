using Azzazelloqq.MVVM.Source.ReactiveLibrary.Property;
using BloodMoonIdle.UI.Joystick.Base;
using UnityEngine;

namespace BloodMoonIdle.UI.Joystick
{
    public class JoystickModel : JoystickModelBase
    {
        public override IReactiveProperty<Vector2> InputVector { get; protected set; }
        public override IReactiveProperty<bool> IsPressed { get; protected set; }
        public override IReactiveProperty<bool> IsActive { get; protected set; }
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            InputVector = new ReactiveProperty<Vector2>(Vector2.zero);
            IsPressed = new ReactiveProperty<bool>(false);
            IsActive = new ReactiveProperty<bool>(true);
            
            compositeDisposable.AddDisposable(InputVector);
            compositeDisposable.AddDisposable(IsPressed);
            compositeDisposable.AddDisposable(IsActive);
        }
        
        public override void SetInputVector(Vector2 input)
        {
            InputVector.SetValue(input);
        }
        
        public override void SetPressed(bool pressed)
        {
            IsPressed.SetValue(pressed);
        }
        
        public override void SetActive(bool active)
        {
            IsActive.SetValue(active);
        }
    }
} 