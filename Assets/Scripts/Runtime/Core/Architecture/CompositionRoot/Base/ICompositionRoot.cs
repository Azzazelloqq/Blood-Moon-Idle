using System;
using System.Threading;
using System.Threading.Tasks;

namespace Runtime.Core.Architecture.CompositionRoot.Base
{
/// <summary>
/// Base interface for all composition roots in the application.
/// </summary>
public interface ICompositionRoot : IDisposable
{
	/// <summary>
	/// Initializes the composition root.
	/// </summary>
	void Initialize();
	
	/// <summary>
	/// Asynchronously initializes the composition root.
	/// </summary>
	/// <param name="token">Cancellation token.</param>
	ValueTask InitializeAsync(CancellationToken token);
}
}

