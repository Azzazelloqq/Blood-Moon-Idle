using BehaviourTree.Source;
using Runtime.Gameplay.AI.Agents;
using UnityEngine;

namespace Runtime.Gameplay.AI.Nodes
{
/// <summary>
/// Action node that makes the citizen flee from detected player
/// </summary>
public class StartFleeingNode : IBehaviourTreeNode
{
	private readonly ICitizenBehaviourAgent _agent;
	private const float FleeDistance = 15f;
	
	public StartFleeingNode(ICitizenBehaviourAgent agent)
	{
		_agent = agent;
	}
	
	public NodeState Tick()
	{
		if (_agent.CurrentPlayerPosition.HasValue)
		{
			var fleeTarget = _agent.GetFleeDirection(_agent.CurrentPlayerPosition.Value, FleeDistance);
			_agent.StartFleeing(fleeTarget);
			return NodeState.Success;
		}
		
		return NodeState.Failure;
	}
	
	public void Dispose()
	{
		// No cleanup needed
	}
}
}
