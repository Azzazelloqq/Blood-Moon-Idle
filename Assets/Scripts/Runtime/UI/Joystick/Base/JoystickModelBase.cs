using Azzazelloqq.MVVM.Core;
using Azzazelloqq.MVVM.ReactiveLibrary;
using UnityEngine;

namespace Runtime.UI.Joystick.Base
{
	public abstract class JoystickModelBase : ModelBase
	{
		public abstract IReadOnlyReactiveProperty<Vector2> InputVector { get; }
		public abstract IReadOnlyReactiveProperty<bool> IsPressed { get;  }
		public abstract IReadOnlyReactiveProperty<bool> IsActive { get;  }
		public abstract void SetInputVector(Vector2 input);
		public abstract void SetPressed(bool pressed);
		public abstract void SetActive(bool active);
	}
}