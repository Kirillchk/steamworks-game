using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class ExampleNETbeh : NetworkActions
{
	[ContextMenu("Test")]
	void SUS()
	{
		TriggerSyncWargs(new Action<int>(Disentary), 1);
	}
	[CanTriggerSyncWargs]
	public void Disentary(int num1)
		=> Debug.Log($"num1{num1} num2{num1}");
}
