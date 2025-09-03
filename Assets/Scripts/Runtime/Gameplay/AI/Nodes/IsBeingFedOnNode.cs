using BehaviourTree.Source;
using Runtime.Gameplay.AI.Agents;

namespace Runtime.Gameplay.AI.Nodes
{
/// <summary>
/// Condition node that checks if the citizen is being fed on by the player
/// </summary>
public class IsBeingFedOnNode : IBehaviourTreeNode
{
	private readonly ICitizenBehaviourAgent _agent;
	
	public IsBeingFedOnNode(ICitizenBehaviourAgent agent)
	{
		_agent = agent;
	}
	
	public NodeState Tick()
	{
		return _agent.CurrentState == AI.Citizen.PersonState.BeingFedOn ? NodeState.Success : NodeState.Failure;
	}
	
	public void Dispose()
	{
		// No cleanup needed
	}
}
}
