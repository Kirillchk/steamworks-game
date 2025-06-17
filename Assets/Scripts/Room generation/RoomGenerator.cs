using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
public class RoomGenerator : MonoBehaviour
{
	private System.Random rng;
	[SerializeField] List<GameObject> Rooms = new();
	GameObject getRandomRoom() => Rooms[rng.Next(Rooms.Count)];
	public static List<GameObject> Doors = new();
	GameObject getRandomDoor() => Doors[rng.Next(Doors.Count)];
	List<BoxCollider> RoomColliders = new();
	bool checkColisions(BoxCollider comp)
	{
		Physics.SyncTransforms();
		foreach (var col in RoomColliders)
			if (comp.bounds.Intersects(col.bounds))
				return true;
		return false;
	}
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
		GameObject firstdoorObject = getRandomDoor();
		firstDoorPosition = firstdoorObject.transform.position;
		GameObject newRoom = Instantiate(getRandomRoom(), firstDoorPosition, new Quaternion());
		Doors.Remove(firstdoorObject);
		Destroy(firstdoorObject);
		// Gets random door of new room and destroys it
		GameObject secondDoorObject = newRoom.GetComponent<RoomBehaviour>().GetRadomDoor(rng);
		Doors.Remove(secondDoorObject);
		Destroy(secondDoorObject);
		//smashes doors together
		Vector3 offset = secondDoorObject.transform.position - firstDoorPosition;
		newRoom.transform.position -= offset;
		// Applies calculated rotation angle to align selected doors
		float angle = Vector3.SignedAngle(
				SnapToCardinal(offset),
				SnapToCardinal(firstdoorObject.transform.localPosition) * -1, Vector3.up
			) - firstdoorObject.transform.parent.rotation.eulerAngles.y;

		newRoom.transform.RotateAround(firstDoorPosition, Vector3.up, angle);
		// Check for overlaping
		var roomCollider = newRoom.GetComponent<BoxCollider>();
		bool intersects = checkColisions(roomCollider);
		if (intersects) {
			firstdoorObject.GetComponent<RoomDoor>().Close();
			Destroy(newRoom);
		}
		else
			RoomColliders.Add(roomCollider);
		return !intersects;
	}
	async void Start()
	{
		rng = new(2);
		RoomColliders.Add(
			Instantiate(Rooms[0], new Vector3(), new Quaternion()).GetComponent<BoxCollider>()
		);
		await Task.Delay(10);
		while (ind < 6)
		{
			var res = spawnRoom();
			Debug.Log($"sucsess: {res} ind: {ind}");
			if (res)
				ind++;
			// TODO: FIX THIS for faster scene loading
			// bullshit solution for rooms overlaping if there is no delay
			await Task.Delay(10);
		}
		foreach (GameObject door in Doors) 
			door.GetComponent<RoomDoor>().Close();
		
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
