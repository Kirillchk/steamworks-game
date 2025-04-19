using UnityEngine;
using System;
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
		Event,
		SEX
	}
	public struct P2PTransformMessage {
		public EPackagePurpuse purpuse { get => EPackagePurpuse.Transform; }
		public Vector3 pos;
		public Quaternion rot;
		public int ID;
		public P2PTransformMessage(in byte[] byteArr){
			float[] farr = new float[7];
			for(int i = 1; i<29; i+=4)
				farr[i/4] = BitConverter.ToSingle(byteArr[i..(i+4)]);
			pos = new(farr[0],farr[1],farr[2]);
			rot = new(farr[3],farr[4],farr[5],farr[6]);
			ID = BitConverter.ToInt32(byteArr, 29);  // Read ID from bytes 29-32
		}
		public P2PTransformMessage(in Vector3 position, in Quaternion rotation, in int id){
			pos = position;
			rot = rotation;
			ID = id;
		}
		public byte[] GetBinaryRepresentation(){
			byte[] data = new byte[33];  // Increased from 29 to 33 to include ID
			data[0] = 0;
			float[] farr = { 
				pos.x, pos.y, pos.z,
				rot.x, rot.y, rot.z, rot.w
			};
			for (int i = 0; i < 7; i++)
				Array.Copy(BitConverter.GetBytes(farr[i]), 0, data, i * 4 + 1, 4);
			Array.Copy(BitConverter.GetBytes(ID), 0, data, 29, 4);  // Add ID at the end
			return data;
		}
	}
}