using UnityEngine;

public class TestActions : NetworkActions 
{
	[ContextMenu("i suck ass"), CanTriggerSync]
	public void Trigger()
	{
		Debug.Log("MY NAME IS BALLS");
	}
	[ContextMenu("i suck assync")]
	public void assync()
	{
		TriggerSync(Trigger);
	}
}
