using Azzazelloqq.DetectionService.Source;
using MVP;
using Runtime.Core.Infrastructure.TransformUtils;
using UnityEngine;

namespace Runtime.Gameplay.Characters.Player.Base
{
public abstract class PlayerViewBase : ViewMonoBehaviour<PlayerPresenterBase>, IDetectable
{
	public abstract Transform VisualRoot { get; }
	public abstract ReadOnlyTransform Transform { get; protected set; }
	public abstract Vector3 Position { get; }
	public abstract bool IsDead { get; }

	public abstract void SetParent(Transform parent);
	public abstract void UpdatePosition(Vector3 position, float deltaTime);
	public abstract void UpdateRotation(Vector3 direction);
	public abstract void UpdateMovementState(bool isMoving);
	public abstract void SetActive(bool isActive);
	public abstract void SetPosition(Vector3 position);
}
}

