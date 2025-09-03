using Runtime.Gameplay.Characters.Citizen.Base;

namespace Runtime.Gameplay.Characters.Citizen
{
/// <summary>
/// Factory interface for creating CitizenBehaviourController instances
/// </summary>
public interface ICitizenBehaviourControllerFactory
{
	/// <summary>
	/// Create a new CitizenBehaviourController instance
	/// </summary>
	/// <param name="citizenPresenter">The citizen presenter to control</param>
	/// <returns>New behavior controller instance</returns>
	CitizenBehaviourController Create(CitizenPresenterBase citizenPresenter);
}

/// <summary>
/// Factory implementation for creating CitizenBehaviourController instances
/// </summary>
public class CitizenBehaviourControllerFactory : ICitizenBehaviourControllerFactory
{
	public CitizenBehaviourController Create(CitizenPresenterBase citizenPresenter)
	{
		return new CitizenBehaviourController(citizenPresenter);
	}
}
}
