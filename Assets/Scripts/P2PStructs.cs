using UnityEngine;
using System;
public struct TransformMessage {
	public Vector3 pos;
	public Quaternion rot;
	public long ID;
	public TransformMessage(in byte[] byteArr){
		float[] farr = new float[7];
		for(int i = 1; i<29; i+=4)
			farr[i/4] = BitConverter.ToSingle(byteArr[i..(i+4)]);
		pos = new(farr[0],farr[1],farr[2]);
		rot = new(farr[3],farr[4],farr[5],farr[6]);
		ID = 0;
	}
	public TransformMessage(Vector3 position, Quaternion rotation){
		pos = position;
		rot = rotation;
		ID = 0;
	}
	public byte[] GetBinaryRepresentation(){
		byte[] data = new byte[29];
		data[0] = 0;
		float[] farr = { 
			pos.x, pos.y, pos.z,
			rot.x, rot.y, rot.z, rot.w
		};
        for (int i = 0; i < 7; i++)
		    Array.Copy(BitConverter.GetBytes(farr[i]), 0, data, i * 4 + 1, 4);
		return data;
	}
}