using Azzazelloqq.MVVM.Core;
using Azzazelloqq.MVVM.ReactiveLibrary;

namespace Runtime.UI.MainMenu.Base
{
	public abstract class MainMenuModelBase : ModelBase
	{
		public abstract IReactiveProperty<bool> IsGameStarting { get; protected set; }
		public abstract IReactiveProperty<bool> IsExiting { get; protected set; }
		
		public abstract void SetGameStarting(bool isStarting);
		public abstract void SetExiting(bool isExiting);
	}
} 