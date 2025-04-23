using UnityEngine;
using Steamworks;
using System;
using System.Runtime.InteropServices;
using P2PMessages;
public class P2PClient : P2PBase
{
	private LobbyManager lobby;
	public void Connect()
	{
		lobby = GetComponent<LobbyManager>();
		if (lobby == null || lobby.lobbyId == CSteamID.Nil)
		{
			Debug.LogError("Lobby not initialized!");
			return;
		}

		CSteamID playerID = SteamMatchmaking.GetLobbyMemberByIndex(lobby.lobbyId, 0);
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
	private void TryRecive()
	{
		if (!isActive || connection == HSteamNetConnection.Invalid)
			return;
		
		IntPtr[] messages = new IntPtr[10];
		int numMessages = SteamNetworkingSockets.ReceiveMessagesOnConnection(connection, messages, messages.Length);
		
		if (numMessages > 0)
			Debug.Log($"Received {numMessages} messages this frame");
		for (int i = 0; i < numMessages; i++)
		{
			try {
				SteamNetworkingMessage_t message = Marshal.PtrToStructure<SteamNetworkingMessage_t>(messages[i]);
				byte[] data = new byte[message.m_cbSize];
				Marshal.Copy(message.m_pData, data, 0, message.m_cbSize);
				ProcesData(data, message);
			} catch (Exception e) {
				Debug.LogError($"Error processing message: {e}");
			} finally {
				SteamNetworkingMessage_t.Release(messages[i]);
			}
		}
	}
	void ProcesData(in byte[]data, SteamNetworkingMessage_t message) 
	{	
		EPackagePurpuse purpose = (EPackagePurpuse)data[0];
		switch (purpose){
			case EPackagePurpuse.Transform: 
			{
				P2PTransformPositionAndRotation transformMessage = new(data);
				NetworkTransform cube = cubes[transformMessage.ID];
				cube.transform.position = transformMessage.pos;
				cube.transform.rotation = transformMessage.rot;
				break;
			} 
			case EPackagePurpuse.TransformPosition: 
			{
				P2PTransformPosition transformPosition = new(data);
				NetworkTransform cube = cubes[transformPosition.ID];
				cube.transform.position = transformPosition.pos;
				break;
			}
			case EPackagePurpuse.TransformRotation: 
			{
				P2PTransformRotation transformRotation = new(data);
				NetworkTransform cube = cubes[transformRotation.ID];
				cube.transform.rotation = transformRotation.rot;
				break;
			} 
			default:
			{
				Debug.LogError("unsupported purpose");
				break;
			}
		}
	}
	void Update() => TryRecive(); 
	void OnDestroy()
	{
		if (connection != HSteamNetConnection.Invalid)
		{
			SteamNetworkingSockets.CloseConnection(connection, 0, "Shutting down", false);
			connection = HSteamNetConnection.Invalid;
		}
	}
}