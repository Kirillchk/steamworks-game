using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBehaviour : NetworkActions
{
	public Transform Target;
	protected NavMeshAgent agent;
	//TODO: Free this hook somehow
	void Awake()
	{
		agent = GetComponent<NavMeshAgent>();
	}
	protected void Vent()
	{
		if (!agent.isOnOffMeshLink) return;
		//TODO: fuck in the ass creator of unitys documentation
		//everything is either depricated or described with a single sentance
		//why the fuck is link data still called offmesh if its depricated?????
		var area = agent.currentOffMeshLinkData.owner.GetComponent<NavMeshLink>().area;
		if (area != NavMesh.GetAreaFromName("Vent")) return;
		agent.Warp(agent.currentOffMeshLinkData.endPos);
		agent.SetDestination(Target.position);
	}
	// TODO: add some kind of invoke repeating
	protected Transform getClosestPlayer()
	{
		Transform t = PlayableBehavior.Players[0].transform;
		float maxDist = float.MaxValue;
		foreach (var p in PlayableBehavior.Players)
		{
			float dist = Vector3.Distance(transform.position, p.transform.position);
			if (maxDist < dist)
			{
				maxDist = dist;
				t = p.transform;
			}
		}
		return t;
	}
}
