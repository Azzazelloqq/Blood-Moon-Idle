using BehaviourTree.Source;
using Runtime.Gameplay.AI.Agents;

namespace Runtime.Gameplay.AI.Nodes
{
/// <summary>
/// Condition node that detects if a player is nearby
/// </summary>
public class DetectPlayerNode : IBehaviourTreeNode
{
	private readonly ICitizenBehaviourAgent _agent;
	
	public DetectPlayerNode(ICitizenBehaviourAgent agent)
	{
		_agent = agent;
	}
	
	public NodeState Tick()
	{
		bool playerDetected = _agent.DetectPlayer();
		return playerDetected ? NodeState.Success : NodeState.Failure;
	}
	
	public void Dispose()
	{
		// No cleanup needed
	}
}
}
