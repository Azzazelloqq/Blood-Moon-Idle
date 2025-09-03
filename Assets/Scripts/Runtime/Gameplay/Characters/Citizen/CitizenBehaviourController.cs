using System;
using BehaviourTree.Source;
using BehaviourTree.Source.Nodes;
using Runtime.Gameplay.AI.Agents;
using Runtime.Gameplay.AI.Citizen;
using Runtime.Gameplay.AI.Nodes;
using Runtime.Gameplay.Characters.Citizen.Base;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Runtime.Gameplay.Characters.Citizen
{
/// <summary>
/// Main behavior tree controller for Citizen AI
/// Manages citizen behavior through a behavior tree system
/// </summary>
public class CitizenBehaviourController : ICitizenBehaviourAgent
{
	public string AgentName => "CitizenBehaviourController";
	
	// Agent state properties
	public Vector3 Position => _citizenPresenter.Position;
	public Vector3 Direction => _citizenPresenter.Transform.Forward;
	public float MovementSpeed => 3f; // TODO: Get from presenter
	public bool CanMove => !IsConsumed && IsAlive;
	public bool IsMoving => _citizenPresenter.IsActive; // TODO: Better implementation
	public bool HasReachedDestination { get; private set; }
	
	public float DetectionRange => 15f; // TODO: Get from context
	public float DetectionAngle => 60f; // TODO: Get from context
	public bool IsPlayerDetected { get; private set; }
	public Vector3? LastKnownPlayerPosition { get; private set; }
	public Vector3? CurrentPlayerPosition { get; private set; }
	
	public PersonState CurrentState => _citizenPresenter.CurrentState;
	public bool IsAlive => !_citizenPresenter.IdDead;
	public bool CanBeConsumed => _citizenPresenter.CanBeConsumed;
	public bool IsConsumed => _citizenPresenter.IdDead;
	
	private readonly CitizenPresenterBase _citizenPresenter;
	private IBehaviourTreeNode _rootNode;
	private bool _isDisposed;
	
	public CitizenBehaviourController(CitizenPresenterBase citizenPresenter)
	{
		_citizenPresenter = citizenPresenter ?? throw new ArgumentNullException(nameof(citizenPresenter));
	}
	
	public void Initialize()
	{
		if (_isDisposed)
			return;
			
		BuildBehaviourTree();
	}
	
	public void UpdateBehaviour()
	{
		if (_isDisposed || _rootNode == null)
			return;
			
		_rootNode.Tick();
	}
	
	public void Reset()
	{
		if (_isDisposed)
			return;
			
		IsPlayerDetected = false;
		LastKnownPlayerPosition = null;
		CurrentPlayerPosition = null;
		HasReachedDestination = false;
		
		_citizenPresenter.SetIdleState();
	}
	
	#region Movement Agent Implementation
	
	public void MoveTo(Vector3 destination)
	{
		if (!CanMove)
			return;
			
		// Use the presenter's existing navigation system via model wander request
		// This triggers OnModelWanderRequested which handles navigation
		HasReachedDestination = false;
		
		// Trigger wander through the existing model system
		// For now, rely on the existing wander system
	}
	
	public void StopMovement()
	{
		HasReachedDestination = true;
	}
	
	public Vector3 GetRandomWanderPoint(float radius)
	{
		var currentPosition = Position;
		
		for (var attempts = 0; attempts < 5; attempts++)
		{
			var randomDirection = Random.insideUnitSphere * radius;
			randomDirection += currentPosition;
			randomDirection.y = currentPosition.y; // Keep same Y level
			
			// Use NavMesh to validate position
			if (NavMesh.SamplePosition(randomDirection, out var hit, radius, NavMesh.AllAreas))
			{
				return hit.position;
			}
		}
		
		// Fall back to current position if no valid point found
		return currentPosition;
	}
	
	#endregion
	
	#region Detection Agent Implementation
	
	public bool DetectPlayer()
	{
		// Player detection is handled by CitizenPresenter and updated via UpdatePlayerDetection
		return IsPlayerDetected;
	}
	
	/// <summary>
	/// Update player detection state (called from CitizenPresenter)
	/// </summary>
	/// <param name="playerPosition">Detected player position</param>
	public void UpdatePlayerDetection(Vector3 playerPosition)
	{
		IsPlayerDetected = true;
		CurrentPlayerPosition = playerPosition;
		LastKnownPlayerPosition = playerPosition;
	}
	
	/// <summary>
	/// Clear player detection state
	/// </summary>
	public void ClearPlayerDetection()
	{
		IsPlayerDetected = false;
		CurrentPlayerPosition = null;
		// Keep LastKnownPlayerPosition for investigation behavior
	}
	
	public Vector3 GetFleeDirection(Vector3 playerPosition, float fleeDistance)
	{
		var fleeDirection = (Position - playerPosition).normalized;
		return Position + fleeDirection * fleeDistance;
	}
	
	#endregion
	
	#region State Agent Implementation
	
	public void SetIdleState()
	{
		_citizenPresenter.SetIdleState();
	}
	
	public void StartFleeing(Vector3 fleeTarget)
	{
		_citizenPresenter.StartFleeing(fleeTarget);
	}
	
	public void StopFleeing()
	{
		_citizenPresenter.StopFleeing();
	}
	
	public void StartBeingFedOn()
	{
		_citizenPresenter.StartBeingFedOn();
	}
	
	public void StopBeingFedOn()
	{
		_citizenPresenter.StopBeingFedOn();
	}
	
	public void Kill()
	{
		_citizenPresenter.Kill();
	}
	
	public void MarkAsConsumed()
	{
		_citizenPresenter.Consume();
	}
	
	#endregion
	
	#region Behavior Tree Construction
	
	/// <summary>
	/// Builds the behavior tree for citizen AI
	/// Structure: Selector (priority-based decision making)
	/// 1. Emergency (being consumed/dying)
	/// 2. Flee from player 
	/// 3. Wander around
	/// </summary>
	private void BuildBehaviourTree()
	{
		_rootNode = new SelectorNode(new[]
		{
			// Priority 1: Handle emergency states (being consumed, dying)
			BuildEmergencyBranch(),
			
			// Priority 2: Flee if player detected  
			BuildFleeingBranch(),
			
			// Priority 3: Default wandering behavior
			BuildWanderingBranch()
		});
	}
	
	private IBehaviourTreeNode BuildEmergencyBranch()
	{
		return new SequenceNode(new IBehaviourTreeNode[]
		{
			new IsBeingFedOnNode(this),
			new StopMovementNode(this)
		});
	}
	
	private IBehaviourTreeNode BuildFleeingBranch()
	{
		return new SequenceNode(new IBehaviourTreeNode[]
		{
			new DetectPlayerNode(this),
			new StartFleeingNode(this)
		});
	}
	
	private IBehaviourTreeNode BuildWanderingBranch()
	{
		return new SequenceNode(new IBehaviourTreeNode[]
		{
			new IsIdleNode(this),
			new WanderNode(this)
		});
	}
	
	#endregion
	
	public void Dispose()
	{
		if (_isDisposed)
			return;
			
		_rootNode?.Dispose();
		_rootNode = null;
		_isDisposed = true;
	}
}
}
