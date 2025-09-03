using BehaviourTree.Source;
using Runtime.Gameplay.AI.Agents;
using UnityEngine;

namespace Runtime.Gameplay.AI.Nodes
{
/// <summary>
/// Action node that makes the citizen wander around randomly
/// </summary>
public class WanderNode : IBehaviourTreeNode
{
	private readonly ICitizenBehaviourAgent _agent;
	private float _nextWanderTime;
	private const float WanderInterval = 3f;
	private const float WanderRadius = 10f;
	
	public WanderNode(ICitizenBehaviourAgent agent)
	{
		_agent = agent;
		_nextWanderTime = Time.time + Random.Range(0f, WanderInterval);
	}
	
	public NodeState Tick()
	{
		// Check if it's time to wander
		if (Time.time >= _nextWanderTime)
		{
			var wanderPoint = _agent.GetRandomWanderPoint(WanderRadius);
			_agent.MoveTo(wanderPoint);
			
			_nextWanderTime = Time.time + WanderInterval + Random.Range(0f, WanderInterval * 0.5f);
			return NodeState.Success;
		}
		
		return NodeState.Running;
	}
	
	public void Dispose()
	{
		// No cleanup needed
	}
}
}
