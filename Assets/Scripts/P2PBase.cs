using UnityEngine;
using Steamworks;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using P2PMessages;
public class P2PBase : MonoBehaviour
{
	public List<NetworkTransform> cubes = new();
    internal HSteamNetConnection connection;
    internal bool isActive = false;
    public void SendMessageToConnection(in byte[] data, in int nSendFlags)
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
            if (result != EResult.k_EResultOK)
				Debug.LogError($"Failed to send message: {result}");
            else 
				Debug.Log($"Message sent successfully (ID: {messageNumber}, Size: {data.Length} bytes)");
        } catch (Exception e) {
            Debug.LogError($"Error sending message: {e}");
        } finally {
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

        try {
            SteamNetworkingUtils.InitRelayNetworkAccess();
        } catch (Exception e) {
            Debug.LogError($"Network initialization error: {e}");
        }

        DontDestroyOnLoad(gameObject);
        
        Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnConnectionStatusChanged);
    }

    private void OnConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t callback)
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
	
    void OnDestroy()
    {
        if (connection != HSteamNetConnection.Invalid)
        {
            SteamNetworkingSockets.CloseConnection(connection, 0, "Shutting down", false);
            connection = HSteamNetConnection.Invalid;
        }
    }
}

