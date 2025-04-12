using UnityEngine;
using Steamworks;
using System;
using System.Runtime.InteropServices;
public class P2PManager : MonoBehaviour
{
	CSteamID lobbyID;
	HSteamListenSocket listenSocket;
    [ContextMenu("listen")]
    void Listen()
    {
		SteamNetworkingConfigValue_t[] configuration = new SteamNetworkingConfigValue_t[1];
		configuration[0].m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_TimeoutConnected;
		configuration[0].m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32;
		configuration[0].m_val.m_int32 = 500;
		SteamNetworkingSockets.CreateListenSocketP2P(0, 0, configuration);
    }

    [ContextMenu("connect")]
    void Connect()
    {
		Debug.Log(lobbyID);
		SteamNetworkingConfigValue_t[] configuration = new SteamNetworkingConfigValue_t[1];
		configuration[0].m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_TimeoutConnected;
		configuration[0].m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32;
		configuration[0].m_val.m_int32 = 2000;
		CSteamID playerID = SteamMatchmaking.GetLobbyMemberByIndex(lobbyID, 0);
		Debug.Log(playerID);
		SteamNetworkingIdentity identity = new();
		identity.SetSteamID(playerID);
		Debug.Log(identity.IsInvalid()?"invalid":"valid");
		SteamNetworkingSockets.ConnectP2P(ref identity, 0, 0, configuration);
    }

    [ContextMenu("send")]
    void Send()
    {
		// SteamNetworkingSockets.SendMessageToConnection()
    }

    void Awake()
    {
		SteamNetworkingUtils.InitRelayNetworkAccess();
        DontDestroyOnLoad(gameObject);
		Callback<SteamNetConnectionStatusChangedCallback_t>.Create(callback=> {
			Debug.Log("SteamNetConnectionStatusChanged Callback was trigered " + callback.m_info.m_eEndReason + " \n" + callback.m_info.m_eState);
			SteamNetworkingSockets.AcceptConnection(callback.m_hConn);
		});
		lobbyID = GetComponent<LobbyManager>().lobbyId;
    }

    void Update()
    {

    }
}