using UnityEngine;
using Steamworks;
using System;
using System.Runtime.InteropServices;
public class P2PManager : MonoBehaviour
{
	HSteamListenSocket listenSocket;
	HSteamNetConnection connection;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
	[SerializeField]ulong ID = 0;
	[ContextMenu("listen")]
	void listen() => 
		listenSocket = SteamNetworkingSockets.CreateListenSocketP2P(0, 0, null);
	[ContextMenu("connect")]
	void connect()
	{
		SteamNetworkingIdentity peerIdentity = new();
		// Set peer identity (e.g., SteamID)
		peerIdentity.SetSteamID(new CSteamID(ID));
		connection = SteamNetworkingSockets.ConnectP2P(ref peerIdentity, 0, 0, null);
	}
	[ContextMenu("send")]
	void send()
	{
		byte[] bytes = System.Text.Encoding.UTF8.GetBytes("Hello, Peer!");
		IntPtr ptr = (IntPtr)BitConverter.ToUInt32(bytes, 0);
		SteamNetworkingSockets.SendMessageToConnection(connection, ptr, (uint)bytes.Length, 0, out _);
	}
    void Start()
    {
    }

    void Update()
    {
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
			Console.WriteLine("Received: " + receivedData);
			message.Release();
		}
    }
}
