using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    private System.Random rng;
	[SerializeField]List<GameObject> Rooms = new();
	[SerializeField]float f = 180;
	GameObject newRoom;
	Vector3 randomDoor;
	[ContextMenu("SpawnRooms")]
	void spawnRoom(){
		var doors = GameObject.FindGameObjectsWithTag("Door");
		GameObject doorObject = doors[rng.Next(0, doors.Length)];
		randomDoor = doorObject.transform.position;
		newRoom = Instantiate(Rooms[rng.Next(0,2)], randomDoor, new Quaternion());
		Destroy(doorObject);

		GameObject[] childrenWithTag = newRoom.GetComponentsInChildren<Transform>(true)
			.Where(child => child.CompareTag("Door"))
			.Select(child => child.gameObject) // Convert Transform to GameObject
			.ToArray();
		
		GameObject secondDoorObject = childrenWithTag[rng.Next(0,childrenWithTag.Length)];
		Destroy(secondDoorObject);
		Vector3 secondDoor = secondDoorObject.transform.position;
		Vector3 offset = secondDoor - randomDoor;

		Debug.Log("new one: " +
			Vector3.SignedAngle(offset, doorObject.transform.localPosition.normalized * -1, Vector3.up)
			+ "already rotated" + doorObject.transform.parent.rotation.eulerAngles.y);
		newRoom.transform.position -= offset;
		float angle =
			Vector3.SignedAngle(offset, doorObject.transform.localPosition.normalized * -1, Vector3.up)
			- doorObject.transform.parent.rotation.eulerAngles.y;
		newRoom.transform.RotateAround(randomDoor, Vector3.up, angle);
	}
	[ContextMenu("Rotate")]
	void Rotate(){
		newRoom.transform.RotateAround(randomDoor, Vector3.up, f);
	}
	void Start(){
		rng = new(1);
		Instantiate(Rooms[0], new Vector3(), new Quaternion());
	}
}
