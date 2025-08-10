using UnityEngine;
public class SiluetBehaviour : EnemyBehaviour
{
	bool isObserved = false;
	GameObject player; 
	LayerMask layerMask;
	void Update()
	{
		if (isObserved)
		{
			//player.transform.LookAt(transform.position);
			Camera.main.transform.parent.transform.LookAt(transform.position);
			// TODO: Fix by modifying mouseX instead or smth like that 
			Vector3 currentRotation = Camera.main.transform.eulerAngles;
			Vector3 newRotation = new Vector3(0f, currentRotation.y, currentRotation.z);
			Camera.main.transform.eulerAngles = newRotation;
		}
		Target = getClosestPlayer();
		this.Sync(Chase, Target.position);
	}
	[CanTriggerSync]
	public void Chase(Vector3 vec)
	{
		Vent();
		agent.SetDestination(vec);
	}
    void OnDestroy() =>
        CancelInvoke(nameof(CheckPlayerLook));

	void Start()
	{
		layerMask = ~(
			(1 << LayerMask.NameToLayer("Ignore Raycast")) |
			(1 << LayerMask.NameToLayer("LVL")) |
			(1 << LayerMask.NameToLayer("Movable")));
		InvokeRepeating(nameof(CheckPlayerLook), 0.5f, 0.5f);
	}
	void CheckPlayerLook()
	{
		player = Camera.main.gameObject;

		Vector3 direction = (transform.position - player.transform.position).normalized;
		float distance = Vector3.Distance(player.transform.position, transform.position);

		var isHit = Physics.Raycast(player.transform.position, direction, out _, distance, layerMask);

		isObserved = !isHit;
	}
}