using UnityEngine;

public class ExampleNETbeh : NetworkActions
{
	[ContextMenu("Test")]
	void SUS()
	{
		TriggerSync(Test);
	}
	[ContextMenu("Test2")]
	[CanTriggerSync]
	public void Test()
	{
		Debug.Log(actions.Count);
	}
}
