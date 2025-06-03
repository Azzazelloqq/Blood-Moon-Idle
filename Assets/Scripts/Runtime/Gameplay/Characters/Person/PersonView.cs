using System.Threading;
using System.Threading.Tasks;
using Runtime.Core.Infrastructure.TransformUtils;
using Runtime.Gameplay.Characters.Person.Base;
using UnityEngine;
using UnityEngine.AI;

namespace Runtime.Gameplay.Characters.Person
{
public class PersonView : PersonViewBase
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

	protected override ValueTask OnInitializeAsync(CancellationToken token)
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

		return default;
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

	public override void StartFleeAnimation()
	{
		SetAnimationBool(_isFleeingParameter, true);

		// Play flee audio
		if (_fleeAudioSource != null && !_fleeAudioSource.isPlaying)
		{
			_fleeAudioSource.Play();
		}
	}

	public override void StopFleeAnimation()
	{
		SetAnimationBool(_isFleeingParameter, false);

		// Stop flee audio
		if (_fleeAudioSource != null && _fleeAudioSource.isPlaying)
		{
			_fleeAudioSource.Stop();
		}
	}

	public override void PlayConsumptionAnimation()
	{
		TriggerAnimation(_consumeTrigger);

		if (_consumptionParticles != null)
		{
			_consumptionParticles.Play();
		}
	}

	public override void SetIdleAnimation()
	{
		SetAnimationBool(_isMovingParameter, false);
		SetAnimationBool(_isFleeingParameter, false);

		// Stop any audio
		if (_fleeAudioSource != null && _fleeAudioSource.isPlaying)
		{
			_fleeAudioSource.Stop();
		}
	}

	public override void OnStateChanged(PersonState newState)
	{
		switch (newState)
		{
			case PersonState.Idle:
				SetIdleAnimation();
				break;
			case PersonState.Fleeing:
				StartFleeAnimation();
				break;
			case PersonState.Consumed:
				PlayConsumptionAnimation();
				break;
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
}
}