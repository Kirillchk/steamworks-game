using UnityEngine;

public class RoomDoor : MonoBehaviour
{
	[SerializeField] GameObject openDoorPref;
	[SerializeField] GameObject closedDoorPref;
	void Start() =>
		RoomGenerator.Doors.Add(gameObject);
	private void createDoor(GameObject doorPref)
	{
		GameObject newDoor = Instantiate(
			doorPref,
			transform.position + doorPref.transform.position,
			new()
		);

		// Debug.Log($"{transform.localPosition}, {SnapVector(transform.localPosition)}");
		
		newDoor.transform.RotateAround(
			transform.position,
			Vector3.up,
			Vector3.SignedAngle(Vector3.left, SnapVector(transform.localPosition), Vector3.up)
			+ transform.parent.eulerAngles.y
		);

		newDoor.transform.SetParent(transform.parent);
		transform.parent.GetComponent<RoomBehaviour>().roomDoors.Remove(gameObject);
		RoomGenerator.Doors.Remove(gameObject);
		Destroy(gameObject);
	}
	public void Close() =>
		createDoor(closedDoorPref);
	public void Open() =>
		createDoor(openDoorPref);
	public Vector3 GetVector1() =>
		SnapVector(transform.position - transform.parent.position);
	public Vector3 GetVector2() =>
		SnapVector(transform.localPosition) * -1;
	Vector3 SnapVector(Vector3 input)
	{
		Vector3 rounded = new(
			Mathf.Round(input.x), 0, // ignoring Y
			Mathf.Round(input.z)
		);

		// Find the axis with the largest absolute value
		float max = Mathf.Max(
			Mathf.Abs(rounded.x),
			Mathf.Abs(rounded.z)
		);

		// Zero out non-dominant axes
		if (Mathf.Abs(rounded.x) != max) rounded.x = 0;
		if (Mathf.Abs(rounded.z) != max) rounded.z = 0;

		return rounded.normalized;
	}
}
