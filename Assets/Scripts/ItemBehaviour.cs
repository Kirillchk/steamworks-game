using System;
using JetBrains.Annotations;
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

		GiveOwnershipBack();
	}
	// TODO: Cheater can take ownership over everything
	// Fixo thiso bulshido
	// Also ownership system should belong in network identity 
	public void TakeOwnership()
	{
		this.Sync(SetOwnershipFalse);
		GetComponent<NetworkIdentity>().isOwner = true;
	}
	[CanTriggerSync]
	public void SetOwnershipFalse() {
		GetComponent<NetworkIdentity>().isOwner = false;
	}
	public void GiveOwnershipBack()
	{
		GetComponent<NetworkIdentity>().isOwner = P2PBase.isHost;
	}
}
