using MVP;
using UnityEngine;

namespace Runtime.Gameplay.Characters.Player.Base
{
public abstract class PlayerModelBase : Model
{
	public abstract Vector3 Position { get; protected set; }
	public abstract Vector3 Direction { get; protected set; }
	public abstract float MovementSpeed { get; protected set; }
	public abstract int CurrentLevel { get; protected set; }
	public abstract bool IsEnable { get; protected set; }
	public abstract bool IsDead { get; protected set; }

	public abstract bool CanMove();
	public abstract void Enable();
	public abstract void Disable();
	public abstract void LevelUp();
	public abstract void SetDirection(Vector3 direction);
	public abstract void ProcessMovement(float deltaTime);
	public abstract void InitializePosition(Vector3 position);
}
}

