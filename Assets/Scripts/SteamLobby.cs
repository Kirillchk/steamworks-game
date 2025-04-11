using UnityEngine;
using Steamworks;
public class LobbyManager : MonoBehaviour
{
	[SerializeField]ulong ID = 0;
	[ContextMenu("Join")]
	private void DebugJoin() => JoinLobby(new(ID));
	[ContextMenu("Create")]
	private void DebugCreate() => CreateLobby();
	private const int MaxLobbyMembers = 4; // Maximum number of players in the lobby

	private Callback<LobbyCreated_t> lobbyCreatedCallback;
	private Callback<LobbyEnter_t> lobbyEnterCallback;
	private Callback<LobbyChatUpdate_t> lobbyChatUpdate;

	private void Start()
	{
		if (!SteamManager.Initialized)
		{
			Debug.LogError("Steamworks is not initialized!");
			return;
		}
		lobbyCreatedCallback = Callback<LobbyCreated_t>.Create(
		(LobbyCreated_t callback) =>
			{
				if (callback.m_eResult == EResult.k_EResultOK)
					Debug.Log($"Lobby created successfully! Lobby ID: {callback.m_ulSteamIDLobby}");
				else
					Debug.LogError($"Failed to create lobby. Error: {callback.m_eResult}");
			}
		);
		lobbyEnterCallback = Callback<LobbyEnter_t>.Create(
		(LobbyEnter_t callback) =>
			Debug.Log($"Successfully entered lobby! Lobby ID: {callback.m_ulSteamIDLobby}")
		);
		lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create((LobbyChatUpdate_t callback) => {
			Debug.Log($"{callback.m_ulSteamIDUserChanged} did {callback.m_rgfChatMemberStateChange}");
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

