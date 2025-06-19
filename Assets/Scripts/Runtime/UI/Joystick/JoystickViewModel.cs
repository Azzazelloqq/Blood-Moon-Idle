using System.Threading;
using System.Threading.Tasks;
using Azzazelloqq.MVVM.Source.Core.Command.Base;
using Azzazelloqq.MVVM.Source.Core.Command.Commands;
using Azzazelloqq.MVVM.Source.ReactiveLibrary.Property;
using BloodMoonIdle.Core.Input;
using BloodMoonIdle.UI.Joystick.Base;
using InGameLogger;
using LightDI.Runtime;
using UnityEngine;

namespace BloodMoonIdle.UI.Joystick
{
	public class JoystickViewModel : JoystickViewModelBase, IInputProvider
	{
		public override IReadOnlyReactiveProperty<Vector2> InputVector => model.InputVector;
		public override IReadOnlyReactiveProperty<bool> IsPressed => model.IsPressed;
		public override IReadOnlyReactiveProperty<bool> IsActive => model.IsActive;

		public override IRelayCommand<Vector2> OnPointerDownCommand { get; protected set; }
		public override IActionCommand OnPointerUpCommand { get; protected set; }
		public override IRelayCommand<Vector2> OnDragCommand { get; protected set; }
		public override IRelayCommand<bool> SetActiveCommand { get; protected set; }

		Vector2 IInputProvider.MovementDirection => model.InputVector.Value;
		bool IInputProvider.IsActive => model.IsPressed.Value;

		private readonly IInGameLogger _logger;

		public JoystickViewModel(
			[Inject] IInGameLogger logger,
			JoystickModel model) : base(model)
		{
			_logger = logger;
		}

		protected override void OnInitialize()
		{
			base.OnInitialize();

			InitializeCommands();
		}

		protected override async Task OnInitializeAsync(CancellationToken token)
		{
			await base.OnInitializeAsync(token);
			
			InitializeCommands();
		}

		private void InitializeCommands()
		{
			OnPointerDownCommand = new RelayCommand<Vector2>(ExecutePointerDown);
			OnPointerUpCommand = new ActionCommand(ExecutePointerUp);
			OnDragCommand = new RelayCommand<Vector2>(ExecuteDrag);
			SetActiveCommand = new RelayCommand<bool>(ExecuteSetActive);

			compositeDisposable.AddDisposable(OnPointerDownCommand);
			compositeDisposable.AddDisposable(OnPointerUpCommand);
			compositeDisposable.AddDisposable(OnDragCommand);
			compositeDisposable.AddDisposable(SetActiveCommand);
		}

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
			model.SetActive(active);
		}

		public override void OnPointerDown(Vector2 position)
		{
			OnPointerDownCommand.Execute(position);
		}

		public override void OnPointerUp()
		{
			OnPointerUpCommand.Execute();
		}

		public override void OnDrag(Vector2 inputVector)
		{
			OnDragCommand.Execute(inputVector);
		}

		public override void SetActive(bool active)
		{
			SetActiveCommand.Execute(active);
		}
	}
}