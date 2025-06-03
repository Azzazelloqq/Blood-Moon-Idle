using System.Threading;
using System.Threading.Tasks;
using Azzazelloqq.MVVM.ReactiveLibrary;
using InGameLogger;
using LightDI.Runtime;
using Runtime.UI.LoadingScreen.Base;

namespace Runtime.UI.LoadingScreen
{
	public class LoadingScreenViewModel : LoadingScreenViewModelBase
	{
		public override IReadOnlyReactiveProperty<float> LoadingProgress => model.LoadingProgress;
		public override IReadOnlyReactiveProperty<string> LoadingText => model.LoadingText;
		public override IReadOnlyReactiveProperty<bool> IsVisible => model.IsVisible;

		private readonly IInGameLogger _logger;

		public LoadingScreenViewModel(
			LoadingScreenModel model, 
			[Inject] IInGameLogger logger) : base(model)
		{
			_logger = logger;
		}

		protected override void OnInitialize()
		{
			throw new System.NotImplementedException();
		}

		protected override ValueTask OnInitializeAsync(CancellationToken token)
		{
			return default;
		}

		protected override void OnDispose()
		{
		}

		protected override ValueTask OnDisposeAsync(CancellationToken token)
		{
			return default;
		}

		public override void SetProgress(float progress)
		{
			model.SetLoadingProgress(progress);
		}

		public override void SetText(string text)
		{
			model.SetLoadingText(text);
		}
		
		public override void SetVisible(bool visible)
		{
			model.SetVisible(visible);
		}
	}
} 