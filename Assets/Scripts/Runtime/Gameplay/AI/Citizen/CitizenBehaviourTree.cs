using BehaviourTree.Source;
using BehaviourTree.Source.Nodes;
using Runtime.Gameplay.AI.Agents;
using Runtime.Gameplay.AI.Nodes;

namespace Runtime.Gameplay.AI.Citizen
{
/// <summary>
/// Behavior tree implementation for Citizen AI
/// Manages the decision-making process through a hierarchical behavior tree
/// </summary>
public class CitizenBehaviourTree : IBehaviourTree
{
	private readonly ICitizenBehaviourAgent _agent;
	private IBehaviourTreeNode _rootNode;
	private bool _isDisposed;
	
	public CitizenBehaviourTree(ICitizenBehaviourAgent agent)
	{
		_agent = agent;
		BuildBehaviourTree();
	}
	
	public void Tick()
	{
		if (_isDisposed || _rootNode == null)
			return;
			
		_rootNode.Tick();
	}
	
	/// <summary>
	/// Builds the behavior tree for citizen AI
	/// Structure: Selector (priority-based decision making)
	/// 1. Emergency (being consumed/dying)
	/// 2. Flee from player 
	/// 3. Wander around
	/// </summary>
	private void BuildBehaviourTree()
	{
		_rootNode = new SelectorNode(new IBehaviourTreeNode[]
		{
			// Priority 1: Handle emergency states (being consumed, dying)
			BuildEmergencyBranch(),
			
			// Priority 2: Flee if player detected  
			BuildFleeingBranch(),
			
			// Priority 3: Default wandering behavior
			BuildWanderingBranch()
		});
	}
	
	private IBehaviourTreeNode BuildEmergencyBranch()
	{
		return new SequenceNode(new IBehaviourTreeNode[]
		{
			new IsBeingFedOnNode(_agent),
			new StopMovementNode(_agent)
		});
	}
	
	private IBehaviourTreeNode BuildFleeingBranch()
	{
		return new SequenceNode(new IBehaviourTreeNode[]
		{
			new DetectPlayerNode(_agent),
			new StartFleeingNode(_agent)
		});
	}
	
	private IBehaviourTreeNode BuildWanderingBranch()
	{
		return new SequenceNode(new IBehaviourTreeNode[]
		{
			new IsIdleNode(_agent),
			new WanderNode(_agent)
		});
	}
	
	public void Dispose()
	{
		if (_isDisposed)
			return;
			
		_rootNode?.Dispose();
		_rootNode = null;
		_isDisposed = true;
	}
}
}
