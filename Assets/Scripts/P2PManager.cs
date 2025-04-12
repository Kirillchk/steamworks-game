using UnityEngine;
using Steamworks;
using System;
using System.Runtime.InteropServices;
public class P2PManager : MonoBehaviour
{
	HSteamListenSocket listenSocket;
    [SerializeField] ulong ID = 76561198831185061;
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
		SteamNetworkingConfigValue_t[] configuration = new SteamNetworkingConfigValue_t[1];
		configuration[0].m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_TimeoutConnected;
		configuration[0].m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32;
		configuration[0].m_val.m_int32 = 2000;
		CSteamID playerID = new(76561199060165244);
		SteamNetworkingIdentity identity = new();
		identity.SetSteamID(playerID);
		Debug.Log(identity.IsInvalid()?"invalid":"valid");
		SteamNetworkingSockets.ConnectP2P(ref identity, 0, 0, configuration);
    }

    [ContextMenu("send")]
    void Send()
    {
		
    }

    void Awake()
    {
		SteamNetworkingUtils.InitRelayNetworkAccess();
        DontDestroyOnLoad(gameObject);
		Callback<SteamNetConnectionStatusChangedCallback_t>.Create(callback=> {
			Debug.Log("SteamNetConnectionStatusChanged Callback was trigered " + callback.m_info.m_eEndReason);
		});
    }

    void Update()
    {

    }
}