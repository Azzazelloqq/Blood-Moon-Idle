using BehaviourTree.Source;
using UnityEngine;

namespace Runtime.Gameplay.AI.Agents
{
/// <summary>
/// Agent interface for detection-related behaviors
/// </summary>
public interface IDetectionAgent : IBehaviourTreeAgent
{
	/// <summary>
	/// Detection range
	/// </summary>
	float DetectionRange { get; }
	
	/// <summary>
	/// Detection angle in degrees
	/// </summary>
	float DetectionAngle { get; }
	
	/// <summary>
	/// Whether player is currently detected
	/// </summary>
	bool IsPlayerDetected { get; }
	
	/// <summary>
	/// Last known player position
	/// </summary>
	Vector3? LastKnownPlayerPosition { get; }
	
	/// <summary>
	/// Current player position if detected
	/// </summary>
	Vector3? CurrentPlayerPosition { get; }
	
	/// <summary>
	/// Check for player in detection range
	/// </summary>
	/// <returns>True if player is detected</returns>
	bool DetectPlayer();
	
	/// <summary>
	/// Get flee direction away from player
	/// </summary>
	/// <param name="playerPosition">Player position to flee from</param>
	/// <param name="fleeDistance">Distance to flee</param>
	/// <returns>Flee direction vector</returns>
	Vector3 GetFleeDirection(Vector3 playerPosition, float fleeDistance);
}
}
