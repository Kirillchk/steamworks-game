using Steamworks;
using System;
using UnityEngine;

public class DataReceiver : MonoBehaviour
{
    private byte[] packetBuffer = new byte[1024];
    void Update()
    {
        if (!SteamManager.Initialized) return;
        uint packetSize;
        // Process all received packets
        while (SteamNetworking.IsP2PPacketAvailable(out packetSize))
        {

            if (SteamNetworking.ReadP2PPacket(packetBuffer, (uint)packetBuffer.Length, out uint msgSize, out CSteamID remoteSteamID, 0))
            {
                byte[] data = new byte[msgSize];
                Array.Copy(packetBuffer, data, msgSize);
                
                // Process the received data
                OnNetworkDataReceived(remoteSteamID, data);
            }
        }
    }

    private void OnNetworkDataReceived(CSteamID senderID, byte[] data)
    {
        // Handle the received data
        Debug.Log(data);
        Debug.Log($"Received {data.Length} bytes from {senderID}");

    }
}