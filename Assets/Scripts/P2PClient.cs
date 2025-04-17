using UnityEngine;
using Steamworks;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
public class P2PClient : MonoBehaviour
{
    private const int k_nSteamNetworkingSend_Unreliable = 0;
    private const int k_nSteamNetworkingSend_NoNagle = 1;
    private const int k_nSteamNetworkingSend_Reliable = 8;
    private LobbyManager lobby;
    private HSteamNetConnection connection;
    private bool isActive = false;
    [ContextMenu("Connect")]
    void Connect()
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

    [ContextMenu("Send")]
    void Send()
    {
        if (!isActive || connection == HSteamNetConnection.Invalid)
        {
            Debug.LogError("Cannot send - no active connection!");
            return;
        }

        byte[] data = Encoding.UTF8.GetBytes("Hello Steam! " + DateTime.Now.ToString("HH:mm:ss.fff"));
        SendMessageToConnection(connection, data, k_nSteamNetworkingSend_Reliable | k_nSteamNetworkingSend_NoNagle);
    }

    public void SendMessageToConnection(HSteamNetConnection hConn, byte[] data, int nSendFlags)
    {
        if (hConn == HSteamNetConnection.Invalid)
        {
            Debug.LogError("Invalid connection handle!");
            return;
        }

        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        try {
            IntPtr pData = handle.AddrOfPinnedObject();
            EResult result = SteamNetworkingSockets.SendMessageToConnection(
                hConn,
                pData,
                (uint)data.Length,
                nSendFlags,
                out long messageNumber
            );
            
            if (result != EResult.k_EResultOK)
            {
                Debug.LogError($"Failed to send message: {result}");
            }
            else
            {
                Debug.Log($"Message sent successfully (ID: {messageNumber}, Size: {data.Length} bytes)");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending message: {e}");
        }
        finally
        {
            handle.Free();
        }
    }
	void Awake()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam not initialized!");
            return;
        }

        try
        {
            SteamNetworkingUtils.InitRelayNetworkAccess();
        }
        catch (Exception e)
        {
            Debug.LogError($"Network initialization error: {e}");
        }

        DontDestroyOnLoad(gameObject);
        
        //Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnConnectionStatusChanged);
    }

}
