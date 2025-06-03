using System;
using System.Threading;
using System.Threading.Tasks;
using Runtime.Core.Architecture.GameGlobalState.Contract;

namespace Runtime.Core.Architecture.GameGlobalState.Core
{
/// <summary>
/// Opaque runtime kernel that owns internal FSMs and exposes only ports.
/// </summary>
public interface IKernel : IDisposable
{
	/// <summary>
	/// State transition port.
	/// </summary>
	IFlowPort Flow { get; }

	/// <summary>
	/// Tick routing port.
	/// </summary>
	ITickPort Ticks { get; }

	/// <summary>
	/// Asynchronously disposes the kernel with a cancellation token.
	/// </summary>
	/// <param name="ct">Cancellation token.</param>
	/// <returns>ValueTask representing the async disposal operation.</returns>
	ValueTask DisposeAsync(CancellationToken ct);
}

/// <summary>
/// Entry point for creating a kernel builder.
/// </summary>
public static class Kernel
{
	/// <summary>
	/// Create a new kernel builder for composing states and levels.
	/// </summary>
	/// <returns>A new kernel builder instance.</returns>
	public static IKernelBuilder Create()
	{
		return new KernelBuilder();
	}
}

/// <summary>
/// Internal kernel implementation.
/// </summary>
internal sealed class KernelImpl : IKernel
{
	public IFlowPort Flow { get; }
	public ITickPort Ticks { get; }
	
	private bool _disposed;

	internal KernelImpl(IFlowPort flow, ITickPort ticks)
	{
		Flow = flow;
		Ticks = ticks;
	}

	/// <summary>
	/// Disposes the kernel and all its FSMs.
	/// </summary>
	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;

		// Dispose ports which should dispose their FSMs
		if (Flow is IDisposable disposableFlow)
			disposableFlow.Dispose();

		if (Ticks is IDisposable disposableTicks)
			disposableTicks.Dispose();
	}

	/// <summary>
	/// Asynchronously disposes the kernel and all its FSMs.
	/// </summary>
	public async ValueTask DisposeAsync(CancellationToken ct)
	{
		if (_disposed)
			return;

		_disposed = true;

		// Dispose ports with their custom DisposeAsync methods
		if (Flow is FlowPort flowPort)
			await flowPort.DisposeAsync(ct);
		else if (Flow is IDisposable disposableFlow)
			disposableFlow.Dispose();

		if (Ticks is TickPort tickPort)
			await tickPort.DisposeAsync(ct);
		else if (Ticks is IDisposable disposableTicks)
			disposableTicks.Dispose();
	}
}
}