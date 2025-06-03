using System.Threading;
using System.Threading.Tasks;
using Azzazelloqq.MVVM.ReactiveLibrary;
using Runtime.UI.Joystick.Base;
using UnityEngine;

namespace Runtime.UI.Joystick
{
	public sealed class JoystickModel : JoystickModelBase
	{
		public override IReadOnlyReactiveProperty<Vector2> InputVector => _inputVector;

		public override IReadOnlyReactiveProperty<bool> IsPressed => _isPressed;
		public override IReadOnlyReactiveProperty<bool> IsActive => _isActive;

		private readonly IReactiveProperty<Vector2> _inputVector = new ReactiveProperty<Vector2>();
		private readonly IReactiveProperty<bool> _isPressed = new ReactiveProperty<bool>();
		private readonly IReactiveProperty<bool> _isActive = new ReactiveProperty<bool>(true);
		
		protected override void OnInitialize()
		{
			compositeDisposable.AddDisposable(InputVector, IsPressed, IsActive);
		}
		
		protected override ValueTask OnInitializeAsync(CancellationToken token)
		{
			compositeDisposable.AddDisposable(InputVector, IsPressed, IsActive);

			return default;
		}

		protected override void OnDispose()
		{
		}

		protected override ValueTask OnDisposeAsync(CancellationToken token)
		{
			return default;
		}

		public override void SetInputVector(Vector2 input)
		{
			_inputVector.SetValue(input);
		}

		public override void SetPressed(bool pressed)
		{
			_isPressed.SetValue(pressed);
		}

		public override void SetActive(bool active)
		{
			_isActive.SetValue(active);
		}
	}
}