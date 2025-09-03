using System;
using System.Threading;
using System.Threading.Tasks;
using Runtime.Gameplay.AI.Citizen;
using Runtime.Gameplay.Characters.Citizen.Base;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Gameplay.Characters.Citizen
{
public class CitizenModel : CitizenModelBase
{
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

	public CitizenModel(float movementSpeed = 3f)
	{
		MovementSpeed = movementSpeed;
		CurrentState = PersonState.Idle;
		IsEnable = true;
	}

	public CitizenModel(float movementSpeed, PersonDetectionContext aiNavigationContext) : this(movementSpeed)
	{
		_aiNavigationContext = aiNavigationContext;

		_nextWanderTime = Time.time + Random.Range(0f, _aiNavigationContext.WanderInterval);
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

	public override void ProcessMovement(float deltaTime, float currentTime)
	{
		if (!CanMove())
		{
			return;
		}

		switch (CurrentState)
		{
			case PersonState.Idle:
				ProcessIdleWandering(deltaTime, currentTime);
				break;
			case PersonState.Fleeing:
			case PersonState.Consumed:
				break;
		}
	}

	private void ProcessIdleWandering(float deltaTime, float currentTime)
	{
		// Check if it's time to wander to a new location
		if (currentTime < _nextWanderTime)
		{
			return;
		}

		RequestNewWanderTarget();
		_nextWanderTime = currentTime + _aiNavigationContext.WanderInterval;
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
	}

	public override void SetFleeTarget(Vector3 playerPosition)
	{
		var fleeDirection = (Position - playerPosition).normalized;
		var desiredFleePosition = playerPosition + fleeDirection * _aiNavigationContext.FleeDistance;

		FleeTarget = desiredFleePosition;
	}

	public override PersonDetectionContext GetNavigationContext()
	{
		return _aiNavigationContext;
	}

	public override void StartBeingFedOn()
	{
		CurrentState = PersonState.BeingFedOn;
		
		OnStateChanged?.Invoke(CurrentState);
	}

	public override void StopBeingFedOn()
	{
		CurrentState = PersonState.Idle;
		
		OnStateChanged?.Invoke(CurrentState);
	}

	public override void Kill()
	{
		CurrentState = PersonState.Dead;
		
		OnStateChanged?.Invoke(CurrentState);
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

		_nextWanderTime = Time.time + Random.Range(1f, _aiNavigationContext.WanderInterval);

		OnStateChanged?.Invoke(CurrentState);
	}

	public override bool CanBeConsumed()
	{
		return !IsConsumed && CurrentState != PersonState.Consumed;
	}
}
}