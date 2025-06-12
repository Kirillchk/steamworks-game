using UnityEngine;

public class RoomDoor : MonoBehaviour
{
	[SerializeField] GameObject doorPref;
	void Start() => RoomGenerator.Doors.Add(gameObject);
	public void Close()
	{
		Instantiate(doorPref, transform.position, transform.rotation)
			.transform.SetParent(transform.parent);
		Destroy(gameObject);
	}
}
