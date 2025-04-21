using UnityEngine;
using System;
using Unity.VisualScripting;
using System.Runtime.InteropServices;
namespace P2PMessages
{
	// wtf is this?
	public enum k_nSteamNetworkingSend : int{
		Unreliable = 0,
		Reliable = 1,
		NoNagle = 2,
		NoDelay = 4,
	}
	public enum EPackagePurpuse : byte {
		Transform,
		TransformPosition,
		TransformRotation,
		Event,
		SEX
	}
	public struct P2PTransformPositionAndRotation {
		EPackagePurpuse purpuse { get => EPackagePurpuse.Transform; }
		public Vector3 pos { get; }  
		public Quaternion rot { get; } 
		public int ID { get; }
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
			data[0] = (byte)purpuse;
			
			// Create the float array
			float[] farr = { pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w };
			
			// Get a span for the entire float array and copy it in one operation
			MemoryMarshal.AsBytes(farr.AsSpan()).CopyTo(data.AsSpan(1, 28));
			
			Array.Copy(BitConverter.GetBytes(ID), 0, data, 29, 4); 
			
			return data;
		}
	}
	public struct P2PTransformPosition {
		EPackagePurpuse purpuse { get => EPackagePurpuse.TransformPosition; }
		public Vector3 pos { get; } 
		public int ID { get; }
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
			data[0] = (byte)purpuse;
			
			float[] farr = { pos.x, pos.y, pos.z, };

			MemoryMarshal.AsBytes(farr.AsSpan()).CopyTo(data.AsSpan(1, 12));
			Array.Copy(BitConverter.GetBytes(ID), 0, data, 13, 4); 
			
			return data;
		}
	}
}