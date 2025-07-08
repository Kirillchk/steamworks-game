using UnityEngine;
using Steamworks;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
public class P2PSteamBehaviour : MonoBehaviour
{
	protected HSteamNetConnection connection;
	protected bool isActive = false;
	internal void SendMessageToConnection(in byte[] data, in int nSendFlags)
	{
		if (!isActive || connection == HSteamNetConnection.Invalid)
		{
			Debug.LogError("Cannot send - no active connection!");
			return;
		}
		GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
		try {
			IntPtr pData = handle.AddrOfPinnedObject();
			EResult result = SteamNetworkingSockets.SendMessageToConnection(
				connection,
				pData,
				(uint)data.Length,
				nSendFlags,
				out long messageNumber
			);
			//if (result != EResult.k_EResultOK)
			//	Debug.LogError($"Failed to send message: {result}");
			//else 
			//	Debug.Log($"Message sent successfully (ID: {messageNumber}, Size: {data.Length} bytes)");
		} catch (Exception e) {
			Debug.LogError($"Error sending message: {e}");
		} finally {
			handle.Free();
		}
	}
	// fucking hell
	internal List<byte[]> TryRecive()
	{
		if (!isActive || connection == HSteamNetConnection.Invalid)
			return null;
		// wha? fuck limit is 10 packs
		IntPtr[] messages = new IntPtr[10];
		int numMessages = SteamNetworkingSockets.ReceiveMessagesOnConnection(connection, messages, messages.Length);
		List<byte[]> res = new();
		if (numMessages > 0)
			Debug.Log($"Received {numMessages} messages this frame");
		for (int i = 0; i < numMessages; i++)
		{
			try
			{
				SteamNetworkingMessage_t message = Marshal.PtrToStructure<SteamNetworkingMessage_t>(messages[i]);
				res[i] = new byte[message.m_cbSize];
				Marshal.Copy(message.m_pData, res[i], 0, message.m_cbSize);
				message.Release();
			}
			catch (Exception e)
			{
				Debug.LogError($"Error processing message: {e}");
			}
			finally
			{
				SteamNetworkingMessage_t.Release(messages[i]);
			}
		}
		return res;
	}
	void Awake()
	{
		if (!SteamManager.Initialized)
		{
			Debug.LogError("Steam not initialized!");
			return;
		}

		try {
			SteamNetworkingUtils.InitRelayNetworkAccess();
		} catch (Exception e) {
			Debug.LogError($"Network initialization error: {e}");
		}

		DontDestroyOnLoad(gameObject);
		Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnConnectionStatusChanged);
	}
	void OnConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t callback)
	{
		Debug.Log($"Connection status changed:\n" +
			$"State: {callback.m_info.m_eState}\n" +
			$"Reason: {callback.m_info.m_eEndReason}\n" +
			$"Remote: {callback.m_info.m_identityRemote.GetSteamID()}\n" +
			$"OldState: {callback.m_eOldState}");

		switch (callback.m_info.m_eState)
		{
			case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
				if (callback.m_info.m_identityRemote.GetSteamID() != CSteamID.Nil && !isActive)
				{
					if (SteamNetworkingSockets.AcceptConnection(callback.m_hConn) == EResult.k_EResultOK)
					{
						connection = callback.m_hConn;
						isActive = true;
						Debug.Log("Accepted incoming connection");
					}
					else
					{
						Debug.LogError("Failed to accept connection");
						// SteamNetworkingSockets.CloseConnection(callback.m_hConn, 0, "Failed to accept", false);
					}
				}
				break;
				
			case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
				if (isActive) 
					Debug.Log("already active");
				else {
					bool result = SteamNetworkingSockets.AcceptConnection(callback.m_hConn) == EResult.k_EResultOK;
					Debug.Log(result?"Successfully acepted":"failed wtf");
				}
				break;
				
			case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
				Debug.Log("Connection closed");
				break;
			case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
				Debug.Log("Connection closed: " + callback.m_info.m_szEndDebug);
				SteamNetworkingSockets.CloseConnection(callback.m_hConn, 0, "Connection closed", false);
				isActive = false;
				break;
		}
	}
	internal bool isHost = true;
	//host
	HSteamListenSocket listenSocket;
    internal void Listen()
    {
        SteamNetworkingConfigValue_t[] configuration = new SteamNetworkingConfigValue_t[2];
        
        // Connection timeout
        configuration[0].m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_TimeoutConnected;
        configuration[0].m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32;
        configuration[0].m_val.m_int32 = 5000;
        
        // Larger buffer size
        configuration[1].m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_SendBufferSize;
        configuration[1].m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32;
        configuration[1].m_val.m_int32 = 65536;
        
        listenSocket = SteamNetworkingSockets.CreateListenSocketP2P(0, configuration.Length, configuration);
        Debug.Log("Listening for P2P connections");
    }
	//client
	LobbyManager lobby;
    public void Connect()
    {
        lobby = GetComponent<LobbyManager>();
        if (lobby == null || LobbyManager.lobbyId == CSteamID.Nil)
        {
            Debug.LogError("Lobby not initialized!");
            return;
        }

        CSteamID playerID = SteamMatchmaking.GetLobbyMemberByIndex(LobbyManager.lobbyId, 0);
        if (playerID == CSteamID.Nil)
        {
            Debug.LogError("No members in lobby!");
            return;
        }

        SteamNetworkingConfigValue_t[] configuration = new SteamNetworkingConfigValue_t[2];
        
        // Connection timeout
        configuration[0].m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_TimeoutConnected;
        configuration[0].m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32;
        configuration[0].m_val.m_int32 = 5000;
        
        // Larger buffer size
        configuration[1].m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_SendBufferSize;
        configuration[1].m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32;
        configuration[1].m_val.m_int32 = 65536;

        SteamNetworkingIdentity identity = new SteamNetworkingIdentity();
        identity.SetSteamID(playerID);
        
        connection = SteamNetworkingSockets.ConnectP2P(ref identity, 0, configuration.Length, configuration);
        isActive = true;
        Debug.Log($"Connecting to {playerID}");
    }
	void OnDestroy()
	{
		if (connection != HSteamNetConnection.Invalid)
		{
			SteamNetworkingSockets.CloseConnection(connection, 0, "Shutting down", false);
			connection = HSteamNetConnection.Invalid;
		}
        if (listenSocket != HSteamListenSocket.Invalid)
        {
            SteamNetworkingSockets.CloseListenSocket(listenSocket);
            listenSocket = HSteamListenSocket.Invalid;
        }
	}
}
