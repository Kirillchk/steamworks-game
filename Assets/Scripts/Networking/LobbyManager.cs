using UnityEngine;
using Steamworks;
using UnityEngine.SceneManagement;
public class LobbyManager : MonoBehaviour
{
	private const int MaxLobbyMembers = 4;
	public static CSteamID lobbyId;
	static int playersOnline {
		get => SteamMatchmaking.GetNumLobbyMembers(lobbyId);
	}
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
				lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
			PlayableBehavior.Players[playersOnline].Possess();
		});
		Callback<LobbyEnter_t>.Create(callback =>
		{
			lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
			if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("Facility"))
				SceneManager.LoadScene("Facility");

			P2PBase networking = GetComponent<P2PBase>() ?? gameObject.AddComponent<P2PBase>();

			if (P2PBase.isHost)
				networking.Listen();
			else
				networking.Connect();
		});
		Callback<LobbyChatUpdate_t>.Create(callback =>
		{
			string action = callback.m_rgfChatMemberStateChange == 1 ? "joined" : "left";
			// fix thiso bulshido
			PlayableBehavior.Players[playersOnline].GetComponent<NetworkIdentity>().isOwner = false;
			for (int i = 0; i > playersOnline; i ++) {
				Debug.Log($"Summoning {i}");
				PlayableBehavior.Players[i].SummonPlayer();
			}
			PlayableBehavior.Players[playersOnline].Possess();
		});
		Callback<GameLobbyJoinRequested_t>.Create(callback =>
		{
			SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
		});
	}
	public void JoinLobbyButton()
		=> SteamFriends.ActivateGameOverlay("Friends");	
	public void HostLobbyButton()
	{
		P2PBase p2p = GetComponent<P2PBase>();
		if(p2p != null)
			Destroy(p2p);
		gameObject.AddComponent<P2PBase>();
		P2PBase.isHost = true;
		SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, MaxLobbyMembers);
		SceneManager.LoadScene("Facility");
	}
}

