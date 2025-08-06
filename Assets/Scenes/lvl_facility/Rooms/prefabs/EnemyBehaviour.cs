using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : MonoBehaviour
{
	NavMeshAgent agent => GetComponent<NavMeshAgent>();
	void Start()
	{
		agent.SetDestination(new Vector3(0, 0, 0));
	}
	void Update()
    {
        
    }
}
