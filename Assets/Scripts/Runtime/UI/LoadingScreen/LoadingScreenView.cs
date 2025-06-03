using System.Threading;
using System.Threading.Tasks;
using Runtime.UI.LoadingScreen.Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI.LoadingScreen
{
	public class LoadingScreenView : LoadingScreenViewBase
	{
		[SerializeField]
		private GameObject _loadingPanel;

		[SerializeField]
		private Slider _progressBar;

		[SerializeField]
		private TMP_Text _loadingText;

		[SerializeField]
		private GameObject _loadingIcon;

		protected override void OnInitialize()
		{
			BindToViewModel();
		}

		protected override ValueTask OnInitializeAsync(CancellationToken token)
		{
			BindToViewModel();

			return default;
		}

		protected override ValueTask OnDisposeAsync(CancellationToken token)
		{
			return default;
		}

		protected override void OnDispose()
		{
		}

		private void BindToViewModel()
		{
			var progressSub = viewModel.LoadingProgress.Subscribe(OnProgressChanged);
			var textSub = viewModel.LoadingText.Subscribe(OnTextChanged);
			var visibleSub = viewModel.IsVisible.Subscribe(OnVisibilityChanged);

			compositeDisposable.AddDisposable(progressSub, textSub, visibleSub);
		}

		private void OnProgressChanged(float progress)
		{
			_progressBar.value = progress;
		}

		private void OnTextChanged(string text)
		{
			_loadingText.text = text;
		}

		private void OnVisibilityChanged(bool isVisible)
		{
			_loadingPanel.SetActive(isVisible);
			_loadingIcon.SetActive(isVisible);
		}
	}
} 