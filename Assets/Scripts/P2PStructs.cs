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
			ReadOnlySpan<float> farr = MemoryMarshal.Cast<byte, float>(byteSpan.Slice(1, 28));
			
			pos = new(farr[0], farr[1], farr[2]);
			rot = new(farr[3], farr[4], farr[5], farr[6]);
			ID = MemoryMarshal.Read<int>(byteSpan.Slice(29));
		}
		public P2PTransformPositionAndRotation(in Vector3 position, in Quaternion rotation, in int id){
			pos = position;
			rot = rotation;
			ID = id;
		}
		public ReadOnlySpan<byte> GetBinaryRepresentation(){
			byte[] data = new byte[messageSzie];
			data[0] = (byte)purpose;
			
			float[] farr = { pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w };
			
			MemoryMarshal.AsBytes(farr.AsSpan()).CopyTo(data.AsSpan(1, 28));
			Array.Copy(BitConverter.GetBytes(ID), 0, data, 29, 4); 
			
			return data;
		}
	}
	public struct P2PTransformPosition : ITransformMessage {
		const int messageSzie = 17;
		static readonly EPackagePurpuse purpose = EPackagePurpuse.TransformPosition;  
		public int ID { get; }
		public Vector3 pos { get; }
		public P2PTransformPosition(ReadOnlySpan<byte> byteSpan){
			ReadOnlySpan<float> farr = MemoryMarshal.Cast<byte, float>(byteSpan.Slice(1, 12));
			
			pos = new(farr[0], farr[1], farr[2]);
			ID = MemoryMarshal.Read<int>(byteSpan.Slice(13));
		}
		public P2PTransformPosition(in Vector3 position, in int id){
			pos = position;
			ID = id;
		}
		public ReadOnlySpan<byte> GetBinaryRepresentation(){
			byte[] data = new byte[messageSzie];
			data[0] = (byte)purpose;
			
			float[] farr = { pos.x, pos.y, pos.z, };

			MemoryMarshal.AsBytes(farr.AsSpan()).CopyTo(data.AsSpan(1, 12));
			Array.Copy(BitConverter.GetBytes(ID), 0, data, 13, 4); 
			
			return data;
		}
	}
	public struct P2PTransformRotation : ITransformMessage{
		const int messageSzie = 21;
		static readonly EPackagePurpuse purpose = EPackagePurpuse.TransformRotation; 
		public int ID { get; }
		public Quaternion rot { get; }
		
		public P2PTransformRotation(ReadOnlySpan<byte> byteSpan) {
			ReadOnlySpan<float> farr = MemoryMarshal.Cast<byte, float>(byteSpan.Slice(1, 16));
			
			rot = new(farr[0], farr[1], farr[2], farr[3]);
			ID = MemoryMarshal.Read<int>(byteSpan.Slice(17));
		}
		
		public P2PTransformRotation(in Quaternion rotation, in int id) {
			rot = rotation;
			ID = id;
		}
		
		public ReadOnlySpan<byte> GetBinaryRepresentation() {
			byte[] data = new byte[messageSzie];
			data[0] = (byte)purpose;
			
			float[] farr = { rot.x, rot.y, rot.z, rot.w };
			
			MemoryMarshal.AsBytes(farr.AsSpan()).CopyTo(data.AsSpan(1, 16));
			Array.Copy(BitConverter.GetBytes(ID), 0, data, 17, 4); 
			
			return data;
		}
		
	}
	public interface ITransformMessage{
		static readonly EPackagePurpuse purpose; 
		public int ID { get; }
		public ReadOnlySpan<byte> GetBinaryRepresentation();
	}
}