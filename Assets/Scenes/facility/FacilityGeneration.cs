using UnityEngine;
public class FacilityGeneration : MapGenerator
{
	public int AmountOfRooms = 10;
	int RoomsCount = 0;
	[SerializeField] GameObject[] Rooms;
	GameObject[] Doors => GameObject.FindGameObjectsWithTag("DoorMark");
	async void Start()
	{
		while (RoomsCount < AmountOfRooms)
		{
			var room = Rooms.RandomElement(rng);
			var door = Doors.RandomElement(rng);
			await AddRoom(door, room);
		}

		foreach (var d in Doors)
			d.GetComponent<RoomDoor>().Close();
			
		//PlayableBehavior.Players[SteamMatchmaking.GetNumLobbyMembers(LobbyManager.lobbyId)-1].Possess();
	}
}