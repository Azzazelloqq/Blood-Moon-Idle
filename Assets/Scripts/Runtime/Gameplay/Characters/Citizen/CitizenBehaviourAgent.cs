using System;
using BehaviourTree.Source;
using Runtime.Gameplay.AI.Agents;
using Runtime.Gameplay.AI.Citizen;
using Runtime.Gameplay.Characters.Citizen.Base;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Runtime.Gameplay.Characters.Citizen
{
/// <summary>
/// Main AI agent for Citizen behavior management
/// Responsible for all AI-related functionality including behavior trees
/// Acts as a bridge between the behavior tree system and the citizen presenter
/// </summary>
public class CitizenBehaviourAgent : ICitizenBehaviourAgent
{
	public string AgentName => "CitizenBehaviourAgent";
	
	// State properties
	public PersonState CurrentState { get; private set; }
	public bool IsAlive => _citizenPresenter != null && !_citizenPresenter.IdDead;
	public bool CanBeConsumed => _citizenPresenter?.CanBeConsumed ?? false;
	public bool IsConsumed => _citizenPresenter?.IdDead ?? true;
	
	// Movement properties  
	public Vector3 Position => _citizenPresenter?.Position ?? Vector3.zero;
	public Vector3 Direction => _citizenPresenter?.Transform.Forward ?? Vector3.forward;
	public float MovementSpeed => 3f;
	public bool CanMove => !IsConsumed && IsAlive;
	public bool IsMoving { get; private set; }
	public bool HasReachedDestination { get; private set; }
	
	// Detection properties
	public float DetectionRange => 15f;
	public float DetectionAngle => 60f; 
	public bool IsPlayerDetected { get; private set; }
	public Vector3? LastKnownPlayerPosition { get; private set; }
	public Vector3? CurrentPlayerPosition { get; private set; }
	
	private readonly CitizenPresenterBase _citizenPresenter;
	private readonly IBehaviourTree _behaviourTree;
	private bool _isDisposed;
	
	public CitizenBehaviourAgent(CitizenPresenterBase citizenPresenter)
	{
		_citizenPresenter = citizenPresenter ?? throw new ArgumentNullException(nameof(citizenPresenter));
		_behaviourTree = new CitizenBehaviourTree(this);
		
		UpdateStateFromPresenter();
	}
	
	public void Initialize()
	{
		if (_isDisposed)
			return;
			
		UpdateStateFromPresenter();
	}
	
	public void UpdateBehaviour()
	{
		if (_isDisposed)
			return;
			
		UpdateStateFromPresenter();
		_behaviourTree?.Tick();
	}
	
	public void Reset()
	{
		if (_isDisposed)
			return;
			
		IsPlayerDetected = false;
		LastKnownPlayerPosition = null;
		CurrentPlayerPosition = null;
		HasReachedDestination = false;
		IsMoving = false;
		
		SetIdleState();
	}
	
	private void UpdateStateFromPresenter()
	{
		if (_citizenPresenter != null)
		{
			CurrentState = _citizenPresenter.CurrentState;
			IsMoving = _citizenPresenter.IsActive;
		}
	}
	
	#region Movement Methods
	
	public void MoveTo(Vector3 destination)
	{
		if (!CanMove)
			return;
		
		HasReachedDestination = false;
		// Movement will be handled by the existing wander system for now
	}
	
	public void StopMovement()
	{
		HasReachedDestination = true;
		IsMoving = false;
	}
	
	public Vector3 GetRandomWanderPoint(float radius)
	{
		var currentPosition = Position;
		
		for (var attempts = 0; attempts < 5; attempts++)
		{
			var randomDirection = Random.insideUnitSphere * radius;
			randomDirection += currentPosition;
			randomDirection.y = currentPosition.y;
			
			if (NavMesh.SamplePosition(randomDirection, out var hit, radius, NavMesh.AllAreas))
			{
				return hit.position;
			}
		}
		
		return currentPosition;
	}
	
	#endregion
	
	#region Detection Methods
	
	public bool DetectPlayer()
	{
		return IsPlayerDetected;
	}
	
	public Vector3 GetFleeDirection(Vector3 playerPosition, float fleeDistance)
	{
		var fleeDirection = (Position - playerPosition).normalized;
		return Position + fleeDirection * fleeDistance;
	}
	
	public void UpdatePlayerDetection(Vector3 playerPosition)
	{
		IsPlayerDetected = true;
		CurrentPlayerPosition = playerPosition;
		LastKnownPlayerPosition = playerPosition;
	}
	
	public void ClearPlayerDetection()
	{
		IsPlayerDetected = false;
		CurrentPlayerPosition = null;
	}
	
	#endregion
	
	#region State Methods
	
	public void SetIdleState()
	{
		_citizenPresenter?.SetIdleState();
	}
	
	public void StartFleeing(Vector3 fleeTarget)
	{
		_citizenPresenter?.StartFleeing(fleeTarget);
	}
	
	public void StopFleeing()
	{
		_citizenPresenter?.StopFleeing();
	}
	
	public void StartBeingFedOn()
	{
		_citizenPresenter?.StartBeingFedOn();
	}
	
	public void StopBeingFedOn()
	{
		_citizenPresenter?.StopBeingFedOn();
	}
	
	public void Kill()
	{
		_citizenPresenter?.Kill();
	}
	
	public void MarkAsConsumed()
	{
		_citizenPresenter?.Consume();
	}
	
	#endregion
	
	public void Dispose()
	{
		if (_isDisposed)
			return;
			
		_behaviourTree?.Dispose();
		_isDisposed = true;
	}
}
}
