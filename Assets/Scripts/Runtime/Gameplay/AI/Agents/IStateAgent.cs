using BehaviourTree.Source;
using UnityEngine;

namespace Runtime.Gameplay.AI.Agents
{
/// <summary>
/// Agent interface for state management behaviors
/// </summary>
public interface IStateAgent : IBehaviourTreeAgent
{
	/// <summary>
	/// Current state of the agent
	/// </summary>
	AI.Citizen.PersonState CurrentState { get; }
	
	/// <summary>
	/// Whether the agent is alive
	/// </summary>
	bool IsAlive { get; }
	
	/// <summary>
	/// Whether the agent can be consumed
	/// </summary>
	bool CanBeConsumed { get; }
	
	/// <summary>
	/// Whether the agent is consumed
	/// </summary>
	bool IsConsumed { get; }
	
	/// <summary>
	/// Set agent to idle state
	/// </summary>
	void SetIdleState();
	
	/// <summary>
	/// Start fleeing behavior
	/// </summary>
	/// <param name="fleeTarget">Target to flee to</param>
	void StartFleeing(Vector3 fleeTarget);
	
	/// <summary>
	/// Stop fleeing behavior
	/// </summary>
	void StopFleeing();
	
	/// <summary>
	/// Start being fed on by player
	/// </summary>
	void StartBeingFedOn();
	
	/// <summary>
	/// Stop being fed on
	/// </summary>
	void StopBeingFedOn();
	
	/// <summary>
	/// Kill the agent
	/// </summary>
	void Kill();
	
	/// <summary>
	/// Mark agent as consumed
	/// </summary>
	void MarkAsConsumed();
}
}
