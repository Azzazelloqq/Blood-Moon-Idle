using Runtime.Gameplay.Characters.Citizen.Base;

namespace Runtime.Gameplay.Characters.Citizen
{
/// <summary>
/// Factory interface for creating CitizenBehaviourAgent instances
/// </summary>
public interface ICitizenBehaviourAgentFactory
{
	/// <summary>
	/// Create a new CitizenBehaviourAgent instance
	/// </summary>
	/// <param name="citizenPresenter">The citizen presenter to control</param>
	/// <returns>New behavior agent instance</returns>
	CitizenBehaviourAgent Create(CitizenPresenterBase citizenPresenter);
}

/// <summary>
/// Factory implementation for creating CitizenBehaviourAgent instances
/// </summary>
public class CitizenBehaviourAgentFactory : ICitizenBehaviourAgentFactory
{
	public CitizenBehaviourAgent Create(CitizenPresenterBase citizenPresenter)
	{
		return new CitizenBehaviourAgent(citizenPresenter);
	}
}
}
