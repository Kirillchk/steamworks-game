using UnityEngine;

public class LeftHandBehaviour : HandsBehaviour
{
	void Start()
	{
		HandRotation = Quaternion.Euler(0, -90, 0);
		PickButton = KeyCode.Q;
		DragButton = 0;
	}
}
