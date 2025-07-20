using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class RoomBehaviour : MonoBehaviour
{
	public GameObject[] roomDoors => GetComponentsInChildren<Transform>(true)
		.Where(t => t.CompareTag("DoorMark"))
		.Select(t => t.gameObject)
		.ToArray();
	public GameObject[] DisableOnInit;
	void Start()
	{
		foreach (GameObject obj in DisableOnInit)
			obj.SetActive(false);
	}
	public async void EnableBack()
	{
		await Task.Delay(20);
		foreach (GameObject obj in DisableOnInit)
			obj.SetActive(true);
	}
}
