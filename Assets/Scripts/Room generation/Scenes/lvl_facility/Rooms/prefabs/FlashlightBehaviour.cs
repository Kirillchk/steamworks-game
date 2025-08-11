using UnityEngine;

public class FlashlightBehaviour : ItemBehaviour
{
	public Light Spotlight;
	void Start() =>
		UseItem = () => this.Sync(ToggleLight);
	[CanTriggerSync]
	public void ToggleLight()
	{
		Spotlight.enabled = !Spotlight.enabled;
	}
}
