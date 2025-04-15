using UnityEngine;
using Steamworks;
using System;
using System.Runtime.InteropServices;
public class P2PManager : MonoBehaviour
{
	bool oibla = false;
	LobbyManager lobby = null;
	HSteamNetConnection connection;
	HSteamListenSocket listenSocket;
    [ContextMenu("listen")]
    void Listen()
    {
		SteamNetworkingConfigValue_t[] configuration = new SteamNetworkingConfigValue_t[1];
		configuration[0].m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_TimeoutConnected;
		configuration[0].m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32;
		configuration[0].m_val.m_int32 = 500;
		listenSocket = SteamNetworkingSockets.CreateListenSocketP2P(0, 0, configuration);
    }

    [ContextMenu("connect")]
    void Connect()
    {
		lobby = GetComponent<LobbyManager>();
		SteamNetworkingConfigValue_t[] configuration = new SteamNetworkingConfigValue_t[1];
		configuration[0].m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_TimeoutConnected;
		configuration[0].m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32;
		configuration[0].m_val.m_int32 = 2000;
		CSteamID playerID = SteamMatchmaking.GetLobbyMemberByIndex(lobby.lobbyId, 0);
		Debug.Log("is host"+playerID.m_SteamID);
		SteamNetworkingIdentity identity = new();
		identity.SetSteamID(playerID);
		Debug.Log(identity.IsInvalid()?"invalid":"valid");
		connection = SteamNetworkingSockets.ConnectP2P(ref identity, 0, 0, configuration);
		oibla = true;
    }

    [ContextMenu("send")]
    void Send()
    {
		byte[] data = System.Text.Encoding.UTF8.GetBytes("Hello Steam!");
		SendMessageToP2PConection(connection, data, 1|8);
    }
	public void SendMessageToP2PConection(HSteamNetConnection hConn, byte[] data, int nSendFlags)
	{
		GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
		try {
			IntPtr pData = handle.AddrOfPinnedObject();
			EResult result = SteamNetworkingSockets.SendMessageToConnection(
				hConn,
				pData,    // Pointer to data
				(uint)data.Length, // Size in bytes
				nSendFlags,
				out long _         // Optional: message number (ignored here)
			);
			if (result != EResult.k_EResultOK)
				Debug.Log($"Send failed: {result}");
		} finally {
			handle.Free();
		}
	}
    [ContextMenu("receive")]
	void Receive(){
		IntPtr[] ptr = new IntPtr[1999];
		int incoming = SteamNetworkingSockets.ReceiveMessagesOnConnection(connection, ptr, 1999);
		for (int i = 0; i < ptr.Length; i++)
		{
			if (ptr[i] != IntPtr.Zero)
			{
				int value = Marshal.ReadInt32(ptr[i]); // Read 4-byte int
				Debug.Log($"ptr[{i}] = {value}");
			}
		}
		if (incoming!=0)
			Debug.Log($"incoming:{incoming}");
	}
    void Awake()
    {
		SteamNetworkingUtils.InitRelayNetworkAccess();
        DontDestroyOnLoad(gameObject);
		Callback<SteamNetConnectionStatusChangedCallback_t>.Create(callback=> {
			Debug.Log("SteamNetConnectionStatusChanged Callback was trigered " + callback.m_info.m_eEndReason + " \n" + callback.m_info.m_eState);
			SteamNetworkingSockets.AcceptConnection(callback.m_hConn);
		});
    }

    void Update()
    {
		if(oibla)
			Receive();
    }
	
}