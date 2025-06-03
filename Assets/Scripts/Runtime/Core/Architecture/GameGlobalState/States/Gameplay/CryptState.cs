using System.Threading;
using System.Threading.Tasks;
using LightDI.Runtime;
using Runtime.Core.Architecture.CompositionRoot.Facade;
using Runtime.Core.Architecture.CompositionRoot.Gameplay.Crypt;
using Runtime.Core.Architecture.GameGlobalState.Contract;

namespace Runtime.Core.Architecture.GameGlobalState.States.Gameplay
{
/// <summary>
/// Crypt gameplay state where the player explores dungeons.
/// </summary>
public sealed class CryptState : StateBase
{
	private readonly CompositionRootProvider _rootProvider;
	private CryptComposition _crypt;

	public CryptState([Inject] CompositionRootProvider rootProvider)
	{
		_rootProvider = rootProvider;
	}
	
	protected override async Task EnterAsync(CancellationToken token)
	{
		_crypt = await _rootProvider.GetRootAsync<CryptComposition>(token);
		
		// Enable the composition root
		await _crypt.EnableAsync(token);
	}

	protected override async Task ExitAsync(CancellationToken token)
	{
		await _rootProvider.ReleaseAsync(_crypt, token);
	}
}
}