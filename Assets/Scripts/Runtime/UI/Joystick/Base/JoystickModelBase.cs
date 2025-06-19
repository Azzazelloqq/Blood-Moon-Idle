using Azzazelloqq.MVVM.Source.Core.Model;
using Azzazelloqq.MVVM.Source.ReactiveLibrary.Property;
using UnityEngine;

namespace BloodMoonIdle.UI.Joystick.Base
{
	public abstract class JoystickModelBase : ModelBase
	{
		public abstract IReactiveProperty<Vector2> InputVector { get; protected set; }
		public abstract IReactiveProperty<bool> IsPressed { get; protected set; }
		public abstract IReactiveProperty<bool> IsActive { get; protected set; }
		public abstract void SetInputVector(Vector2 input);
		public abstract void SetPressed(bool pressed);
		public abstract void SetActive(bool active);
	}
}