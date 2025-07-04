using UnityEngine;
using System;
using System.Runtime.InteropServices;
using MessagePack;

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
		TransformPosition,
		TransformRotation,
		TransformScale,
		Action,
		Delegate
	}
	public interface INetworkMessage
	{
		public static byte Purpuse;
		// TODO: should not be static 
		public static ReadOnlySpan<byte> StructToSpan<T>(T inp) where T : unmanaged
			=> MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref inp, 1));
	}
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
	[StructLayout(LayoutKind.Sequential)]
	public struct DelegateInvokeMessage : INetworkMessage
	{
		// TODO: this is just absolute ass 
		public byte[] GetBinary()
		{
			byte[] bytes = new byte[20 + Args.Length];

			// Write ID (Vector3 - 12 bytes)
			MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref ID, 1))
				.CopyTo(bytes.AsSpan(0, 12));

			// Write Index (4 bytes)
			MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref Index, 1))
				.CopyTo(bytes.AsSpan(12, 4));

			// Write Length (4 bytes)
			MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref Length, 1))
				.CopyTo(bytes.AsSpan(16, 4));

			if (Args.Length > 0)
				Args.CopyTo(bytes.AsSpan(20));

			return bytes;
		}
		public static byte Purpuse = (byte)EPackagePurpuse.Delegate;
		public Vector3 ID;
		public int Index;
		public int Length;
		public byte[] Args;
	}
}