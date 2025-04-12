using UnityEngine;
using Steamworks;
using System;
using System.Runtime.InteropServices;
public class P2PManager : MonoBehaviour
{
    HSteamListenSocket listenSocket = HSteamListenSocket.Invalid;
    HSteamNetConnection connection = HSteamNetConnection.Invalid;

    [SerializeField] ulong ID = 76561198831185061;

    [ContextMenu("listen")]
    void Listen()
    {
        listenSocket = SteamNetworkingSockets.CreateListenSocketP2P(0, 0, null);
        Debug.Log("Listening...");
    }

    [ContextMenu("connect")]
    void Connect()
    {
        SteamNetworkingIdentity peerIdentity = new();
        peerIdentity.SetSteamID(new CSteamID(ID));
        connection = SteamNetworkingSockets.ConnectP2P(ref peerIdentity, 0, 0, null);
        Debug.Log("Connecting...");
    }

    [ContextMenu("send")]
    void Send()
    {
        if (connection == HSteamNetConnection.Invalid)
        {
            Debug.LogError("No valid connection to send data");
            return;
        }

        // Create a byte array with a single number (42 in this example)
        byte[] data = { 42 };
        
        // Allocate unmanaged memory and copy the bytes
        IntPtr ptr = Marshal.AllocHGlobal(data.Length);
        Marshal.Copy(data, 0, ptr, data.Length);
        
        // Send the data
        SteamNetworkingSockets.SendMessageToConnection(connection, ptr, (uint)data.Length, 0, out _);
        Debug.Log("Sent number: " + data[0]);
        
        // Free the unmanaged memory
        Marshal.FreeHGlobal(ptr);
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        SteamNetworkingSockets.RunCallbacks();

        if (connection != HSteamNetConnection.Invalid)
        {
            IntPtr[] messages = new IntPtr[1];
            int messageCount = SteamNetworkingSockets.ReceiveMessagesOnConnection(connection, messages, 1);
            
            if (messageCount > 0)
            {
                SteamNetworkingMessage_t message = Marshal.PtrToStructure<SteamNetworkingMessage_t>(messages[0]);
                byte[] receivedData = new byte[message.m_cbSize];
                Marshal.Copy(message.m_pData, receivedData, 0, message.m_cbSize);
                
                Debug.Log("Received number: " + receivedData[0]);
                
                SteamNetworkingMessage_t.Release(messages[0]);
            }
        }
    }
}