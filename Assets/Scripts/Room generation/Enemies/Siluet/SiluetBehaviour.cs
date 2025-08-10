using UnityEngine;
public class SiluetBehaviour : EnemyBehaviour
{
	void Update()
	{
		if (isObserved())
		{
			//player.transform.LookAt(transform.position);
			Camera.main.transform.parent.transform.LookAt(transform.position);
			// TODO: Fix by modifying mouseX instead or smth like that 
			Vector3 currentRotation = Camera.main.transform.eulerAngles;
			Vector3 newRotation = new Vector3(0f, currentRotation.y, currentRotation.z);
			Camera.main.transform.eulerAngles = newRotation;
		}
		Target = getClosestPlayer();
		Vent();
		agent.SetDestination(Target.position);
	}
}