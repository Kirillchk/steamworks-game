using UnityEngine;

public class RoomDoor : MonoBehaviour
{
	static int autoID = 0;
	[SerializeField] GameObject doorPref;
	void Start() => RoomGenerator.Doors.Add(gameObject);
	public void Close()
	{
		Vector3 from = SnapToCardinal(transform.localPosition);
		Vector3 to = transform.parent.rotation.y==0 ? Vector3.left : Vector3.forward;
		Vector3 normal = Vector3.up;

		//Vector3 offset = new(0, autoID, 0);

		//Debug.DrawLine(offset, from+offset, Color.red, float.MaxValue);
		//Debug.DrawLine(offset, to+offset, Color.green, float.MaxValue);
		//Debug.DrawLine(offset, normal+offset, Color.blue, float.MaxValue);

		float angle = Vector3.SignedAngle(from, to, normal);
		angle *= angle == 90 | angle == -90 ? -1:1;

		GameObject newDoor = Instantiate(doorPref,
			transform.position + doorPref.transform.position,
			new()
		);
		Debug.Log($"{autoID} {angle} {transform.position} {transform.parent.rotation.y}");
		newDoor.transform.RotateAround(transform.position,Vector3.up, angle);
		newDoor.transform.SetParent(transform.parent);
		Destroy(gameObject);
		autoID++;
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
