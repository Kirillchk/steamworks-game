using UnityEngine;
using System;
using System.Runtime.InteropServices;

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
		const int messageSzie = 33;
		static readonly EPackagePurpuse purpose = EPackagePurpuse.Transform; 
		public int ID { get; } 
		public Vector3 pos { get; }  
		public Quaternion rot { get; }
		public P2PTransformPositionAndRotation(ReadOnlySpan<byte> byteSpan){
			pos = MemoryMarshal.Read<Vector3>(byteSpan.Slice(1, 12));
			rot = MemoryMarshal.Read<Quaternion>(byteSpan.Slice(13, 28));
			ID = MemoryMarshal.Read<int>(byteSpan.Slice(29));
		}
		public P2PTransformPositionAndRotation(in Vector3 position, in Quaternion rotation, in int id){
			pos = position;
			rot = rotation;
			ID = id;
		}
		public ReadOnlySpan<byte> GetBinaryRepresentation(){
			Span<byte> data = new byte[messageSzie];
			data[0] = (byte)purpose;

			MemoryMarshal.Cast<byte, Vector3>(data.Slice(1, 12))[0] = pos;
			MemoryMarshal.Cast<byte, Quaternion>(data.Slice(13, 16))[0] = rot;
			MemoryMarshal.Cast<byte, int>(data.Slice(29, 4))[0] = ID;
					
			return data;
		}
	}
	public struct P2PTransformPosition : ITransformMessage {
		const int messageSzie = 17;
		static readonly EPackagePurpuse purpose = EPackagePurpuse.TransformPosition;  
		public int ID { get; }
		public Vector3 pos { get; }
		public P2PTransformPosition(ReadOnlySpan<byte> byteSpan){
			pos = MemoryMarshal.Read<Vector3>(byteSpan.Slice(1, 12));
			ID = MemoryMarshal.Read<int>(byteSpan.Slice(13));
		}
		public P2PTransformPosition(in Vector3 position, in int id){
			pos = position;
			ID = id;
		}
		public ReadOnlySpan<byte> GetBinaryRepresentation() {
			Span<byte> data = new byte[17];
			data[0] = (byte)purpose;

			MemoryMarshal.Cast<byte, Vector3>(data.Slice(1, 12))[0] = pos;
			MemoryMarshal.Cast<byte, int>(data.Slice(13, 4))[0] = ID;

			return data;
		}
	}
	public struct P2PTransformRotation : ITransformMessage{
		const int messageSzie = 21;
		static readonly EPackagePurpuse purpose = EPackagePurpuse.TransformRotation; 
		public int ID { get; }
		public Quaternion rot { get; }
		
		public P2PTransformRotation(ReadOnlySpan<byte> byteSpan) {
			rot = MemoryMarshal.Read<Quaternion>(byteSpan.Slice(1, 16));
			ID = MemoryMarshal.Read<int>(byteSpan.Slice(17));
		}
		
		public P2PTransformRotation(in Quaternion rotation, in int id) {
			rot = rotation;
			ID = id;
		}
		
		public ReadOnlySpan<byte> GetBinaryRepresentation() {
			Span<byte> data = new byte[21];
			data[0] = (byte)purpose;

			MemoryMarshal.Cast<byte, Quaternion>(data.Slice(1, 16))[0] = rot;
			MemoryMarshal.Cast<byte, int>(data.Slice(17, 4))[0] = ID;

			return data;
		}
		
	}
	public interface ITransformMessage{
		static readonly EPackagePurpuse purpose; 
		public int ID { get; }
		public ReadOnlySpan<byte> GetBinaryRepresentation();
	}
}