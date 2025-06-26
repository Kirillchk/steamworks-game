using System;
using UnityEngine;

public class ExampleNETbeh : NetworkActions
{
	[ContextMenu("Test1")]
	void SUS()
	{
		TriggerSyncWargs(new Action<int, int>(Disentary), 1, 2);
	}
	[ContextMenu("Test2")]
	void sus()
	{
		TriggerSyncWargs(new Action(AFAF));
	}
	[ContextMenu("Test3")]
	void sosat()
	{
		TriggerSyncWargs(new Action(AFAF));
		TriggerSyncWargs(new Action<int, int>(Disentary), 1, 2);
		TriggerSyncWargs(new Action<int, int>(Disentary), 1, 2);
		TriggerSyncWargs(new Action(AFAF));
	}
	[CanTriggerSync]
	public void Disentary(int num1, int num2)
		=> Debug.Log($"num1{num1} num2{num2}");
	[CanTriggerSync]
	public void AFAF()
		=> Debug.Log($"triggered nahui");
}
