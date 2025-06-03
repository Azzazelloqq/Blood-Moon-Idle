using System.Threading;
using System.Threading.Tasks;
using LightDI.Runtime;
using Runtime.Core.Architecture.CompositionRoot.Facade;
using Runtime.Core.Architecture.CompositionRoot.Gameplay.City;
using Runtime.Core.Architecture.GameGlobalState.Contract;

namespace Runtime.Core.Architecture.GameGlobalState.States.Gameplay
{

public sealed class CityState : StateBase
{
	private readonly CompositionRootProvider _compositionRootProvider;
	private CityComposition _cityCompositionRoot;

	public CityState(
		[Inject] CompositionRootProvider compositionRootProvider)
	{
		_compositionRootProvider = compositionRootProvider;
	}
	
	protected override async Task EnterAsync(CancellationToken token)
	{
		_cityCompositionRoot = await _compositionRootProvider.GetRootAsync<CityComposition>(token);
		
		// Enable the composition root
		await _cityCompositionRoot.EnableAsync(token);
	}

	protected override async Task ExitAsync(CancellationToken token)
	{
		await _compositionRootProvider.ReleaseAsync(_cityCompositionRoot, token);
	}
}
}