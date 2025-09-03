using BehaviourTree.Source;
using UnityEngine;

namespace Runtime.Gameplay.AI.Agents
{
/// <summary>
/// Agent interface for movement-related behaviors
/// </summary>
public interface IMovementAgent : IBehaviourTreeAgent
{
	/// <summary>
	/// Current position of the agent
	/// </summary>
	Vector3 Position { get; }
	
	/// <summary>
	/// Current movement direction
	/// </summary>
	Vector3 Direction { get; }
	
	/// <summary>
	/// Movement speed
	/// </summary>
	float MovementSpeed { get; }
	
	/// <summary>
	/// Whether the agent can move
	/// </summary>
	bool CanMove { get; }
	
	/// <summary>
	/// Whether the agent is currently moving
	/// </summary>
	bool IsMoving { get; }
	
	/// <summary>
	/// Move to a specific destination
	/// </summary>
	/// <param name="destination">Target destination</param>
	void MoveTo(Vector3 destination);
	
	/// <summary>
	/// Stop current movement
	/// </summary>
	void StopMovement();
	
	/// <summary>
	/// Get a random wander point around current position
	/// </summary>
	/// <param name="radius">Wander radius</param>
	/// <returns>Random wander point</returns>
	Vector3 GetRandomWanderPoint(float radius);
	
	/// <summary>
	/// Check if agent has reached destination
	/// </summary>
	bool HasReachedDestination { get; }
}
}
