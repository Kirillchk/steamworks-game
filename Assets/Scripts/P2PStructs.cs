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
	public struct P2PTransformMessage {
		EPackagePurpuse purpuse { get => EPackagePurpuse.Transform; }
		public Vector3 pos { get; }  
		public Quaternion rot { get; } 
		public int ID { get; }
		public P2PTransformMessage(in byte[] byteArr){
			if (byteArr.Length < 33)
				throw new ArgumentException("Byte array too short");

			ReadOnlySpan<byte> floatBytes = byteArr.AsSpan(1, 28);
			ReadOnlySpan<float> farr = MemoryMarshal.Cast<byte, float>(floatBytes);
			
			pos = new(farr[0], farr[1], farr[2]);
			rot = new(farr[3], farr[4], farr[5], farr[6]);
			ID = MemoryMarshal.Read<int>(byteArr.AsSpan(29));
		}
		public P2PTransformMessage(in Vector3 position, in Quaternion rotation, in int id){
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
	//public struct P2PTransformPosition {
	//	public EPackagePurpuse purpuse { get => EPackagePurpuse.TransformPosition; }
	//	public Vector3 pos;
	//	public int ID;
	//	public P2PTransformPosition(in byte[] byteArr){
	//		float[] farr = new float[7];
	//		for(int i = 1; i<29; i+=4)
	//			farr[i/4] = BitConverter.ToSingle(byteArr[i..(i+4)]);
	//		pos = new(farr[0],farr[1],farr[2]);
	//		ID = BitConverter.ToInt32(byteArr, 29);  // Read ID from bytes 29-32
	//	}
	//	public P2PTransformPosition(in Vector3 position, in int id){
	//		pos = position;
	//		ID = id;
	//	}
	//	public byte[] GetBinaryRepresentation(){
	//		byte[] data = new byte[33];  // Increased from 29 to 33 to include ID
	//		data[0] = 0;
	//		float[] farr = { 
	//			pos.x, pos.y, pos.z,
	//		};
	//		for (int i = 0; i < 7; i++)
	//			Array.Copy(BitConverter.GetBytes(farr[i]), 0, data, i * 4 + 1, 4);
	//		Array.Copy(BitConverter.GetBytes(ID), 0, data, 29, 4);  // Add ID at the end
	//		return data;
	//	}
	//}
}