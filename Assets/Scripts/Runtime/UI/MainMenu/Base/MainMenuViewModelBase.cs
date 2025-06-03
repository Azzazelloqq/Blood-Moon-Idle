using Azzazelloqq.MVVM.Core;
using Azzazelloqq.MVVM.ReactiveLibrary;

namespace Runtime.UI.MainMenu.Base
{
	public abstract class MainMenuViewModelBase : ViewModelBase<MainMenuModelBase>
	{
		public MainMenuViewModelBase(MainMenuModelBase model) : base(model)
		{
		}

		public abstract IReadOnlyReactiveProperty<bool> IsGameStarting { get; }
		public abstract IReadOnlyReactiveProperty<bool> IsExiting { get; }
		
		public abstract IActionCommand StartGameCommand { get; protected set; }
		public abstract IActionCommand ExitGameCommand { get; protected set; }
	}
} 