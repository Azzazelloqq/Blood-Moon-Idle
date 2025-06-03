using System.Threading;
using System.Threading.Tasks;
using Azzazelloqq.MVVM.ReactiveLibrary;
using Runtime.UI.MainMenu.Base;

namespace Runtime.UI.MainMenu
{
public sealed class MainMenuModel : MainMenuModelBase
{
	public override IReactiveProperty<bool> IsGameStarting { get; protected set; } =
		new ReactiveProperty<bool>(false);

	public override IReactiveProperty<bool> IsExiting { get; protected set; } =
		new ReactiveProperty<bool>(false);

	protected override void OnInitialize()
	{
		compositeDisposable.AddDisposable(IsGameStarting, IsExiting);
	}

	protected override void OnDispose()
	{
	}

	protected override ValueTask OnDisposeAsync(CancellationToken token)
	{
		return default;
	}

	protected override ValueTask OnInitializeAsync(CancellationToken token)
	{
		compositeDisposable.AddDisposable(IsGameStarting, IsExiting);

		return default;
	}

	public override void SetGameStarting(bool isStarting)
	{
		IsGameStarting.SetValue(isStarting);
	}

	public override void SetExiting(bool isExiting)
	{
		IsExiting.SetValue(isExiting);
	}
}
}