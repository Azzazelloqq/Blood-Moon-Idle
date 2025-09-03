using Azzazelloqq.DetectionService.Source;
using MVP;
using Runtime.Core.Infrastructure.TransformUtils;
using Runtime.Gameplay.AI.Citizen;
using Runtime.Gameplay.Characters.Player;
using UnityEngine;

namespace Runtime.Gameplay.Characters.Citizen.Base
{
public abstract class CitizenViewBase : ViewMonoBehaviour<CitizenPresenterBase>, IDetectable, IKillableByPlayer
{
	public abstract Transform VisualRoot { get; }
	public abstract ReadOnlyTransform Transform { get; protected set; }
	public abstract Vector3 Position { get; }
	public abstract bool IsDead { get; }
	public abstract float Magnitude { get; }
	public abstract bool PathPending { get; }
	public abstract float RemainingDistance { get; }

	public abstract void SetParent(Transform parent);
	public abstract void UpdatePosition(Vector3 position, float deltaTime);
	public abstract void UpdateRotation(Vector3 direction);
	public abstract void UpdateMovementState(bool isMoving);
	public abstract void SetActive(bool isActive);
	public abstract void SetPosition(Vector3 position);
	public abstract void OnStateChanged(PersonState newState);
	public abstract void SetMoveSpeed(float movementSpeed);
	public abstract void SetStoppingDistance(float stopppingDistance);
	public abstract void SetTargetDestination(Vector3 fleeTarget);
	public abstract void StartKilling();
	public abstract void StopKilling();
	public abstract void Kill();
}
}