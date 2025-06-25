using System;
using UnityEngine;
using P2PMessages;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class ExampleNETbeh : NetworkActions
{
	[ContextMenu("Test")]
	void SUS()
	{
		//InvokeFromBytes(BitConverter.GetBytes(42), logAction);
		//DelegateInvokeMessage v1 = new()
		//{
		//	ID = new Vector3(0, 2, 1),
		//	Index = 2,
		//	Length = 8,
		//	Args = new byte[] { 0, 2, 3, 4, 7, 8, 9, 1 }
		//};
		//DelegateInvokeMessage v2 = new()
		//{
		//	ID = new Vector3(0, 3, 0),
		//	Index = 4,
		//	Length = 4,
		//	Args = new byte[] { 0, 2, 3, 4 }
		//};
		//DelegateInvokeMessage v3 = new()
		//{
		//	ID = new Vector3(0, 2, 1),
		//	Index = 2,
		//	Length = 12,
		//	Args = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }
		//};
		//List<DelegateInvokeMessage> list = new() { v1, v2, v3 };
		//List<byte> bytes = new();
		//foreach (var v in list)
		//	bytes.AddRange(v.GetBinary());
		//byte[] bytesArr = bytes.ToArray();

	}
	[CanTriggerSyncWargs]
	public void Disentary(int num1, int num2)
		=> Debug.Log($"num1{num1} num2{num2}");
}
