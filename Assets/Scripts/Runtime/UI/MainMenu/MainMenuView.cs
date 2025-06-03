using System.Threading;
using System.Threading.Tasks;
using Azzazelloqq.MVVM.ReactiveLibrary.Callbacks;
using Runtime.UI.MainMenu.Base;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI.MainMenu
{
	public class MainMenuView : MainMenuViewBase
	{
		[SerializeField]
		private Button _startGameButton;

		[SerializeField]
		private Button _exitGameButton;

		[SerializeField]
		private GameObject _loadingIndicator;

		private Subscription<bool> _isGameStartingSub;
		private Subscription<bool> _isExitingSub;

		protected override void OnInitialize()
		{
			BindToViewModel();
			SetupButtons();
		}

		protected override ValueTask OnInitializeAsync(CancellationToken token)
		{
			BindToViewModel();
			SetupButtons();
			
			return default;
		}

		protected override void OnDispose()
		{
			_startGameButton.onClick.RemoveListener(OnStartGameButtonClicked);
			_exitGameButton.onClick.RemoveListener(OnExitGameButtonClicked);
		}
		
		protected override ValueTask OnDisposeAsync(CancellationToken token)
		{
			return default;
		}

		private void BindToViewModel()
		{
			_isGameStartingSub = viewModel.IsGameStarting.Subscribe(OnGameStartingStateChanged);
			_isExitingSub = viewModel.IsExiting.Subscribe(OnExitingStateChanged);

			compositeDisposable.AddDisposable(_isGameStartingSub, _isExitingSub);
		}

		private void SetupButtons()
		{
			_startGameButton.onClick.AddListener(OnStartGameButtonClicked);
			_exitGameButton.onClick.AddListener(OnExitGameButtonClicked);
		}

		private void OnStartGameButtonClicked()
		{
			viewModel.StartGameCommand.Execute();
		}

		private void OnExitGameButtonClicked()
		{
			viewModel.ExitGameCommand.Execute();
		}

		private void OnGameStartingStateChanged(bool isStarting)
		{
			_loadingIndicator.SetActive(isStarting);
			_startGameButton.interactable = !isStarting;
			_exitGameButton.interactable = !isStarting;
		}

		private void OnExitingStateChanged(bool isExiting)
		{
			_startGameButton.interactable = !isExiting;
			_exitGameButton.interactable = !isExiting;
		}
	}
} 