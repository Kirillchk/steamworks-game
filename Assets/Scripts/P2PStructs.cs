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
	}
	public enum EPackagePurpuse : byte {
		Transform,
		TransformPosition,
		TransformRotation,
		Event
	}
	public struct P2PTransformPositionAndRotation : ITransformMessage {
		EPackagePurpuse purpose => EPackagePurpuse.Transform; 
		public int ID { get; } 
		public Vector3 pos { get; }  
		public Quaternion rot { get; }
		public P2PTransformPositionAndRotation(in byte[] byteArr){
			if (byteArr.Length < 33)
				throw new ArgumentException("Byte array too short");

			ReadOnlySpan<byte> floatBytes = byteArr.AsSpan(1, 28);
			ReadOnlySpan<float> farr = MemoryMarshal.Cast<byte, float>(floatBytes);
			
			pos = new(farr[0], farr[1], farr[2]);
			rot = new(farr[3], farr[4], farr[5], farr[6]);
			ID = MemoryMarshal.Read<int>(byteArr.AsSpan(29));
		}
		public P2PTransformPositionAndRotation(in Vector3 position, in Quaternion rotation, in int id){
			pos = position;
			rot = rotation;
			ID = id;
		}
		public byte[] GetBinaryRepresentation(){
			byte[] data = new byte[33];
			data[0] = (byte)purpose;
			
			float[] farr = { pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w };
			
			MemoryMarshal.AsBytes(farr.AsSpan()).CopyTo(data.AsSpan(1, 28));
			Array.Copy(BitConverter.GetBytes(ID), 0, data, 29, 4); 
			
			return data;
		}
	}
	public struct P2PTransformPosition : ITransformMessage {
		EPackagePurpuse purpose => EPackagePurpuse.TransformPosition;  
		public int ID { get; }
		public Vector3 pos { get; }
		public P2PTransformPosition(in byte[] byteArr){
			if (byteArr.Length < 17)
				throw new ArgumentException("Byte array too short");

			ReadOnlySpan<byte> floatBytes = byteArr.AsSpan(1, 12);
			ReadOnlySpan<float> farr = MemoryMarshal.Cast<byte, float>(floatBytes);
			
			pos = new(farr[0], farr[1], farr[2]);
			ID = MemoryMarshal.Read<int>(byteArr.AsSpan(13));
		}
		public P2PTransformPosition(in Vector3 position, in int id){
			pos = position;
			ID = id;
		}
		public byte[] GetBinaryRepresentation(){
			byte[] data = new byte[17];
			data[0] = (byte)purpose;
			
			float[] farr = { pos.x, pos.y, pos.z, };

			MemoryMarshal.AsBytes(farr.AsSpan()).CopyTo(data.AsSpan(1, 12));
			Array.Copy(BitConverter.GetBytes(ID), 0, data, 13, 4); 
			
			return data;
		}
	}
	public struct P2PTransformRotation : ITransformMessage{
		EPackagePurpuse purpose => EPackagePurpuse.TransformRotation; 
		public int ID { get; }
		public Quaternion rot { get; }
		
		public P2PTransformRotation(in byte[] byteArr) {
			if (byteArr.Length < 21)
				throw new ArgumentException("Byte array too short");

			ReadOnlySpan<byte> floatBytes = byteArr.AsSpan(1, 16);
			ReadOnlySpan<float> farr = MemoryMarshal.Cast<byte, float>(floatBytes);
			
			rot = new(farr[0], farr[1], farr[2], farr[3]);
			ID = MemoryMarshal.Read<int>(byteArr.AsSpan(17));
		}
		
		public P2PTransformRotation(in Quaternion rotation, in int id) {
			rot = rotation;
			ID = id;
		}
		
		public byte[] GetBinaryRepresentation() {
			byte[] data = new byte[21];
			data[0] = (byte)purpose;
			
			float[] farr = { rot.x, rot.y, rot.z, rot.w };
			
			MemoryMarshal.AsBytes(farr.AsSpan()).CopyTo(data.AsSpan(1, 16));
			Array.Copy(BitConverter.GetBytes(ID), 0, data, 17, 4); 
			
			return data;
		}
	}
	public interface ITransformMessage{
		EPackagePurpuse purpose => default; 
		public int ID { get; }
		public byte[] GetBinaryRepresentation();	
	}
}