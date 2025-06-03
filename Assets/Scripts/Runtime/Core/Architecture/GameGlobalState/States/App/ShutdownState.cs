using System.Threading;
using System.Threading.Tasks;
using Runtime.Core.Architecture.GameGlobalState.Contract;
using UnityEngine;

namespace Runtime.Core.Architecture.GameGlobalState.States.App
{
/// <summary>
/// Shutdown state for graceful application exit.
/// </summary>
public sealed class ShutdownState : StateBase
{
	protected override Task EnterAsync(CancellationToken token)
	{
		Application.Quit();
		
		return Task.CompletedTask;
	}

	protected override Task ExitAsync(CancellationToken token)
	{
		return Task.CompletedTask;
	}
}
}