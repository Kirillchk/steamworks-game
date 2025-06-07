using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
	private System.Random rng;
	[SerializeField] List<GameObject> Rooms = new();
	List<BoxCollider> RoomColliders = new();
	GameObject newRoom;
	Vector3 firstDoorPosition;
	int ind = 0;
	[ContextMenu("SpawnRooms")]
	void TryAddRoom()
	{
		var res = spawnRoom();
		Debug.Log($"sucsess: {res} ind: {ind}");
		if (res)
			ind++;
	}
	bool spawnRoom()
	{
		// Selects random door inits new room and then destroys selected door
		var doors = GameObject.FindGameObjectsWithTag("Door");
		GameObject firstdoorObject = doors[rng.Next(0, doors.Length)];
		firstDoorPosition = firstdoorObject.transform.position;
		newRoom = Instantiate(Rooms[rng.Next(0, 2)], firstDoorPosition, new Quaternion());
		Destroy(firstdoorObject);
		// Gets random door of new room and destroys it
		GameObject[] childrenWithTag = newRoom.GetComponentsInChildren<Transform>(true)
			.Where(child => child.CompareTag("Door"))
			.Select(child => child.gameObject)
			.ToArray();
		GameObject secondDoorObject = childrenWithTag[rng.Next(0, childrenWithTag.Length)];
		Destroy(secondDoorObject);
		//smashes doors together
		Vector3 secondDoorPosition = secondDoorObject.transform.position;
		Vector3 offset = secondDoorPosition - firstDoorPosition;
		newRoom.transform.position -= offset;
		// Applies calculated rotation angle to align selected doors
		float angle =
			Vector3.SignedAngle(SnapToCardinal(offset), SnapToCardinal(firstdoorObject.transform.localPosition) * -1, Vector3.up)
			- firstdoorObject.transform.parent.rotation.eulerAngles.y;

		Debug.DrawLine(new(), offset, Color.red, 10);
		Debug.DrawLine(new(0, 1), firstdoorObject.transform.localPosition * -1, Color.green, 10);

		Debug.Log(angle + $" vars: ofs:{offset} dorobj:{firstdoorObject.transform.localPosition * -1}");
		newRoom.transform.RotateAround(firstDoorPosition, Vector3.up, angle);

		// Check for overlaping
		// TODO: orgonize maybe
		var comp = newRoom.GetComponent<BoxCollider>();
		Physics.SyncTransforms();
		bool flag = false;
		foreach (var col in RoomColliders) {
			if (comp.bounds.Intersects(col.bounds))
			{
				Debug.Log($"{comp.bounds} intersects {col.bounds}");
				flag = true;
				break;
			}
		}
		if (flag) {
			Destroy(newRoom);
			return false;
		}
		else {
			RoomColliders.Add(comp);
			Debug.Log($"{comp.bounds} added to list Room {newRoom.transform.position}");
			return true;
		}
	}
	async void Start()
	{
		rng = new(2);
		RoomColliders.Add(
			Instantiate(Rooms[0], new Vector3(), new Quaternion()).GetComponent<BoxCollider>()
		);
		//while (ind < 8)
		//{
		//	var res = spawnRoom();
		//	Debug.Log($"sucsess: {res} ind: {ind}");
		//	if (res)
		//		ind++;
		//	// TODO: FIX THIS for faster scene loading
		//	// bullshit solution for rooms overlaping if there is no delay
		//	await Task.Delay(100); 
		//}
	}
	Vector3 SnapToCardinal(Vector3 input)
	{
		Vector3 rounded = new Vector3(
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
