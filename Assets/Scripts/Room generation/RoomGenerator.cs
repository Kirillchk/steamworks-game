using System;
using System.Collections.Generic;
using System.Linq;
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
	[ContextMenu("SpawnRooms")]
	void TryAddRoom(){
		Debug.Log(spawnRoom()?"Room spawned":"Room fucked up");
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
		var comp = newRoom.GetComponent<BoxCollider>();
		Physics.SyncTransforms();
		bool flag = false;
		foreach (var col in RoomColliders) {
			if (comp.bounds.Intersects(col.bounds))
			{
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
			return true;
		}
	}
	void Start()
	{
		rng = new(1);
		Instantiate(Rooms[0], new Vector3(), new Quaternion());
		for (int i = 0; i<8; ) {
			if (spawnRoom())
				i++;
			
		}
	}
}
