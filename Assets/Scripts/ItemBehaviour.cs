using System;
using UnityEngine;

public class ItemBehaviour : NetworkActions
{
	public Action UseItem;
	[CanTriggerSync]
	public void PickUp()
	{
		var RB = GetComponent<Rigidbody>();
		RB.isKinematic = true;
		RB.useGravity = false;
		
		var colliders = GetComponents<Collider>();
		foreach (var collider in colliders)
			collider.enabled = false;
	}
	[CanTriggerSync]
	public void Drop()
	{
		var RB = GetComponent<Rigidbody>();
		RB.isKinematic = false;
		RB.useGravity = true;

		var colliders = GetComponents<Collider>();
		foreach (var collider in colliders)
			collider.enabled = true;
	}
}
