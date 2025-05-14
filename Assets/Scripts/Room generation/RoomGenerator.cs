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
			RoomDoor door = GameObject.FindGameObjectsWithTag("Door")[0].GetComponent<RoomDoor>();
			randomDoor = door.transform.position;
			v = Instantiate(Rooms[1], randomDoor, new Quaternion());
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
			if((ang >= 135 && ang <= 180)||(ang <= -135 && ang <= 180)){
				v.transform.RotateAround(randomDoor, Vector3.up, ang);
				v.transform.position -= offset;
			} else if((ang <= 45 && ang >= 0)||(ang >= -45 && ang <= 0)){
				v.transform.RotateAround(randomDoor, Vector3.up, ang);
				v.transform.position += offset;
			} else if(ang >= -135 && ang <= -45){
				v.transform.position += offset;
				v.transform.RotateAround(randomDoor, Vector3.up, ang);
			} else if(ang <= 135 && ang >= 45){
				v.transform.position += offset;
				v.transform.RotateAround(randomDoor, Vector3.up, ang);
			}
		}
	}
	void Start() => 
		Instantiate(Rooms[0], new Vector3(), new Quaternion());
}
