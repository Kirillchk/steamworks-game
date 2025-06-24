using System;
using System.ComponentModel;
using UnityEngine;

public class ExampleNETbeh : NetworkActions
{
	Action<int> logAction = new Action<int>(x => Debug.Log(x));
	[ContextMenu("Test")]
	void SUS()
	{
		InvokeFromBytes(BitConverter.GetBytes(42),logAction);
	}
}
