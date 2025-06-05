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
	Vector3 randomDoor;
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
		GameObject doorObject = doors[rng.Next(0, doors.Length)];
		randomDoor = doorObject.transform.position;
		newRoom = Instantiate(Rooms[rng.Next(0, 2)], randomDoor, new Quaternion());
		Destroy(doorObject);
		// Gets random door of new room and destroys it
		GameObject[] childrenWithTag = newRoom.GetComponentsInChildren<Transform>(true)
			.Where(child => child.CompareTag("Door"))
			.Select(child => child.gameObject)
			.ToArray();
		GameObject secondDoorObject = childrenWithTag[rng.Next(0, childrenWithTag.Length)];
		Destroy(secondDoorObject);
		// Applies calculated rotation angle to align selected doors
		Vector3 secondDoor = secondDoorObject.transform.position;
		Vector3 offset = secondDoor - randomDoor;
		newRoom.transform.position -= offset;
		float angle =
			Vector3.SignedAngle(offset, doorObject.transform.localPosition.normalized * -1, Vector3.up)
			- doorObject.transform.parent.rotation.eulerAngles.y;
		newRoom.transform.RotateAround(randomDoor, Vector3.up, angle);
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
		rng = new(1);
		Instantiate(Rooms[0], new Vector3(), new Quaternion());
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
}
