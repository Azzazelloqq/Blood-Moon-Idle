using Azzazelloqq.MVVM.Core;
using Azzazelloqq.MVVM.ReactiveLibrary;

namespace Runtime.UI.LoadingScreen.Base
{
	public abstract class LoadingScreenModelBase : ModelBase
	{
		public abstract IReadOnlyReactiveProperty<float> LoadingProgress { get; }
		public abstract IReadOnlyReactiveProperty<string> LoadingText { get; }
		public abstract IReadOnlyReactiveProperty<bool> IsVisible { get; }
		
		public abstract void SetLoadingProgress(float progress);
		public abstract void SetLoadingText(string text);
		public abstract void SetVisible(bool visible);
	}
} 