using System;
using UnityEngine;

public class CatEnemyBehaviour : EnemyBehaviour
{
	float startleTimer = 0;
	Renderer rend;
	LayerMask layerMask;
	void Start()
	{
		rend = GetComponent<Renderer>();
		layerMask = ~(
			(1 << LayerMask.NameToLayer("Ignore Raycast")) |
			(1 << LayerMask.NameToLayer("LVL")) |
			(1 << LayerMask.NameToLayer("Movable")));
	}
	void FixedUpdate()
	{
		startleTimer -= .02f;
		if (rend.isVisible && !(rend.isVisible && !isObserved()))
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
	bool isObserved()
	{
		GameObject player = Camera.main.gameObject; 

		Vector3 direction = (transform.position - player.transform.position).normalized;
		float distance = Vector3.Distance(player.transform.position, transform.position);

		var isHit = Physics.Raycast(player.transform.position, direction, out _, distance, layerMask);
		return !isHit;
	}
}
