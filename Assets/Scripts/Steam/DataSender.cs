using UnityEngine;
using Steamworks;

public class DataSender
{
    private LobbyManager lobbyManager;
    public byte[] data = new byte[1111111111];
    public void SendToAllPlayers(byte[] data, EP2PSend sendType = EP2PSend.k_EP2PSendReliable)
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam not initialized!");
            return;
        }

        // Get the number of players in the lobby
        int numPlayers = 1;
        // SteamMatchmaking.GetNumLobbyMembers(lobbyManager.lobbyId);
        
        for (int i = 0; i < numPlayers; i++)
        {
            //CSteamID playerID = SteamMatchmaking.GetLobbyMemberByIndex(lobbyManager.lobbyId, i);
            CSteamID playerID = SteamUser.GetSteamID();
            // Don't send to ourselves
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
