using Runtime.Core.Architecture.GameGlobalState.Contract;

namespace Runtime.Core.Architecture.GameGlobalState.Contract
{
/// <summary>
/// Factory contract for creating state instances with their dependencies.
/// </summary>
public interface IStateFactory
{
	/// <summary>
	/// Creates a state instance by its ID.
	/// </summary>
	/// <param name="stateId">The state identifier.</param>
	/// <returns>The created state instance or null if state type is unknown.</returns>
	IState CreateState(string stateId);
}
}