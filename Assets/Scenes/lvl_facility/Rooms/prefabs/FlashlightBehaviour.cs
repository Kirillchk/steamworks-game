using UnityEngine;

public class FlashlightBehaviour : ItemBehaviour
{
	public Light Spotlight;
	void Start() =>
		UseItem = () =>
			Spotlight.enabled = !Spotlight.enabled;
	
}
