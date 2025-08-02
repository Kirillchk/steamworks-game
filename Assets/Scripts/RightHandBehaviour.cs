using UnityEngine;

public class RightHandBehaviour : HandsBehaviour
{
	void Start()
	{
		HandRotation = Quaternion.Euler(0,90,0);
	}
    void Update()
    {
		if (Input.GetKeyDown(KeyCode.E) && holding != null)
			Drop();
		// TODO: turn into a helper or smth
		bool res = Physics.Raycast(
			Camera.main.ViewportPointToRay(new (0.5f, 0.5f, 0)),
			out RaycastHit hit,
			10, ~0, QueryTriggerInteraction.Ignore
		);
		if (!res)
			return;
		var target = hit.transform.gameObject;
		if (Input.GetMouseButtonDown(1))
			Grab(target, hit.point);
		if (!Input.GetMouseButton(1))
			Relese();
		if (Input.GetKeyDown(KeyCode.E) && holding == null)
			PickUp(target);
    }
}
