using UnityEngine;

public class RoomDoor : MonoBehaviour
{
	[SerializeField] GameObject doorPref;
	void Start() => RoomGenerator.Doors.Add(gameObject);
	public void Close()
	{
		GameObject newDoor = Instantiate(doorPref,
			transform.position + doorPref.transform.position,
			new()
		);
		Debug.Log($"{transform.localPosition}, {SnapToCardinal(transform.localPosition)}");
		newDoor.transform.RotateAround(transform.position,
		Vector3.up,
		Vector3.SignedAngle(Vector3.left, SnapToCardinal(transform.localPosition), Vector3.up)
		+ transform.parent.eulerAngles.y);
		newDoor.transform.SetParent(transform.parent);
		Destroy(gameObject);
	}
	Vector3 SnapToCardinal(Vector3 input)
	{
		Vector3 rounded = new (
			Mathf.Round(input.x),
			Mathf.Round(input.y),
			Mathf.Round(input.z)
		);

		// Find the axis with the largest absolute value
		float max = Mathf.Max(
			Mathf.Abs(rounded.x),
			Mathf.Abs(rounded.y),
			Mathf.Abs(rounded.z)
		);

		// Zero out non-dominant axes
		if (Mathf.Abs(rounded.x) != max) rounded.x = 0;
		if (Mathf.Abs(rounded.y) != max) rounded.y = 0;
		if (Mathf.Abs(rounded.z) != max) rounded.z = 0;

		return rounded.normalized;
	}
}
