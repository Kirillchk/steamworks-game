using UnityEngine;
using Steamworks;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using P2PMessages;
public class P2PBase : MonoBehaviour
{
	enum EBulkPackage : byte {
		Transform,
		Action
	}
	internal static Dictionary<Vector3, NetworkTransform> networkTransforms = new();
	internal static List<ITransformMessage> transformMessages = new();
    protected HSteamNetConnection connection;
    protected bool isActive = false;
	void LateUpdate()
	{
		if(transformMessages.Count == 0) return;
		List<byte> bulk = new(transformMessages.Count * 33 + 1)
		{
			(byte)EBulkPackage.Transform
		};
		foreach(ITransformMessage message in transformMessages)
			bulk.AddRange(message.GetBinaryRepresentation().ToArray());
		SendMessageToConnection(bulk.ToArray(), (int)k_nSteamNetworkingSend.ReliableNoNagle);
		transformMessages.Clear();
	}
	void SendMessageToConnection(in byte[] data, in int nSendFlags)
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
	void TryRecive(){
        if (!isActive || connection == HSteamNetConnection.Invalid)
            return;
        // Receive messages
        IntPtr[] messages = new IntPtr[10];//why?
        int numMessages = SteamNetworkingSockets.ReceiveMessagesOnConnection(connection, messages, messages.Length);
        
        if (numMessages > 0)
			Debug.Log($"Received {numMessages} messages this frame");
        for (int i = 0; i < numMessages; i++)
        {
            try {
                SteamNetworkingMessage_t message = Marshal.PtrToStructure<SteamNetworkingMessage_t>(messages[i]);
                byte[] data = new byte[message.m_cbSize];
                Marshal.Copy(message.m_pData, data, 0, message.m_cbSize);
				ProcesData((EBulkPackage)data[0], data[1..]);
            } catch (Exception e) {
                Debug.LogError($"Error processing message: {e}");
            } finally {
                SteamNetworkingMessage_t.Release(messages[i]);
            }
        }
	}
	void ProcesData(EBulkPackage bulkPurpose, in byte[] bulkData) {	
		switch(bulkPurpose){
			case EBulkPackage.Transform:
			{
				int position = 0;
				
				while (position < bulkData.Length)
				{
					EPackagePurpuse purpose = (EPackagePurpuse)bulkData[position];
					
					int messageSize = purpose switch
					{
						EPackagePurpuse.Transform => 41,
						EPackagePurpuse.TransformPosition => 25,
						EPackagePurpuse.TransformRotation => 29,
						_ => throw new InvalidOperationException($"Unknown message type: {purpose}")
					};
					
					if (position + messageSize > bulkData.Length)
						throw new InvalidOperationException("Incomplete message in bulk data");
					
					byte[] messageBytes = new byte[messageSize];
					Array.Copy(bulkData, position, messageBytes, 0, messageSize);
					position += messageSize;
					switch(purpose)
					{
						case EPackagePurpuse.Transform:
						{
							var message = MemoryMarshal.Read<P2PTransformPositionAndRotation>(messageBytes);
							networkTransforms[message.ID].MoveToSync(message.rot, message.pos);
							break;
						}
						case EPackagePurpuse.TransformRotation:
						{
							var message = MemoryMarshal.Read<P2PTransformRotation>(messageBytes);
							networkTransforms[message.ID].MoveToSync(message.rot);
							break;
						}
						case EPackagePurpuse.TransformPosition:
						{
							var message = MemoryMarshal.Read<P2PTransformPosition>(messageBytes);
							networkTransforms[message.ID].MoveToSync(null, message.pos);
							break;
						}
					}
				}
				break;
			}
			case EBulkPackage.Action:
			{
				
				break;
			}
			default: 
			{
				Debug.LogError("UNSUPORTED BULK");
				break;
			}
		}
	}
    void Update() => TryRecive();
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

    void OnConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t callback)
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

