using BehaviourTree.Source;
using Runtime.Gameplay.AI.Agents;

namespace Runtime.Gameplay.AI.Nodes
{
/// <summary>
/// Condition node that checks if the citizen is in idle state
/// </summary>
public class IsIdleNode : IBehaviourTreeNode
{
	private readonly ICitizenBehaviourAgent _agent;
	
	public IsIdleNode(ICitizenBehaviourAgent agent)
	{
		_agent = agent;
	}
	
	public NodeState Tick()
	{
		return _agent.CurrentState == AI.Citizen.PersonState.Idle ? NodeState.Success : NodeState.Failure;
	}
	
	public void Dispose()
	{
		// No cleanup needed
	}
}
}
