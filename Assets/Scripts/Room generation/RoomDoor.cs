using System.Collections.Generic;
using UnityEngine;

public class RoomDoor : MonoBehaviour
{
	public string DoorType = "";
	public Dictionary<GameObject, List<GameObject>> RoomBanDict = new();
	// Absolute vector
	public Vector3 VectorA => SnapVector(helper.transform.position - transform.parent.position);
	GameObject helper;
	void Start()
	{
		helper = new("Helper");
		helper.transform.parent = transform.parent;
		helper.transform.localPosition = -VectorB;
	}
	void OnDestroy() => 
		Destroy(helper);
	// Local vector
	public Vector3 VectorB;
	[SerializeField] GameObject openDoorPref;
	[SerializeField] GameObject closedDoorPref;
	void createDoor(GameObject doorPref)
	{
		GameObject newDoor = Instantiate(
			doorPref,
			transform.position + doorPref.transform.position,
			new()
		);

		newDoor.transform.RotateAround(
			transform.position,
			Vector3.up,
			Vector3.SignedAngle(
				Vector3.left,
				VectorB*-1,
				Vector3.up
			) + transform.parent.eulerAngles.y
		);

		newDoor.transform.SetParent(transform.parent);
		Destroy(gameObject);
	}
	public void Close() =>
		createDoor(closedDoorPref);
	public void Open() =>
		createDoor(openDoorPref);
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
	[ExecuteInEditMode, ContextMenu("CalcVectorB")]
	public Vector3 CalcVectorB()
	{
		Debug.Log($"VectorB for this door is {SnapVector(transform.localPosition) * -1}");
		return SnapVector(transform.localPosition) * -1;
	}
}
