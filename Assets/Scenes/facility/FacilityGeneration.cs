using UnityEngine;
public class FacilityGeneration : MapGenerator
{
	public int AmountOfRooms = 10;
	[SerializeField] GameObject[] Rooms;
	GameObject[] Doors => GameObject.FindGameObjectsWithTag("DoorMark");
	async void Start()
	{
		for (int i = AmountOfRooms; i >= 0; i--)
		{
			var room = Rooms.RandomElement(rng);
			var door = Doors.RandomElement(rng);
			bool failed = await AddRoom(door, room);
			Debug.Log($"faied?{failed} : {i}");
			if (failed) i++;
		}

		foreach (var d in Doors)
			d.GetComponent<RoomDoor>().Close();
			
		//PlayableBehavior.Players[SteamMatchmaking.GetNumLobbyMembers(LobbyManager.lobbyId)-1].Possess();
	}
}