using System.Threading;
using System.Threading.Tasks;
using Azzazelloqq.MVVM.ReactiveLibrary;
using Runtime.UI.LoadingScreen.Base;

namespace Runtime.UI.LoadingScreen
{
	public sealed class LoadingScreenModel : LoadingScreenModelBase
	{
		public override IReadOnlyReactiveProperty<float> LoadingProgress => _loadingProgress;

		public override IReadOnlyReactiveProperty<string> LoadingText => _loadingText;
			
		public override IReadOnlyReactiveProperty<bool> IsVisible => _isVisible;

		private IReactiveProperty<float> _loadingProgress;
		private IReactiveProperty<string> _loadingText;
		private IReactiveProperty<bool> _isVisible;
		
		protected override void OnInitialize()
		{
			compositeDisposable.AddDisposable(LoadingProgress, LoadingText, IsVisible);
		}

		protected override ValueTask OnInitializeAsync(CancellationToken token)
		{
			compositeDisposable.AddDisposable(LoadingProgress, LoadingText, IsVisible);

			return default;
		}

		protected override void OnDispose()
		{
		}

		protected override ValueTask OnDisposeAsync(CancellationToken token)
		{
			return default;
		}

		public override void SetLoadingProgress(float progress)
		{
			_loadingProgress.SetValue(progress);
		}

		public override void SetLoadingText(string text)
		{
			_loadingText.SetValue(text);
		}
		
		public override void SetVisible(bool visible)
		{
			_isVisible.SetValue(visible);
		}
	}
} 