using System.Collections.Generic;
using UnityEngine;

public class RoomBehaviour : MonoBehaviour
{
	public List<GameObject> roomDoors;
	public GameObject GetRadomDoor(System.Random rng) =>
		roomDoors[rng.Next(roomDoors.Count)];
}
