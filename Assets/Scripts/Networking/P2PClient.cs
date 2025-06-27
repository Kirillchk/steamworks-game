using UnityEngine;
using Steamworks;

public class P2PClient : P2PBase
{
    // Send flags
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
    }
}