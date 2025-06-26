using UnityEngine;
using Steamworks;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using P2PMessages;
using Adrenak.UniVoice;
using UnityEditor;
using Adrenak.UniMic;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
public class P2PBase : MonoBehaviour
{
    enum EBulkPackage : byte
    {
        Transform,
        Action,
        Audio
	}
	internal static Dictionary<Vector3, NetworkTransform> networkTransforms = new();
	internal static List<byte> TransformBulk = new() { (byte)EBulkPackage.Transform };
	internal static Dictionary<Vector3, NetworkActions> networkActionScripts = new();
	internal static List<ActionInvokeMessage> networkActions = new();
    internal static AudioFrame audioFrame = new();
    protected HSteamNetConnection connection;
    protected bool isActive = false;
    public static event Action<AudioFrame> OnAudioRecieve;
    void LateUpdate()
    {
        if (!isActive || connection == HSteamNetConnection.Invalid) return;
        if (TransformBulk.Count > 1)
        {
            SendMessageToConnection(TransformBulk.ToArray(), (int)k_nSteamNetworkingSend.UnreliableNoNagle);
            TransformBulk = new() { (byte)EBulkPackage.Transform };
        }

        if (networkActions.Count != 0)
        {
            const int maxMessageSize = 16;
            List<byte> bulk = new(networkActions.Count * maxMessageSize + 1)
            {
                (byte)EBulkPackage.Action
            };
            bulk.AddRange(MemoryMarshal.AsBytes(networkActions.ToArray().AsSpan()).ToArray());
            SendMessageToConnection(bulk.ToArray(), (int)k_nSteamNetworkingSend.Reliable);
            networkActions.Clear();
        }
        if (audioFrame.samples != null)
        {

            int size = Marshal.SizeOf(audioFrame);
            byte[] arr = new byte[size];
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(audioFrame, ptr, false);
                Marshal.Copy(ptr, arr, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            Debug.Log("size:"+arr.Length);
            Debug.Log("bytes");
            foreach (byte b in arr)
                Debug.Log(b);
            Debug.Log("end");
            SendMessageToConnection(arr, (int)k_nSteamNetworkingSend.Reliable);
            
            audioFrame.samples = null;
        }
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
				ProcesData((EBulkPackage)data[0], data[1..]);
            } catch (Exception e) {
                Debug.LogError($"Error processing message: {e}");
            } finally {
                SteamNetworkingMessage_t.Release(messages[i]);
            }
        }
	}
    void ProcesData(EBulkPackage bulkPurpose, in byte[] bulkData)
    {
        switch (bulkPurpose)
        {
            case EBulkPackage.Transform:
                {
                    for (int i = 0; i < bulkData.Length; i += 32)
                    {
                        Span<byte> span = bulkData[i..(i + 32)];
                        if (span[i] == TransformRot.Purpuse)
                        {
                            var inst = MemoryMarshal.Read<TransformRot>(span);
                            networkTransforms[inst.ID].MoveToSync(inst.rot);
                            Debug.Log($"Recived: {inst.purpuse} {inst.ID} {inst.rot}");
                        }
                        else if (span[i] == TransformPos.Purpuse)
                        {
                            var inst = MemoryMarshal.Read<TransformPos>(span);
                            networkTransforms[inst.ID].MoveToSync(null, inst.pos);
                            Debug.Log($"Recived: {inst.purpuse} {inst.ID} {inst.pos}");
                        }
                    }
                    break;
                }
            case EBulkPackage.Action:
                {
                    Span<ActionInvokeMessage> InvokeMessage = MemoryMarshal.Cast<byte, ActionInvokeMessage>(bulkData);
                    foreach (ActionInvokeMessage a in InvokeMessage)
                    {
                        NetworkActions entityInstance = networkActionScripts[a.ID];
                        entityInstance.TriggerByIndex(a.Index);
                    }
                    break;
                }
            case EBulkPackage.Audio:
                {
                    AudioFrame audioFrame = new AudioFrame();
                    int size = Marshal.SizeOf(audioFrame);
                    IntPtr ptr = IntPtr.Zero;
                    try
                    {
                        ptr = Marshal.AllocHGlobal(size);

                        Marshal.Copy(bulkData, 0, ptr, size);
                        audioFrame = (AudioFrame)Marshal.PtrToStructure(ptr, audioFrame.GetType());
                        byte[] samples = new byte[audioFrame.samplesLength];

                        Marshal.Copy(audioFrame.intPtr, samples, 0, samples.Length);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                    OnAudioRecieve?.Invoke(audioFrame);
                    Debug.Log("Size:"+audioFrame.samples.Length);
                    break;
                }
            default:
                {
                    Debug.LogError("UNSUPORTED BULK");
                    Debug.LogWarning(bulkPurpose);
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

