using Azzazelloqq.MVVM.Core;
using Azzazelloqq.MVVM.ReactiveLibrary;
using UnityEngine;

namespace Runtime.UI.Joystick.Base
{
	public abstract class JoystickViewModelBase : ViewModelBase<JoystickModelBase>
	{
		public JoystickViewModelBase(JoystickModelBase model) : base(model)
		{
		}

		public abstract IReadOnlyReactiveProperty<Vector2> InputVector { get; }
		public abstract IReadOnlyReactiveProperty<bool> IsPressed { get; }
		public abstract IReadOnlyReactiveProperty<bool> IsActive { get; }
		public abstract IRelayCommand<Vector2> OnPointerDownCommand { get; protected set; }
		public abstract IActionCommand OnPointerUpCommand { get; protected set; }
		public abstract IRelayCommand<Vector2> OnDragCommand { get; protected set; }
		public abstract IRelayCommand<bool> SetActiveCommand { get; protected set; }

		public abstract void OnPointerDown(Vector2 position);
		public abstract void OnPointerUp();
		public abstract void OnDrag(Vector2 inputVector);
		public abstract void SetActive(bool active);
	}
}