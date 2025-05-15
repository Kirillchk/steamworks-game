using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    private System.Random rng;
	[SerializeField]List<GameObject> Rooms = new();
	[ContextMenu("SpawnRooms")]
	void spawnRoom(){
		GameObject doorObject = GameObject.FindGameObjectsWithTag("Door")[0];
		Vector3 randomDoor = doorObject.transform.position;
		GameObject v = Instantiate(Rooms[rng.Next(0,2)], randomDoor, new Quaternion());
		Destroy(doorObject);
		Debug.DrawLine(randomDoor, randomDoor+Vector3.up, Color.green, float.MaxValue);

		GameObject[] childrenWithTag = v.GetComponentsInChildren<Transform>(true)
			.Where(child => child.CompareTag("Door"))
			.Select(child => child.gameObject) // Convert Transform to GameObject
			.ToArray();
		
		GameObject secondDoorObject = childrenWithTag[rng.Next(0,childrenWithTag.Length)];
		Destroy(secondDoorObject);
		Vector3 secondDoor = secondDoorObject.transform.position;
		Vector3 offset = secondDoor - randomDoor;
		Debug.DrawLine(secondDoor, secondDoor + Vector3.up, Color.blue, float.MaxValue);

		float ang = Vector3.SignedAngle(offset, Vector3.forward, Vector3.up);
		Debug.DrawLine(new(), offset.normalized, Color.yellow, float.MaxValue);
		Debug.DrawLine(new Vector3()+Vector3.up, Vector3.back + Vector3.up, Color.yellow, float.MaxValue);
		
		Debug.Log(ang);
		v.transform.position -= offset;
		
		//if((ang >= 135 && ang <= 180)||(ang <= -135 && ang <= 180)){
		//	v.transform.RotateAround(randomDoor, Vector3.up, 180);
		//	v.transform.position -= offset;
		//} else if((ang <= 45 && ang >= 0)||(ang >= -45 && ang <= 0)){
		//	v.transform.RotateAround(randomDoor, Vector3.up, 0);
		//	v.transform.position += offset;
		//} else if(ang >= -135 && ang <= -45){
		//	v.transform.position += offset;
		//	v.transform.RotateAround(randomDoor, Vector3.up, -90);
		//} else if(ang <= 135 && ang >= 45){
		//	v.transform.position += offset;
		//	v.transform.RotateAround(randomDoor, Vector3.up, 90);
		//}

	}
	void Start(){
		rng = new(1);
		Instantiate(Rooms[0], new Vector3(), new Quaternion());
	}
}
