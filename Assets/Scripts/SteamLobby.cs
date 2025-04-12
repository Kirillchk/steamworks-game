using UnityEngine;
using Steamworks;
using System.Collections.Generic;
using System;
public class LobbyManager : MonoBehaviour
{
	[SerializeField]ulong ID = 76561198831185061;
	[ContextMenu("Join")]
	private void DebugJoin() => JoinLobby(new(ID));
	[ContextMenu("Create")]
	private void DebugCreate() => CreateLobby();
	private const int MaxLobbyMembers = 4; // Maximum number of players in the lobby
	private void Awake()
	{
		if (!SteamManager.Initialized)
		{
			Debug.LogError("Steamworks is not initialized!");
			return;
		}
		Callback<LobbyCreated_t>.Create(callback =>
			{
				if (callback.m_eResult == EResult.k_EResultOK)
					Debug.Log($"Lobby created successfully! Lobby ID: {callback.m_ulSteamIDLobby}");
				else
					Debug.LogError($"Failed to create lobby. Error: {callback.m_eResult}");
				Debug.Log(SteamUser.GetSteamID());
			}
		);
		Callback<LobbyEnter_t>.Create(callback =>
			Debug.Log($"Successfully entered lobby! Lobby ID: {callback.m_ulSteamIDLobby}")
		);
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
}

