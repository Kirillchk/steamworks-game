using UnityEngine;
using Steamworks;
using System;

public class DataSender : MonoBehaviour
{
    private LobbyManager lobbyManager;
    public void Start()
    {
        lobbyManager = GetComponent<LobbyManager>();
    }
    [ContextMenu("Send")]
    public void SendData()
    {
        byte[] data = new byte[11];
        SendToAllPlayers(data);
    }
    public void SendToAllPlayers(byte[] data, EP2PSend sendType = EP2PSend.k_EP2PSendReliable)
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam not initialized!");
            return;
        }


        int numPlayers = SteamMatchmaking.GetNumLobbyMembers(lobbyManager.lobbyId);
        
        for (int i = 0; i < numPlayers; i++)
        {
            CSteamID playerID = SteamMatchmaking.GetLobbyMemberByIndex(lobbyManager.lobbyId, i);
            SendToPlayer(playerID, data, sendType);
            // if (playerID != SteamUser.GetSteamID())
            // {
            //     SendToPlayer(playerID, data, sendType);
            // }
        }
    }
        private void SendToPlayer(CSteamID targetPlayer, byte[] data, EP2PSend sendType)
    {
        bool success = SteamNetworking.SendP2PPacket(
            targetPlayer,
            data,
            (uint)data.Length,
            sendType,
            0
        );

        if (!success)
        {
            Debug.LogError("Failed to send P2P packet to player: " + targetPlayer);
        }
    }
}
