using System.Threading.Tasks;
using UnityEngine;
public class FacilityGeneration : MapGenerator
{
	[SerializeField] GameObject[] Section1;
	[SerializeField] GameObject[] CoolRooms1;
	[SerializeField] GameObject[] Section2;
	[SerializeField] GameObject[] CoolRooms2;
	GameObject[] Doors => GameObject.FindGameObjectsWithTag("DoorMark");
	async void Start()
	{
		foreach (var room in Section1)
		{
		}
		for (int i = Section1.Length; i >= 0; i--)
		{
			var door = Doors.RandomElement(rng);
			bool failed = await AddRoom(door, Section1[i]);
			Debug.Log($"faied?{failed} : {i}");
			if (failed) i++;
		}

		foreach (var d in Doors)
			d.GetComponent<RoomDoor>().Close();

		//PlayableBehavior.Players[SteamMatchmaking.GetNumLobbyMembers(LobbyManager.lobbyId)-1].Possess();
	}
}