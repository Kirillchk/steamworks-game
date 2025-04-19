using UnityEngine;
using Steamworks;

public class P2PHost : P2PBase
{
    HSteamListenSocket listenSocket;
    public void Listen()
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
    void OnDestroy()
    {
        if (listenSocket != HSteamListenSocket.Invalid)
        {
            SteamNetworkingSockets.CloseListenSocket(listenSocket);
            listenSocket = HSteamListenSocket.Invalid;
        }
    }
}