using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace P2PMessages
{
	public enum k_nSteamNetworkingSend : int
	{
		//  https://github.com/rlabrecque/SteamworksSDK/blob/main/public/steam/steamnetworkingtypes.h#L954
		Unreliable = 0,
		NoNagle = 1,
		NoDelay = 4,
		Reliable = 8,
		UnreliableNoNagle = Unreliable | NoNagle,
		UnreliableNoDelay = Unreliable | NoDelay | NoNagle,
		ReliableNoNagle = Reliable | NoNagle,
	}
	enum EPackagePurpuse : byte
	{
		TransformPosRot,
		TransformPosition,
		TransformRotation,
		Action
	}
	
	public interface INetworkMessage
	{
		// TODO: should not be static 
		public static ReadOnlySpan<byte> StructToSpan<T>(T inp) where T : unmanaged
			=> MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref inp, 1));
	}
	[StructLayout(LayoutKind.Sequential, Size = 32)]
	public struct TransformPos : INetworkMessage
	{
		public static byte Purpuse = (byte)EPackagePurpuse.TransformPosition;
		public byte purpuse;
		public Vector3 ID;
		public Vector3 pos;
	}
	[StructLayout(LayoutKind.Sequential, Size = 32)]
	public struct TransformRot : INetworkMessage
	{
		public static byte Purpuse = (byte)EPackagePurpuse.TransformRotation;
		public byte purpuse;
		public Vector3 ID;
		public Quaternion rot;
	}
	public struct ActionInvokeMessage : INetworkMessage
	{
		const byte purpose = (byte)EPackagePurpuse.Action;
		const int messageSzie = 16; // ???
		public readonly Vector3 ID;
		public readonly int Index;
		public ActionInvokeMessage(in Vector3 id, in int index)
		{
			ID = id;
			Index = index;
		}
	}
}