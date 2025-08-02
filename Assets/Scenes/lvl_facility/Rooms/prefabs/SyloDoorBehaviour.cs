using UnityEngine;

public class SyloDoorBehaviour : MonoBehaviour
{
	public GameObject DoorRef;
	void OnTriggerEnter(Collider other) =>
		DoorRef.transform.localPosition = new(.875f, 3, -1);
}
