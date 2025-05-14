using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
	bool b = false;
	Vector3 randomDoor = new();
	GameObject v;
	[SerializeField]List<GameObject> Rooms = new();
	[ContextMenu("SpawnRooms")]
	void spawnRoom(){
		if(!b){
			GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");
			RoomDoor door = doors[0].GetComponent<RoomDoor>();
			randomDoor = door.transform.position;
			v = Instantiate(Rooms[0], randomDoor, new Quaternion());
			Debug.DrawRay(randomDoor, Vector3.up, Color.white, 50, true);
			Destroy(door);
			b = !b;
		} else {
			Transform[] childrenWithTag = v.GetComponentsInChildren<Transform>(true)
				.Where(child => child.CompareTag("Door"))
				.ToArray();
			Vector3 secondDoor = childrenWithTag[0].position;
			Debug.DrawRay(secondDoor, Vector3.up, Color.green, int.MaxValue, true);
			Vector3 offset = randomDoor - secondDoor;
			Debug.DrawRay(randomDoor, offset, Color.red, int.MaxValue, true);
			float ang = Vector3.SignedAngle(offset, randomDoor, Vector3.up);
			Debug.Log(ang);
			if(ang == 180){
				v.transform.position -= offset;
			} else if(ang == 0){
				v.transform.position += offset;
			}
			//Debug.Log($"{offset} {randomDoor} {secondDoor}");
		}
		
	}
	void Start() => 
		Instantiate(Rooms[0], new Vector3(), new Quaternion());
}
