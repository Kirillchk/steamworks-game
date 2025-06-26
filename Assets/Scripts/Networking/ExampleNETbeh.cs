using System;
using System.Runtime.InteropServices;
using UnityEngine;
using MessagePack;

public class ExampleNETbeh : NetworkActions
{
	[ContextMenu("Test")]
	void SUS()
	{
		TriggerSyncWargs(new Action<int, int>(Disentary), 1, 2);
	}
	[CanTriggerSyncWargs]
	public void Disentary(int num1, int num2)
		=> Debug.Log($"num1{num1} num2{num2}");
}
