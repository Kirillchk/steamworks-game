using UnityEngine;

public class RoomBehaviour : MonoBehaviour
{
	[SerializeField]GameObject[] roomDoors;
	public GameObject GetRadomDoor(System.Random rng) =>
		roomDoors[rng.Next(roomDoors.Length)];
}
