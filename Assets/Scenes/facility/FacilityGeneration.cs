using System.Threading.Tasks;
using UnityEngine;
public class FacilityGeneration : MapGenerator
{
	public int AmountOfRooms = 10;
	[SerializeField] GameObject[] Rooms;
	GameObject[] Doors => GameObject.FindGameObjectsWithTag("DoorMark");
	async void Start()
	{
		for (int i = 0; AmountOfRooms > i; i++)
		{
			Debug.Log($"blia doors:{Doors.Length}");
			var room = Rooms.RandomElement(rng);
			var door = Doors.RandomElement(rng);
			await AddRoom(door, room);
		}

		foreach (var d in Doors)
			d.GetComponent<RoomDoor>().Close();
			
		//PlayableBehavior.Players[SteamMatchmaking.GetNumLobbyMembers(LobbyManager.lobbyId)-1].Possess();
	}
}