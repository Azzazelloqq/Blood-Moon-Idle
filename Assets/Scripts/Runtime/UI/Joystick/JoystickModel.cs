using System.Threading;
using System.Threading.Tasks;
using Azzazelloqq.MVVM.Source.ReactiveLibrary.Property;
using BloodMoonIdle.UI.Joystick.Base;
using UnityEngine;

namespace BloodMoonIdle.UI.Joystick
{
	public sealed class JoystickModel : JoystickModelBase
	{
		public override IReactiveProperty<Vector2> InputVector { get; protected set; } =
			new ReactiveProperty<Vector2>(Vector2.zero);

		public override IReactiveProperty<bool> IsPressed { get; protected set; } = new ReactiveProperty<bool>(false);
		public override IReactiveProperty<bool> IsActive { get; protected set; } = new ReactiveProperty<bool>(true);

		protected override void OnInitialize()
		{
			base.OnInitialize();

			compositeDisposable.AddDisposable(InputVector, IsPressed, IsActive);
		}

		protected override async Task OnInitializeAsync(CancellationToken token)
		{
			await base.OnInitializeAsync(token);

			compositeDisposable.AddDisposable(InputVector, IsPressed, IsActive);
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