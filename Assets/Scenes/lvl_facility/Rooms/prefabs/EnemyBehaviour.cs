using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBehaviour : MonoBehaviour
{
	public Transform Target;
	NavMeshAgent agent => GetComponent<NavMeshAgent>();
	[ContextMenu("Activate")]
	public void ActivateEnemy()
	{
		agent.SetDestination(Target.position);
	}
	void Update()
	{
		if (!agent.isOnOffMeshLink)
			return;
		agent.Warp(agent.currentOffMeshLinkData.endPos);
		agent.CompleteOffMeshLink();
		agent.SetDestination(Target.position);
		agent.isStopped = false;
	}
}
