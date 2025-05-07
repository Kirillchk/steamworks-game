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
	public enum EPackagePurpuse : byte 
	{
		Transform,
		TransformPosition,
		TransformRotation,
	}
	public struct TransformMessage
	{
		readonly EPackagePurpuse purpuse;
		readonly int messageSzie;
		public readonly Vector3 ID;
		public readonly Vector3? pos;
		public readonly Quaternion? rot;
		public TransformMessage(in Vector3 id, in Vector3? position = null, in Quaternion? rotation = null){
			ID = id;
			pos = position;
			rot = rotation;
			if(pos != null && rot != null) {
				purpuse = EPackagePurpuse.Transform;
				messageSzie = 41;
			} else if (pos!=null && rot == null) {
				purpuse = EPackagePurpuse.TransformPosition;
				messageSzie = 25;
			} else if (pos == null && rot != null) {
				purpuse = EPackagePurpuse.TransformRotation;
				messageSzie = 29;
			} else {
				throw new("FUCKED CONSTURCTOR");
			}
		}
		public TransformMessage(ReadOnlySpan<byte> byteSpan){
			purpuse = MemoryMarshal.Read<EPackagePurpuse>(byteSpan.Slice(0));
			if (purpuse == EPackagePurpuse.Transform) {
				pos = MemoryMarshal.Read<Vector3>(byteSpan.Slice(1, 12));
				rot = MemoryMarshal.Read<Quaternion>(byteSpan.Slice(13, 28));
				ID = MemoryMarshal.Read<Vector3>(byteSpan.Slice(29));
				messageSzie = 41;
			} else if(purpuse == EPackagePurpuse.TransformPosition) {
				pos = MemoryMarshal.Read<Vector3>(byteSpan.Slice(1, 12));
				rot = null;
				ID = MemoryMarshal.Read<Vector3>(byteSpan.Slice(13));
				messageSzie = 25;
			} else if (purpuse == EPackagePurpuse.TransformRotation){
				pos = null;
				rot = MemoryMarshal.Read<Quaternion>(byteSpan.Slice(1, 16));
				ID = MemoryMarshal.Read<Vector3>(byteSpan.Slice(17));
				messageSzie = 29;
			} else {
				throw new("FUCKED CONSTURCTOR");
			}
		}
		public ReadOnlySpan<byte> GetBinaryRepresentation(){
			Span<byte>  res = new byte[messageSzie];
			res[0] = (byte)purpuse;

			if (purpuse == EPackagePurpuse.Transform) {
				MemoryMarshal.Cast<byte, Vector3>(res.Slice(1, 12))[0] = pos.Value;
				MemoryMarshal.Cast<byte, Quaternion>(res.Slice(13, 16))[0] = rot.Value;
				MemoryMarshal.Cast<byte, Vector3>(res.Slice(29, 12))[0] = ID; 
			} else if(purpuse == EPackagePurpuse.TransformPosition) {
				MemoryMarshal.Cast<byte, Vector3>(res.Slice(1, 12))[0] = pos.Value;
				MemoryMarshal.Cast<byte, Vector3>(res.Slice(13, 12))[0] = ID; 
			} else if (purpuse == EPackagePurpuse.TransformRotation){
				MemoryMarshal.Cast<byte, Quaternion>(res.Slice(1, 16))[0] = rot.Value;
				MemoryMarshal.Cast<byte, Vector3>(res.Slice(17, 12))[0] = ID;
			} else {
				throw new("FUCKED CONSTURCTOR");
			}
			return res;
		}
	}
	public struct ActionInvokeMessage
	{
		const int messageSzie = 16;
		public readonly Vector3 ID;
		public readonly int Index;
		public ActionInvokeMessage(in Vector3 id, in int index)
		{
			ID = id;
			Index = index;
		}
		public ActionInvokeMessage(ReadOnlySpan<byte> byteSpan)
		{
			ID = MemoryMarshal.Read<Vector3>(byteSpan.Slice(0,12));
			Index = MemoryMarshal.Read<int>(byteSpan.Slice(13));
		}
		public ReadOnlySpan<byte> GetBinaryRepresenation(){
			Span<byte> res = new byte[messageSzie];
			MemoryMarshal.Cast<byte, Vector3>(res.Slice(0,12))[0] = ID;
			MemoryMarshal.Cast<byte, int>(res.Slice(13,4))[0] = Index;
			return res;
		}
	}
}