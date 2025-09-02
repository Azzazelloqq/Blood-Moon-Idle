using System;
using MVP;
using UnityEngine;

namespace Runtime.Gameplay.Characters.Person.Base
{
public abstract class CitizenModelBase : Model
{
	public abstract event Action<Vector3> OnDirectionChanged;
	public abstract event Action<bool> OnMovingStateChanged;
	public abstract event Action<PersonState> OnStateChanged;
	public abstract event Action OnConsumed;
	public abstract event Action OnWanderRequested;

	public abstract PersonState CurrentState { get; protected set; }
	public abstract Vector3 FleeTarget { get; protected set; }
	public abstract bool IsConsumed { get; protected set; }
	public abstract bool IsEnable { get; protected set; }
	public abstract Vector3 Position { get; protected set; }
	public abstract Vector3 Direction { get; protected set; }
	public abstract float MovementSpeed { get; protected set; }

	public abstract void SetFleeTarget(Vector3 target);
	public abstract void StartFleeing();
	public abstract void StopFleeing();
	public abstract void MarkAsConsumed();
	public abstract void SetIdleState();
	public abstract bool CanBeConsumed();
	public abstract void InitializePosition(Vector3 position);
	public abstract void Enable();
	public abstract void Disable();
	public abstract void ProcessMovement(float deltaTime, float currentTime);
	public abstract bool CanMove();
	public abstract void UpdatePositionFromNavigation(Vector3 currentPosition, bool isMoving);
	public abstract void OnReachedDestination();
	public abstract PersonDetectionContext GetNavigationContext();
	public abstract void StartBeingFedOn();
	public abstract void StopBeingFedOn();
	public abstract void Kill();
}

public enum PersonState
{
	Idle,
	Fleeing,
	Consumed,
	BeingFedOn,
	Dying,
	Dead,
}
}