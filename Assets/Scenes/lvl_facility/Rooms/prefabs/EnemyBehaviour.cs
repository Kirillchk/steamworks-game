using Unity.AI.Navigation;
using Unity.VisualScripting;
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
	void FixedUpdate()
	{
		if (agent.isOnOffMeshLink) Vent();
	}
	void Vent()
	{
		//TODO: fuck in the ass creator of unitys documentation
		//everything is either depricated or described with a single sentance
		//why the fuck is link data still called offmesh if its depricated?????
		var area = agent.currentOffMeshLinkData.owner.GetComponent<NavMeshLink>().area;
		if (area!= NavMesh.GetAreaFromName("Vent")) return;
		agent.Warp(agent.currentOffMeshLinkData.endPos);
		agent.SetDestination(Target.position);
	}
}
