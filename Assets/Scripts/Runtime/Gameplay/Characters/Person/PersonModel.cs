using System;
using System.Threading;
using System.Threading.Tasks;
using Runtime.Gameplay.Characters.Person.Base;
using UnityEngine;

namespace Runtime.Gameplay.Characters.Person
{
public class PersonModel : PersonModelBase
{
	public override event Action<Vector3> OnPositionChanged;
	public override event Action<Vector3> OnDirectionChanged;
	public override event Action<bool> OnMovingStateChanged;
	public override event Action<PersonState> OnStateChanged;
	public override event Action OnConsumed;
	public override event Action OnWanderRequested;

	public override Vector3 Position { get; protected set; }
	public override Vector3 Direction { get; protected set; }
	public sealed override float MovementSpeed { get; protected set; }
	public sealed override bool IsEnable { get; protected set; }
	public sealed override PersonState CurrentState { get; protected set; }
	public override Vector3 FleeTarget { get; protected set; }
	public override bool IsConsumed { get; protected set; }

	private bool _isMoving;
	private float _nextWanderTime;
	private readonly PersonDetectionContext _aiNavigationContext;

	public PersonModel(float movementSpeed = 3f)
	{
		MovementSpeed = movementSpeed;
		CurrentState = PersonState.Idle;
		IsEnable = true;
	}

	public PersonModel(float movementSpeed, PersonDetectionContext aiNavigationContext) : this(movementSpeed)
	{
		_aiNavigationContext = aiNavigationContext;
	}


	protected override void OnInitialize()
	{
	}

	protected override ValueTask OnInitializeAsync(CancellationToken token)
	{
		return default;
	}

	protected override void OnDispose()
	{
	}

	protected override ValueTask OnDisposeAsync(CancellationToken token)
	{
		return default;
	}

	public override bool CanMove()
	{
		return IsEnable && !IsConsumed && CurrentState != PersonState.Consumed;
	}

	public override void Enable()
	{
		IsEnable = true;
	}

	public override void Disable()
	{
		IsEnable = false;
	}

	public void SetDirection(Vector3 direction)
	{
		if (Direction != direction)
		{
			Direction = direction;
			OnDirectionChanged?.Invoke(Direction);
		}
	}

	public override void ProcessMovement(float deltaTime)
	{
		if (!CanMove())
		{
			return;
		}

		switch (CurrentState)
		{
			case PersonState.Idle:
				ProcessIdleWandering(deltaTime);
				break;

			case PersonState.Fleeing:
			case PersonState.Consumed:
				// No additional logic needed - presenter handles navigation
				break;
		}
	}

	private void ProcessIdleWandering(float deltaTime)
	{
		// Check if it's time to wander to a new location
		if (Time.time >= _nextWanderTime)
		{
			RequestNewWanderTarget();
			_nextWanderTime = Time.time + _aiNavigationContext.WanderInterval;
		}
	}

	private void RequestNewWanderTarget()
	{
		OnWanderRequested?.Invoke();
	}

	public override void UpdatePositionFromNavigation(Vector3 newPosition, bool isMoving)
	{
		if (Vector3.Distance(Position, newPosition) > 0.01f)
		{
			Position = newPosition;
			OnPositionChanged?.Invoke(Position);
		}

		if (_isMoving != isMoving)
		{
			_isMoving = isMoving;
			OnMovingStateChanged?.Invoke(_isMoving);
		}
	}

	public override void OnReachedDestination()
	{
		if (CurrentState == PersonState.Fleeing)
		{
			SetIdleState();
		}
	}

	public override void InitializePosition(Vector3 position)
	{
		Position = position;
		OnPositionChanged?.Invoke(Position);
	}

	public override void SetFleeTarget(Vector3 playerPosition)
	{
		// Calculate desired flee direction and distance - presenter will handle NavMesh validation
		var fleeDirection = (Position - playerPosition).normalized;
		var desiredFleePosition = playerPosition + fleeDirection * _aiNavigationContext.FleeDistance;

		FleeTarget = desiredFleePosition;
	}

	// Getter for AI navigation context (for presenter to use)
	public override PersonDetectionContext GetNavigationContext()
	{
		return _aiNavigationContext;
	}

	public override void StartFleeing()
	{
		if (IsConsumed)
		{
			return;
		}

		CurrentState = PersonState.Fleeing;
		OnStateChanged?.Invoke(CurrentState);
	}

	public override void StopFleeing()
	{
		if (IsConsumed)
		{
			return;
		}

		SetIdleState();
	}

	public override void MarkAsConsumed()
	{
		IsConsumed = true;
		CurrentState = PersonState.Consumed;
		IsEnable = false;

		OnStateChanged?.Invoke(CurrentState);
		OnConsumed?.Invoke();
	}

	public override void SetIdleState()
	{
		if (IsConsumed)
		{
			return;
		}

		CurrentState = PersonState.Idle;
		FleeTarget = Vector3.zero;
		OnStateChanged?.Invoke(CurrentState);
	}

	public override bool CanBeConsumed()
	{
		return !IsConsumed && CurrentState != PersonState.Consumed;
	}
}
}