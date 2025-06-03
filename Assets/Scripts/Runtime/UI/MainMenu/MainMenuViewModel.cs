using System.Threading;
using System.Threading.Tasks;
using Azzazelloqq.MVVM.Core;
using Azzazelloqq.MVVM.ReactiveLibrary;
using InGameLogger;
using LightDI.Runtime;
using Runtime.UI.MainMenu.Base;
using UnityEditor;

namespace Runtime.UI.MainMenu
{
	public class MainMenuViewModel : MainMenuViewModelBase
	{
		public override IReadOnlyReactiveProperty<bool> IsGameStarting => model.IsGameStarting;
		public override IReadOnlyReactiveProperty<bool> IsExiting => model.IsExiting;

		public override IActionCommand StartGameCommand { get; protected set; }
		public override IActionCommand ExitGameCommand { get; protected set; }

		private readonly IInGameLogger _logger;

		public MainMenuViewModel(
			MainMenuModel model, 
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
			StartGameCommand = new ActionCommand(ExecuteStartGame);
			ExitGameCommand = new ActionCommand(ExecuteExitGame);

			compositeDisposable.AddDisposable(StartGameCommand);
			compositeDisposable.AddDisposable(ExitGameCommand);
		}

		private void ExecuteStartGame()
		{
			model.SetGameStarting(true);
		}

		private void ExecuteExitGame()
		{
			model.SetExiting(true);
			
			// Логика выхода из игры
			#if UNITY_EDITOR
				EditorApplication.isPlaying = false;
			#else
				Application.Quit();
			#endif
		}
	}
} 