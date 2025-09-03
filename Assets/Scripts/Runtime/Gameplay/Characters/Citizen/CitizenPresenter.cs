using System;
using System.Threading;
using System.Threading.Tasks;
using Azzazelloqq.DetectionService.Source;
using LightDI.Runtime;
using Runtime.Core.Infrastructure.TransformUtils;
using Runtime.Gameplay.AI.Citizen;
using Runtime.Gameplay.Characters.Citizen.Base;
using Runtime.Gameplay.Characters.Player;
using TickHandler;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Runtime.Gameplay.Characters.Citizen
{
public class CitizenPresenter : CitizenPresenterBase
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
	private readonly CitizenBehaviourController _behaviourController;
	private float _lastDetectionCheck;

	public CitizenPresenter(
		CitizenViewBase view,
		CitizenModelBase model,
		[Inject] IDetectionService detectionService,
		[Inject] ITickHandler tickHandler,
		ICitizenBehaviourControllerFactory behaviourControllerFactory,
		PersonDetectionContext detectionContext) : base(view, model)
	{
		_detectionService = detectionService;
		_tickHandler = tickHandler;
		_detectionContext = detectionContext;
		_behaviourController = behaviourControllerFactory.Create(this);
	}

	protected override void OnInitialize()
	{
		SubscribeOnModelEvents();

		view.SetMoveSpeed(model.MovementSpeed);
		_detectionService.RegisterObject(view);
		_tickHandler.SubscribeOnFrameUpdate(OnUpdate);
		
		// Initialize behavior tree controller
		_behaviourController.Initialize();
	}

	protected override ValueTask OnInitializeAsync(CancellationToken token)
	{
		SubscribeOnModelEvents();

		view.SetMoveSpeed(model.MovementSpeed);
		_detectionService.RegisterObject(view);
		_tickHandler.SubscribeOnFrameUpdate(OnUpdate);
		
		// Initialize behavior tree controller
		_behaviourController.Initialize();

		return default;
	}

	protected override void OnDispose()
	{
		_tickHandler.UnsubscribeOnFrameUpdate(OnUpdate);
		_detectionService.UnregisterObject(view);
		UnsubscribeOnModelEvents();
		_behaviourController?.Dispose();
	}

	protected override ValueTask OnDisposeAsync(CancellationToken token)
	{
		_tickHandler.UnsubscribeOnFrameUpdate(OnUpdate);
		_detectionService.UnregisterObject(view);
		UnsubscribeOnModelEvents();
		_behaviourController?.Dispose();

		return default;
	}

	public override void InitializePosition(Vector3 position)
	{
		var oldPosition = model.Position;
		model.InitializePosition(position);

		view.SetPosition(model.Position);

		_detectionService.UpdateObjectPosition(view, oldPosition);
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

	public override void Consume()
	{
		if (!model.CanBeConsumed())
		{
			return;
		}

		model.MarkAsConsumed();
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

	public override void StartBeingFedOn()
	{
		model.StartBeingFedOn();
	}

	public override void StopBeingFedOn()
	{
		model.StopBeingFedOn();
	}

	public override void Kill()
	{
		model.Kill();
	}

	private void SubscribeOnModelEvents()
	{
		model.OnDirectionChanged += OnModelDirectionChanged;
		model.OnMovingStateChanged += OnModelMovingStateChanged;
		model.OnStateChanged += OnModelStateChanged;
		model.OnConsumed += OnModelConsumed;
		model.OnWanderRequested += OnModelWanderRequested;
	}

	private void UnsubscribeOnModelEvents()
	{
		model.OnDirectionChanged -= OnModelDirectionChanged;
		model.OnMovingStateChanged -= OnModelMovingStateChanged;
		model.OnStateChanged -= OnModelStateChanged;
		model.OnConsumed -= OnModelConsumed;
		model.OnWanderRequested -= OnModelWanderRequested;
	}


	private void OnModelWanderRequested()
	{
		if (model.CurrentState != PersonState.Idle)
		{
			return;
		}

		var wanderTarget = GetRandomWanderPoint();
		view.SetTargetDestination(wanderTarget);
	}

	private void UpdateNavigationState()
	{
		var isMoving = view.Magnitude > 0.1f;
		var currentPosition = view.transform.position;

		model.UpdatePositionFromNavigation(currentPosition, isMoving);

		// Check if reached destination
		if (!view.PathPending && view.RemainingDistance < 0.5f)
		{
			model.OnReachedDestination();
		}
	}

	private Vector3 GetRandomWanderPoint()
	{
		if (model == null)
		{
			return default;
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

		return default;
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

		switch (newState)
		{
			case PersonState.Fleeing:

				var fleeTarget = ValidateNavMeshPosition(model.FleeTarget);
				view.SetTargetDestination(fleeTarget);
				break;
			case PersonState.Idle:
				break;
			case PersonState.Consumed:
				break;
			case PersonState.BeingFedOn:
				break;
			case PersonState.Dying:
				break;
			case PersonState.Dead:
				Dispose();
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
		}
	}


	private Vector3 ValidateNavMeshPosition(Vector3 targetPosition)
	{
		if (NavMesh.SamplePosition(targetPosition, out var hit, 5f, NavMesh.AllAreas))
		{
			return hit.position;
		}

		return default;
	}

	private void OnModelConsumed()
	{
	}

	private void OnUpdate(float deltaTime)
	{
		var oldPosition = model.Position;

		// Use behavior tree for AI decisions
		_behaviourController?.UpdateBehaviour();

		// Still need model movement processing and navigation state updates
		model.ProcessMovement(deltaTime, Time.time);
		UpdateNavigationState();

		// Update detection service if position changed
		if (Vector3.Distance(oldPosition, model.Position) > 0.01f)
		{
			_detectionService.UpdateObjectPosition(view, oldPosition);
		}

		// Process player detection for behavior tree
		ProcessPlayerDetectionForBehaviourTree(deltaTime);
	}

	private void ProcessPlayerDetectionForBehaviourTree(float deltaTime)
	{
		if (!model.CanMove())
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

		bool playerFound = false;
		foreach (var detectedObject in detectedObjects)
		{
			if (detectedObject is not PlayerView)
			{
				continue;
			}

			// Update behavior controller with player detection
			UpdateBehaviourControllerPlayerDetection(detectedObject.Position);
			playerFound = true;
			break;
		}
		
		// Clear detection if no player found
		if (!playerFound)
		{
			_behaviourController?.ClearPlayerDetection();
		}
	}

	private void UpdateBehaviourControllerPlayerDetection(Vector3 playerPosition)
	{
		// Update the behavior controller's player detection state
		_behaviourController?.UpdatePlayerDetection(playerPosition);
	}

	#if UNITY_EDITOR
	public float GetDetectionDistance()
	{
		return _detectionContext.DetectionDistance;
	}

	public float GetDetectionAngle()
	{
		return _detectionContext.DetectionAngle;
	}
	#endif
}
}