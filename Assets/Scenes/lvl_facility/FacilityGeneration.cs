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
		for (int i = Section1.Length-1; i >= 0; i--)
		{
			var door = Doors.RandomElement(rng);
			bool failed = await AddRoom(door, Section1[i]);
			Debug.Log($"SEC1 faied?{failed} : {i}");
			if (failed) i++;
		}
		for (int i = CoolRooms1.Length-1; i >= 0; i--)
		{
			var door = Doors.RandomElement(rng);
			bool failed = await AddRoom(door, CoolRooms1[i], "main");
			Debug.Log($"CLR1 faied?{failed} : {i}");
			if (failed) i++;
		}
		foreach (var d in Doors)
			if(d.GetComponent<RoomDoor>().DoorType != "main")
				d.GetComponent<RoomDoor>().Close();
		await Task.Delay(20);
		for (int i = Section2.Length - 1; i >= 0; i--)
		{
			var door = Doors.RandomElement(rng);
			bool failed = await AddRoom(door, Section2[i]);
			Debug.Log($"SEC2 faied? {failed} : {i}");
			if (failed) i++;
		}
		for (int i = CoolRooms2.Length-1; i >= 0; i--)
		{
			var door = Doors.RandomElement(rng);
			bool failed = await AddRoom(door, CoolRooms2[i]);
			Debug.Log($"CLR2 faied?{failed} : {i}");
			if (failed) i++;
		}
		foreach (var d in Doors)
			d.GetComponent<RoomDoor>().Close();


		//PlayableBehavior.Players[SteamMatchmaking.GetNumLobbyMembers(LobbyManager.lobbyId)-1].Possess();
	}
}