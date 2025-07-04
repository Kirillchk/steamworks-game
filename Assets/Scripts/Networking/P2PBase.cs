using UnityEngine;
using Steamworks;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Adrenak.UniVoice;
using MessagePack;
using Adrenak.UniMic;
public class P2PBase : MonoBehaviour
{
	public enum k_nSteamNetworkingSend : int
	{
		// https://github.com/rlabrecque/SteamworksSDK/blob/main/public/steam/steamnetworkingtypes.h#L954
		Unreliable = 0,
		NoNagle = 1,
		NoDelay = 4,
		Reliable = 8,
		UnreliableNoNagle = Unreliable | NoNagle,
		UnreliableNoDelay = Unreliable | NoDelay | NoNagle,
		ReliableNoNagle = Reliable | NoNagle,
	}
	enum EBulkPackage : byte
	{
		Transform,
		Delegate,
		Audio
	}
	internal static Dictionary<Vector3, NetworkTransform> networkTransforms = new();
	internal static List<TransformPack> TransformPacks = new();
	[MessagePackObject]
	public struct TransformPack
	{
		[Key(0)]
		public Vector3 ID;
		[Key(1)]
		public Vector3? newPos;
		[Key(2)]
		public Quaternion? newRot;
		[Key(3)]
		public Vector3? newScl;
	}
	internal static Dictionary<Vector3, NetworkActions> networkActionScripts = new();
	internal static List<DelegatePack> DelegatePacks = new();
	[MessagePackObject]
	public struct DelegatePack
	{
		[Key(0)]
		public Vector3 ID;
		[Key(1)]
		public int Index;
		[Key(2)]
		public int Length;
		[Key(3)]
		public byte[] Args;
	}
	public MicAudioSource source;
	public static AudioFrame audioFrame = new AudioFrame();

	protected HSteamNetConnection connection;
	protected bool isActive = false;
	void LateUpdate()
	{
		if (!isActive || connection == HSteamNetConnection.Invalid) return;
		if (TransformPacks.Count > 0)
		{
			List<byte> bulk = new(1024 + 1) { (byte)EBulkPackage.Transform };
			bulk.AddRange(MessagePackSerializer.Serialize(TransformPacks));
			SendMessageToConnection(bulk.ToArray(), (int)k_nSteamNetworkingSend.UnreliableNoNagle);
			TransformPacks.Clear();
		}
		if (DelegatePacks.Count > 0)
		{
			List<byte> bulk = new(64 + 1) { (byte)EBulkPackage.Delegate };
			bulk.AddRange(MessagePackSerializer.Serialize(DelegatePacks));
			SendMessageToConnection(bulk.ToArray(), (int)k_nSteamNetworkingSend.Reliable);
			DelegatePacks.Clear();
		}
		if (audioFrame.samples != null)
		{
			// che bliat?
			// audioFrame.id = 2; <- nahuia
			List<byte> bulk = new(1024 + 1) { (byte)EBulkPackage.Audio };
			bulk.AddRange(MessagePackSerializer.Serialize(audioFrame));
			SendMessageToConnection(bulk.ToArray(), (int)k_nSteamNetworkingSend.Reliable);
			// Debug.Log("audioFrame.samples.Length" + audioFrame.samples.Length);
			// Debug.Log("bytes.Length"+bytes.Length);
			// Debug.Log("BYTES");
			// for (int i = 0; i < 10; i++)
			//     Debug.Log(bytes[i]);

			audioFrame.samples = null;
		}
	}
	
	void ProcesData(EBulkPackage bulkPurpose, in byte[] bulkData) {	
		switch(bulkPurpose)
		{
			case EBulkPackage.Transform:
			{
				var packs = MessagePackSerializer.Deserialize<List<TransformPack>>(bulkData);
				foreach (var pack in packs)
					networkTransforms[pack.ID].TransformSync(pack);
				break;
			}
			case EBulkPackage.Delegate:
			{
				var packs = MessagePackSerializer.Deserialize<List<DelegatePack>>(bulkData);
				foreach (var pack in packs) 
					networkActionScripts[pack.ID].InvokeFromBytes(pack.Index, pack.Args);
				break;
			}
			case EBulkPackage.Audio:
			{
				var audioFrame = MessagePackSerializer.Deserialize<AudioFrame>(bulkData);
				source.RecieveFrame(audioFrame);
				break;
			}
			default: 
			{
				Debug.LogError("UNSUPORTED BULK");
				break;
			}
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
			//if (result != EResult.k_EResultOK)
			//	Debug.LogError($"Failed to send message: {result}");
			//else 
			//	Debug.Log($"Message sent successfully (ID: {messageNumber}, Size: {data.Length} bytes)");
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
		source = MicAudioSource.only;
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

