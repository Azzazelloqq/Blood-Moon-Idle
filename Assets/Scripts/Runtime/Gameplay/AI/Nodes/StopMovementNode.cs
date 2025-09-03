using BehaviourTree.Source;
using Runtime.Gameplay.AI.Agents;

namespace Runtime.Gameplay.AI.Nodes
{
/// <summary>
/// Action node that stops the citizen's movement
/// </summary>
public class StopMovementNode : IBehaviourTreeNode
{
	private readonly ICitizenBehaviourAgent _agent;
	
	public StopMovementNode(ICitizenBehaviourAgent agent)
	{
		_agent = agent;
	}
	
	public NodeState Tick()
	{
		_agent.StopMovement();
		return NodeState.Success;
	}
	
	public void Dispose()
	{
		// No cleanup needed
	}
}
}
