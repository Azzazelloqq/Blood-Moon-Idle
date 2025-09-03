namespace Runtime.Gameplay.AI.Citizen
{
/// <summary>
/// States that a person/citizen can be in
/// Used by AI system for behavior tree decisions
/// </summary>
public enum PersonState
{
	Idle,
	Fleeing,
	Consumed,
	BeingFedOn,
	Dying,
	Dead
}
}
