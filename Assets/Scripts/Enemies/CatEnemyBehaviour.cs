using System;
using UnityEngine;

public class CatEnemyBehaviour : EnemyBehaviour
{
	float startleTimer = 0;
	Renderer rend;
	void Start()
	{
		rend = GetComponent<Renderer>();
	}
	void FixedUpdate()
	{
		startleTimer -= .02f;
		if (rend.isVisible)
			this.Sync(Startle);
		if (startleTimer > 0)
			return;
		agent.isStopped = false;
		Vent();
		Target = getClosestPlayer();
		agent.SetDestination(Target.position);
	}
	[CanTriggerSync]
	public void Startle()
	{
		startleTimer = .06f;
		agent.velocity = Vector3.zero;
		agent.isStopped = true;
	}
}
