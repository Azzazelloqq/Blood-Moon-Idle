namespace Runtime.Gameplay.Characters.Player
{
public interface IKillableByPlayer
{
	public void StartKilling();
	public void StopKilling();
	public void Kill();
}
}