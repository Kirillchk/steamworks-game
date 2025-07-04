using UnityEngine;
using Steamworks;
using UnityEngine.SceneManagement;
public class LobbyManager : MonoBehaviour
{
	[SerializeField]ulong ID = 0;
	[ContextMenu("Join")]
	private void DebugJoin() => JoinLobby(new(ID));
	[ContextMenu("Create")]
	private void DebugCreate() => CreateLobby();
	private const int MaxLobbyMembers = 4; // Maximum number of players in the lobby
	public static CSteamID lobbyId;
	private void Awake()
	{
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
					lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
					Debug.Log($"my steam id{SteamUser.GetSteamID()}");
				}
				else
					Debug.LogError($"Failed to create lobby. Error: {callback.m_eResult}");
				Debug.Log(SteamUser.GetSteamID());
			}
		);
		Callback<LobbyEnter_t>.Create(callback =>
		{
			Debug.Log($"Lobby ID: {callback.m_ulSteamIDLobby} Lobby users {SteamMatchmaking.GetNumLobbyMembers(lobbyId)}");		
			lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
			if(SceneManager.GetActiveScene()!= SceneManager.GetSceneByName("Lobby"))
				SceneManager.LoadScene("Lobby");
			if(GetComponent<P2PBase>() == null)
				gameObject.AddComponent<P2PClient>();
			P2PClient client = GetComponent<P2PClient>();
			P2PHost host = GetComponent<P2PHost>();
			if(client != null)
				client.Connect();
			if(host != null)
				host.Listen();
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
		P2PBase p2p = GetComponent<P2PBase>();
		if(p2p != null)
			Destroy(p2p); 
		gameObject.AddComponent<P2PClient>();
		SteamMatchmaking.JoinLobby(lobbyID);
	}
	public void JoinLobbyButton()=>SteamFriends.ActivateGameOverlay("Friends");	
	public void HostLobbyButton()
	{
		P2PBase p2p = GetComponent<P2PBase>();
		if(p2p != null)
			Destroy(p2p); 
		gameObject.AddComponent<P2PHost>();
		CreateLobby();
		SceneManager.LoadScene("Lobby");
	}
}

