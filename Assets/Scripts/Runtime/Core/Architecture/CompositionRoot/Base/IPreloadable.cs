using System.Threading;
using System.Threading.Tasks;

namespace Runtime.Core.Architecture.CompositionRoot.Base
{
/// <summary>
/// Interface for composition roots that support preloading of resources.
/// Preloading allows heavy resources to be loaded in advance before they are needed.
/// </summary>
public interface IPreloadable
{
	/// <summary>
	/// Preloads resources asynchronously.
	/// This method should load heavy resources that the composition root needs.
	/// </summary>
	/// <param name="token">Cancellation token to cancel the preload operation.</param>
	/// <returns>A task that completes when preloading is finished.</returns>
	ValueTask PreloadAsync(CancellationToken token);
}
}
