using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBehaviour : NetworkActions
{
	protected NavMeshAgent agent;
	//TODO: Free this hook somehow
	void Awake()
	{
		agent = GetComponent<NavMeshAgent>();
		InvokeRepeating(nameof(InfrequentUpdate), 0, 1);
	}
	protected virtual void InfrequentUpdate()
	{
		if (isStill())
			agent.SetDestination(GetRandomPointOnNavMesh());
	}

    public static Vector3 GetRandomPointOnNavMesh()
    {
		Vector3 center = Vector3.zero;
		float maxDistance = float.MaxValue-9999;
        Vector3 randomDirection = Random.insideUnitSphere * maxDistance;
        randomDirection += center;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, maxDistance, NavMesh.AllAreas))
            return hit.position;
        return center;
    }
	protected void Vent()
	{
		if (!agent.isOnOffMeshLink) return;
		var cache = agent.pathEndPosition;
		//TODO: fuck in the ass creator of unitys documentation
		//everything is either depricated or described with a single sentance
		//why the fuck is link data still called offmesh if its depricated?????
		var area = agent.currentOffMeshLinkData.owner.GetComponent<NavMeshLink>().area;
		if (area != NavMesh.GetAreaFromName("Vent")) return;
		agent.Warp(agent.currentOffMeshLinkData.endPos);
		agent.SetDestination(cache);
	}
	// TODO: add some kind of invoke repeating
	protected Transform getClosestPlayer()
	{
		Transform t = PlayableBehavior.Players[0].transform;
		float maxDist = float.MaxValue;
		foreach (var p in PlayableBehavior.Players)
		{
			float dist = Vector3.Distance(transform.position, p.transform.position);
			if (dist < maxDist)
			{
				maxDist = dist;
				t = p.transform;
			}
		}
		return t;
	}
	protected bool CanSee(Vector3 lookAt)
	{
		Vector3 direction = (transform.position - lookAt).normalized;
		float distance = Vector3.Distance(lookAt, transform.position);

		var isHit = Physics.Raycast(
			lookAt,
			direction,
			out _,
			distance,
			~(1 << LayerMask.NameToLayer("Ignore Raycast")),
			QueryTriggerInteraction.Ignore
		);
		return !isHit;
	}
	protected Transform getClosestObserved()
	{
		Transform t = null;
		float maxDist = float.MaxValue;
		foreach (var p in PlayableBehavior.Players)
		{
			if (CanSee(p.transform.position)) continue;
			float dist = Vector3.Distance(transform.position, p.transform.position);
			if (dist < maxDist)
			{
				maxDist = dist;
				t = p.transform;
			}
		}
		return t;
	}
	protected bool isStill()
	{
		return agent.remainingDistance <= agent.stoppingDistance && agent.velocity.sqrMagnitude == 0f;
	}
}
