using UnityEngine;

public class RightHandBehaviour : HandsBehaviour
{
	void Start()
	{
		HandRotation = Quaternion.Euler(0,90,0);
		PickButton = KeyCode.E;
		DragButton = 1;
	}
}
