using UnityEngine;
using Steamworks;
using System.Collections.Generic;
using Adrenak.UniVoice;
using MessagePack;
using Adrenak.UniMic;
public class P2PBase : P2PSteamBehaviour
{
	static internal bool isHost = false;
	enum k_nSteamNetworkingSend : int
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
	public static MicAudioSource source;
	public static AudioFrame audioFrame = new AudioFrame();
	void SendPackages()
	{
		if (!isActive || connection == HSteamNetConnection.Invalid)
			return;
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
			List<byte> bulk = new(1024 + 1) { (byte)EBulkPackage.Audio };
			bulk.AddRange(MessagePackSerializer.Serialize(audioFrame));
			SendMessageToConnection(bulk.ToArray(), (int)k_nSteamNetworkingSend.Reliable);
			audioFrame.samples = null;
		}
	}
	void ProcesData(in List<byte[]> data) {
		if (data == null)
			return;
		for (int i = 0; i < data.Count; i++)
		{
			byte[] bulk = data[i];
			var bulkPurpose = (EBulkPackage)bulk[0];
			var bulkData = bulk[1..];
			switch (bulkPurpose)
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
	}
	void Update()
		=> ProcesData(TryReceive());
	void LateUpdate()
		=> SendPackages();
}

