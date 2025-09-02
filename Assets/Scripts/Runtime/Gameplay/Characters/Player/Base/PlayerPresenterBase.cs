using MVP;
using Runtime.Core.Infrastructure.TransformUtils;
using UnityEngine;

namespace Runtime.Gameplay.Characters.Player.Base
{
public abstract class PlayerPresenterBase : Presenter<PlayerViewBase, PlayerModelBase>
{
	public abstract ReadOnlyTransform CharacterTransform { get; }
	public abstract Vector3 Position { get; }

	protected PlayerPresenterBase(PlayerViewBase view, PlayerModelBase model) : base(view, model)
	{
	}

	public abstract void InitializePosition(Vector3 position);
	public abstract void Enable();
	public abstract void Disable();
	public abstract void UpdateParent(Transform parent);
	public abstract void OnTriggerEnter(Collider other);
}
}