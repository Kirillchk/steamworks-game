using UnityEngine;
using Steamworks;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.Mathematics;
public class P2PBase : MonoBehaviour
{
	public List<CubeBehavior> cubes = new();
	// wtf is this?
	private const int k_nSteamNetworkingSend_Unreliable = 0;
	private const int k_nSteamNetworkingSend_Reliable = 1;
	private const int k_nSteamNetworkingSend_NoNagle = 2;
	private const int k_nSteamNetworkingSend_NoDelay = 4;
    internal HSteamNetConnection connection;
    internal bool isActive = false;
	enum EPackagePurpuse : byte {
		Transform,
		Event,
		SEX
	}

    [ContextMenu("Send")]
    void Send()
    {
		//byte[] data = Encoding.UTF8.GetBytes("Hello Steam! " + DateTime.Now.ToString("HH:mm:ss.fff"));
		byte[] data = new byte[29];
		data[0] = (byte)EPackagePurpuse.Transform;
		
		Vector3 posit = new(0.1123f, 0.132f, 2);
		Quaternion quatern = new(0.555f, 0.444f, 0.333f, 0.5f);
		float[] farr = { 
			posit.x, posit.y, posit.z,
			quatern.x, quatern.y, quatern.z, quatern.w
		};
        for (int i = 0; i < 7; i++)
		    Array.Copy(BitConverter.GetBytes(farr[i]), 0, data, i * 4 + 1, 4);
	
		SendMessageToConnection(data, k_nSteamNetworkingSend_Unreliable | k_nSteamNetworkingSend_NoNagle);
    }
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
	private void TryRecive(){
        if (!isActive || connection == HSteamNetConnection.Invalid)
            return;
        // Receive messages
        IntPtr[] messages = new IntPtr[10];
        int numMessages = SteamNetworkingSockets.ReceiveMessagesOnConnection(connection, messages, messages.Length);
        
        if (numMessages > 0)
			Debug.Log($"Received {numMessages} messages this frame");
        for (int i = 0; i < numMessages; i++)
        {
            try {
                SteamNetworkingMessage_t message = Marshal.PtrToStructure<SteamNetworkingMessage_t>(messages[i]);
                byte[] data = new byte[message.m_cbSize];
                Marshal.Copy(message.m_pData, data, 0, message.m_cbSize);
				ProcesData(data, message);
            } catch (Exception e) {
                Debug.LogError($"Error processing message: {e}");
            } finally {
                SteamNetworkingMessage_t.Release(messages[i]);
            }
        }
	}
	private void ProcesData(in byte[]data, SteamNetworkingMessage_t message) {	
		EPackagePurpuse purpose = (EPackagePurpuse)data[0];
		switch (purpose){
			case EPackagePurpuse.Transform:
				float[] farr = new float[7];
				for(int i = 1; i<29; i+=4)
					farr[i/4] = BitConverter.ToSingle(data[i..(i+4)]);
				cubes[0].transform.position = new(farr[0],farr[1],farr[2]);
				cubes[0].transform.rotation = new(farr[3],farr[4],farr[5],farr[6]);
				break;
			case EPackagePurpuse.SEX:
				Debug.Log($"Processed message from {message.m_identityPeer.GetSteamID()}: SEXXXXXXXXXXXXXXXXXXXX");
				break;
			default:
				Debug.LogError("unsupported purpose");
				break;
		}
	}
    void Update()
    {
		TryRecive();
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
