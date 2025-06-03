namespace Runtime.Core.Architecture.CompositionRoot.Base
{
/// <summary>
/// Marker interface for composition roots that persist throughout the application lifetime.
/// These roots are created once and never disposed until the application shuts down.
/// </summary>
public interface IPersistentRoot
{
	// Marker interface - no additional methods required
	// The persistence is managed by the CompositionRootProvider
}
}
