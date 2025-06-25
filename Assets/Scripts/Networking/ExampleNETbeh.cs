using System;
using UnityEngine;

public class ExampleNETbeh : NetworkActions
{
	[ContextMenu("Test")]
	void SUS()
	{
		//TriggerSyncWargs(this.Disentary, BitConverter.GetBytes(9));
	}
	[CanTriggerSyncWargs]
	public void Disentary(int num1)
		=> Debug.Log($"num1{num1} num2{num1}");
}
