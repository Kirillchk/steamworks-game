using UnityEngine;
using Steamworks;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public class LobbyManager : MonoBehaviour
{
	[SerializeField]ulong ID = 76561198831185061;
	[ContextMenu("Join")]
	private void DebugJoin() => JoinLobby(new(ID));
	[ContextMenu("Create")]
	private void DebugCreate() => CreateLobby();
	private const int MaxLobbyMembers = 4; // Maximum number of players in the lobby
	public CSteamID lobbyId;
	private DataSender dataSender;
	private void Awake()
	{
		dataSender = GetComponent<DataSender>();
		DontDestroyOnLoad(gameObject);
		if (!SteamManager.Initialized)
		{
			Debug.LogError("Steamworks is not initialized!");
			return;
		}
		Callback<LobbyCreated_t>.Create(callback =>
			{
				if (callback.m_eResult == EResult.k_EResultOK)
				{
					Debug.Log($"Lobby created successfully! Lobby ID: {callback.m_ulSteamIDLobby}");
					lobbyId=new CSteamID(callback.m_ulSteamIDLobby);
					Debug.Log($"my steam id{SteamUser.GetSteamID()}");
				}

				else
					Debug.LogError($"Failed to create lobby. Error: {callback.m_eResult}");
				Debug.Log(SteamUser.GetSteamID());
			}
		);
		Callback<LobbyEnter_t>.Create(callback =>
		{
			Debug.Log($"Successfully entered lobby! Lobby ID: {callback.m_ulSteamIDLobby}");
			if(SceneManager.GetActiveScene()!= SceneManager.GetSceneByName("Lobby"))
			{
				SceneManager.LoadScene("Lobby");
			}
		});
		Callback<LobbyChatUpdate_t>.Create(callback => {
			string action = callback.m_rgfChatMemberStateChange == 1 ? "joined" : "left";
			Debug.Log($"{callback.m_ulSteamIDUserChanged} just {action}");
			
		});
		Callback<GameLobbyJoinRequested_t>.Create(callback => {
			SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
		});
	}

	public void CreateLobby()
	{
		Debug.Log("Creating lobby...");
		SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, MaxLobbyMembers);
	}

	public void JoinLobby(CSteamID lobbyID)
	{
		Debug.Log($"Joining lobby: {lobbyID}");
		SteamMatchmaking.JoinLobby(lobbyID);
	}
	public void JoinLobbyButton()=>SteamFriends.ActivateGameOverlay("Friends");	
	public void HostLobbyButton()
	{
		CreateLobby();
		SceneManager.LoadScene("Lobby");
	}
	[ContextMenu("send")]
	public void SendData()
	{
		byte[] data = new byte[111];
		dataSender.SendToAllPlayers(data);
	}
}

