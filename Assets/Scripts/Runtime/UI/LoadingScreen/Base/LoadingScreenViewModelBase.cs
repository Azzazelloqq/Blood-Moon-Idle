using Azzazelloqq.MVVM.Core;
using Azzazelloqq.MVVM.ReactiveLibrary;

namespace Runtime.UI.LoadingScreen.Base
{
	public abstract class LoadingScreenViewModelBase : ViewModelBase<LoadingScreenModelBase>
	{
		public LoadingScreenViewModelBase(LoadingScreenModelBase model) : base(model)
		{
		}

		public abstract IReadOnlyReactiveProperty<float> LoadingProgress { get; }
		public abstract IReadOnlyReactiveProperty<string> LoadingText { get; }
		public abstract IReadOnlyReactiveProperty<bool> IsVisible { get; }
		
		public abstract void SetProgress(float progress);
		public abstract void SetText(string text);
		public abstract void SetVisible(bool visible);
	}
} 