using System.Threading;
using System.Threading.Tasks;
using Azzazelloqq.MVVM.Core;
using Azzazelloqq.MVVM.ReactiveLibrary;
using InGameLogger;
using LightDI.Runtime;
using Runtime.Core.Architecture.Input;
using Runtime.UI.Joystick.Base;
using UnityEngine;

namespace Runtime.UI.Joystick
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
			JoystickModel model, 
			[Inject] IInGameLogger logger) : base(model)
		{
			_logger = logger;
		}

		protected override void OnInitialize()
		{
			InitializeCommands();
		}

		protected override ValueTask OnInitializeAsync(CancellationToken token)
		{
			InitializeCommands();

			return default;
		}

		protected override void OnDispose()
		{
		}

		protected override ValueTask OnDisposeAsync(CancellationToken token)
		{
			return default;
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