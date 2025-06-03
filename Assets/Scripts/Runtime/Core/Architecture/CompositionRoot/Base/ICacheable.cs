using System.Threading;
using System.Threading.Tasks;

namespace Runtime.Core.Architecture.CompositionRoot.Base
{
/// <summary>
/// Interface for composition roots that support caching.
/// Cached roots can be disabled when not in use and re-enabled when needed again.
/// Note: When precaching, the root is first initialized normally, then immediately disabled.
/// </summary>
public interface ICacheable
{
	/// <summary>
	/// Disables the composition root for caching.
	/// This should deactivate UI, gameplay elements, and other active components.
	/// </summary>
	void Disable();
	
	/// <summary>
	/// Asynchronously disables the composition root for caching.
	/// </summary>
	/// <param name="token">Cancellation token.</param>
	ValueTask DisableAsync(CancellationToken token);
	
	/// <summary>
	/// Enables the composition root from cache.
	/// This should reactivate all previously disabled components.
	/// </summary>
	void Enable();
	
	/// <summary>
	/// Asynchronously enables the composition root from cache.
	/// </summary>
	/// <param name="token">Cancellation token.</param>
	ValueTask EnableAsync(CancellationToken token);
}
}
