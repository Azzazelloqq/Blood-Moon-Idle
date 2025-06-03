using System.Threading;
using System.Threading.Tasks;
using Azzazelloqq.DetectionService.Source;
using LightDI.Runtime;
using Runtime.Core.Infrastructure.TransformUtils;
using Runtime.Gameplay.Characters.Person.Base;
using TickHandler;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Runtime.Gameplay.Characters.Person
{
public class PersonPresenter : PersonPresenterBase
{
	public override ReadOnlyTransform Transform => view.Transform;
	public override PersonState CurrentState => model.CurrentState;
	public override bool CanBeConsumed => model.CanBeConsumed();
	public override bool IdDead => model.IsConsumed;
	public override Vector3 Position => model.Position;
	public override bool IsActive => model.IsEnable && !model.IsConsumed;

	private readonly IDetectionService _detectionService;
	private readonly PersonDetectionContext _detectionContext;
	private readonly ITickHandler _tickHandler;
	private float _lastDetectionCheck;

	public PersonPresenter(
		PersonViewBase view,
		PersonModelBase model,
		[Inject] IDetectionService detectionService,
		[Inject] ITickHandler tickHandler,
		PersonDetectionContext detectionContext) : base(view, model)
	{
		_detectionService = detectionService;
		_tickHandler = tickHandler;
		_detectionContext = detectionContext;
	}

	protected override void OnInitialize()
	{
		SubscribeOnModelEvents();

		view.SetMoveSpeed(model.MovementSpeed);

		_detectionService.RegisterObject(view);

		_tickHandler.SubscribeOnFrameUpdate(OnUpdate);
	}

	protected override ValueTask OnInitializeAsync(CancellationToken token)
	{
		SubscribeOnModelEvents();

		view.SetMoveSpeed(model.MovementSpeed);
		_detectionService.RegisterObject(view);

		_tickHandler.SubscribeOnFrameUpdate(OnUpdate);

		return default;
	}

	protected override void OnDispose()
	{
		_tickHandler.UnsubscribeOnFrameUpdate(OnUpdate);

		// Unregister from detection service
		_detectionService.UnregisterObject(view);

		UnsubscribeOnModelEvents();
	}

	protected override ValueTask OnDisposeAsync(CancellationToken token)
	{
		return default;
	}

	public override void InitializePosition(Vector3 position)
	{
		model.InitializePosition(position);

		view.SetPosition(model.Position);
	}

	public override void Enable()
	{
		model.Enable();
		view.SetActive(model.IsEnable);
	}

	public override void Disable()
	{
		model.Disable();
		view.SetActive(model.IsEnable);
	}

	public override void UpdateParent(Transform parent)
	{
		view.SetParent(parent);
	}

	public override void StartFleeing(Vector3 fleeTarget)
	{
		if (!model.CanMove())
		{
			return;
		}

		model.SetFleeTarget(fleeTarget);
		model.StartFleeing();
	}

	public override void StopFleeing()
	{
		model.StopFleeing();
	}

	public override void ConsumeByPlayer()
	{
		if (!model.CanBeConsumed())
		{
			return;
		}

		model.MarkAsConsumed();
		view.PlayConsumptionAnimation();
	}

	public override void SetIdleState()
	{
		model.SetIdleState();
	}

	public override void OnPlayerDetected(Vector3 playerPosition)
	{
		if (!model.CanMove() || model.CurrentState == PersonState.Fleeing)
		{
			return;
		}

		// Calculate flee target - opposite direction from player
		var fleeDirection = (model.Position - playerPosition).normalized;
		var fleeDistance = Random.Range(10f, 20f); // Random flee distance
		var fleeTarget = model.Position + fleeDirection * fleeDistance;

		StartFleeing(fleeTarget);
	}


	private void ProcessMovement(float deltaTime)
	{
		if (!model.CanMove())
		{
			return;
		}

		var oldPosition = model.Position;

		model.ProcessMovement(deltaTime);

		// Update detection service position if person moved
		if (_detectionService != null && oldPosition != model.Position)
		{
			_detectionService.UpdateObjectPosition(view, oldPosition);
		}

		ApplyMovementToView();
	}

	private void ProcessPlayerDetection(float deltaTime)
	{
		if (!model.CanMove())
		{
			return;
		}

		if (model.CurrentState == PersonState.Fleeing)
		{
			return;
		}

		_lastDetectionCheck += deltaTime;
		var detectionCheckInterval = _detectionContext.DetectionCheckInterval;
		if (_lastDetectionCheck < detectionCheckInterval)
		{
			return;
		}

		_lastDetectionCheck = 0f;

		var detectionAngle = _detectionContext.DetectionAngle;
		var detectionDistance = _detectionContext.DetectionDistance;
		var obstacleLayerMask = _detectionContext.ObstacleLayerMask;

		var detectedObjects = _detectionService.DetectObjectsInView(
			model.Position,
			view.transform.forward,
			detectionAngle,
			detectionDistance,
			obstacleLayerMask);

		foreach (var detectedObject in detectedObjects)
		{
			// For now, detect any IDetectable object as potential player
			// In a more sophisticated setup, you'd check for specific player interface
			OnPlayerDetected(detectedObject.Position);
			break; // Only react to first detected object
		}
	}

	private void ApplyMovementToView()
	{
		view.UpdatePosition(model.Position, Time.deltaTime);
		if (model.Direction != Vector3.zero)
		{
			view.UpdateRotation(model.Direction);
		}
	}

	private void SubscribeOnModelEvents()
	{
		if (model != null)
		{
			model.OnPositionChanged += OnModelPositionChanged;
			model.OnDirectionChanged += OnModelDirectionChanged;
			model.OnMovingStateChanged += OnModelMovingStateChanged;
			model.OnStateChanged += OnModelStateChanged;
			model.OnConsumed += OnModelConsumed;
			model.OnWanderRequested += OnModelWanderRequested;
		}
	}

	private void UnsubscribeOnModelEvents()
	{
		if (model != null)
		{
			model.OnPositionChanged -= OnModelPositionChanged;
			model.OnDirectionChanged -= OnModelDirectionChanged;
			model.OnMovingStateChanged -= OnModelMovingStateChanged;
			model.OnStateChanged -= OnModelStateChanged;
			model.OnConsumed -= OnModelConsumed;
			model.OnWanderRequested -= OnModelWanderRequested;
		}
	}

	private void OnModelPositionChanged(Vector3 position)
	{
		// Update detection service about new position
		if (_detectionService != null)
		{
			_detectionService.UpdateObjectPosition(view, position);
		}

		view.UpdatePosition(position, Time.deltaTime);
		UpdateNavigationState();
	}

	private void OnModelWanderRequested()
	{
		var wanderTarget = GetRandomWanderPoint();
		if (wanderTarget.HasValue)
		{
			view.SetTargetDestination(wanderTarget.Value);
		}
	}

	private void UpdateNavigationState()
	{
		// Update model with current navigation state
		var isMoving = view.Magnitude > 0.1f;
		var currentPosition = view.transform.position;

		model.UpdatePositionFromNavigation(currentPosition, isMoving);

		// Check if reached destination
		if (!view.PathPending && view.RemainingDistance < 0.5f)
		{
			model.OnReachedDestination();
		}
	}

	private Vector3? GetRandomWanderPoint()
	{
		if (model == null)
		{
			return null;
		}

		var navContext = model.GetNavigationContext();
		var currentPosition = model.Position;

		for (var attempts = 0; attempts < 5; attempts++)
		{
			var randomDirection = Random.insideUnitSphere * navContext.WanderRadius;
			randomDirection += currentPosition;

			if (NavMesh.SamplePosition(randomDirection, out var hit, navContext.WanderRadius, NavMesh.AllAreas))
			{
				return hit.position;
			}
		}

		return null;
	}

	private void OnModelDirectionChanged(Vector3 direction)
	{
		view.UpdateRotation(direction);
	}

	private void OnModelMovingStateChanged(bool isMoving)
	{
		view.UpdateMovementState(isMoving);
	}

	private void OnModelStateChanged(PersonState newState)
	{
		view.OnStateChanged(newState);

		// Update navigation based on state

		switch (newState)
		{
			case PersonState.Fleeing:
				if (model.FleeTarget != Vector3.zero)
				{
					var fleeTarget = ValidateNavMeshPosition(model.FleeTarget);
					if (fleeTarget.HasValue)
					{
						view.SetTargetDestination(fleeTarget.Value);
					}
				}

				break;
		}
	}

	private Vector3? ValidateNavMeshPosition(Vector3 targetPosition)
	{
		if (NavMesh.SamplePosition(targetPosition, out var hit, 5f, NavMesh.AllAreas))
		{
			return hit.position;
		}

		return null;
	}

	private void OnModelConsumed()
	{
	}

	private void OnUpdate(float deltaTime)
	{
		model.ProcessMovement(deltaTime);

		UpdateNavigationState();

		ProcessPlayerDetection(deltaTime);
	}
}
}