using System.Linq;
using UnityEngine;

public class RoomBehaviour : MonoBehaviour
{
	public GameObject[] roomDoors => GetComponentsInChildren<Transform>(true)
    	.Where(t => t.CompareTag("DoorMark"))
    	.Select(t => t.gameObject)
    	.ToArray();	
}
