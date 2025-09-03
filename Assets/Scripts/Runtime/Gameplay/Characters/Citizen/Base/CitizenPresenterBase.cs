using MVP;
using Runtime.Core.Infrastructure.TransformUtils;
using Runtime.Gameplay.AI.Citizen;
using Runtime.Gameplay.Characters.Base;
using UnityEngine;

namespace Runtime.Gameplay.Characters.Citizen.Base
{
public abstract class CitizenPresenterBase : Presenter<CitizenViewBase, CitizenModelBase>, ICharacter
{
	public abstract PersonState CurrentState { get; }
	public abstract bool CanBeConsumed { get; }
	public abstract bool IdDead { get; }
	public abstract Vector3 Position { get; }
	public abstract ReadOnlyTransform Transform { get; }
	public abstract bool IsActive { get; }

	protected CitizenPresenterBase(CitizenViewBase view, CitizenModelBase model) : base(view, model)
	{
	}

	public abstract void InitializePosition(Vector3 position);
	public abstract void Enable();
	public abstract void Disable();
	public abstract void UpdateParent(Transform parent);
	public abstract void StartFleeing(Vector3 fleeTarget);
	public abstract void StopFleeing();
	public abstract void Consume();
	public abstract void SetIdleState();
	public abstract void OnPlayerDetected(Vector3 playerPosition);
	public abstract void StartBeingFedOn();
	public abstract void StopBeingFedOn();
	public abstract void Kill();
}
}