using UnityEngine;
public class SiluetBehaviour : EnemyBehaviour
{
	void Update()
	{
		Vent();
		if (CanSee(Camera.main.gameObject.transform.position))
		{
			//player.transform.LookAt(transform.position);
			Camera.main.transform.parent.transform.LookAt(transform.position);
			// TODO: Fix by modifying mouseX instead or smth like that 
			Vector3 currentRotation = Camera.main.transform.eulerAngles;
			Vector3 newRotation = new Vector3(0f, currentRotation.y, currentRotation.z);
			Camera.main.transform.eulerAngles = newRotation;

			var plr = getClosestObserved();
			if (plr != null)
				agent.SetDestination(plr.position);
		}
	}
}