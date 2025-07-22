using System.Linq;
using UnityEngine;

public class RoomBehaviour : MonoBehaviour
{
	void Start() => MapGenerator.Finished += EnableBack;
	public GameObject[] roomDoors => GetComponentsInChildren<Transform>(true)
		.Where(t => t.CompareTag("DoorMark"))
		.Select(t => t.gameObject)
		.ToArray();
	public GameObject[] EnableOnInit;
	public void EnableBack()
	{
		foreach (GameObject obj in EnableOnInit)
			obj.SetActive(true);
	}
	
}
