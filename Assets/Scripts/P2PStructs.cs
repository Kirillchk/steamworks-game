using UnityEngine;
using System;
using System.Runtime.InteropServices;
using UnityEditor.PackageManager;

namespace P2PMessages
{
	// https://github.com/rlabrecque/SteamworksSDK/blob/main/public/steam/steamnetworkingtypes.h#L954
	public enum k_nSteamNetworkingSend : int{
		Unreliable = 0,
		NoNagle = 1,
		NoDelay = 4,
		Reliable = 8,
		UnreliableNoNagle = Unreliable | NoNagle,
		UnreliableNoDelay = Unreliable | NoDelay | NoNagle,
		ReliableNoNagle = Reliable | NoNagle,
	}
	public enum EPackagePurpuse : byte {
		Transform,
		TransformPosition,
		TransformRotation,
	}
	public struct P2PTransformPositionAndRotation : ITransformMessage {
		const int messageSzie = 41; // Updated: 33 + 8 (Vector3 ID is 12 bytes instead of 4)
		static readonly EPackagePurpuse purpose = EPackagePurpuse.Transform; 
		public Vector3 ID { get; } 
		public Vector3 pos { get; }  
		public Quaternion rot { get; }
		public P2PTransformPositionAndRotation(ReadOnlySpan<byte> byteSpan){
			pos = MemoryMarshal.Read<Vector3>(byteSpan.Slice(1, 12));
			rot = MemoryMarshal.Read<Quaternion>(byteSpan.Slice(13, 28));
			ID = MemoryMarshal.Read<Vector3>(byteSpan.Slice(29));
		}
		public P2PTransformPositionAndRotation(in Vector3 position, in Quaternion rotation, in Vector3 id){
			pos = position;
			rot = rotation;
			ID = id;
		}
		public ReadOnlySpan<byte> GetBinaryRepresentation(){
			Span<byte> data = new byte[messageSzie];
			data[0] = (byte)purpose;

			MemoryMarshal.Cast<byte, Vector3>(data.Slice(1, 12))[0] = pos;
			MemoryMarshal.Cast<byte, Quaternion>(data.Slice(13, 16))[0] = rot;
			MemoryMarshal.Cast<byte, Vector3>(data.Slice(29, 12))[0] = ID; // Updated slice size for Vector3
					
			return data;
		}
	}
	public struct P2PTransformPosition : ITransformMessage {
		const int messageSzie = 25; // Updated: 17 + 8 (Vector3 ID is 12 bytes instead of 4)
		static readonly EPackagePurpuse purpose = EPackagePurpuse.TransformPosition;  
		public Vector3 ID { get; }
		public Vector3 pos { get; }
		public P2PTransformPosition(ReadOnlySpan<byte> byteSpan){
			pos = MemoryMarshal.Read<Vector3>(byteSpan.Slice(1, 12));
			ID = MemoryMarshal.Read<Vector3>(byteSpan.Slice(13));
		}
		public P2PTransformPosition(in Vector3 position, in Vector3 id){
			pos = position;
			ID = id;
		}
		public ReadOnlySpan<byte> GetBinaryRepresentation() {
			Span<byte> data = new byte[messageSzie];
			data[0] = (byte)purpose;

			MemoryMarshal.Cast<byte, Vector3>(data.Slice(1, 12))[0] = pos;
			MemoryMarshal.Cast<byte, Vector3>(data.Slice(13, 12))[0] = ID; // Updated slice size for Vector3

			return data;
		}
	}
	public struct P2PTransformRotation : ITransformMessage{
		const int messageSzie = 29; // Updated: 21 + 8 (Vector3 ID is 12 bytes instead of 4)
		static readonly EPackagePurpuse purpose = EPackagePurpuse.TransformRotation; 
		public Vector3 ID { get; }
		public Quaternion rot { get; }
		
		public P2PTransformRotation(ReadOnlySpan<byte> byteSpan) {
			rot = MemoryMarshal.Read<Quaternion>(byteSpan.Slice(1, 16));
			ID = MemoryMarshal.Read<Vector3>(byteSpan.Slice(17));
		}
		
		public P2PTransformRotation(in Quaternion rotation, in Vector3 id) {
			rot = rotation;
			ID = id;
		}
		
		public ReadOnlySpan<byte> GetBinaryRepresentation() {
			Span<byte> data = new byte[messageSzie];
			data[0] = (byte)purpose;

			MemoryMarshal.Cast<byte, Quaternion>(data.Slice(1, 16))[0] = rot;
			MemoryMarshal.Cast<byte, Vector3>(data.Slice(17, 12))[0] = ID; // Updated slice size for Vector3

			return data;
		}
		
	}
	public interface ITransformMessage{
		static readonly EPackagePurpuse purpose; 
		public Vector3 ID { get; }
		public ReadOnlySpan<byte> GetBinaryRepresentation();
	}
	public struct TransformMessage{
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
}