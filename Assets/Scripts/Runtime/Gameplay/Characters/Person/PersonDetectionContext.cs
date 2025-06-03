using UnityEngine;

namespace Runtime.Gameplay.Characters.Person
{
public readonly struct PersonDetectionContext
{
	public readonly float DetectionDistance;
	public readonly float DetectionAngle;
	public readonly LayerMask ObstacleLayerMask;
	public readonly float DetectionCheckInterval;
	public readonly float FleeDistance;
	public readonly float WanderRadius;
	public readonly float WanderInterval;

	public PersonDetectionContext(
		float detectionDistance,
		float detectionAngle,
		LayerMask obstacleLayerMask,
		float detectionCheckInterval,
		float fleeDistance,
		float wanderRadius,
		float wanderInterval)
	{
		DetectionDistance = detectionDistance;
		DetectionAngle = detectionAngle;
		ObstacleLayerMask = obstacleLayerMask;
		DetectionCheckInterval = detectionCheckInterval;
		FleeDistance = fleeDistance;
		WanderRadius = wanderRadius;
		WanderInterval = wanderInterval;
	}
}
}