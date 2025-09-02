using System;
using System.Threading;
using System.Threading.Tasks;
using Runtime.Core.Infrastructure.TransformUtils;
using Runtime.Gameplay.Characters.Person.Base;
using UnityEngine;
using UnityEngine.AI;

namespace Runtime.Gameplay.Characters.Person
{
public class CitizenView : CitizenViewBase
{
	[SerializeField]
	private Transform _visualRoot;

	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private ParticleSystem _movementParticles;

	[SerializeField]
	private ParticleSystem _consumptionParticles;

	[SerializeField]
	private AudioSource _fleeAudioSource;

	[SerializeField]
	private string _isMovingParameter = "IsMoving";

	[SerializeField]
	private string _isFleeingParameter = "IsFleeing";

	[SerializeField]
	private string _consumeTrigger = "Consume";

	public override Vector3 Position => presenter.Position;
	public override bool IsDead => presenter.IdDead;
	public override float Magnitude { get; }
	public override bool PathPending { get; }
	public override float RemainingDistance { get; }
	public override Transform VisualRoot => _visualRoot != null ? _visualRoot : transform;
	public override ReadOnlyTransform Transform { get; protected set; }

	public NavMeshAgent NavMeshAgent { get; private set; }

	protected override void OnInitialize()
	{
		InitializeComponents();
	}

	protected override ValueTask OnInitializeAsync(CancellationToken token)
	{
		InitializeComponents();
		return default;
	}

	private void InitializeComponents()
	{
		Transform = new ReadOnlyTransform(_visualRoot != null ? _visualRoot : transform);

		// Get or add NavMeshAgent component
		NavMeshAgent = GetComponent<NavMeshAgent>();
		if (NavMeshAgent == null)
		{
			NavMeshAgent = gameObject.AddComponent<NavMeshAgent>();
		}

		// Configure NavMeshAgent defaults
		NavMeshAgent.speed = 3f;
		NavMeshAgent.acceleration = 8f;
		NavMeshAgent.angularSpeed = 200f;
		NavMeshAgent.stoppingDistance = 0.5f;
		NavMeshAgent.radius = 0.5f;
		NavMeshAgent.height = 2f;
		NavMeshAgent.autoRepath = true;
	}

	protected override void OnDispose()
	{
		// Stop any playing effects
		if (_movementParticles != null && _movementParticles.isPlaying)
		{
			_movementParticles.Stop();
		}

		if (_consumptionParticles != null && _consumptionParticles.isPlaying)
		{
			_consumptionParticles.Stop();
		}

		if (_fleeAudioSource != null && _fleeAudioSource.isPlaying)
		{
			_fleeAudioSource.Stop();
		}
	}

	protected override ValueTask OnDisposeAsync(CancellationToken token)
	{
		return default;
	}

	public override void SetParent(Transform parent)
	{
		transform.SetParent(parent);
	}

	public override void UpdatePosition(Vector3 position, float deltaTime)
	{
		// NavMeshAgent controls the position, so we don't need to set it manually
		// Just ensure the NavMeshAgent is at the correct position if needed
		if (NavMeshAgent != null && NavMeshAgent.isOnNavMesh)
		{
			var distanceToTarget = Vector3.Distance(transform.position, position);
			if (distanceToTarget > 2f) // Teleport if too far away
			{
				NavMeshAgent.Warp(position);
			}
		}
		else if (VisualRoot.position != position)
		{
			VisualRoot.position = position;
		}
	}

	public override void UpdateRotation(Vector3 direction)
	{
		// NavMeshAgent handles rotation automatically, but we can override if needed
		if (direction != Vector3.zero && NavMeshAgent != null)
		{
			NavMeshAgent.updateRotation = true;
		}
		else if (direction != Vector3.zero)
		{
			var targetRotation = Quaternion.LookRotation(direction);
			VisualRoot.rotation = Quaternion.Slerp(VisualRoot.rotation, targetRotation, Time.deltaTime * 10f);
		}
	}

	public override void UpdateMovementState(bool isMoving)
	{
		SetAnimationBool(_isMovingParameter, isMoving);

		if (_movementParticles != null)
		{
			if (isMoving && !_movementParticles.isPlaying)
			{
				_movementParticles.Play();
			}
			else if (!isMoving && _movementParticles.isPlaying)
			{
				_movementParticles.Stop();
			}
		}
	}

	public override void SetActive(bool isActive)
	{
		gameObject.SetActive(isActive);

		// Enable/disable NavMeshAgent as well
		if (NavMeshAgent != null)
		{
			NavMeshAgent.enabled = isActive;
		}
	}

	public override void SetPosition(Vector3 position)
	{
		if (NavMeshAgent != null && NavMeshAgent.isOnNavMesh)
		{
			NavMeshAgent.Warp(position);
		}
		else
		{
			transform.position = position;

			if (_visualRoot != null)
			{
				_visualRoot.position = position;
			}
		}
	}

	public override void OnStateChanged(PersonState newState)
	{
		switch (newState)
		{
			case PersonState.Idle:
				StartIdle();
				break;
			case PersonState.Fleeing:
				StartFleeing();
				break;
			case PersonState.Consumed:
				StartConsumption();
				break;
			case PersonState.BeingFedOn:
				StartBeingFedOn();
				break;
			case PersonState.Dying:
				StartDying();
				break;
			case PersonState.Dead:
				OnDead();
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
		}
	}

	public override void SetMoveSpeed(float movementSpeed)
	{
		NavMeshAgent.speed = movementSpeed;
	}

	public override void SetStoppingDistance(float stopppingDistance)
	{
		NavMeshAgent.stoppingDistance = stopppingDistance;
	}

	public override void SetTargetDestination(Vector3 fleeTarget)
	{
		NavMeshAgent.SetDestination(fleeTarget);
	}

	public override void StartKilling()
	{
		presenter.StartBeingFedOn();
	}

	public override void StopKilling()
	{
		presenter.StopBeingFedOn();
	}

	public override void Kill()
	{
		presenter.Kill();
	}

	private void SetAnimationBool(string parameter, bool value)
	{
		if (_animator != null && !string.IsNullOrEmpty(parameter))
		{
			_animator.SetBool(parameter, value);
		}
	}

	private void TriggerAnimation(string trigger)
	{
		if (_animator != null && !string.IsNullOrEmpty(trigger))
		{
			_animator.SetTrigger(trigger);
		}
	}
	
	private void OnDead()
	{
		PlayDeadAnimation();
	}

	private void PlayDeadAnimation()
	{
	}

	private void StartDying()
	{
		PlayDyingAnimation();
	}

	private void PlayDyingAnimation()
	{
		
	}

	private void StartBeingFedOn()
	{
		PlayBeingFedOnAnimation();
	}

	private void StartConsumption()
	{
		PlayConsumptionAnimation();
	}

	private void StartFleeing()
	{
		StartFleeAnimation();
	}

	private void StartIdle()
	{
		PlayIdleAnimation();
	}

	private void StartFleeAnimation()
	{
		SetAnimationBool(_isFleeingParameter, true);

		// Play flee audio
		if (_fleeAudioSource != null && !_fleeAudioSource.isPlaying)
		{
			_fleeAudioSource.Play();
		}
	}

	private void StopFleeAnimation()
	{
		SetAnimationBool(_isFleeingParameter, false);

		// Stop flee audio
		if (_fleeAudioSource != null && _fleeAudioSource.isPlaying)
		{
			_fleeAudioSource.Stop();
		}
	}

	private void PlayConsumptionAnimation()
	{
		TriggerAnimation(_consumeTrigger);

		if (_consumptionParticles != null)
		{
			_consumptionParticles.Play();
		}
	}

	private void PlayIdleAnimation()
	{
		SetAnimationBool(_isMovingParameter, false);
		SetAnimationBool(_isFleeingParameter, false);

		// Stop any audio
		if (_fleeAudioSource != null && _fleeAudioSource.isPlaying)
		{
			_fleeAudioSource.Stop();
		}
	}
	
	private void PlayBeingFedOnAnimation()
	{
		
	}

	#if UNITY_EDITOR
	private void OnDrawGizmosSelected()
	{
		DrawDetectionArea();
	}

	private void DrawDetectionArea()
	{
		if (presenter == null)
		{
			return;
		}

		var citizenPresenter = presenter as CitizenPresenter;
		if (citizenPresenter == null)
		{
			return;
		}

		// Get detection parameters from presenter's detection context
		var detectionDistance = citizenPresenter.GetDetectionDistance();
		var detectionAngle = citizenPresenter.GetDetectionAngle();

		var position = transform.position;
		var forward = transform.forward;

		// Set gizmo colors
		Gizmos.color = Color.yellow;
		UnityEditor.Handles.color = Color.yellow;

		// Draw detection range circle
		UnityEditor.Handles.DrawWireDisc(position, Vector3.up, detectionDistance);

		// Draw detection cone if angle is less than 360 degrees
		if (detectionAngle < 360f)
		{
			var halfAngle = detectionAngle * 0.5f;
			var leftDirection = Quaternion.AngleAxis(-halfAngle, Vector3.up) * forward;
			var rightDirection = Quaternion.AngleAxis(halfAngle, Vector3.up) * forward;

			// Draw cone lines
			Gizmos.DrawLine(position, position + leftDirection * detectionDistance);
			Gizmos.DrawLine(position, position + rightDirection * detectionDistance);

			// Draw arc for the detection cone
			UnityEditor.Handles.DrawWireArc(position, Vector3.up, leftDirection, detectionAngle, detectionDistance);
		}

		// Draw forward direction
		Gizmos.color = Color.red;
		Gizmos.DrawLine(position, position + forward * detectionDistance * 0.5f);
	}
	#endif
}
}