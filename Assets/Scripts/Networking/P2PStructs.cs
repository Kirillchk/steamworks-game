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
	[StructLayout(LayoutKind.Sequential, Size = 32)]
	public struct TransformScl : INetworkMessage
	{
		public static byte Purpuse = (byte)EPackagePurpuse.TransformScale;
		public byte purpuse;
		public Vector3 ID;
		public Vector3 scl;
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