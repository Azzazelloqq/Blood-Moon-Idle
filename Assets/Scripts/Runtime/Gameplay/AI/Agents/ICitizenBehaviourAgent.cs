using BehaviourTree.Source;
using UnityEngine;

namespace Runtime.Gameplay.AI.Agents
{
/// <summary>
/// Main AI agent interface for Citizen
/// Combines all agent capabilities for comprehensive AI behavior
/// </summary>
public interface ICitizenBehaviourAgent : IMovementAgent, IDetectionAgent, IStateAgent
{
	/// <summary>
	/// Initialize the AI agent
	/// </summary>
	void Initialize();
	
	/// <summary>
	/// Update the AI behavior (called each frame)
	/// </summary>
	void UpdateBehaviour();
	
	/// <summary>
	/// Reset agent to initial state
	/// </summary>
	void Reset();
	
	/// <summary>
	/// Update player detection state (called from CitizenPresenter)
	/// </summary>
	/// <param name="playerPosition">Detected player position</param>
	void UpdatePlayerDetection(Vector3 playerPosition);
	
	/// <summary>
	/// Clear player detection state
	/// </summary>
	void ClearPlayerDetection();
}
}
