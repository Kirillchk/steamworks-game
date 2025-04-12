using UnityEngine;
using Steamworks;
using System;
using System.Runtime.InteropServices;

public class P2PManager : MonoBehaviour
{
    HSteamListenSocket listenSocket;
    HSteamNetConnection connection;
    
    [SerializeField] ulong ID = 0;
    
    [ContextMenu("listen")]
    void listen()
    {
        Debug.Log("im listening");
        listenSocket = SteamNetworkingSockets.CreateListenSocketP2P(0, 0, null);
    }
    
    [ContextMenu("connect")]
    void connect()
    {
        SteamNetworkingIdentity peerIdentity = new();
        peerIdentity.SetSteamID(new CSteamID(ID));
        connection = SteamNetworkingSockets.ConnectP2P(ref peerIdentity, 0, 0, null);
        Debug.Log("connected");
    }
    
    [ContextMenu("send")]
    void send()
    {
        if (connection == HSteamNetConnection.Invalid) 
        {
            Debug.LogError("No valid connection to send data");
            return;
        }
        
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes("Hello, Peer!");
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            IntPtr ptr = handle.AddrOfPinnedObject();
            SteamNetworkingSockets.SendMessageToConnection(connection, ptr, (uint)bytes.Length, 0, out _);
            Debug.Log("data sent");
        }
        finally
        {
            if (handle.IsAllocated)
                handle.Free();
        }
        
    }
    
    void Update()
    {
        if (connection == HSteamNetConnection.Invalid) 
            return;
            
        IntPtr[] messages = new IntPtr[1];
        int messageCount = SteamNetworkingSockets.ReceiveMessagesOnConnection(connection, messages, 1);

        if (messageCount > 0) 
        {
            SteamNetworkingMessage_t message = Marshal.PtrToStructure<SteamNetworkingMessage_t>(messages[0]);
            
            // Create a managed byte array copy
            byte[] data = new byte[message.m_cbSize];
            Marshal.Copy(message.m_pData, data, 0, message.m_cbSize);
            
            // Convert to string
            string receivedData = System.Text.Encoding.UTF8.GetString(data);
            Debug.Log("Received: " + receivedData);
            
            // Release the message
            SteamNetworkingMessage_t.Release(messages[0]);
        }
    }
}