using UnityEngine;
using Steamworks;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;

public class P2PManager : MonoBehaviour
{
    // Send flags
    private const int k_nSteamNetworkingSend_Unreliable = 0;
    private const int k_nSteamNetworkingSend_NoNagle = 1;
    private const int k_nSteamNetworkingSend_Reliable = 8;
    
    private HSteamNetConnection connection;
    private HSteamListenSocket listenSocket;
    private LobbyManager lobby;
    private bool isActive = false;
    private Queue<string> messageQueue = new();
    private object messageLock = new object();
    [ContextMenu("Listen")]
    void Listen()
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

    void Update()
    {
        // Process any queued messages
        lock (messageLock)
        {
            while (messageQueue.Count > 0)
            {
                Debug.Log("Processing message: " + messageQueue.Dequeue());
            }
        }

        if (!isActive || connection == HSteamNetConnection.Invalid)
            return;

        // Receive messages
        IntPtr[] messages = new IntPtr[10];
        int numMessages = SteamNetworkingSockets.ReceiveMessagesOnConnection(connection, messages, messages.Length);
        
        if (numMessages > 0)
        {
            Debug.Log($"Received {numMessages} messages this frame");
        }

        for (int i = 0; i < numMessages; i++)
        {
            try
            {
                SteamNetworkingMessage_t message = Marshal.PtrToStructure<SteamNetworkingMessage_t>(messages[i]);
                
                byte[] data = new byte[message.m_cbSize];
                Marshal.Copy(message.m_pData, data, 0, (int)message.m_cbSize);
                
                string receivedText = Encoding.UTF8.GetString(data);
                
                lock (messageLock)
                {
                    messageQueue.Enqueue(receivedText);
                }

                Debug.Log($"Processed message from {message.m_identityPeer.GetSteamID()}: {receivedText}");
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
        if (listenSocket != HSteamListenSocket.Invalid)
        {
            SteamNetworkingSockets.CloseListenSocket(listenSocket);
            listenSocket = HSteamListenSocket.Invalid;
        }
        
        if (connection != HSteamNetConnection.Invalid)
        {
            SteamNetworkingSockets.CloseConnection(connection, 0, "Shutting down", false);
            connection = HSteamNetConnection.Invalid;
        }
    }

    void OnGUI()
    {
        if (isActive)
        {
            GUI.Label(new Rect(10, 10, 300, 20), $"Connection: {(connection == HSteamNetConnection.Invalid ? "Invalid" : "Active")}");
            
            // Basic connection status display
            if (connection != HSteamNetConnection.Invalid)
            {
                GUI.Label(new Rect(10, 30, 300, 20), $"Messages in queue: {messageQueue.Count}");
            }
        }
    }
}