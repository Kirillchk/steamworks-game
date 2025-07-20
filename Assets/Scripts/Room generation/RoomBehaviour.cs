using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class RoomBehaviour : MonoBehaviour
{
	void Start() => MapGenerator.Finished += EnableBack;
	public GameObject[] roomDoors => GetComponentsInChildren<Transform>(true)
		.Where(t => t.CompareTag("DoorMark"))
		.Select(t => t.gameObject)
		.ToArray();
	public GameObject[] EnableOnInit;
	public async void EnableBack()
	{
		await Task.Delay(20);
		foreach (GameObject obj in EnableOnInit)
			obj.SetActive(true);
	}
	
}
